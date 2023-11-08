
namespace BuildScripts;

[TaskName("Build Linux")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildLinuxTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnLinux();

    public override void Run(BuildContext context)
    {
        // Make sure it statically links the dpeendencies
        context.ReplaceRegexInFiles("freetype/meson.build", @" dependency\('([^']+)',", "dependency('$1', static: true,");
        context.ReplaceTextInFiles("freetype/meson.build", "meson.override_dependency('freetype2', freetype_dep)", "");

        // Build
        context.StartProcess("meson", new ProcessSettings { WorkingDirectory = "freetype", Arguments = "setup -Ddefault_library=shared -Dbzip2=disabled --force-fallback-for=libpng,harfbuzz,zlib builddir" });
        context.StartProcess("meson", new ProcessSettings { WorkingDirectory = "freetype", Arguments = "compile -C builddir" });

        foreach (var filePath in Directory.GetFiles("freetype/builddir"))
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
