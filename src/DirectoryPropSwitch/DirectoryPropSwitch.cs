using Microsoft.Extensions.Logging;
using DirectoryPropSwitch.internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DirectoryPropSwitch
{
    public class DirectoryPropSwitchSettings
    {
        public string? XmlKey { get; set; }
        public string FileName { get; set; } = "Directory.Build.props";
        public SearchOption SearchOption { get; set; } = SearchOption.AllDirectories;
    }

    public class DirectoryPropSwitch
    {
        private const string xmlCommentOutPattern = @"<!--.*-->";
        private static readonly Regex xmlCommentOutRegEx = new Regex(xmlCommentOutPattern, RegexOptions.Multiline | RegexOptions.CultureInvariant);
        private const string xmlCommentSectionPattern = @"(<!--\s*)(<.*>)(\s*-->)";
        private static readonly Regex xmlCommentSectionRegEx = new Regex(xmlCommentSectionPattern, RegexOptions.Multiline | RegexOptions.CultureInvariant);

        private readonly Regex xmlPropLineRegEx;
        private readonly Regex xmlNoneCommentOutSectionRegEx;

        private readonly DirectoryPropSwitchSettings _settings;
        private readonly ILogger _logger;

        public DirectoryPropSwitch(DirectoryPropSwitchSettings settings, ILogger logger)
        {
            if (settings.XmlKey == null) throw new ArgumentNullException(nameof(settings.XmlKey));

            _settings = settings;
            _logger = logger;

            var xmlPropXmlPattern = $@".*<{_settings.XmlKey}>.*</{_settings.XmlKey}>.*";
            this.xmlPropLineRegEx = new Regex(xmlPropXmlPattern, RegexOptions.Multiline | RegexOptions.CultureInvariant);

            var xmlNonCommentOutPattern = $@"(\s*)(<{_settings.XmlKey}>.*</{_settings.XmlKey}>)(\s*)";
            this.xmlNoneCommentOutSectionRegEx = new Regex(xmlNonCommentOutPattern, RegexOptions.Multiline | RegexOptions.CultureInvariant);
        }

        public async ValueTask DisableAsync(string basePath, bool isDryrun)
        {
            var paths = GetFiles(basePath, _settings.FileName, _settings.SearchOption).ToArray();
            for (var i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                var current = $"{i + 1}/{paths.Length}";
                var content = await File.ReadAllTextAsync(path);

                _logger.LogInformation($"#{current}; disabling {nameof(path)}={path}");
                await DisableAsync(content, path, isDryrun);
            }
        }
        public async ValueTask DisableAsync(string content, string path, bool isDryrun)
        {
            var search = xmlPropLineRegEx.Match(content);
            if (!search.Success)
            {
                _logger.LogInformation($"{_settings.XmlKey} not detected.");
                return;
            }

            var isCommented = IsCommented(search.Value);
            if (isCommented)
            {
                _logger.LogInformation($"{_settings.XmlKey} already disabled.");
                return;
            }

            var replaced = AddCommentElement(content, _settings.XmlKey!, search.Value);
            if (string.IsNullOrWhiteSpace(replaced))
            {
                _logger.LogInformation($"{_settings.XmlKey} not detected.");
                return;
            }
            if (!IsChanged(content, replaced))
            {
                _logger.LogInformation($"{_settings.XmlKey} nothing changed, skip.");
                return;
            }

            Save(path, replaced, isDryrun);
        }

        public async ValueTask EnableAsync(string basePath, bool isDryrun)
        {
            var paths = GetFiles(basePath, _settings.FileName, _settings.SearchOption).ToArray();
            for (var i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                var current = $"{i + 1}/{paths.Length}";
                var content = await File.ReadAllTextAsync(path);

                _logger.LogInformation($"#{current}; enabling {nameof(path)}={path}");
                await EnableAsync(content, path, isDryrun);
            }
        }
        public async ValueTask EnableAsync(string content, string path, bool isDryrun)
        {
            var search = xmlPropLineRegEx.Match(content);
            if (!search.Success)
            {
                _logger.LogInformation($"{_settings.XmlKey} not detected.");
                return;
            }

            var isCommented = IsCommented(search.Value);
            if (!isCommented)
            {
                _logger.LogInformation($"{_settings.XmlKey} already enabled.");
                return;
            }

            var replaced = RemoveCommentElement(content, search.Value);
            if (string.IsNullOrWhiteSpace(replaced))
            {
                _logger.LogInformation($"{_settings.XmlKey} not detected.");
                return;
            }
            if (!IsChanged(content, replaced))
            {
                _logger.LogInformation($"{_settings.XmlKey} nothing changed, skip.");
                return;
            }

            Save(path, replaced, isDryrun);
        }

        public async ValueTask ToggleAsync(string basePath, bool isDryrun)
        {
            var paths = GetFiles(basePath, _settings.FileName, _settings.SearchOption).ToArray();
            for (var i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                var current = $"{i + 1}/{paths.Length}";
                var content = await File.ReadAllTextAsync(path);

                _logger.LogInformation($"#{current}; toggling {nameof(path)}={path}");
                await ToggleAsync(content, path, isDryrun);
            }
        }
        public async ValueTask ToggleAsync(string content, string path, bool isDryrun)
        {
            var search = xmlPropLineRegEx.Match(content);
            if (!search.Success)
            {
                _logger.LogInformation($"{_settings.XmlKey} not detected.");
                return;
            }

            var isCommented = IsCommented(search.Value);
            var replaced = isCommented
                ? RemoveCommentElement(content, search.Value)
                : AddCommentElement(content, _settings.XmlKey!, search.Value);
            if (string.IsNullOrWhiteSpace(replaced))
            {
                _logger.LogInformation($"{_settings.XmlKey} not detected.");
                return;
            }
            if (!IsChanged(content, replaced))
            {
                _logger.LogInformation($"{_settings.XmlKey} nothing changed, skip.");
                return;
            }

            if (isCommented)
            {
                _logger.LogInformation($"toggle disabling {_settings.XmlKey}.");
            }
            else
            {
                _logger.LogInformation($"toggle enabling {_settings.XmlKey}.");
            }
            Save(path, replaced, isDryrun);
        }

        private string RemoveCommentElement(string contents, string sentence)
        {
            var line = xmlCommentSectionRegEx.Match(sentence);
            if (!line.Success || line.Groups.Count != 4) return "";

            var section = line.Groups.Values
                .Where(group => !group.Value.TrimStart().StartsWith("<!--"))
                .Where(group => !group.Value.TrimEnd().EndsWith("-->"))
                .SingleOrDefault();
            if (section == null || string.IsNullOrWhiteSpace(section.Value)) return "";

            var newProp = contents.Replace(line.Value, section.Value);
            return newProp;
        }

        private string AddCommentElement(string contents, string xmlKey, string sentence)
        {
            var line = xmlNoneCommentOutSectionRegEx.Match(sentence);
            if (!line.Success) return "";

            // get none white spaced section
            var section = line.Groups.Values
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Where(group => group.Value.StartsWith($"<{xmlKey}"))
                .FirstOrDefault();

            // indent should be white space or equivalants.
            var indent = line.Groups.Values.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Value));
            if (section == null || string.IsNullOrWhiteSpace(section.Value)) return "";

            // SHOULD BE: {indent}<-- {section} -->
            var sectionEol = FileEolDetector.TrimEnd(section.Value.AsSpan(), line.Value);
            var commentedSection = $"{indent.ToString()}<!-- {sectionEol} -->";

            var newProp = contents.Replace(line.Value, commentedSection);
            return newProp;
        }

        private void Save(string path, string content, bool isDryrun)
        {
            var encoding = FileBomDetector.Detect(path);
            var eol = FileEolDetector.Detect(path).GetLabel();
            content = content.Replace("\r", "").Replace("\n", eol);

            if (isDryrun)
            {
                _logger.LogInformation($"dry run detected. skip perform change; {nameof(path)}={path}");
                _logger.LogDebug($"```xml");
                _logger.LogDebug(content);
                _logger.LogDebug($"```");
                return;
            }

            File.WriteAllText(path, content, encoding);
        }

        private IEnumerable<string> GetFiles(string path, string filePattern, SearchOption option) => Directory.EnumerateFiles(path, filePattern, option);
        private bool IsChanged(string original, string replaced) => replaced.GetHashCode() != original.GetHashCode();
        private bool IsCommented(string section) => xmlCommentOutRegEx.IsMatch(section);
    }
}
