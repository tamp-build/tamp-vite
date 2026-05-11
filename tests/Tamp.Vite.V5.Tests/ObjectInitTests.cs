using System.IO;
using Tamp;
using Xunit;

namespace Tamp.Vite.V5.Tests;

/// <summary>
/// Object-init overloads (TAM-161): every fluent <c>Action&lt;TSettings&gt;</c> wrapper
/// has a parallel <c>TSettings</c> overload that produces an identical CommandPlan.
/// </summary>
public sealed class ObjectInitTests
{
    private static Tool FakeTool(string name = "vite") =>
        new(AbsolutePath.Create(Path.Combine(Path.GetTempPath(), name)));

    // ---- Vite ----

    [Fact]
    public void Vite_Dev_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var t = FakeTool();
        var fluent = Vite.Dev(t, s =>
        {
            s.SetPort(5174).SetStrictPort().SetHost("0.0.0.0").SetHttps().SetRoot("./frontend");
            s.SetConfig("vite.config.ts").SetMode("development");
        });

        var objectInit = Vite.Dev(t, new ViteDevSettings
        {
            Config = "vite.config.ts",
            Mode = "development",
            Port = 5174,
            StrictPort = true,
            Host = "0.0.0.0",
            Https = true,
            Root = "./frontend",
        });

        Assert.Equal(fluent.Executable, objectInit.Executable);
        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void Vite_BuildProject_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var t = FakeTool();
        var fluent = Vite.BuildProject(t, s =>
        {
            s.SetOutDir("dist")
                .SetAssetsDir("static")
                .SetSourcemap("hidden")
                .SetMinify("terser")
                .SetManifest()
                .SetEmptyOutDir(true)
                .AddTarget("es2020")
                .AddTarget("chrome100");
            s.SetMode("production");
        });

        var objectInit = Vite.BuildProject(t, new ViteBuildSettings
        {
            Mode = "production",
            OutDir = "dist",
            AssetsDir = "static",
            Sourcemap = "hidden",
            Minify = "terser",
            Manifest = true,
            EmptyOutDir = true,
            Target = { "es2020", "chrome100" },
        });

        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void Vite_Preview_ObjectInit_Round_Trips()
    {
        var t = FakeTool();
        var args = Vite.Preview(t, new VitePreviewSettings
        {
            Host = "0.0.0.0",
            Port = 4173,
            StrictPort = true,
            Https = true,
            OutDir = "dist",
        }).Arguments;

        Assert.Equal("preview", args[0]);
        Assert.Contains("--host", args);
        Assert.Contains("0.0.0.0", args);
        Assert.Contains("--port", args);
        Assert.Contains("4173", args);
        Assert.Contains("--strictPort", args);
        Assert.Contains("--https", args);
        Assert.Contains("--outDir", args);
        Assert.Contains("dist", args);
    }

    [Fact]
    public void Vite_OptimizeDeps_ObjectInit_Round_Trips()
    {
        var t = FakeTool();
        var args = Vite.OptimizeDeps(t, new ViteOptimizeDepsSettings
        {
            Force = true,
            Root = "./frontend",
        }).Arguments;

        Assert.Equal("optimize", args[0]);
        Assert.Contains("--force", args);
        Assert.Equal("./frontend", args[^1]);
    }

    [Fact]
    public void Vite_ObjectInit_Surface_Compiles_And_Returns_CommandPlan()
    {
        var t = FakeTool();
        Assert.NotNull(Vite.Dev(t, new ViteDevSettings()));
        Assert.NotNull(Vite.BuildProject(t, new ViteBuildSettings()));
        Assert.NotNull(Vite.Preview(t, new VitePreviewSettings()));
        Assert.NotNull(Vite.OptimizeDeps(t, new ViteOptimizeDepsSettings()));
    }

    [Fact]
    public void Vite_ObjectInit_Null_Tool_Throws_For_Every_Verb()
    {
        Assert.Throws<ArgumentNullException>(() => Vite.Dev(null!, new ViteDevSettings()));
        Assert.Throws<ArgumentNullException>(() => Vite.BuildProject(null!, new ViteBuildSettings()));
        Assert.Throws<ArgumentNullException>(() => Vite.Preview(null!, new VitePreviewSettings()));
        Assert.Throws<ArgumentNullException>(() => Vite.OptimizeDeps(null!, new ViteOptimizeDepsSettings()));
    }

