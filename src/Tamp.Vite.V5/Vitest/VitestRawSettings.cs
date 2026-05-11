namespace Tamp.Vite.V5;

/// <summary>Escape hatch for vitest flag combinations we haven't typed.</summary>
public sealed class VitestRawSettings : VitestSettingsBase
{
    public List<string> RawArguments { get; } = [];

    public VitestRawSettings AddArgs(params string[] args) { RawArguments.AddRange(args); return this; }

    protected override IEnumerable<string> BuildVerbArguments() => RawArguments;
}
