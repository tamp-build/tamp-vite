namespace Tamp.Vite.V5;

/// <summary>
/// Settings for <c>vite optimize</c> — pre-bundle dependencies into
/// node_modules/.vite. Mainly used to force a re-bundle after a
/// package-lock change.
/// </summary>
public sealed class ViteOptimizeDepsSettings : ViteSettingsBase
{
    /// <summary>Optional project root (positional, last).</summary>
    public string? Root { get; set; }

    public ViteOptimizeDepsSettings SetRoot(string? path) { Root = path; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        var args = new List<string> { "optimize" };
        EmitCommonArguments(args);
        if (!string.IsNullOrEmpty(Root)) args.Add(Root!);
        return args;
    }
}
