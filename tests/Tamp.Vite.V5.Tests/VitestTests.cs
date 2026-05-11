using System.IO;
using Bogus;
using Tamp;
using Tamp.Vite.V5;
using Xunit;

namespace Tamp.Vite.V5.Tests;

public sealed class VitestTests
{
    private static Tool FakeTool(string name = "vitest") =>
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
        Assert.Equal(t.Executable.Value, Vitest.Run(t).Executable);
        Assert.Equal(t.Executable.Value, Vitest.Watch(t).Executable);
        Assert.Equal(t.Executable.Value, Vitest.Related(t, s => s.AddFile("a.ts")).Executable);
        Assert.Equal(t.Executable.Value, Vitest.Bench(t).Executable);
        Assert.Equal(t.Executable.Value, Vitest.Typecheck(t).Executable);
        Assert.Equal(t.Executable.Value, Vitest.Raw(t, "--version").Executable);
    }

    [Fact]
    public void Verbs_Begin_With_Their_Verb_Token()
    {
        Assert.Equal("run", Vitest.Run(FakeTool()).Arguments[0]);
        Assert.Equal("watch", Vitest.Watch(FakeTool()).Arguments[0]);
        Assert.Equal("related", Vitest.Related(FakeTool(), s => s.AddFile("a.ts")).Arguments[0]);
        Assert.Equal("bench", Vitest.Bench(FakeTool()).Arguments[0]);
        Assert.Equal("typecheck", Vitest.Typecheck(FakeTool()).Arguments[0]);
    }

    [Fact]
    public void Run_Common_Flags_Round_Trip()
    {
        var plan = Vitest.Run(FakeTool(), s => s
            .SetConfig("vitest.config.ts")
            .SetRoot(".")
            .SetDir("src")
            .SetMode("ci")
            .SetSilent()
            .SetBail(1)
            .SetPool("forks")
            .AddReporter("default")
            .AddReporter("junit")
            .SetOutputFile("junit=./junit.xml")
            .SetPassWithNoTests()
            .SetLogHeapUsage()
            .SetCoverage()
            .SetCoverageProvider("v8")
            .AddCoverageReporter("text")
            .AddCoverageReporter("lcov")
            .SetCoverageReportsDirectory("coverage"));
        var args = plan.Arguments;

        Assert.Contains("--config", args); Assert.Contains("vitest.config.ts", args);
        Assert.Contains("--root", args);
        Assert.Contains("--dir", args); Assert.Contains("src", args);
        Assert.Contains("--mode", args); Assert.Contains("ci", args);
        Assert.Contains("--silent", args);
        Assert.Contains("--bail", args); Assert.Contains("1", args);
        Assert.Contains("--pool", args); Assert.Contains("forks", args);
        var firstReporter = IndexOf(args, "--reporter");
        var secondReporter = IndexOf(args, "--reporter", firstReporter + 1);
        Assert.True(firstReporter >= 0 && secondReporter > firstReporter);
        Assert.Equal("default", args[firstReporter + 1]);
        Assert.Equal("junit", args[secondReporter + 1]);
        Assert.Contains("--outputFile", args); Assert.Contains("junit=./junit.xml", args);
        Assert.Contains("--passWithNoTests", args);
        Assert.Contains("--logHeapUsage", args);
        Assert.Contains("--coverage", args);
        Assert.Contains("--coverage.provider", args); Assert.Contains("v8", args);
        var firstCov = IndexOf(args, "--coverage.reporter");
        var secondCov = IndexOf(args, "--coverage.reporter", firstCov + 1);
        Assert.Equal("text", args[firstCov + 1]);
        Assert.Equal("lcov", args[secondCov + 1]);
        Assert.Contains("--coverage.reportsDirectory", args); Assert.Contains("coverage", args);
    }

    [Fact]
    public void Run_TestNamePattern_And_Update()
    {
        var plan = Vitest.Run(FakeTool(), s => s
            .SetTestNamePattern("auth.*login")
            .SetUpdateSnapshots());
        var args = plan.Arguments;
        Assert.Contains("--testNamePattern", args);
        Assert.Contains("auth.*login", args);
        Assert.Contains("--update", args);
    }

    [Fact]
    public void Run_Changed_With_Since()
    {
        var plan = Vitest.Run(FakeTool(), s => s.SetChanged("HEAD~1"));
        var args = plan.Arguments;
        var idx = IndexOf(args, "--changed");
        Assert.True(idx >= 0);
        Assert.Equal("HEAD~1", args[idx + 1]);
    }

    [Fact]
    public void Run_Changed_Without_Since_Just_Emits_Flag()
    {
        var plan = Vitest.Run(FakeTool(), s => s.SetChanged());
        var args = plan.Arguments;
        var idx = IndexOf(args, "--changed");
        Assert.True(idx >= 0);
        // Either last or followed by a positional / unrelated arg.
        Assert.True(idx == args.Count - 1 || !args[idx + 1].StartsWith("--"));
    }

    [Fact]
    public void Run_File_Patterns_Tail_The_Verb()
    {
        var plan = Vitest.Run(FakeTool(), s => s
            .AddFilePattern("src/auth/**/*.test.ts")
            .AddFilePattern("src/billing/**/*.test.ts"));
        var args = plan.Arguments;
        Assert.Equal("src/auth/**/*.test.ts", args[^2]);
        Assert.Equal("src/billing/**/*.test.ts", args[^1]);
    }

    [Fact]
    public void Watch_Ui_And_Api()
    {
        var plan = Vitest.Watch(FakeTool(), s => s.SetUi().SetApi(51204));
        var args = plan.Arguments;
        Assert.Contains("--ui", args);
        var apiIdx = IndexOf(args, "--api");
        Assert.True(apiIdx >= 0);
        Assert.Equal("51204", args[apiIdx + 1]);
    }

    [Fact]
    public void Watch_Api_Without_Port_Just_Emits_Flag()
    {
        var plan = Vitest.Watch(FakeTool(), s => s.SetApi());
        var args = plan.Arguments;
        var apiIdx = IndexOf(args, "--api");
        Assert.True(apiIdx >= 0);
        Assert.True(apiIdx == args.Count - 1 || !args[apiIdx + 1].All(char.IsDigit));
    }

    [Fact]
    public void Related_Requires_At_Least_One_File()
    {
        Assert.Throws<InvalidOperationException>(() => Vitest.Related(FakeTool(), s => { }));
    }

    [Fact]
    public void Related_Files_Tail_The_Verb()
    {
        var plan = Vitest.Related(FakeTool(), s => s
            .AddFile("src/auth/login.ts")
            .AddFile("src/auth/logout.ts"));
        var args = plan.Arguments;
        Assert.Equal("related", args[0]);
        Assert.Equal("src/auth/login.ts", args[^2]);
        Assert.Equal("src/auth/logout.ts", args[^1]);
    }

    [Fact]
    public void Typecheck_Only_Flag()
    {
        var plan = Vitest.Typecheck(FakeTool(), s => s.SetOnly());
        Assert.Contains("--typecheck.only", plan.Arguments);
    }

    [Fact]
    public void Raw_Requires_Args()
    {
        Assert.Throws<ArgumentException>(() => Vitest.Raw(FakeTool()));
    }

    [Fact]
    public void Raw_Verbatim()
    {
        var plan = Vitest.Raw(FakeTool(), "--version");
        Assert.Equal(["--version"], plan.Arguments);
    }

    [Fact]
    public void Null_Tool_Throws_For_Every_Verb()
    {
        Assert.Throws<ArgumentNullException>(() => Vitest.Run(null!));
        Assert.Throws<ArgumentNullException>(() => Vitest.Watch(null!));
        Assert.Throws<ArgumentNullException>(() => Vitest.Related(null!, s => s.AddFile("a")));
        Assert.Throws<ArgumentNullException>(() => Vitest.Related(FakeTool(), (Action<VitestRelatedSettings>)null!));
        Assert.Throws<ArgumentNullException>(() => Vitest.Bench(null!));
        Assert.Throws<ArgumentNullException>(() => Vitest.Typecheck(null!));
        Assert.Throws<ArgumentNullException>(() => Vitest.Raw(null!, "--help"));
    }

    [Fact]
    public void Many_Reporters_Preserve_Order_Under_Random_Names()
    {
        // Vitest's reporter chain executes in declaration order, so the
        // order across many --reporter flags is observable.
        var faker = new Faker();
        var reporters = Enumerable.Range(0, 6).Select(_ => faker.Random.AlphaNumeric(8)).ToArray();
        var plan = Vitest.Run(FakeTool(), s =>
        {
            foreach (var r in reporters) s.AddReporter(r);
        });
        var observed = new List<string>();
        for (var i = 0; i < plan.Arguments.Count - 1; i++)
            if (plan.Arguments[i] == "--reporter") observed.Add(plan.Arguments[i + 1]);
        Assert.Equal(reporters, observed);
    }
}
