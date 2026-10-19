using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Services.ServiceModels.AccountAdministration;
namespace ASI.Basecode.Services.Interfaces;

// Contract only: persistence and production DI arrive in Phase 2/3.
public interface IAccountAdministrationService
{
    Task<AccountPageResponse> ListAsync(int actorId, AccountListRequest request, CancellationToken ct);
    Task<DeactivationImpactResponse> PreviewDeactivationAsync(int actorId, int accountId, AccountListRequest request, CancellationToken ct);
    Task<AccountChangeResponse> ChangeRoleAsync(int actorId, int accountId, ChangeAccountRoleRequest request, CancellationToken ct);
    Task<AccountChangeResponse> ChangeStatusAsync(int actorId, int accountId, ChangeAccountStatusRequest request, CancellationToken ct);
}
