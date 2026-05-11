namespace Tamp.Vite.V5;

/// <summary>Facade for Vite 5.x CLI verbs.</summary>
/// <remarks>
/// <para>Resolve via <c>[NuGetPackage(UseSystemPath = true)]</c>:</para>
/// <code>
/// [NuGetPackage("vite", UseSystemPath = true)]
/// readonly Tool Vite;
/// </code>
/// </remarks>
public static class Vite
{
    public static CommandPlan Dev(Tool tool, Action<ViteDevSettings>? configure = null)
        => Build<ViteDevSettings>(tool, configure);

    public static CommandPlan BuildProject(Tool tool, Action<ViteBuildSettings>? configure = null)
        => Build<ViteBuildSettings>(tool, configure);

    public static CommandPlan Preview(Tool tool, Action<VitePreviewSettings>? configure = null)
        => Build<VitePreviewSettings>(tool, configure);

    public static CommandPlan OptimizeDeps(Tool tool, Action<ViteOptimizeDepsSettings>? configure = null)
        => Build<ViteOptimizeDepsSettings>(tool, configure);

    /// <summary>Escape hatch for verbs we haven't typed.</summary>
    public static CommandPlan Raw(Tool tool, params string[] arguments)
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        if (arguments is null || arguments.Length == 0)
            throw new ArgumentException("Raw requires at least one argument.", nameof(arguments));
        var s = new ViteRawSettings();
        s.AddArgs(arguments);
        return s.ToCommandPlan(tool);
    }

    private static CommandPlan Build<T>(Tool tool, Action<T>? configure) where T : ViteSettingsBase, new()
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        var s = new T();
        configure?.Invoke(s);
        return s.ToCommandPlan(tool);
    }
}
