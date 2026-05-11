namespace Tamp.Vite.V5;

/// <summary>Settings for <c>vitest bench</c> — benchmark runner mode.</summary>
public sealed class VitestBenchSettings : VitestSettingsBase
{
    public List<string> FilePatterns { get; } = [];

    public VitestBenchSettings AddFilePattern(string p) { FilePatterns.Add(p); return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        var args = new List<string> { "bench" };
        EmitCommonArguments(args);
        foreach (var p in FilePatterns) args.Add(p);
        return args;
    }
}
