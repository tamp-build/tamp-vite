using System.IO;
using Tamp;
using Tamp.Vite.V5;
using Xunit;
using Xunit.Abstractions;

namespace Tamp.Vite.V5.IntegrationTests;

/// <summary>
/// Exercises Vite + Vitest wrappers against real CLIs. Stages a tiny
/// vite project with a vitest spec per fixture.
/// </summary>
public sealed class ViteIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly AbsolutePath _workdir;

    public ViteIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        _workdir = AbsolutePath.Create(Path.Combine(Path.GetTempPath(), $"tamp-vite-it-{Guid.NewGuid():N}"));
        Directory.CreateDirectory(_workdir.Value);

        File.WriteAllText(Path.Combine(_workdir.Value, "package.json"), """
            {
              "name": "tamp-vite-smoke",
              "private": true,
              "type": "module"
            }
            """);

        File.WriteAllText(Path.Combine(_workdir.Value, "vite.config.js"), """
            export default { root: '.', build: { emptyOutDir: true } };
            """);

        File.WriteAllText(Path.Combine(_workdir.Value, "index.html"), """
            <!doctype html><html><body><script type="module" src="./main.js"></script></body></html>
            """);

        File.WriteAllText(Path.Combine(_workdir.Value, "main.js"), """
            export const greet = (name) => `hello, ${name}`;
            console.log(greet('world'));
            """);

        File.WriteAllText(Path.Combine(_workdir.Value, "main.test.js"), """
            import { describe, it, expect } from 'vitest';
            import { greet } from './main.js';
            describe('greet', () => {
              it('returns a friendly greeting', () => {
                expect(greet('vite')).toBe('hello, vite');
              });
            });
            """);

        File.WriteAllText(Path.Combine(_workdir.Value, "vitest.config.js"), """
            export default { test: { include: ['**/*.test.js'] } };
            """);
    }

    private static string? ResolveOnPath(string baseName)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        var names = OperatingSystem.IsWindows()
            ? new[] { $"{baseName}.cmd", $"{baseName}.exe", $"{baseName}.bat", $"{baseName}.ps1", baseName }
            : new[] { baseName };
        foreach (var dir in pathEnv.Split(Path.PathSeparator))
        {
            if (string.IsNullOrEmpty(dir)) continue;
            foreach (var n in names)
            {
                var c = Path.Combine(dir, n);
                if (File.Exists(c)) return c;
            }
        }
        return null;
    }

    private static Tool ResolveTool(string name)
    {
        var p = ResolveOnPath(name)
            ?? throw new InvalidOperationException($"{name} not found on PATH. Install: npm i -g vite@5 vitest@1");
        return new Tool(AbsolutePath.Create(p));
    }

    /// <summary>
    /// Vite/Vitest resolve their plugins + node_modules from CWD. Since
    /// the fixture is bare, set NODE_PATH to the global node_modules so
    /// fallback resolution finds them.
    /// </summary>
    private static string? GetGlobalNodeModules()
    {
        var npm = ResolveOnPath("npm");
        if (npm is null) return null;
        var psi = new System.Diagnostics.ProcessStartInfo(npm)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        psi.ArgumentList.Add("root");
        psi.ArgumentList.Add("-g");
        using var p = System.Diagnostics.Process.Start(psi);
        if (p is null) return null;
        var stdout = p.StandardOutput.ReadToEnd().Trim();
        p.WaitForExit();
        return string.IsNullOrEmpty(stdout) ? null : stdout;
    }

    private CommandPlan WithNodePath(CommandPlan plan)
    {
        var globalRoot = GetGlobalNodeModules();
        if (globalRoot is null) return plan;
        var env = new Dictionary<string, string>(plan.Environment) { ["NODE_PATH"] = globalRoot };
        return plan with { Environment = env };
    }

    private CaptureResult Run(CommandPlan plan)
    {
        _output.WriteLine($"$ {plan.Executable} {string.Join(' ', plan.Arguments)}");
        var result = ProcessRunner.Capture(plan);
        foreach (var line in result.Lines)
            _output.WriteLine($"  [{line.Type}] {line.Text}");
        _output.WriteLine($"  → exit {result.ExitCode}");
        return result;
    }

    [Fact]
    public void Vite_Build_Produces_Dist_Directory()
    {
        var tool = ResolveTool("vite");
        var plan = WithNodePath(Vite.BuildProject(tool, s => s
            .SetLogLevel("error")
            .SetWorkingDirectory(_workdir.Value)));
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        Assert.True(Directory.Exists(Path.Combine(_workdir.Value, "dist")));
    }

    [Fact]
    public void Vite_Raw_Version_Returns_Version_String()
    {
        var tool = ResolveTool("vite");
        var plan = WithNodePath(Vite.Raw(tool, "--version"));
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        var combined = result.StdoutText + result.StderrText;
        Assert.Matches(@"vite/\d+\.\d+\.\d+", combined);
    }

    [Fact]
    public void Vitest_Run_Passes_On_Green_Spec()
    {
        var tool = ResolveTool("vitest");
        var plan = WithNodePath(Vitest.Run(tool, s => s
            .SetSilent()
            .SetWorkingDirectory(_workdir.Value)));
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void Vitest_Run_With_TestNamePattern_Filter_Passes()
    {
        // vitest 1.x: --testNamePattern filters by full test name (the
        // `it` description). Our spec's "returns a friendly greeting"
        // is matched by ".*friendly.*".
        var tool = ResolveTool("vitest");
        var plan = WithNodePath(Vitest.Run(tool, s => s
            .SetTestNamePattern(".*friendly.*")
            .SetSilent()
            .SetWorkingDirectory(_workdir.Value)));
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
    }

    public void Dispose()
    {
        try { Directory.Delete(_workdir.Value, recursive: true); } catch { }
    }
}
