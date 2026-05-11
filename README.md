# Tamp.Vite

Vite + Vitest CLI wrapper for [Tamp](https://github.com/tamp-build/tamp).

| Package | Vite | Vitest | Status |
|---|---|---|---|
| [`Tamp.Vite.V5`](src/Tamp.Vite.V5) | 5.x | 1.x | preview |

Requires `Tamp.Core ≥ 1.0.3`.

## Why both in one package

Vitest's major tracks Vite's: Vitest 1 ↔ Vite 5, Vitest 2 ↔ Vite 6.
Per ADR 0002 the V-pin tracks the headline tool (Vite), and Vitest
gets a sub-namespace.

## Install

```xml
<PackageVersion Include="Tamp.Vite.V5" Version="0.1.0" />
```

```xml
<PackageReference Include="Tamp.Vite.V5" />
```

## Verbs

### `Tamp.Vite.V5.Vite`

| Verb | Notes |
|---|---|
| `Dev` | Dev server. `--host`, `--port`, `--strictPort`, `--https`, `--open`, `--cors`. |
| `BuildProject` | Production build. `--outDir`, `--assetsDir`, `--ssr`, `--sourcemap`, `--minify`, `--manifest`, `--ssrManifest`, `--emptyOutDir`, `--watch`, repeated `--target`. |
| `Preview` | Local preview of `dist/`. |
| `OptimizeDeps` | Force pre-bundle of deps. |
| `Raw` | Escape hatch. |

Common flags (all verbs): `--config`, `--mode`, `--base`,
`--logLevel`, `--clearScreen` / `--no-clearScreen`, `--force`,
`--debug [category]`, `--filter`.

### `Tamp.Vite.V5.Vitest`

| Verb | Notes |
|---|---|
| `Run` | Single-shot test run. Pattern via `--testNamePattern`, `--changed [since]`, positional file patterns. |
| `Watch` | Interactive mode. `--ui`, `--api [port]`. |
| `Related` | Run only tests related to given files. |
| `Bench` | Benchmark mode. |
| `Typecheck` | tsc-via-vitest. |
| `Raw` | Escape hatch. |

Common flags (all verbs): `--config`, `--root`, `--dir`, `--mode`,
`--silent`, `--bail`, `--pool`, repeated `--reporter`, `--outputFile`,
`--passWithNoTests`, `--logHeapUsage`, `--coverage` /
`--coverage.provider` / `--coverage.reporter` (repeatable) /
`--coverage.reportsDirectory`.

## Quick example — HoldFast frontend CI

```csharp
using Tamp;
using Tamp.Vite.V5;

[NuGetPackage("vite", UseSystemPath = true)]
readonly Tool ViteTool = null!;

[NuGetPackage("vitest", UseSystemPath = true)]
readonly Tool VitestTool = null!;

Target Build => _ => _.Executes(() =>
    Vite.BuildProject(ViteTool, s => s
        .SetMode("production")
        .SetOutDir("dist")
        .SetEmptyOutDir(true)
        .SetSourcemap("hidden")
        .SetWorkingDirectory(RootDirectory / "frontend")));

Target Test => _ => _.Executes(() =>
    Vitest.Run(VitestTool, s => s
        .SetCoverage()
        .SetCoverageProvider("v8")
        .AddCoverageReporter("text")
        .AddCoverageReporter("lcov")
        .AddReporter("default")
        .AddReporter("junit")
        .SetOutputFile("junit=./TestResults/junit.xml")
        .SetWorkingDirectory(RootDirectory / "frontend")));
```

## Releasing

See [MAINTAINERS.md](MAINTAINERS.md).
