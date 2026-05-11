namespace Tamp.Vite.V5;

/// <summary>
/// Settings for <c>vite</c> / <c>vite dev</c> / <c>vite serve</c> — the
/// dev server. Long-running; you typically only invoke this through a
/// foreground target during interactive workflows.
/// </summary>
public sealed class ViteDevSettings : ViteSettingsBase
{
    /// <summary>Network interface to listen on. <c>true</c> = expose on all addresses. Maps to <c>--host [host]</c>.</summary>
    public string? Host { get; set; }

    /// <summary>Port number. Maps to <c>--port</c>.</summary>
    public int? Port { get; set; }

    /// <summary>Fail if the chosen port is in use rather than walking up. Maps to <c>--strictPort</c>.</summary>
    public bool StrictPort { get; set; }

    /// <summary>Use HTTPS. Maps to <c>--https</c>.</summary>
    public bool Https { get; set; }

    /// <summary>Open the browser on start. Optional URL. Maps to <c>--open [path]</c>.</summary>
    public string? Open { get; set; }

    /// <summary>Enable CORS. Maps to <c>--cors</c>.</summary>
    public bool Cors { get; set; }

    /// <summary>Optional project root (positional, last).</summary>
    public string? Root { get; set; }

    public ViteDevSettings SetHost(string? host) { Host = host; return this; }
    public ViteDevSettings SetPort(int port) { Port = port; return this; }
    public ViteDevSettings SetStrictPort(bool v = true) { StrictPort = v; return this; }
    public ViteDevSettings SetHttps(bool v = true) { Https = v; return this; }
    public ViteDevSettings SetOpen(string? path = "") { Open = path ?? ""; return this; }
    public ViteDevSettings SetCors(bool v = true) { Cors = v; return this; }
    public ViteDevSettings SetRoot(string? path) { Root = path; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        var args = new List<string> { "dev" };
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
        if (Cors) args.Add("--cors");
        if (!string.IsNullOrEmpty(Root)) args.Add(Root!);
        return args;
    }
}
