namespace Tamp.Vite.V5;

/// <summary>
/// Settings for <c>vitest run</c> — single non-watching run, the
/// canonical CI invocation.
/// </summary>
public sealed class VitestRunSettings : VitestSettingsBase
{
    /// <summary>Test name regex filter. Maps to <c>--testNamePattern</c> / <c>-t</c>.</summary>
    public string? TestNamePattern { get; set; }

    /// <summary>Update snapshots. Maps to <c>--update</c> / <c>-u</c>.</summary>
    public bool UpdateSnapshots { get; set; }

    /// <summary>Run only files changed since the given ref / true for working-tree dirty files. Maps to <c>--changed [since]</c>.</summary>
    public string? Changed { get; set; }

    /// <summary>Whether to use the changed flag at all (without a since value).</summary>
    public bool ChangedFlag { get; set; }

    /// <summary>Optional positional file patterns to filter the run.</summary>
    public List<string> FilePatterns { get; } = [];

    public VitestRunSettings SetTestNamePattern(string pattern) { TestNamePattern = pattern; return this; }
    public VitestRunSettings SetUpdateSnapshots(bool v = true) { UpdateSnapshots = v; return this; }
    public VitestRunSettings SetChanged(string? since = null) { ChangedFlag = true; Changed = since; return this; }
    public VitestRunSettings AddFilePattern(string p) { FilePatterns.Add(p); return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        var args = new List<string> { "run" };
        EmitCommonArguments(args);
        if (!string.IsNullOrEmpty(TestNamePattern)) { args.Add("--testNamePattern"); args.Add(TestNamePattern!); }
        if (UpdateSnapshots) args.Add("--update");
        if (ChangedFlag)
        {
            args.Add("--changed");
            if (!string.IsNullOrEmpty(Changed)) args.Add(Changed!);
        }
        foreach (var p in FilePatterns) args.Add(p);
        return args;
    }
}
