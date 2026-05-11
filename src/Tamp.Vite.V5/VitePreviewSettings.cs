namespace Tamp.Vite.V5;

/// <summary>
/// Settings for <c>vite preview</c> — serves the production bundle
/// locally, the smoke-test step before deploying a built dist.
/// </summary>
public sealed class VitePreviewSettings : ViteSettingsBase
{
    public string? Host { get; set; }
    public int? Port { get; set; }
    public bool StrictPort { get; set; }
    public bool Https { get; set; }
    public string? Open { get; set; }
    public string? OutDir { get; set; }
    public string? Root { get; set; }

    public VitePreviewSettings SetHost(string? host) { Host = host; return this; }
    public VitePreviewSettings SetPort(int port) { Port = port; return this; }
    public VitePreviewSettings SetStrictPort(bool v = true) { StrictPort = v; return this; }
    public VitePreviewSettings SetHttps(bool v = true) { Https = v; return this; }
    public VitePreviewSettings SetOpen(string? path = "") { Open = path ?? ""; return this; }
    public VitePreviewSettings SetOutDir(string? path) { OutDir = path; return this; }
    public VitePreviewSettings SetRoot(string? path) { Root = path; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        var args = new List<string> { "preview" };
        EmitCommonArguments(args);
        if (Host is not null)
        {
            args.Add("--host");
            if (Host.Length > 0) args.Add(Host);
        }
        if (Port is { } p) { args.Add("--port"); args.Add(p.ToString()); }
        if (StrictPort) args.Add("--strictPort");
        if (Https) args.Add("--https");
        if (Open is not null)
        {
            args.Add("--open");
            if (Open.Length > 0) args.Add(Open);
        }
        if (!string.IsNullOrEmpty(OutDir)) { args.Add("--outDir"); args.Add(OutDir!); }
        if (!string.IsNullOrEmpty(Root)) args.Add(Root!);
        return args;
    }
}
