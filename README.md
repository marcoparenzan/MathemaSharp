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

## Coherent SI dimension parsing

SiDimensions and SiUnitDimensions model seven base exponents and parse coherent
SI products with integer powers. CODATA coverage293 supported/62 unsupported rows;
unknown units are not inferred dimensionless. No scale conversion/prefixes/non-SI
or parenthesis/slash support. Core210/210,no package or typed physical catalog yet.

## Checksummed CODATA source import

NIST CODATA2022 imported as355 source records,81 exact markers and62 printed
truncated values. Parser retains text/units and flags; no typed physical catalog or
SI dimension inference yet. Pinned SHA256 protects source changes. Core183/183.
Generated data under ops/data; raw snapshot external. No package/builtin change.

## Mathematical definition catalog (source branch)

MathematicalConstants.Definitions/Get expose13 immutable definitions: symbol,
Mathematical status,dimensionless SI vector,generated decimal value,source/precision
and zero measurement uncertainty. Value<T>() parses invariantly,matching typed fields.
Core173/173;no physical/CODATA entries or package. Zero uncertainty is not decimal approximation error.

## Mathematical family values covered (source branch)

EulerGamma/GoldenRatio/Catalan/Apery complete the13 mathematical values listed in
COMMON-CORE3.3. Seventy-eight six-format rounding references pass (Core164/164).
All values generated at100 digits; full metadata catalog and physical constants
remain unimplemented, and no new package was produced.

## Base-logarithm constants (source branch)

Sqrt3/Log2E/Log10E added: nine generated100-digit values and54 format references.
Log2E/Log10E are computed at high precision before rounding, not from rounded Ln2/
Ln10. Core164/164 (existing format tests expanded), no package or builtin change.

## Additional mathematical constants (source branch)

MathConstants<T> now also exposes Sqrt2/Ln2/Ln10. Six generated100-digit values,
36 rounded format references, Core164/164. Decimal quantization follows magnitude
exponent; prior Pi/Tau/E references remain protected. No new package or builtin.

## Generated mathematical constants (source branch)

MathConstants<T>.Pi/Tau/E parse generated100-digit values once per floating type.
Mpmath1.3.0 calculations agree at160/200 digits; six IEEE rounded references pass
(Core164/164). Tau is generated independently, not twice rounded Pi. No physical
catalog or other mathematical constants yet; no new package.

## Integral rounding spans (source branch)

Floor/Ceiling/Truncate now have scalar-equivalent span overloads with exact in-place
and pre-write overlap/length validation. Seven formats plus warmed-double allocation
check pass (Core158/158). No new package or interpreter builtin.

## Explicit rounding spans (source branch)

Round span overloads require RoundingMode, optionally digits, with exact in-place
and pre-write destination/policy validation. Seven formats pass scalar-bit parity
for every mode; Core150/150. Digits delegate to T: huge positive digits can return
unchanged values on IEEE types, while decimal rejects them. No package/builtin.

## Elementary spans (source branch)

Sqrt/Exp/Log now share the span contract: scalar-equivalent bits, exact in-place,
pre-write bounds/overlap validation and untouched output tail. Six IEEE format tests
and warmed-double zero-allocation check pass; Core143/143. No new package/builtin.

## Trigonometric spans (source branch)

Sin/Cos/Tan now accept ReadOnlySpan input and Span destination, using the same
scalar operations. Exact in-place allowed; partial overlap/short destinations
rejected before writing. Six IEEE formats pass bit parity; warmed double calls
allocate zero managed bytes in the test. Core136/136, no new package or builtin.

## Independent double reference corpus

Forty finite Sqrt/Exp/Log samples are generated from exact binary64 inputs using
Python standard-library decimal at 100/200 digits, requiring stable reference bits.
Maximum measured error is 0 ULP on the recorded Windows runtime; acceptance tests
allow at most 1 ULP for this sample corpus. Core suite 129/129. This is not a universal
accuracy bound; mpmath, trig and other-format external references remain pending.

## Common-core elementary forwards (source branch)

GenericMath now also exposes Sqrt, Exp and Log with the weakest root/exponential/
logarithmic interface constraints. Six IEEE types pass domain, special-value and
same-BCL byte-parity tests; Core suite is 126/126. No external accuracy claim or
language builtin migration. New source API is not packaged.

## Common-core trigonometric forwards (source branch)

`GenericMath.Sin`, `Cos` and `Tan` now forward to T's BCL implementation using
`ITrigonometricFunctions<T>`. Six IEEE formats pass byte-parity and special-value
checks (Core 120/120). This verifies forwarding, not external mathematical accuracy
or cross-platform reproducibility. No Python/MATLAB builtin changed.

## Common-core scalar rounding (source branch)

The scalar surface also provides `Floor<T>`, `Ceiling<T>` and `Truncate<T>` as
`IFloatingPoint<T>` BCL forwards. All seven formats pass finite and IEEE special-value
checks; Core tests are now 114/114. No interpreter builtin has been switched.

`MathemaSharp.GenericMath.Round<T>` forwards to BCL generic rounding with explicit
`RoundingMode`: ties-to-even, ties-away, toward zero, negative or positive infinity.
Both integer and decimal-digit overloads use `IFloatingPoint<T>`. Digit ranges are
those of T; negative-digit rounding is not added. Seven-format tests pass; no
language builtin migrated, new package produced or cross-platform claim made.

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
