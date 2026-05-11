using System.IO;
using Tamp;
using Xunit;

namespace Tamp.Vite.V5.Tests;

public sealed class ViteTests
{
    private static Tool FakeTool(string name = "vite") =>
        new(AbsolutePath.Create(Path.Combine(Path.GetTempPath(), name)));

    private static int IndexOf(IReadOnlyList<string> args, string value, int start = 0)
    {
        for (var i = start; i < args.Count; i++)
            if (args[i] == value) return i;
        return -1;
    }

    [Fact]
    public void Every_Verb_Uses_Tool_Path()
    {
        var t = FakeTool();
        Assert.Equal(t.Executable.Value, Vite.Dev(t).Executable);
        Assert.Equal(t.Executable.Value, Vite.BuildProject(t).Executable);
        Assert.Equal(t.Executable.Value, Vite.Preview(t).Executable);
        Assert.Equal(t.Executable.Value, Vite.OptimizeDeps(t).Executable);
        Assert.Equal(t.Executable.Value, Vite.Raw(t, "--version").Executable);
    }

    [Fact]
    public void Verbs_Begin_With_Their_Verb_Token()
    {
        Assert.Equal("dev", Vite.Dev(FakeTool()).Arguments[0]);
        Assert.Equal("build", Vite.BuildProject(FakeTool()).Arguments[0]);
        Assert.Equal("preview", Vite.Preview(FakeTool()).Arguments[0]);
        Assert.Equal("optimize", Vite.OptimizeDeps(FakeTool()).Arguments[0]);
    }

    [Fact]
    public void Common_Flags_Round_Trip_On_Dev()
    {
        var plan = Vite.Dev(FakeTool(), s => s
            .SetConfig("vite.config.ts")
            .SetMode("staging")
            .SetBase("/app/")
            .SetLogLevel("warn")
            .SetClearScreen(false)
            .SetForce()
            .SetDebug("resolve")
            .SetFilter("plugin:*"));
        var args = plan.Arguments;
        Assert.Contains("--config", args);
        Assert.Contains("vite.config.ts", args);
        Assert.Contains("--mode", args);
        Assert.Contains("staging", args);
        Assert.Contains("--base", args);
        Assert.Contains("/app/", args);
        Assert.Contains("--logLevel", args);
        Assert.Contains("warn", args);
        Assert.Contains("--no-clearScreen", args);
        Assert.Contains("--force", args);
        Assert.Contains("--debug", args);
        Assert.Contains("resolve", args);
        Assert.Contains("--filter", args);
        Assert.Contains("plugin:*", args);
    }

    [Fact]
    public void Dev_Host_Without_Value_Just_Emits_Flag()
    {
        var plan = Vite.Dev(FakeTool(), s => s.SetHost(""));
        var args = plan.Arguments;
        var hostIdx = IndexOf(args, "--host");
        Assert.True(hostIdx >= 0);
        // The arg after --host is either nothing (last) or something other than a value.
        Assert.True(hostIdx == args.Count - 1 || args[hostIdx + 1].StartsWith("--"));
    }

    [Fact]
    public void Dev_Host_With_Value_Emits_Value_After_Flag()
    {
        var plan = Vite.Dev(FakeTool(), s => s.SetHost("0.0.0.0"));
        var args = plan.Arguments;
        var hostIdx = IndexOf(args, "--host");
        Assert.Equal("0.0.0.0", args[hostIdx + 1]);
    }

    [Fact]
    public void Dev_Port_And_StrictPort_And_Https_Round_Trip()
    {
        var plan = Vite.Dev(FakeTool(), s => s.SetPort(5174).SetStrictPort().SetHttps());
        Assert.Contains("--port", plan.Arguments);
        Assert.Contains("5174", plan.Arguments);
        Assert.Contains("--strictPort", plan.Arguments);
        Assert.Contains("--https", plan.Arguments);
    }

    [Fact]
    public void Dev_Root_Sits_Last()
    {
        var plan = Vite.Dev(FakeTool(), s => s.SetPort(3000).SetRoot("./frontend"));
        Assert.Equal("./frontend", plan.Arguments[^1]);
    }

    [Fact]
    public void Build_Default_Is_Just_The_Verb()
    {
        var plan = Vite.BuildProject(FakeTool());
        Assert.Equal(["build"], plan.Arguments);
    }

