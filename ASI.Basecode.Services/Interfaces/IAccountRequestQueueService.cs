using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Interfaces;

public interface IAccountRequestQueueService
{
    Task<PagedResult<AccountRequestQueueItemViewModel>> GetPageAsync(string status, int? pageNumber, CancellationToken cancellationToken = default);
}
