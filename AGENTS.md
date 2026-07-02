# Repository Guidelines

## Project Structure & Module Organization

This repository is a .NET solution for NCalc. The main solution is `NCalc.slnx`.
Production code lives in `src/`: core evaluation logic is in `NCalc.Core`, expression domain types in `NCalc.Domain`, parsing in `NCalc.Parser`, dependency injection support in `NCalc.DependencyInjection`, and optional integrations in `src/Plugins/*`.

Tests and development utilities live in `test/`: `NCalc.Tests` contains the automated test suite, `NCalc.Benchmarks` contains BenchmarkDotNet benchmarks, and `NCalc.Play` is a small playground app. Documentation is under `docs/` and is built with DocFX. Shared build settings are in `Directory.Build.props`, `src/Directory.Build.props`, and `global.json`.

## Build, Test, and Development Commands

Don't narrate your actions. Only report when an operation fails or requires my attention. Do not print progress updates like "running build" or "waiting for completion".

- `dotnet restore NCalc.slnx`: restores all solution dependencies.
- `dotnet build NCalc.slnx`: builds all projects using the pinned .NET SDK from `global.json`.
- `dotnet test --project test/NCalc.Tests/NCalc.Tests.csproj`: runs the TUnit test suite through Microsoft.Testing.Platform. The newer `dotnet test` CLI used here requires the project to be passed with `--project`.
- `dotnet tool update -g docfx` then `docfx docs/docfx.json --serve`: builds and serves documentation locally.

## Coding Style & Naming Conventions

Use C# with 4-space indentation, spaces instead of tabs, sorted `System` directives, and trimmed trailing whitespace as defined in `.editorconfig`. Most source projects enable nullable reference types and implicit usings; tests currently disable nullable. Production projects treat warnings as errors, so fix analyzer warnings instead of suppressing them unless there is a clear reason. Use PascalCase for public types and members, camelCase for locals and parameters, and keep test method names descriptive, for example `ShouldCompareNullableToNonNullable`.

## Testing Guidelines

Tests use TUnit in `test/NCalc.Tests` and run through Microsoft.Testing.Platform. Add tests near the feature area being changed, and place reusable fixtures or data in `Fixtures/` or `TestData/` when appropriate. Prefer behavior-focused test names beginning with `Should...` when adding new cases. Run `dotnet test --project test/NCalc.Tests/NCalc.Tests.csproj` before submitting changes; run the full solution build when public APIs, package projects, or plugins are touched.

Do not use the usual VSTest-style `--filter` option with this test project; it is reported as an unknown option. To inspect available tests, use `dotnet test --project test/NCalc.Tests/NCalc.Tests.csproj --list-tests --no-progress`. For focused runs, use Microsoft.Testing.Platform/TUnit-supported filters such as `--treenode-filter` or `--filter-uid`, and confirm the command runs a non-zero number of tests.

## Commit & Pull Request Guidelines

Recent commits use concise imperative summaries, often with a scope or category, for example `Refactor: Split parser and domain into dedicated assemblies (#560)` or `Update packages (#556)`. Keep commits focused and mention breaking changes explicitly. Pull requests should describe the change, explain user-visible behavior or API impact, link related issues, and include test results. Add screenshots only for documentation or visual site changes.
