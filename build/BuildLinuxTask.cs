
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
        context.ReplaceTextInFiles("freetype/meson.build", "dependency('libpng',", "dependency('libpng', static: true,");
        context.ReplaceTextInFiles("freetype/meson.build", "dependency('harfbuzz',", "dependency('harfbuzz', static: true,");
        context.ReplaceTextInFiles("freetype/meson.build", "dependency('zlib',", "dependency('zlib', static: true,");

        // Build
        context.StartProcess("meson", new ProcessSettings { WorkingDirectory = "freetype", Arguments = "setup -Ddefault_library=shared --force-fallback-for=libpng,harfbuzz,zlib builddir" });
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
