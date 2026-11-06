using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;

namespace ASI.Basecode.Data.Interfaces;

public interface IAccountRequestRepository
{
    Task<AccountRequest> LockAccountRequestAsync(int requestId, CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(AccountRequestStatusCode status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountRequestQueueItem>> GetPageByStatusAsync(AccountRequestStatusCode status, int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a new account request. The caller saves through the unit of work.
    /// </summary>
    Task AddAsync(AccountRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a pending request already uses the email. Finalized requests are ignored.
    /// </summary>
    Task<bool> PendingEmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a pending request already uses the student number. Finalized requests are ignored.
    /// </summary>
    Task<bool> PendingStudentNumberExistsAsync(string studentNumber, CancellationToken cancellationToken = default);
}
