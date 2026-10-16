using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Services.ServiceModels.CertificateEligibility;

namespace ASI.Basecode.Services.Interfaces;

// Contract only: concrete persistence implementation and DI registration arrive in Phase 2/3.
public interface ICertificateEligibilityService
{
    Task<CertificateReviewPageResponse> GetPendingReviewsAsync(
        CertificateReviewListRequest request,
        int trustedActorId,
        CancellationToken cancellationToken = default);

    Task<CertificateReviewResponse> ReviewAsync(
        ReviewCertificateRequest request,
        int trustedActorId,
        CancellationToken cancellationToken = default);
}
