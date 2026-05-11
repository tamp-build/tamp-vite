namespace Tamp.Vite.V5;

/// <summary>
/// Common base for Vitest 1.x verb settings. Covers the knobs every
/// vitest invocation accepts: config, root, dir, mode, reporters,
/// pool / threading, bail, silent, etc.
/// </summary>
public abstract class VitestSettingsBase
{
    public string? WorkingDirectory { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; } = new();

    /// <summary>Path to vitest config. Maps to <c>--config</c> / <c>-c</c>.</summary>
    public string? Config { get; set; }

    /// <summary>Project root. Maps to <c>--root</c> / <c>-r</c>.</summary>
    public string? Root { get; set; }

    /// <summary>Directory to look for tests in. Maps to <c>--dir</c>.</summary>
    public string? Dir { get; set; }

    /// <summary>Vite mode. Maps to <c>--mode</c> / <c>-m</c>.</summary>
    public string? Mode { get; set; }

    /// <summary>Suppress test output (only summary). Maps to <c>--silent</c>.</summary>
    public bool Silent { get; set; }

    /// <summary>Stop after N test failures. Maps to <c>--bail</c>.</summary>
    public int? Bail { get; set; }

    /// <summary>Pool implementation (<c>threads</c>, <c>forks</c>, <c>vmThreads</c>, <c>vmForks</c>). Maps to <c>--pool</c>.</summary>
    public string? Pool { get; set; }

    /// <summary>Reporters. Repeated as <c>--reporter &lt;name&gt;</c>.</summary>
    public List<string> Reporter { get; } = [];

    /// <summary>Output file. <c>name=path</c> form is allowed. Maps to <c>--outputFile</c>.</summary>
    public string? OutputFile { get; set; }

    /// <summary>Pass when no tests are matched (helpful in monorepos). Maps to <c>--passWithNoTests</c>.</summary>
    public bool PassWithNoTests { get; set; }

    /// <summary>Log heap usage per test. Maps to <c>--logHeapUsage</c>.</summary>
    public bool LogHeapUsage { get; set; }

    /// <summary>Coverage enable. Maps to <c>--coverage</c>.</summary>
    public bool Coverage { get; set; }

    /// <summary>Coverage provider (<c>v8</c>, <c>istanbul</c>, custom). Maps to <c>--coverage.provider</c>.</summary>
    public string? CoverageProvider { get; set; }

    /// <summary>Coverage reporters. Repeated as <c>--coverage.reporter &lt;name&gt;</c>.</summary>
    public List<string> CoverageReporter { get; } = [];

    /// <summary>Coverage output directory. Maps to <c>--coverage.reportsDirectory</c>.</summary>
    public string? CoverageReportsDirectory { get; set; }

    public VitestSettingsBase SetWorkingDirectory(string? cwd) { WorkingDirectory = cwd; return this; }
    public VitestSettingsBase SetEnv(string key, string value) { EnvironmentVariables[key] = value; return this; }
    public VitestSettingsBase SetConfig(string? path) { Config = path; return this; }
    public VitestSettingsBase SetRoot(string? path) { Root = path; return this; }
    public VitestSettingsBase SetDir(string? path) { Dir = path; return this; }
    public VitestSettingsBase SetMode(string? mode) { Mode = mode; return this; }
    public VitestSettingsBase SetSilent(bool v = true) { Silent = v; return this; }
    public VitestSettingsBase SetBail(int n) { Bail = n; return this; }
    public VitestSettingsBase SetPool(string pool) { Pool = pool; return this; }
    public VitestSettingsBase AddReporter(string name) { Reporter.Add(name); return this; }
    public VitestSettingsBase SetOutputFile(string spec) { OutputFile = spec; return this; }
    public VitestSettingsBase SetPassWithNoTests(bool v = true) { PassWithNoTests = v; return this; }
    public VitestSettingsBase SetLogHeapUsage(bool v = true) { LogHeapUsage = v; return this; }
    public VitestSettingsBase SetCoverage(bool v = true) { Coverage = v; return this; }
    public VitestSettingsBase SetCoverageProvider(string provider) { CoverageProvider = provider; return this; }
    public VitestSettingsBase AddCoverageReporter(string name) { CoverageReporter.Add(name); return this; }
    public VitestSettingsBase SetCoverageReportsDirectory(string path) { CoverageReportsDirectory = path; return this; }

    protected abstract IEnumerable<string> BuildVerbArguments();

    protected virtual IReadOnlyList<Secret> CollectSecrets() => Array.Empty<Secret>();

    protected void EmitCommonArguments(List<string> args)
    {
        if (!string.IsNullOrEmpty(Config)) { args.Add("--config"); args.Add(Config!); }
        if (!string.IsNullOrEmpty(Root)) { args.Add("--root"); args.Add(Root!); }
        if (!string.IsNullOrEmpty(Dir)) { args.Add("--dir"); args.Add(Dir!); }
        if (!string.IsNullOrEmpty(Mode)) { args.Add("--mode"); args.Add(Mode!); }
        if (Silent) args.Add("--silent");
        if (Bail is { } n) { args.Add("--bail"); args.Add(n.ToString()); }
        if (!string.IsNullOrEmpty(Pool)) { args.Add("--pool"); args.Add(Pool!); }
        foreach (var r in Reporter) { args.Add("--reporter"); args.Add(r); }
        if (!string.IsNullOrEmpty(OutputFile)) { args.Add("--outputFile"); args.Add(OutputFile!); }
        if (PassWithNoTests) args.Add("--passWithNoTests");
        if (LogHeapUsage) args.Add("--logHeapUsage");
        if (Coverage) args.Add("--coverage");
        if (!string.IsNullOrEmpty(CoverageProvider)) { args.Add("--coverage.provider"); args.Add(CoverageProvider!); }
        foreach (var r in CoverageReporter) { args.Add("--coverage.reporter"); args.Add(r); }
        if (!string.IsNullOrEmpty(CoverageReportsDirectory)) { args.Add("--coverage.reportsDirectory"); args.Add(CoverageReportsDirectory!); }
    }

    public CommandPlan ToCommandPlan(Tool tool)
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        var args = BuildVerbArguments().ToList();
        return new CommandPlan
        {
            Executable = tool.Executable.Value,
            Arguments = args,
            Environment = new Dictionary<string, string>(EnvironmentVariables),
            WorkingDirectory = WorkingDirectory,
            Secrets = CollectSecrets(),
        };
    }
}
