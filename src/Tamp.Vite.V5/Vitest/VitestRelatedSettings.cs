namespace Tamp.Vite.V5;

/// <summary>
/// Settings for <c>vitest related [files...]</c> — runs only the
/// tests that import the given source files. Great for "verify
/// changed files" gates.
/// </summary>
public sealed class VitestRelatedSettings : VitestSettingsBase
{
    public List<string> Files { get; } = [];

    public VitestRelatedSettings AddFile(string path) { Files.Add(path); return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (Files.Count == 0)
            throw new InvalidOperationException("vitest related: at least one file is required.");
        var args = new List<string> { "related" };
        EmitCommonArguments(args);
        foreach (var f in Files) args.Add(f);
        return args;
    }
}
