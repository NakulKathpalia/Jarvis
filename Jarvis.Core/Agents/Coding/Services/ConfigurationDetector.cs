namespace Jarvis.Core.Agents.Coding.Services;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Detects common repository configuration files.
/// </summary>
public sealed class ConfigurationDetector
{
    private static readonly HashSet<string> KnownConfigurations = new(StringComparer.OrdinalIgnoreCase)
    {
        ".sln",
        ".csproj",
        "Directory.Build.props",
        "package.json",
        "tsconfig.json",
        "requirements.txt",
        "Cargo.toml",
        "pom.xml",
        "build.gradle"
    };

    /// <summary>
    /// Detects configuration files from scanned files.
    /// </summary>
    /// <param name="files">The scanned files.</param>
    /// <returns>The detected configurations.</returns>
    public IReadOnlyList<RepositoryConfiguration> Detect(IEnumerable<RepositoryFile> files)
    {
        return files
            .Where(IsConfiguration)
            .Select(file => new RepositoryConfiguration
            {
                ConfigurationType = GetConfigurationType(file),
                RelativePath = file.RelativePath
            })
            .OrderBy(configuration => configuration.ConfigurationType, StringComparer.OrdinalIgnoreCase)
            .ThenBy(configuration => configuration.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsConfiguration(RepositoryFile file)
    {
        return KnownConfigurations.Contains(file.Name) || KnownConfigurations.Contains(file.Extension);
    }

    private static string GetConfigurationType(RepositoryFile file)
    {
        return file.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase) ||
            file.Extension.Equals(".sln", StringComparison.OrdinalIgnoreCase)
            ? file.Extension
            : file.Name;
    }
}
