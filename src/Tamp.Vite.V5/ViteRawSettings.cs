namespace Tamp.Vite.V5;

/// <summary>Escape hatch for vite flag combinations we haven't typed.</summary>
public sealed class ViteRawSettings : ViteSettingsBase
{
    public List<string> RawArguments { get; } = [];

    public ViteRawSettings AddArgs(params string[] args) { RawArguments.AddRange(args); return this; }

    protected override IEnumerable<string> BuildVerbArguments() => RawArguments;
}
