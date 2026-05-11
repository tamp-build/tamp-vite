namespace Tamp.Vite.V5;

/// <summary>
/// Common base for Vite 5.x verb settings. Holds the flags every verb
/// accepts: <c>--config</c>, <c>--mode</c>, <c>--base</c>,
/// <c>--logLevel</c>, <c>--clearScreen</c>, <c>--force</c>,
/// <c>--debug</c>, <c>--filter</c>.
/// </summary>
public abstract class ViteSettingsBase
{
    public string? WorkingDirectory { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; } = new();

    /// <summary>Path to the vite config file. Maps to <c>--config</c> / <c>-c</c>.</summary>
    public string? Config { get; set; }

    /// <summary>Mode (development / production / custom). Maps to <c>--mode</c> / <c>-m</c>.</summary>
    public string? Mode { get; set; }

    /// <summary>Public base path. Maps to <c>--base</c>.</summary>
    public string? Base { get; set; }

    /// <summary>Log level (<c>info</c>, <c>warn</c>, <c>error</c>, <c>silent</c>). Maps to <c>--logLevel</c> / <c>-l</c>.</summary>
    public string? LogLevel { get; set; }

    /// <summary>Allow/forbid terminal clear at startup. <c>true</c> → <c>--clearScreen</c>; <c>false</c> → <c>--no-clearScreen</c>.</summary>
    public bool? ClearScreen { get; set; }

    /// <summary>Force optimizer to ignore cache and re-bundle. Maps to <c>--force</c>.</summary>
    public bool Force { get; set; }

    /// <summary>Show debug logs. Optional category filter. Maps to <c>--debug</c> / <c>-d</c>.</summary>
    public string? Debug { get; set; }

    /// <summary>Filter debug logs. Maps to <c>--filter</c>.</summary>
    public string? Filter { get; set; }

    public ViteSettingsBase SetWorkingDirectory(string? cwd) { WorkingDirectory = cwd; return this; }
    public ViteSettingsBase SetEnv(string key, string value) { EnvironmentVariables[key] = value; return this; }
    public ViteSettingsBase SetConfig(string? path) { Config = path; return this; }
    public ViteSettingsBase SetMode(string? mode) { Mode = mode; return this; }
    public ViteSettingsBase SetBase(string? path) { Base = path; return this; }
    public ViteSettingsBase SetLogLevel(string? level) { LogLevel = level; return this; }
    public ViteSettingsBase SetClearScreen(bool v) { ClearScreen = v; return this; }
    public ViteSettingsBase SetForce(bool v = true) { Force = v; return this; }
    public ViteSettingsBase SetDebug(string? category = "") { Debug = category ?? ""; return this; }
    public ViteSettingsBase SetFilter(string filter) { Filter = filter; return this; }

    protected abstract IEnumerable<string> BuildVerbArguments();

    protected virtual IReadOnlyList<Secret> CollectSecrets() => Array.Empty<Secret>();

    protected void EmitCommonArguments(List<string> args)
    {
        if (!string.IsNullOrEmpty(Config)) { args.Add("--config"); args.Add(Config!); }
        if (!string.IsNullOrEmpty(Mode)) { args.Add("--mode"); args.Add(Mode!); }
        if (!string.IsNullOrEmpty(Base)) { args.Add("--base"); args.Add(Base!); }
        if (!string.IsNullOrEmpty(LogLevel)) { args.Add("--logLevel"); args.Add(LogLevel!); }
        if (ClearScreen is true) args.Add("--clearScreen");
        else if (ClearScreen is false) args.Add("--no-clearScreen");
        if (Force) args.Add("--force");
        if (Debug is not null)
        {
            args.Add("--debug");
            if (Debug.Length > 0) args.Add(Debug);
        }
        if (!string.IsNullOrEmpty(Filter)) { args.Add("--filter"); args.Add(Filter!); }
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
