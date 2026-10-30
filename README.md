<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="branding/logo-dark.svg">
    <img alt="MàthemaSharp" src="branding/logo.svg" height="72">
  </picture>
</p>

# MàthemaSharp

> **Part of [ScienceSharp](../ScienceSharp)** — the Science suite. Targets **.NET 11**. Status: preview, not published. License: MIT.
> **Form:** NuGet libraries (`MathemaSharp.Core`, `.LinearAlgebra`, `.TimeSeries`, `.Quantum`, `.NDSharp`). Global tool `mathema` (stats, acf, diff, forecast on a CSV column). Also hosts the ecosystem gate (`ops/ValidateEcosystem.cs`).

| [README](README.md) | [USAGE](USAGE.md) (install first) | [ARCHITECTURE](ARCHITECTURE.md) | [ROADMAP](ROADMAP.md) | [OPENPOINTS](OPENPOINTS.md) |
| --- | --- | --- | --- | --- |

A family of mathematics libraries based on Generic Math. The brand is written **MàthemaSharp**, with a grave accent on the first a; packages, assemblies and namespaces use **MathemaSharp** without the accent.

## Documentation and history

- [CHANGELOG.md](CHANGELOG.md): reconstructed log of the changes, checks, packages and limits.
- [ROADMAP.md](ROADMAP.md): current state and activities still to be completed.
- [NET11-MIGRATION.md](NET11-MIGRATION.md): detail of the migration and of the ecosystem checks.
- [ARCHITECTURE.md](ARCHITECTURE.md): structure, decisions and API inventory.

For the up-to-date state the first section of the roadmap prevails; the historical counts reported further on describe earlier checks.

## Full verification

```sh
dotnet msbuild ops/validate.proj -t:Baseline
```

On Windows: Release build, 22 selected notebooks and tests, with reports/logs in `ops/artifacts/baseline/`. Last verification: **272 tests passed, 3 suspended**. Explicit exclusions: NDSharp, PySharp and scenario notebooks. Details in [USAGE.md](USAGE.md#full-baseline--one-command).

## Libraries

| Library | Contents | Usage | Reference |
|---|---|---|---|
| MathemaSharp.Core | Generic Math, precision, SIMD and DFT | [Usage](USAGE.md#mathemasharpcore--usage) | [Reference](ARCHITECTURE.md#mathemasharpcore--reference) |
| MathemaSharp.LinearAlgebra | Matrices, QR, LU, norms and linear systems | [Usage](USAGE.md#mathemasharplinearalgebra--usage) | [Reference](ARCHITECTURE.md#mathemasharplinearalgebra--reference) |
| MathemaSharp.Quantum | Generic complex states and operators | [Usage](USAGE.md#mathemasharpquantum--usage) | [Reference](ARCHITECTURE.md#mathemasharpquantum--reference) |
| MathemaSharp.TimeSeries | Statistics and time-series forecasting | [Usage](USAGE.md#mathemasharptimeseries--usage) | [Reference](ARCHITECTURE.md#mathemasharptimeseries--reference) |
| MathemaSharp.NDSharp | ndarray bridge, not yet fully generic | [Usage](USAGE.md#mathemasharpndsharp--usage) | [Reference](ARCHITECTURE.md#mathemasharpndsharp--reference) |

## Structure

```text
src/MathemaSharp.slnx
src/libs/MathemaSharp.<lib>/
notebooks/Core/
notebooks/LinearAlgebra/
notebooks/Quantum/
notebooks/TimeSeries/Scenarios/
notebooks/NDSharp/
notebooks/shared/
refs/branding/
```

Usage and Reference are organised by library in the root documents, without duplicating the catalogue. The `src/imports/NDSharp` snapshot remains unchanged and outside the solution.

## Getting started

```sh
dotnet build src/MathemaSharp.slnx -c Release
dotnet test src/MathemaSharp.slnx
pwsh scripts/run-notebooks.ps1
dotnet run --project src/apps/MathemaSharp.Docs/MathemaSharp.Docs/MathemaSharp.Docs.csproj
```

The environment requires the .NET 11 SDK indicated by `global.json` and the feeds in `nuget.config`. The kernels already on .NET 11 support the new Decimal32/64/128 according to their respective constraints. A few projects remain net10.0: this reorganisation does not change their target. Not all the imported kernels are converted to Generic Math.

The renaming is a **breaking change**: update `using Numbers.*`, project references and assembly names to `MathemaSharp.*`. The notebooks were moved and the previous outputs cleared: they must be rerun. The documentation site has not been verified in the browser.

## License

MIT, Copyright (c) 2026 Marco Parenzan; see [LICENSE](LICENSE). This code was developed with the assistance of generative AI tools. Third-party material, where present, keeps its own license; see THIRD-PARTY-NOTICES.md in `NDSharp/src/NDSharp.Plot` for the matplotlib-derived parts.

## A note on licensing

This is a development exercise, not a commercial effort and not a way to take anything from anyone. It tries to show that .NET and C# can do the same kind of
scientific work as Python, in the hope that someone will try it. Its licensing, including the third-party material it adapts or credits, was reviewed with the help
of generative AI, with the aim of the greatest respect for the rights of the owners. That is not legal advice, and some points are still open: they are listed in
[LICENSING.md](../ScienceSharp/LICENSING.md). If you own material that is used here and have a concern, please open an issue: it will be discussed and, if needed, fixed or removed.
