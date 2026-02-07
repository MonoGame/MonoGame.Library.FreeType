using Cake.Common.Tools.MSBuild;

namespace BuildScripts;

[TaskName("Build Windows")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildWindowsTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        //  Ensure statically linked
        context.ReplaceTextInFiles("freetype/builds/windows/vc2010/freetype.vcxproj", "MultiThreadedDLL", "MultiThreaded");

        BuildForArchitecture(context, PlatformTarget.x64, "win-x64");
        BuildForArchitecture(context, PlatformTarget.ARM64, "win-arm64");
    }

    private void BuildForArchitecture(BuildContext context, PlatformTarget platform, string rid)
    {
        MSBuildSettings buildSettings = new()
        {
            Verbosity = Verbosity.Normal,
            Configuration = "Release",
            PlatformTarget = platform
        };

        context.MSBuild("freetype/builds/windows/vc2010/freetype.vcxproj", buildSettings);
        
        context.CreateDirectory($"{context.ArtifactsDir}/{rid}");
        context.CopyFile($"freetype-demos/bin/freetype.dll", $"{context.ArtifactsDir}/{rid}/freetype.dll");
    }
}
