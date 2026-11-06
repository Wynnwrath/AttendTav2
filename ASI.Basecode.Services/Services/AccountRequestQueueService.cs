using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Services;

public sealed class AccountRequestQueueService(IAccountRequestRepository accountRequestRepository) : IAccountRequestQueueService
{
    public async Task<PagedResult<AccountRequestQueueItemViewModel>> GetPageAsync(
        string status,
        int? pageNumber,
        CancellationToken cancellationToken = default)
    {
        var statusCode = ParseStatus(status);
        var totalCount = await accountRequestRepository.CountByStatusAsync(statusCode, cancellationToken);
        var pageRequest = new PageRequest(pageNumber);
        var normalizedPage = pageRequest.NormalizePageNumber(totalCount);
        var records = await accountRequestRepository.GetPageByStatusAsync(
            statusCode,
            pageRequest.GetOffset(normalizedPage),
            PageRequest.FixedPageSize,
            cancellationToken);

        var items = records.Select(record => new AccountRequestQueueItemViewModel
        {
            RequestId = record.RequestId,
            StudentNumber = record.StudentNumber,
            FullName = record.FullName,
            Email = record.Email,
            Course = record.Course,
            YearLevel = record.YearLevel,
            Section = record.Section,
            MobileNumber = record.MobileNumber,
            Status = StatusText(record.Status),
            RequestedAt = record.RequestedAt
        }).ToList();

        return new PagedResult<AccountRequestQueueItemViewModel>(items, normalizedPage, totalCount);
    }

    private static AccountRequestStatusCode ParseStatus(string status) => status?.Trim().ToLowerInvariant() switch
    {
        "approved" => AccountRequestStatusCode.Approved,
        "rejected" => AccountRequestStatusCode.Rejected,
        _ => AccountRequestStatusCode.Pending
    };

    private static string StatusText(AccountRequestStatusCode status) => status switch
    {
        AccountRequestStatusCode.Approved => "Approved",
        AccountRequestStatusCode.Rejected => "Rejected",
        _ => "Pending"
    };
}
