using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Interfaces;

/// <summary>Renders fixed certificate snapshots and the development tracer sample.</summary>
public interface ICertificatePdfRenderer
{
    Task<byte[]> RenderSampleAsync(CancellationToken cancellationToken = default);

    /// <summary>Renders the fixed certificate template from captured values.</summary>
    Task<byte[]> RenderSnapshotAsync(CertificatePdfSnapshot snapshot, CancellationToken cancellationToken = default);
}
