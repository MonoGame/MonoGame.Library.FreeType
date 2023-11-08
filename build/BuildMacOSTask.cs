
namespace BuildScripts;

[TaskName("Build macOS")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildMacOSTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnMacOs();

    public override void Run(BuildContext context)
    {
        // Make sure it statically links the dpeendencies
        context.ReplaceTextInFiles("freetype/CMakeLists.txt", "# Find dependencies", "set(CMAKE_FIND_LIBRARY_SUFFIXES \".a\")");

        // Build
        var buildDir = "freetype/build";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "../ -DBUILD_SHARED_LIBS=true -DCMAKE_OSX_DEPLOYMENT_TARGET=13.0 -DFT_DISABLE_HARFBUZZ=TRUE -DCMAKE_BUILD_TYPE=Release" });
        context.StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });

        foreach (var filePath in Directory.GetFiles("freetype/build"))
        {
            if (!filePath.EndsWith(".dylib") ||
                File.GetAttributes(filePath).HasFlag(FileAttributes.ReparsePoint))
                continue;

            context.CopyFile(filePath, $"{context.ArtifactsDir}/libfreetype.dylib");
            return;
        }

        throw new Exception("No built library found :(");
    }
}
