
using Cake.Common.Build;

namespace BuildScripts;

[TaskName("Package")]
public sealed class PackageTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var major = context.FindRegexMatchGroupInFile("freetype/include/freetype/freetype.h", @"#define FREETYPE_MAJOR +(?<ver>\d+)", 1, System.Text.RegularExpressions.RegexOptions.Singleline);
        var minor = context.FindRegexMatchGroupInFile("freetype/include/freetype/freetype.h", @"#define FREETYPE_MINOR +(?<ver>\d+)", 1, System.Text.RegularExpressions.RegexOptions.Singleline);
        var patch = context.FindRegexMatchGroupInFile("freetype/include/freetype/freetype.h", @"#define FREETYPE_PATCH +(?<ver>\d+)", 1, System.Text.RegularExpressions.RegexOptions.Singleline);
        var version = $"{major}.{minor}.{patch}";
        var dnMsBuildSettings = new DotNetMSBuildSettings();

        if (context.BuildSystem().IsRunningOnGitHubActions)
        {
            dnMsBuildSettings.WithProperty("Version", version + "." + context.EnvironmentVariable("GITHUB_RUN_NUMBER"));
            dnMsBuildSettings.WithProperty("RepositoryUrl", "https://github.com/" + context.EnvironmentVariable("GITHUB_REPOSITORY"));
        }
        else
        {
            dnMsBuildSettings.WithProperty("Version", version);
        }

        context.DotNetPack("src/MonoGame.Library.FreeType.csproj", new DotNetPackSettings
        {
            MSBuildSettings = dnMsBuildSettings,
            Verbosity = DotNetVerbosity.Minimal,
            Configuration = "Release"
        });
    }
}
