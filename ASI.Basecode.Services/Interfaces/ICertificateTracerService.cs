using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces;

/// <summary>Creates a temporary sample PDF download for a confirmed registration.</summary>
public interface ICertificateTracerService
{
    /// <summary>Returns a stream that deletes its promoted private PDF when disposed, or null if the registration is not confirmed.</summary>
    Task<Stream> CreateSamplePdfDownloadAsync(int registrationId, CancellationToken cancellationToken = default);
}