    [Fact]
    public void Build_All_Flags_Round_Trip()
    {
        var plan = Vite.BuildProject(FakeTool(), s => s
            .SetOutDir("dist")
            .SetAssetsDir("static")
            .SetAssetsInlineLimit(4096)
            .SetSsr("src/entry-server.ts")
            .SetSourcemap("hidden")
            .SetMinify("terser")
            .SetManifest()
            .SetSsrManifest()
            .SetEmptyOutDir(true)
            .SetWatch()
            .AddTarget("es2020")
            .AddTarget("chrome100"));
        var args = plan.Arguments;
        Assert.Contains("--outDir", args);
        Assert.Contains("dist", args);
        Assert.Contains("--assetsDir", args);
        Assert.Contains("static", args);
        Assert.Contains("--assetsInlineLimit", args);
        Assert.Contains("4096", args);
        Assert.Contains("--ssr", args);
        Assert.Contains("src/entry-server.ts", args);
        Assert.Contains("--sourcemap", args);
        Assert.Contains("hidden", args);
        Assert.Contains("--minify", args);
        Assert.Contains("terser", args);
        Assert.Contains("--manifest", args);
        Assert.Contains("--ssrManifest", args);
        Assert.Contains("--emptyOutDir", args);
        Assert.Contains("--watch", args);

        // Multiple targets emit as repeated --target pairs in order.
        var first = IndexOf(args, "--target");
        var second = IndexOf(args, "--target", first + 1);
        Assert.True(first >= 0 && second > first);
        Assert.Equal("es2020", args[first + 1]);
        Assert.Equal("chrome100", args[second + 1]);
    }

    [Fact]
    public void Build_EmptyOutDir_False_Uses_Equals_Form()
    {
        var plan = Vite.BuildProject(FakeTool(), s => s.SetEmptyOutDir(false));
        Assert.Contains("--emptyOutDir=false", plan.Arguments);
        Assert.DoesNotContain("--emptyOutDir", plan.Arguments.Where(a => a == "--emptyOutDir"));
    }

    [Fact]
    public void Build_Ssr_Without_Entry_Just_Emits_Flag()
    {
        var plan = Vite.BuildProject(FakeTool(), s => s.SetSsr());
        var args = plan.Arguments;
        var idx = IndexOf(args, "--ssr");
        Assert.True(idx >= 0);
        Assert.True(idx == args.Count - 1 || args[idx + 1].StartsWith("--"));
    }

    [Fact]
    public void Preview_Common_Flags_Round_Trip()
    {
        var plan = Vite.Preview(FakeTool(), s => s
            .SetHost("0.0.0.0")
            .SetPort(4173)
            .SetStrictPort()
            .SetHttps()
            .SetOpen("/")
            .SetOutDir("dist"));
        var args = plan.Arguments;
        Assert.Contains("--host", args);
        Assert.Contains("0.0.0.0", args);
        Assert.Contains("--port", args);
        Assert.Contains("4173", args);
        Assert.Contains("--strictPort", args);
        Assert.Contains("--https", args);
        Assert.Contains("--open", args);
        Assert.Contains("/", args);
        Assert.Contains("--outDir", args);
        Assert.Contains("dist", args);
    }

    [Fact]
    public void OptimizeDeps_With_Force()
    {
        var plan = Vite.OptimizeDeps(FakeTool(), s => s.SetForce());
        Assert.Equal("optimize", plan.Arguments[0]);
        Assert.Contains("--force", plan.Arguments);
    }

    [Fact]
    public void Raw_Requires_At_Least_One_Arg()
    {
        Assert.Throws<ArgumentException>(() => Vite.Raw(FakeTool()));
    }

    [Fact]
    public void Raw_Forwards_Arguments_Verbatim()
    {
        var plan = Vite.Raw(FakeTool(), "--config", "vite.config.ts");
        Assert.Equal(["--config", "vite.config.ts"], plan.Arguments);
    }

    [Fact]
    public void Null_Tool_Throws_For_Every_Verb()
    {
        Assert.Throws<ArgumentNullException>(() => Vite.Dev(null!));
        Assert.Throws<ArgumentNullException>(() => Vite.BuildProject(null!));
        Assert.Throws<ArgumentNullException>(() => Vite.Preview(null!));
        Assert.Throws<ArgumentNullException>(() => Vite.OptimizeDeps(null!));
        Assert.Throws<ArgumentNullException>(() => Vite.Raw(null!, "--help"));
    }

    [Fact]
    public void Working_Directory_And_Env_Flow_To_Plan()
    {
        var cwd = Path.GetTempPath();
        var plan = Vite.BuildProject(FakeTool(), s => s
            .SetWorkingDirectory(cwd)
            .SetEnv("VITE_API_URL", "https://api.example.com"));
        Assert.Equal(cwd, plan.WorkingDirectory);
        Assert.Equal("https://api.example.com", plan.Environment["VITE_API_URL"]);
    }
}
