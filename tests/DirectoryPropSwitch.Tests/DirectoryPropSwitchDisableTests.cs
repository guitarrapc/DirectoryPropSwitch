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
    public class DirectoryPropSwitchDisableTests : IClassFixture<TestFixture>
    {

        private readonly TestFixture _fixture;
        private readonly ILogger _logger;

        public DirectoryPropSwitchDisableTests(TestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _logger = new TestOutputLogger(output, LogLevel.Information);
        }

        [Fact]
        public async Task DisablePathMapTest()
        {
            var shouldBe = _fixture.CreateDirectoryBuildProp(string.Join('\n', TestData.DisableData), $"{nameof(DisablePathMapTest)}_expected");
            var expected = _fixture.Read(shouldBe);

            var fileName = $"{nameof(DisablePathMapTest)}_actual";
            var testPath = _fixture.CreateDirectoryBuildProp(string.Join('\n', TestData.EnableData), fileName);
            var settings = new DirectoryPropSwitchSettings()
            {
                SearchOption = SearchOption.TopDirectoryOnly,
                XmlKey = "PathMap",
                FileName = fileName,
            };
            var switcher = new DirectoryPropSwitch(settings, _logger);

            // change executed
            await switcher.DisableAsync(_fixture.Folder, false);
            var actual = _fixture.Read(testPath);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task DisablePathMapDryRunTest()
        {
            var shouldBe = _fixture.CreateDirectoryBuildProp(string.Join('\n', TestData.EnableData), $"{nameof(DisablePathMapDryRunTest)}_expected");
            var expected = _fixture.Read(shouldBe);

            var fileName = $"{nameof(DisablePathMapDryRunTest)}_actual";
            var testPath = _fixture.CreateDirectoryBuildProp(string.Join('\n', TestData.EnableData), fileName);
            var settings = new DirectoryPropSwitchSettings()
            {
                SearchOption = SearchOption.TopDirectoryOnly,
                XmlKey = "PathMap",
                FileName = fileName,
            };
            var switcher = new DirectoryPropSwitch(settings, _logger);

            // nothing change on dryrun
            await switcher.DisableAsync(_fixture.Folder, true);
            var actual = _fixture.Read(testPath);
            Assert.Equal(expected, actual);
        }
    }
}
