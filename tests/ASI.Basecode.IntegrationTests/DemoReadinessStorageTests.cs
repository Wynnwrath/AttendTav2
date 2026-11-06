using ASI.Basecode.WebApp.Readiness;
using Xunit;

namespace ASI.Basecode.IntegrationTests;

public class DemoReadinessStorageTests
{
    [Fact]
    public void ExistingWritableAreasPassAndLeaveNoProbeFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), "attendta-readiness-" + Guid.NewGuid().ToString("N"));
        try
        {
            foreach (var area in new[] { "keys", "logs", "temporary", "uploads/venues", "uploads/events", "certificates" })
                Directory.CreateDirectory(Path.Combine(root, area));
            DemoReadiness.ValidateStorage(root);
            Assert.Empty(Directory.GetFiles(root, ".readiness-*", SearchOption.AllDirectories));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void MissingAreaFailsAndCleansEarlierProbes()
    {
        var root = Path.Combine(Path.GetTempPath(), "attendta-readiness-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "keys"));
            Assert.Throws<DirectoryNotFoundException>(() => DemoReadiness.ValidateStorage(root));
            Assert.Empty(Directory.GetFiles(root, ".readiness-*", SearchOption.AllDirectories));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
