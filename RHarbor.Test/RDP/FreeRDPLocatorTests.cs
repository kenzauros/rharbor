using kenzauros.RHarbor.RDP;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace kenzauros.RHarbor.Test.RDP
{
    [TestFixture]
    public class FreeRDPLocatorTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "FreeRDPLocatorTests_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        private void CreateFakeExe(string relativePath)
        {
            var fullPath = Path.Combine(_tempDir, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, string.Empty);
        }

        [Test]
        public void FindExecutables_ReturnsEmpty_WhenNoFreeRDPFolder()
        {
            var result = FreeRDPLocator.FindExecutables(_tempDir);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void FindExecutables_ReturnsEmpty_WhenFolderExistsButNoExe()
        {
            Directory.CreateDirectory(Path.Combine(_tempDir, "freerdp3"));
            var result = FreeRDPLocator.FindExecutables(_tempDir);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void FindExecutables_FindsSdlFreeRDP()
        {
            CreateFakeExe(@"freerdp3\sdl-freerdp.exe");
            var result = FreeRDPLocator.FindExecutables(_tempDir).ToList();
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Does.EndWith("sdl-freerdp.exe"));
        }

        [Test]
        public void FindExecutables_FindsWFreeRDP()
        {
            CreateFakeExe(@"freerdp3\wfreerdp.exe");
            var result = FreeRDPLocator.FindExecutables(_tempDir).ToList();
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Does.EndWith("wfreerdp.exe"));
        }

        [Test]
        public void FindExecutables_PrefersSdlFreeRDP_OverWFreeRDP()
        {
            CreateFakeExe(@"freerdp3\sdl-freerdp.exe");
            CreateFakeExe(@"freerdp3\wfreerdp.exe");
            var result = FreeRDPLocator.FindExecutables(_tempDir).ToList();
            // First candidate must be sdl-freerdp.exe
            Assert.That(result[0], Does.EndWith("sdl-freerdp.exe"));
            // Second candidate must be wfreerdp.exe (fallback)
            Assert.That(result[1], Does.EndWith("wfreerdp.exe"));
        }

        [Test]
        public void FindExecutables_PrefersHigherVersionFolder()
        {
            CreateFakeExe(@"freerdp3.26.0\sdl-freerdp.exe");
            CreateFakeExe(@"freerdp3.10.0\sdl-freerdp.exe");
            var result = FreeRDPLocator.FindExecutables(_tempDir).ToList();
            // freerdp3.26.0 > freerdp3.10.0 lexicographically
            Assert.That(result[0], Does.Contain("freerdp3.26.0"));
        }

        [Test]
        public void FindExecutables_IgnoresNonFreeRDPFolders()
        {
            Directory.CreateDirectory(Path.Combine(_tempDir, "other_tool"));
            CreateFakeExe(@"other_tool\sdl-freerdp.exe");
            var result = FreeRDPLocator.FindExecutables(_tempDir);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void FindExecutables_ReturnsEmpty_WhenBaseDirectoryDoesNotExist()
        {
            var nonExistent = Path.Combine(_tempDir, "does_not_exist");
            var result = FreeRDPLocator.FindExecutables(nonExistent);
            Assert.That(result, Is.Empty);
        }
    }
}
