namespace Tamp.Vite.V5;

/// <summary>Settings for <c>vitest watch</c> / <c>vitest dev</c> — interactive watch mode.</summary>
public sealed class VitestWatchSettings : VitestSettingsBase
{
    /// <summary>Enable the UI. Maps to <c>--ui</c>.</summary>
    public bool Ui { get; set; }

    /// <summary>Open the API socket. Optional port. Maps to <c>--api [port]</c>.</summary>
    public int? ApiPort { get; set; }
    public bool ApiEnabled { get; set; }

    public List<string> FilePatterns { get; } = [];

    public VitestWatchSettings SetUi(bool v = true) { Ui = v; return this; }
    public VitestWatchSettings SetApi(int? port = null) { ApiEnabled = true; ApiPort = port; return this; }
    public VitestWatchSettings AddFilePattern(string p) { FilePatterns.Add(p); return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        var args = new List<string> { "watch" };
        EmitCommonArguments(args);
        if (Ui) args.Add("--ui");
        if (ApiEnabled)
        {
            args.Add("--api");
            if (ApiPort is { } p) args.Add(p.ToString());
        }
        foreach (var p in FilePatterns) args.Add(p);
        return args;
    }
}