    [Fact]
    public void Vite_ObjectInit_Null_Settings_Throws_For_Every_Verb()
    {
        var t = FakeTool();
        Assert.Throws<ArgumentNullException>(() => Vite.Dev(t, (ViteDevSettings)null!));
        Assert.Throws<ArgumentNullException>(() => Vite.BuildProject(t, (ViteBuildSettings)null!));
        Assert.Throws<ArgumentNullException>(() => Vite.Preview(t, (VitePreviewSettings)null!));
        Assert.Throws<ArgumentNullException>(() => Vite.OptimizeDeps(t, (ViteOptimizeDepsSettings)null!));
    }

    // ---- Vitest ----

    [Fact]
    public void Vitest_Run_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var t = FakeTool("vitest");
        var fluent = Vitest.Run(t, s =>
        {
            s.SetTestNamePattern("math").SetUpdateSnapshots().AddFilePattern("src/**/*.test.ts");
            s.SetConfig("vitest.config.ts").SetMode("ci");
        });

        var objectInit = Vitest.Run(t, new VitestRunSettings
        {
            Config = "vitest.config.ts",
            Mode = "ci",
            TestNamePattern = "math",
            UpdateSnapshots = true,
            FilePatterns = { "src/**/*.test.ts" },
        });

        Assert.Equal(fluent.Executable, objectInit.Executable);
        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void Vitest_Watch_ObjectInit_Round_Trips()
    {
        var t = FakeTool("vitest");
        var args = Vitest.Watch(t, new VitestWatchSettings
        {
            Ui = true,
            ApiEnabled = true,
            ApiPort = 51204,
            FilePatterns = { "src/foo.test.ts" },
        }).Arguments;

        Assert.Equal("watch", args[0]);
        Assert.Contains("--ui", args);
        Assert.Contains("--api", args);
        Assert.Contains("51204", args);
        Assert.Contains("src/foo.test.ts", args);
    }

    [Fact]
    public void Vitest_Related_ObjectInit_Round_Trips()
    {
        var t = FakeTool("vitest");
        var args = Vitest.Related(t, new VitestRelatedSettings
        {
            Files = { "src/foo.ts", "src/bar.ts" },
        }).Arguments;

        Assert.Equal("related", args[0]);
        Assert.Contains("src/foo.ts", args);
        Assert.Contains("src/bar.ts", args);
    }

    [Fact]
    public void Vitest_Typecheck_ObjectInit_Round_Trips()
    {
        var t = FakeTool("vitest");
        var args = Vitest.Typecheck(t, new VitestTypecheckSettings
        {
            Only = true,
        }).Arguments;

        Assert.Equal("typecheck", args[0]);
        Assert.Contains("--typecheck.only", args);
    }

    [Fact]
    public void Vitest_ObjectInit_Surface_Compiles_And_Returns_CommandPlan()
    {
        var t = FakeTool("vitest");
        Assert.NotNull(Vitest.Run(t, new VitestRunSettings()));
        Assert.NotNull(Vitest.Watch(t, new VitestWatchSettings()));
        Assert.NotNull(Vitest.Related(t, new VitestRelatedSettings { Files = { "a.ts" } }));
        Assert.NotNull(Vitest.Bench(t, new VitestBenchSettings()));
        Assert.NotNull(Vitest.Typecheck(t, new VitestTypecheckSettings()));
    }

    [Fact]
    public void Vitest_ObjectInit_Null_Tool_Throws_For_Every_Verb()
    {
        Assert.Throws<ArgumentNullException>(() => Vitest.Run(null!, new VitestRunSettings()));
        Assert.Throws<ArgumentNullException>(() => Vitest.Watch(null!, new VitestWatchSettings()));
        Assert.Throws<ArgumentNullException>(() => Vitest.Related(null!, new VitestRelatedSettings()));
        Assert.Throws<ArgumentNullException>(() => Vitest.Bench(null!, new VitestBenchSettings()));
        Assert.Throws<ArgumentNullException>(() => Vitest.Typecheck(null!, new VitestTypecheckSettings()));
    }

    [Fact]
    public void Vitest_ObjectInit_Null_Settings_Throws_For_Every_Verb()
    {
        var t = FakeTool("vitest");
        Assert.Throws<ArgumentNullException>(() => Vitest.Run(t, (VitestRunSettings)null!));
        Assert.Throws<ArgumentNullException>(() => Vitest.Watch(t, (VitestWatchSettings)null!));
        Assert.Throws<ArgumentNullException>(() => Vitest.Related(t, (VitestRelatedSettings)null!));
        Assert.Throws<ArgumentNullException>(() => Vitest.Bench(t, (VitestBenchSettings)null!));
        Assert.Throws<ArgumentNullException>(() => Vitest.Typecheck(t, (VitestTypecheckSettings)null!));
    }
}
