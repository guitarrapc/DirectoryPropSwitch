using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DirectoryPropSwitch.Tests
{
    public class TestFixture : IDisposable
    {
        public string Folder { get; }
        public TestFixture()
        {
            Folder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);
        }

        public void Dispose()
        {
            RemoveTestEnvironment();
        }

        public string CreateDirectoryBuildProp(string contents, string fileName = "Directory.Build.props", bool isBom = false)
        {
            var inputPath = Path.Combine(Folder, fileName);
            File.WriteAllText(inputPath, contents, new UTF8Encoding(isBom));
            return inputPath;
        }

        public byte[] Read(string fileName = "Directory.Build.props")
        {
            return File.ReadAllBytes(Path.Combine(Folder, fileName));
        }

        public void RemoveTestEnvironment()
        {
            Directory.Delete(Folder, true);
        }
    }
}
