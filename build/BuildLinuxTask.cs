
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
        context.StartProcessWithDocker("meson", workingDirectory: "freetype", args: "setup --buildtype=release -Db_ndebug=true -Ddefault_library=shared -Dbzip2=disabled --force-fallback-for=libpng,harfbuzz,zlib builddir");
        context.StartProcessWithDocker("meson", workingDirectory: "freetype", args: "compile -C builddir");

        foreach (var filePath in Directory.GetFiles("freetype/builddir"))
        {
            if (!filePath.Contains(".so") ||
                File.GetAttributes(filePath).HasFlag(FileAttributes.ReparsePoint))
                continue;

            var artifactPath = $"{context.ArtifactsDir}/libfreetype.so";
            context.CopyFile(filePath, artifactPath);

            var stripArguments = new ProcessArgumentBuilder();
            stripArguments.Append("--strip-unneeded");
            stripArguments.AppendQuoted(artifactPath);
            context.StartProcessWithDocker("strip", workingDirectory: "", args: stripArguments);
            return;
        }

        throw new Exception("No built library found :(");
    }
}
