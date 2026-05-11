namespace Tamp.Vite.V5;

/// <summary>
/// Settings for <c>vite build</c>. Most-used wrapper verb — drives the
/// production bundle for frontend deploys.
/// </summary>
public sealed class ViteBuildSettings : ViteSettingsBase
{
    /// <summary>Output directory (overrides config). Maps to <c>--outDir</c>.</summary>
    public string? OutDir { get; set; }

    /// <summary>Assets directory under outDir. Maps to <c>--assetsDir</c>.</summary>
    public string? AssetsDir { get; set; }

    /// <summary>Bytes threshold below which an asset is inlined. Maps to <c>--assetsInlineLimit</c>.</summary>
    public int? AssetsInlineLimit { get; set; }

    /// <summary>Build for SSR. Optional entrypoint path. Maps to <c>--ssr [entry]</c>.</summary>
    public string? Ssr { get; set; }

    /// <summary>Source map mode (<c>true</c>, <c>false</c>, <c>inline</c>, <c>hidden</c>). Maps to <c>--sourcemap [mode]</c>.</summary>
    public string? Sourcemap { get; set; }

    /// <summary>Minifier (<c>esbuild</c>, <c>terser</c>, or <c>false</c>). Maps to <c>--minify</c>.</summary>
    public string? Minify { get; set; }

    /// <summary>Emit manifest.json. Maps to <c>--manifest</c>.</summary>
    public bool Manifest { get; set; }

    /// <summary>Emit ssr-manifest.json. Maps to <c>--ssrManifest</c>.</summary>
    public bool SsrManifest { get; set; }

    /// <summary>Empty outDir before write. Maps to <c>--emptyOutDir</c>.</summary>
    public bool? EmptyOutDir { get; set; }

    /// <summary>Watch mode rebuild. Maps to <c>--watch</c> / <c>-w</c>.</summary>
    public bool Watch { get; set; }

    /// <summary>Target output formats (overrides config). Repeated. Maps to <c>--target</c>.</summary>
    public List<string> Target { get; } = [];

    /// <summary>Optional project root (positional, last).</summary>
    public string? Root { get; set; }

    public ViteBuildSettings SetOutDir(string? path) { OutDir = path; return this; }
    public ViteBuildSettings SetAssetsDir(string? path) { AssetsDir = path; return this; }
    public ViteBuildSettings SetAssetsInlineLimit(int bytes) { AssetsInlineLimit = bytes; return this; }
    public ViteBuildSettings SetSsr(string? entry = "") { Ssr = entry ?? ""; return this; }
    public ViteBuildSettings SetSourcemap(string mode) { Sourcemap = mode; return this; }
    public ViteBuildSettings SetMinify(string mode) { Minify = mode; return this; }
    public ViteBuildSettings SetManifest(bool v = true) { Manifest = v; return this; }
    public ViteBuildSettings SetSsrManifest(bool v = true) { SsrManifest = v; return this; }
    public ViteBuildSettings SetEmptyOutDir(bool v) { EmptyOutDir = v; return this; }
    public ViteBuildSettings SetWatch(bool v = true) { Watch = v; return this; }
    public ViteBuildSettings AddTarget(string t) { Target.Add(t); return this; }
    public ViteBuildSettings SetRoot(string? path) { Root = path; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        var args = new List<string> { "build" };
        EmitCommonArguments(args);
        if (!string.IsNullOrEmpty(OutDir)) { args.Add("--outDir"); args.Add(OutDir!); }
        if (!string.IsNullOrEmpty(AssetsDir)) { args.Add("--assetsDir"); args.Add(AssetsDir!); }
        if (AssetsInlineLimit is { } bytes) { args.Add("--assetsInlineLimit"); args.Add(bytes.ToString()); }
        if (Ssr is not null)
        {
            args.Add("--ssr");
            if (Ssr.Length > 0) args.Add(Ssr);
        }
        if (!string.IsNullOrEmpty(Sourcemap)) { args.Add("--sourcemap"); args.Add(Sourcemap!); }
        if (!string.IsNullOrEmpty(Minify)) { args.Add("--minify"); args.Add(Minify!); }
        if (Manifest) args.Add("--manifest");
        if (SsrManifest) args.Add("--ssrManifest");
        if (EmptyOutDir is true) args.Add("--emptyOutDir");
        else if (EmptyOutDir is false) args.Add("--emptyOutDir=false");
        if (Watch) args.Add("--watch");
        foreach (var t in Target) { args.Add("--target"); args.Add(t); }
        if (!string.IsNullOrEmpty(Root)) args.Add(Root!);
        return args;
    }
}
