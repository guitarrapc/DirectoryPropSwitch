using MicroBatchFramework;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DirectoryPropSwitch
{
    class Program
    {
        static async Task Main(string[] args) 
            => await BatchHost.CreateDefaultBuilder().RunBatchEngineAsync<PathMapBatch>(args);
    }

    public class PathMapBatch :BatchBase
    {
        [Command("version")]
        public void Version() => Context.Logger.LogInformation($"version: {Assembly.GetEntryAssembly().GetName().Version.ToString()}");

        [Command("enable", "enable key in Directory.Build.Prop")]
        public async Task Enable(
            [Option("-k", "Use for Property Key to handle.")]string key, 
            [Option("-p", "Use for base path to find file.")]string path, 
            [Option("-f", "Use for File name of a Directory.Build.props.")]string fileName = "Directory.Build.props", 
            [Option("-r", "Use for find file for directory recursively.")]bool recursive = true, 
            [Option("-dry", "Use for dry run.")]bool dryRun = true)
        {
            Context.Logger.LogDebug($"Parameter -{nameof(key)}={key}");
            Context.Logger.LogDebug($"Parameter -{nameof(path)}={path}");
            Context.Logger.LogDebug($"Parameter -{nameof(fileName)}={fileName}");
            Context.Logger.LogDebug($"Parameter -{nameof(recursive)}={recursive}");
            Context.Logger.LogDebug($"Parameter -{nameof(dryRun)}={dryRun}");

            var settings = new DirectoryPropSwitchSettings()
            {
                XmlKey = key,
                FileName = fileName,
                SearchOption = recursive ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly,
            };
            var switcher = new DirectoryPropSwitch(settings, Context.Logger);
            await switcher.EnableAsync(path, dryRun);
        }

        [Command("disable", "disable key in Directory.Build.Prop")]
        public async Task Disable(
            [Option("-k", "Use for Property Key to handle.")]string key,
            [Option("-p", "Use for base path to find file.")]string path,
            [Option("-f", "Use for File name of a Directory.Build.props.")]string fileName = "Directory.Build.props",
            [Option("-r", "Use for find file for directory recursively.")]bool recursive = true,
            [Option("-dry", "Use for dry run.")]bool dryRun = true)
        {
            Context.Logger.LogDebug($"Parameter -{nameof(key)}={key}");
            Context.Logger.LogDebug($"Parameter -{nameof(path)}={path}");
            Context.Logger.LogDebug($"Parameter -{nameof(fileName)}={fileName}");
            Context.Logger.LogDebug($"Parameter -{nameof(recursive)}={recursive}");
            Context.Logger.LogDebug($"Parameter -{nameof(dryRun)}={dryRun}");

            var settings = new DirectoryPropSwitchSettings()
            {
                XmlKey = key,
                FileName = fileName,
                SearchOption = recursive ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly,
            };
            var switcher = new DirectoryPropSwitch(settings, Context.Logger);
            await switcher.DisableAsync(path, dryRun);
        }

        [Command("toggle", "toggle key in Directory.Build.Prop")]
        public async Task Toggle(
            [Option("-k", "Use for Property Key to handle.")]string key,
            [Option("-p", "Use for base path to find file.")]string path,
            [Option("-f", "Use for File name of a Directory.Build.props.")]string fileName = "Directory.Build.props",
            [Option("-r", "Use for find file for directory recursively.")]bool recursive = true,
            [Option("-dry", "Use for dry run.")]bool dryRun = true)
        {
            Context.Logger.LogDebug($"Parameter -{nameof(key)}={key}");
            Context.Logger.LogDebug($"Parameter -{nameof(path)}={path}");
            Context.Logger.LogDebug($"Parameter -{nameof(fileName)}={fileName}");
            Context.Logger.LogDebug($"Parameter -{nameof(recursive)}={recursive}");
            Context.Logger.LogDebug($"Parameter -{nameof(dryRun)}={dryRun}");

            var settings = new DirectoryPropSwitchSettings()
            {
                XmlKey = key,
                FileName = fileName,
                SearchOption = recursive ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly,
            };
            var switcher = new DirectoryPropSwitch(settings, Context.Logger);
            await switcher.ToggleAsync(path, dryRun);
        }
    }
}
