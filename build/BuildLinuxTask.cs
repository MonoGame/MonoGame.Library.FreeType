
namespace BuildScripts;

[TaskName("Build Linux")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildLinuxTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnLinux();

    public override void Run(BuildContext context)
    {
        // Build
        var buildDir = "freetype/build";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "../ -DBUILD_SHARED_LIBS=true -DCMAKE_BUILD_TYPE=Release" });
        context.StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });

        foreach (var filePath in Directory.GetFiles("freetype/build"))
        {
            if (!filePath.Contains(".so") ||
                File.GetAttributes(filePath).HasFlag(FileAttributes.ReparsePoint))
                continue;

            context.CopyFile(filePath, $"{context.ArtifactsDir}/libfreetype.so");
            return;
        }

        throw new Exception("No built library found :(");
    }
}
