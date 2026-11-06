using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services;

public sealed class CertificateTracerService : ICertificateTracerService
{
    private readonly IRegistrationRepository registrations;
    private readonly ICertificatePdfRenderer renderer;
    private readonly IPrivateFileStore files;
    private readonly ILogger<CertificateTracerService> logger;

    public CertificateTracerService(
        IRegistrationRepository registrations,
        ICertificatePdfRenderer renderer,
        IPrivateFileStore files,
        ILogger<CertificateTracerService> logger)
    {
        this.registrations = registrations;
        this.renderer = renderer;
        this.files = files;
        this.logger = logger;
    }

    public async Task<Stream> CreateSamplePdfDownloadAsync(int registrationId, CancellationToken cancellationToken = default)
    {
        if (registrationId <= 0 || !await registrations.IsConfirmedAsync(registrationId, cancellationToken))
        {
            return null;
        }

        string temporaryPath = null;
        string promotedKey = null;
        Stream openedFile = null;
        try
        {
            var pdf = await renderer.RenderSampleAsync(cancellationToken);
            await using (var content = new MemoryStream(pdf ?? Array.Empty<byte>(), writable: false))
            {
                temporaryPath = await files.SaveAsync(PrivateFileArea.Temporary, content, ".pdf", cancellationToken);
            }

            promotedKey = await files.PromoteAsync(temporaryPath, PrivateFileArea.Certificates, cancellationToken);
            temporaryPath = null;
            openedFile = await files.OpenReadAsync(promotedKey, cancellationToken);

            var keyToDelete = promotedKey;
            var download = new DeleteOnDisposeStream(openedFile, () => DeleteBestEffort(keyToDelete));
            openedFile = null;
            promotedKey = null;
            return download;
        }
        catch
        {
            if (openedFile is not null)
            {
                await openedFile.DisposeAsync();
            }

            DeleteBestEffort(temporaryPath);
            DeleteBestEffort(promotedKey);
            throw;
        }
    }

    private void DeleteBestEffort(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            files.Delete(path);
        }
        catch (Exception)
        {
            logger.LogWarning("Could not clean up a development certificate PDF.");
        }
    }

    private sealed class DeleteOnDisposeStream : Stream
    {
        private readonly Stream inner;
        private readonly Action delete;
        private bool disposed;

        public DeleteOnDisposeStream(Stream inner, Action delete)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
            this.delete = delete ?? throw new ArgumentNullException(nameof(delete));
        }

        public override bool CanRead => !disposed && inner.CanRead;
        public override bool CanSeek => !disposed && inner.CanSeek;
        public override bool CanWrite => !disposed && inner.CanWrite;
        public override long Length => inner.Length;
        public override long Position { get => inner.Position; set => inner.Position = value; }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override int Read(Span<byte> buffer) => inner.Read(buffer);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => inner.ReadAsync(buffer, cancellationToken);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => inner.ReadAsync(buffer, offset, count, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
        public override void SetLength(long value) => inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
        public override void Write(ReadOnlySpan<byte> buffer) => inner.Write(buffer);
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => inner.WriteAsync(buffer, cancellationToken);
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => inner.WriteAsync(buffer, offset, count, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInner();
            }

            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            if (!disposed)
            {
                disposed = true;
                try
                {
                    await inner.DisposeAsync();
                }
                finally
                {
                    delete();
                }
            }

            GC.SuppressFinalize(this);
        }

        private void DisposeInner()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            try
            {
                inner.Dispose();
            }
            finally
            {
                delete();
            }
        }
    }
}
