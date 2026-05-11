namespace Tamp.Vite.V5;

/// <summary>Settings for <c>vitest typecheck</c> — vitest's tsc integration.</summary>
public sealed class VitestTypecheckSettings : VitestSettingsBase
{
    public bool Only { get; set; }

    public VitestTypecheckSettings SetOnly(bool v = true) { Only = v; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        var args = new List<string> { "typecheck" };
        EmitCommonArguments(args);
        if (Only) args.Add("--typecheck.only");
        return args;
    }
}
