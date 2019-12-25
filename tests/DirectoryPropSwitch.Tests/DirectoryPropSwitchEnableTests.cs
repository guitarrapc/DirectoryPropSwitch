using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryPropSwitch.Tests
{
    /// <summary>
    /// disable data should be change to enabledata
    /// </summary>
    public class DirectoryPropSwitchEnableTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        private readonly ILogger _logger;

        public DirectoryPropSwitchEnableTests(TestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _logger = new TestOutputLogger(output, LogLevel.Information);
        }

        [Fact]
        public async Task EnablePathMapTest()
        {
            var shouldBe = _fixture.CreateDirectoryBuildProp(string.Join('\n', TestData.EnableData), $"{nameof(EnablePathMapTest)}_expected");
            var expected = _fixture.Read(shouldBe);

            var fileName = $"{nameof(EnablePathMapTest)}_actual";
            var testPath = _fixture.CreateDirectoryBuildProp(string.Join('\n', TestData.DisableData), fileName);
            var settings = new DirectoryPropSwitchSettings()
            {
                SearchOption = SearchOption.TopDirectoryOnly,
                XmlKey = "PathMap",
                FileName = fileName,
            };
            var switcher = new DirectoryPropSwitch(settings, _logger);

            // change executed
            await switcher.EnableAsync(_fixture.Folder, false);
            var actual = _fixture.Read(testPath);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task EnablePathMapDryRunTest()
        {
            var shouldBe = _fixture.CreateDirectoryBuildProp(string.Join('\n', TestData.DisableData), $"{nameof(EnablePathMapDryRunTest)}_expected");
            var expected = _fixture.Read(shouldBe);

            var fileName = $"{nameof(EnablePathMapDryRunTest)}_actual";
            var testPath = _fixture.CreateDirectoryBuildProp(string.Join('\n', TestData.DisableData), fileName);
            var settings = new DirectoryPropSwitchSettings()
            {
                SearchOption = SearchOption.TopDirectoryOnly,
                XmlKey = "PathMap",
                FileName = fileName,
            };
            var switcher = new DirectoryPropSwitch(settings, _logger);

            // nothing change on dryrun
            await switcher.EnableAsync(_fixture.Folder, true);
            var actual = _fixture.Read(testPath);
            Assert.Equal(expected, actual);
        }
    }
}
