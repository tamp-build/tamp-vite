namespace Tamp.Vite.V5;

/// <summary>Facade for Vitest 1.x CLI verbs. Lives in the same package as <see cref="Tamp.Vite.V5.Vite"/> because Vitest's major tracks Vite's.</summary>
/// <remarks>
/// <para>Resolve via <c>[NuGetPackage(UseSystemPath = true)]</c>:</para>
/// <code>
/// [NuGetPackage("vitest", UseSystemPath = true)]
/// readonly Tool VitestTool;
/// </code>
/// </remarks>
public static class Vitest
{
    public static CommandPlan Run(Tool tool, Action<VitestRunSettings>? configure = null)
        => Build<VitestRunSettings>(tool, configure);

    public static CommandPlan Watch(Tool tool, Action<VitestWatchSettings>? configure = null)
        => Build<VitestWatchSettings>(tool, configure);

    public static CommandPlan Related(Tool tool, Action<VitestRelatedSettings> configure)
    {
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        return Build(tool, configure);
    }

    public static CommandPlan Bench(Tool tool, Action<VitestBenchSettings>? configure = null)
        => Build<VitestBenchSettings>(tool, configure);

    public static CommandPlan Typecheck(Tool tool, Action<VitestTypecheckSettings>? configure = null)
        => Build<VitestTypecheckSettings>(tool, configure);

    public static CommandPlan Raw(Tool tool, params string[] arguments)
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        if (arguments is null || arguments.Length == 0)
            throw new ArgumentException("Raw requires at least one argument.", nameof(arguments));
        var s = new VitestRawSettings();
        s.AddArgs(arguments);
        return s.ToCommandPlan(tool);
    }

    private static CommandPlan Build<T>(Tool tool, Action<T>? configure) where T : VitestSettingsBase, new()
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        var s = new T();
        configure?.Invoke(s);
        return s.ToCommandPlan(tool);
    }
}
