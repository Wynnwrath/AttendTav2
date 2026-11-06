using System;
using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Resources;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Services;

/// <summary>
/// Administrator-only role assignment. Every change runs in one transaction that takes the
/// <c>attendta:last-admin</c> application lock before the <c>users</c> row lock whenever the
/// change could remove an active Administrator.
/// </summary>
public sealed class AccountRoleService : IAccountRoleService
{
    private const int ReasonMaxLength = 1000;

    private readonly IAccountAdministrationRepository _accountAdministrationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentActor _currentActor;
    private readonly IActivityLogWriter _activityLogWriter;

    public AccountRoleService(
        IAccountAdministrationRepository accountAdministrationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ICurrentActor currentActor,
        IActivityLogWriter activityLogWriter)
    {
        _accountAdministrationRepository = accountAdministrationRepository ?? throw new ArgumentNullException(nameof(accountAdministrationRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _currentActor = currentActor ?? throw new ArgumentNullException(nameof(currentActor));
        _activityLogWriter = activityLogWriter ?? throw new ArgumentNullException(nameof(activityLogWriter));
    }

    /// <inheritdoc />
    public async Task<ChangeAccountRoleViewModel> GetRoleChangeAsync(int accountId, CancellationToken cancellationToken = default)
    {
        RequireAdministratorActor();

        var account = await _accountAdministrationRepository.GetAccountAsync(accountId, cancellationToken);
        if (account is null)
        {
            throw new DomainException(OutcomeCode.NotFound, "Account not found.");
        }

        var roles = await _accountAdministrationRepository.GetRolesAsync(cancellationToken);

        return new ChangeAccountRoleViewModel
        {
            AccountId = account.AccountId,
            FullName = account.FullName,
            Email = account.Email,
            CurrentRoleCode = account.RoleCode,
            CurrentRoleLabel = account.RoleLabel,
            RoleCode = account.RoleCode,
            Roles = roles
        };
    }

    /// <inheritdoc />
    public async Task ChangeRoleAsync(ChangeAccountRoleViewModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var actorUserId = RequireAdministratorActor();
        var requestedRole = RequiredRole(model.RoleCode);
        var reason = RequiredReason(model.Reason);

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var snapshot = await _accountAdministrationRepository.GetAccountAsync(model.AccountId, ct);
            if (snapshot is null)
            {
                throw new DomainException(OutcomeCode.NotFound, "Account not found.");
            }

            // The pre-lock read only decides whether the application lock is needed. The
            // authoritative decision is made after the users row lock is held.
            var preLockDemotion = IsActiveAdministratorDemotion(
                snapshot.IsActive,
                string.Equals(snapshot.RoleCode, RoleCodes.Admin, StringComparison.Ordinal),
                requestedRole);

            if (preLockDemotion)
            {
                await _accountAdministrationRepository.AcquireLastAdministratorLockAsync(ct);
            }

            var user = await _userRepository.LockUserAsync(model.AccountId, ct);
            if (user is null)
            {
                throw new DomainException(OutcomeCode.NotFound, "Account not found.");
            }

            var postLockDemotion = IsActiveAdministratorDemotion(user.IsActive, user.RoleId == RoleCode.Admin, requestedRole);
            if (postLockDemotion && !preLockDemotion)
            {
                // The row changed between the pre-lock read and the row lock, so taking the
                // application lock now would break the documented lock order.
                throw new DomainException(OutcomeCode.Stale, "This account changed while the role was being assigned. Review it and try again.");
            }

            if (user.RoleId == requestedRole)
            {
                // A no-op succeeds without a duplicate change log.
                return;
            }

            if (postLockDemotion)
            {
                var activeAdministrators = await _accountAdministrationRepository.CountActiveAdministratorsAsync(ct);
                if (activeAdministrators <= 1)
                {
                    throw new DomainException(OutcomeCode.Stale, "The last active Administrator cannot be demoted.");
                }
            }

            var oldRoleCode = await _userRepository.GetRoleCodeAsync(user.RoleId, ct) ?? user.RoleId.ToString();
            var newRoleCode = await _userRepository.GetRoleCodeAsync(requestedRole, ct) ?? requestedRole.ToString();

            user.RoleId = requestedRole;
            user.UpdatedById = actorUserId;
            user.UpdatedTime = _timeProvider.GetUtcNow().UtcDateTime;
            await _unitOfWork.SaveChangesAsync(ct);

            _activityLogWriter.Append(new AuditEntry(
                AuditActions.UserRoleAssigned,
                "users",
                user.Id,
                Reason: reason,
                OldValues: new RoleAuditValues(oldRoleCode),
                NewValues: new RoleAuditValues(newRoleCode)));

            await _unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
    }

    private static bool IsActiveAdministratorDemotion(bool isActive, bool isAdministrator, RoleCode requestedRole) =>
        isActive && isAdministrator && requestedRole != RoleCode.Admin;

    private static RoleCode RequiredRole(string roleCode) =>
        roleCode?.Trim().ToLowerInvariant() switch
        {
            RoleCodes.Member => RoleCode.Member,
            RoleCodes.Officer => RoleCode.Officer,
            RoleCodes.Admin => RoleCode.Admin,
            _ => throw new DomainException(OutcomeCode.Invalid, "Select a valid role.", nameof(ChangeAccountRoleViewModel.RoleCode))
        };

    private static string RequiredReason(string reason)
    {
        var normalized = reason?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainException(OutcomeCode.Invalid, "A reason is required.", nameof(ChangeAccountRoleViewModel.Reason));
        }

        if (normalized.Length > ReasonMaxLength)
        {
            throw new DomainException(OutcomeCode.Invalid, "The reason must be 1000 characters or fewer.", nameof(ChangeAccountRoleViewModel.Reason));
        }

        return normalized;
    }

    private int RequireAdministratorActor()
    {
        if (!_currentActor.IsAuthenticated)
        {
            throw new DomainException(OutcomeCode.Forbidden, "Access denied.");
        }

        int userId;
        try
        {
            userId = _currentActor.UserId;
        }
        catch (InvalidOperationException)
        {
            throw new DomainException(OutcomeCode.Forbidden, "Access denied.");
        }

        if (userId <= 0 || !string.Equals(_currentActor.Role, RoleCodes.Admin, StringComparison.Ordinal))
        {
            throw new DomainException(OutcomeCode.Forbidden, "Access denied.");
        }

        return userId;
    }

    private sealed record RoleAuditValues(string RoleCode) : IAuditValues;
}
