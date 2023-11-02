
using Cake.Common.Tools.MSBuild;

namespace BuildScripts;

[TaskName("Build Windows")]
[IsDependentOn(typeof(PrepTask))]
public sealed class BuildWindowsTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        MSBuildSettings buildSettings = new()
        {
            Verbosity = Verbosity.Normal,
            Configuration = "Release",
            PlatformTarget = PlatformTarget.x64
        };

        context.MSBuild("freetype/builds/windows/vc2010/freetype.vcxproj", buildSettings);
        context.CopyFile("freetype-demos/bin/freetype.dll", $"{context.ArtifactsDir}/freetype.dll");
    }
}
