# MathemaSharp — Open points

## Current inverse trigonometric span continuation

`Asin`, `Acos` and `Atan` now accept `ReadOnlySpan<T>` and `Span<T>`.
Loops preserve scalar bits on all six IEEE formats, including signed zero, domain
boundaries and adjacent samples, NaN and infinities. Exact in-place is supported;
short destinations and both shifted overlaps are rejected before writes. Input and
extra output elements remain unchanged; disjoint slices of shared storage work.
Warmed double calls allocate zero managed bytes on this runtime. Custom T may
allocate or throw, with no rollback of earlier writes. Release baseline: 599 passed
(Core 292); after: 606 passed (Core 299), zero failures/skips. Build: zero warnings/errors.
Core preview.29 is source-only. Post-change ecosystem gate passed 33/33 with
`--no-images`: `D:\Dev\BuildArtifacts\ecosystem-validation\20261101-204038`.
Core/12 added only six generated inverse-trigonometric declarations; code cells
were unchanged. Complex shapes and independent accuracy remain pending.
No builtin migration; step 1 remains partial.
Gate handoff: `D:\Dev\BuildArtifacts\handoff\core-step1-inverse-trig-gate`.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-inverse-trig-spans`.

## Current scalar inverse trigonometric continuation

`GenericMath.Asin`, `Acos` and `Atan` now use `ITrigonometricFunctions<T>`
without conversion to double. Asin/Acos have real domain [-1, 1] and return NaN
outside it; Atan accepts all real values and signed infinities. Six IEEE formats
verify signed zero, NaN, infinities, subnormals, ordinary values and adjacent
representable values on both sides of the domain boundaries. Tests compare BCL
bits on the current runtime, not independent mathematical accuracy. Release solution
baseline: 593 passed (Core 286); after: 599 passed (Core 292), zero failures/skips.
Build: zero warnings/errors. Core preview.28 is source-only, not packaged.
Span and complex overloads, independent accuracy and the post-change ecosystem gate
remain pending. No language builtin migration; common-core step 1 remains partial.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-inverse-trig-scalar`.

## Current logarithmic span continuation

`Log2`, `Log10` and `LogP1` now also accept `ReadOnlySpan<T>` and `Span<T>`.
Each loop calls its scalar facade, preserving LogP1 negative zero. Six IEEE formats
verify bit parity, domains, near-zero samples, in-place, input immutability and output
tails. Empty input is a no-op; short output and both shifted overlaps are rejected
before writes. Warmed double calls allocate zero managed bytes on this runtime;
custom T may allocate or throw without rollback. Core baseline 279 → 286 passed;
solution after 593 passed, zero failures/skips. Core preview.27 is source-only,
not packaged. Full ecosystem gate passed 33/33 with `--no-images` on rerun:
`D:\Dev\BuildArtifacts\ecosystem-validation\20261101-185621`. The first gate
(`20261101-184002`) failed in baseline: Core/12 exited 143 with no C# diagnostic.
The isolated notebook and full rerun passed without code changes; interruption cause
remains unknown, not a demonstrated fix. Core/12 added only six generated API lines.
Complex shapes and independent accuracy remain pending; no builtin migration,
step 1 remains partial.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-logarithmic-spans`.
The following sections record earlier verified tranches.

## Current scalar logarithmic variants

`Log2`, `Log10` and `LogP1` use `ILogarithmicFunctions<T>` without conversion.
Log2/Log10 forward directly; LogP1 returns zero inputs unchanged and forwards others
without forming 1+x. First tests exposed binary BCL LogP1(-0) returning +0; the
facade was corrected without weakening signed-zero assertions. Six IEEE formats
cover domains, special values, subnormals, near-zero and boundary-adjacent samples.
Core baseline 273 → 279 passed; solution after 586 passed, zero failures/skips.
Core preview.26 is source-only, not packaged. Span/complex shapes, independent
accuracy and post-change full gate remain pending; no language builtin migration.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-logarithmic-scalar`.
The following sections record earlier verified tranches; step 1 remains partial.

## Current exponential variant span continuation

`Exp2`, `Exp10` and `ExpM1` now also accept `ReadOnlySpan<T>` and `Span<T>`.
Each loop calls the scalar facade, including ExpM1's signed-zero correction.
Six IEEE formats verify scalar bit parity, in-place, input immutability, output tails,
empty inputs, short destinations and both partial-overlap directions before writes.
Warmed double calls allocate zero managed bytes on this runtime; custom T may
allocate or throw, with no rollback of earlier writes. Core baseline 266 → 273
passed; solution after 580 passed, zero failures/skips. Core preview.25 is source-only,
not packaged. Post-change ecosystem gate passed 33/33 with `--no-images`:
`D:\Dev\BuildArtifacts\ecosystem-validation\20261101-175059`. Core/12 added only
six generated exponential declarations; notebook code cells were unchanged.
Complex shapes and independent accuracy remain pending. No builtin migration;
step 1 remains partial.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-exponential-spans`.
The following sections record earlier verified tranches.

## Current scalar exponential variants

`Exp2`, `Exp10` and `ExpM1` now use `IExponentialFunctions<T>`. Exp2/Exp10 forward
directly; ExpM1 returns zero inputs unchanged and forwards all others. The first
new test run exposed loss of negative zero in the binary BCL ExpM1 implementations;
the facade was corrected, retaining the signed-zero assertion. No Exp(x)-1 rewrite.
Six formats cover special values, subnormals, near-zero and large finite samples.
Core baseline 260 → 266 passed; solution after 573 passed. Core preview.24 is
source-only, not packaged. Span/complex shapes, independent accuracy and post-change
full ecosystem gate remain pending; no language builtin migration.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-exponential-scalar`.
The following sections record earlier verified tranches.

## Current inverse hyperbolic span continuation

`Asinh`, `Acosh` and `Atanh` now also accept `ReadOnlySpan<T>` and `Span<T>`.
The loops preserve scalar bits on six IEEE formats, including domain boundaries
and their adjacent values. Exact in-place is supported; partial overlaps in both
directions and short destinations are rejected before writes. Empty input is a
no-op; extra output elements and input storage remain unchanged. Warmed double
calls allocate zero managed bytes on this runtime; custom T may allocate or throw.
Core baseline 253 → 260 passed; solution after 567 passed, zero failures/skips.
Core preview.23 is source-only, not packaged. Post-change ecosystem gate passed
33/33 with `--no-images`: `D:\Dev\BuildArtifacts\ecosystem-validation\20261101-172318`.
Only six generated inverse-hyperbolic declarations changed in Core/12; code cells
were unchanged. Independent accuracy and complex shapes remain pending;
step 1 remains partial.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-inverse-hyperbolic-spans`.
The following sections record earlier verified tranches.

## Current inverse hyperbolic scalar continuation

`GenericMath.Asinh<T>`, `Acosh<T>` and `Atanh<T>` directly forward to the BCL with
`IHyperbolicFunctions<T>` constraints. No conversion or language builtin change.
Six IEEE formats verify signed zeros, NaN, infinities, subnormals, domain boundaries
and adjacent representable values. Acosh rejects x < 1 with NaN; Atanh returns
signed infinities at ±1 and NaN outside [-1, 1], as tested on this runtime.
Core baseline 247 → 253 passed; solution after 560 passed. Core preview.22 is
source-only, not packaged. Inverse spans, complex shapes, independent accuracy and
the post-change full ecosystem gate remain pending; step 1 remains partial.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-inverse-hyperbolic`.
The following sections record earlier verified tranches.

## Current hyperbolic span continuation

`Sinh`, `Cosh` and `Tanh` now also accept `ReadOnlySpan<T>` and `Span<T>`.
They reuse destination validation: exact in-place is supported; short output and
partial overlap in either direction are rejected before writes. Empty input is a
no-op; extra output elements are preserved. Six IEEE formats match scalar bits,
including signed zeros, nonfinite values, subnormals and finite overflow samples.
Warmed double calls allocated zero managed bytes on the tested runtime; custom T
may allocate or throw, and element failures do not roll back earlier writes.
Core baseline 240 → 247 passed; solution after 554 passed. Core preview.21 is
source-only, not packaged. Post-change full ecosystem gate passed 33/33 with
`--no-images`: `D:\Dev\BuildArtifacts\ecosystem-validation\20261101-165215`.
Only six generated API declarations changed in Core/12; code cells stayed unchanged.
Independent accuracy and complex shapes remain pending. The following scalar section
is historical evidence; step 1 is not complete.
Logs: `D:\Dev\BuildArtifacts\handoff\core-step1-hyperbolic-spans`.

## Common-core continuation: scalar hyperbolic functions

`GenericMath.Sinh<T>`, `Cosh<T>` and `Tanh<T>` are direct BCL fast-profile forwards
with `IHyperbolicFunctions<T>` constraints. No language builtin has been switched.
Six IEEE formats are tested for signed zeros, NaN, infinities, subnormals and finite
samples (including overflow), with bit parity against T's BCL implementation.
Core baseline: 234 passed; after: 240 passed. Solution after: 547 passed.
The independent hyperbolic accuracy corpus, span/complex shapes and post-change
full ecosystem gate remain pending. Core source version is preview.20; no package
was produced. Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-hyperbolic-resume`.


Suite-wide points are in [../ScienceSharp/OPENPOINTS.md](../ScienceSharp/OPENPOINTS.md). The status log is [ROADMAP.md](ROADMAP.md);
the history is [CHANGELOG.md](CHANGELOG.md).

| # | Point | Status |
| --- | --- | --- |
| MA-25 | F# source runner validates 624 C#/F# rows (1 integration test, solution 541/541). Same common algorithms/runtime only. PySharp cancellation fix reviewed against CPython 3.12.10 and committed as 9765f45; consolidation gate 33/33 (20261101-133722). Exact final source heads/acceptance evidence in authorized-final bundle. F# library/units of measure, independent accuracy and other platforms pending. SDK FSharp.Core copy-local is not a production Core dependency. | partial, source milestone consolidated |
| MA-24 | R/F/hbar regenerated fromexactinputs,formulas/dimensions/finitehbar precision explicit (Core234/234);NISTellipsisnotpromotedtofullvalue. Post-changegate passed33/33 (20261101-080517);remainingderived/measured/non-SI/F# pending. | partial |
| MA-23 | G/alpha/mu0 measuredsubset sourced withuncertainty (Core226/226);mu0 explicitlyMeasured,catalog7SI+3measured. Uncertaintyunderflow rejected. Post-changegate passed33/33 (20261101-072631);othermeasured/derived/non-SI/F# pending. | partial |
| MA-22 | Seven exact SI definitions generated withtypedGet/double facades (Core219/219). Nonfinite/zero-underflow values rejected;ExactSi not a guarantee of exact T representation. Post-change gate passed33/33 (20261101-065718);measured/derived catalogs,non-SI conversion andF# pending. | partial |
| MA-21 | CoherentSI dimensions implemented/tested (Core210/210),293 CODATA rows recognized,62 explicit unsupported. Post-change gate passed33/33 (20261101-004757);non-SI/scales/prefixes andtyped physical API pending. TryParsefalse/defaultvector is not a dimensionless classification. | partial |
| MA-20 | Checksummed NIST CODATA2022 source imported:355 records,exact/truncated flags preserved (Core183/183). Post-import gate passed33/33 (20261031-234405);typed physical definitions,SI dimensions/units andderived high-precision formulas pending. Printed ellipses must not become exact values. | partial |
| MA-19 | Mathematical definition metadata/catalog implemented for13 generated values (Core173/173):immutable lookup and six-type conversion. Post-metadata gate passed33/33 (20261031-225534);SI/CODATA records and full physical dimensional model remain pending. Measurement uncertainty0 is not decimal approximation error. | partial |
| MA-18 | MathConstants<T> all13 mathematical values listed in COMMON-CORE3.3 generated;78 IEEE format encodings tested (Core164/164). Gate after latest7 passed33/33 (20261031-221915). Complete catalog/other mathematical and physical constants, custom >100-digit formats pending; full post-change gate passed33/33 (20261031-214535). External pinned mpmath generator dependency only. | partial |
| MA-17 | Integral rounding spans verified across seven formats plus warmed-double allocation (Core158/158). Full gate after Round/integral spans passed33/33 (20261031-211557); custom T allocation/exception guarantees excluded. | partial |
| MA-16 | Explicit Round span overloads verified across seven formats/all modes (Core150/150). Digits are type-specific, including oversized positive digits; no universal cap. Post-Round-span gate passed33/33 (20261031-211557); remaining shapes pending. | partial |
| MA-15 | Trig and elementary span shapes implemented: six IEEE scalar-bit comparisons, domain cases, in-place, pre-write overlap/length validation and warmed-double allocation (Core143/143). Post-span full gate passed33/33 (20261031-191731); other spans and complex overloads pending; custom T allocation/exception behaviour not guaranteed. | partial |
| MA-14 | Common-core step 1: scalar Round and Floor/Ceiling/Truncate (seven formats), Sin/Cos/Tan and Sqrt/Exp/Log (six IEEE formats) implemented (Core 126/126); trig/elementary post-change gate passed 33/33 (20261031-133944); scalar rounding post-change gate passed 33/33 (20261031-094510); 40 independent double Sqrt/Exp/Log references pass (max measured 0 ULP, Core 129/129); other scalar functions/constants, trig/other-format reference corpus pending. Post-oracle gate failed in MatSharp warning-output test (20261031-153903); unchanged rerun 166/166 is not a fix. Consolidation blocked pending diagnosis, possible MatSharp M-1 shared warning sink. Digit range delegates to T; no negative-digit extension or language builtin migration. | partial |
| MA-1 | **Perimeter** (R10): classify notebooks, legacy helpers and assets still left; migrate the relevant ones or justify keeping them. | open |
| MA-2 | **Precision and formats** (R06): a successful build does not prove that every API supports every format (Decimal32/64/128). The supported platform matrix is not decided. | open |
| MA-3 | **Performance** work is deferred until correctness, contracts and notebooks are settled. | deferred |
| MA-4 | CLI `mathema` exists (stats, acf, diff, forecast on one CSV column). No ARIMA, STL, Kalman, FFT or matrix commands yet. | done, extend later (suite G9) |
| MA-5 | The ecosystem gate lives here (`ops/ValidateEcosystem.cs`, 32 steps plus `skip-report`, all exit 0). It takes over 10 minutes and rewrites notebooks in place. | known |
| MA-6 | Notebook kernelspec metadata is now `jupytersharp` for notebooks saved by JupyterSharp and `jupyternet` for older ones; the structure test accepts both. Normalise. | open |
| MA-7 | `scripts/run-notebooks.ps1`, the `ops` scripts, the notebooks and the code were translated to English on 2026-10-30 and the kernel is called JupyterSharp; `.config/dotnet-tools.json` no longer pins `jupyternet.cli`. Some `ops/*.proj` descriptions may still say "jupyternet". | partly done |
| MA-8 | 1507 PySharp tests pass in the gate with 13 skipped (12 PostgreSQL, 1 model download). | accepted |
| MA-9 | `src/imports/*` snapshots: origin not audited (suite L3). | open |
| MA-10 | The documents are now in English (suite G13), but the status sections still stack on top of older ones ("the first section prevails"). Rewrite as one current state. | open |
| MA-11 | The accent in the brand "MàthemaSharp" versus the package/namespace "MathemaSharp" is a convention that every document must keep. | known |
| MA-12 | **Gate output is reproducible.** The 37 notebooks are committed with their outputs and a gate run leaves the working tree clean: PlotSharp `preview.2` names SVG clip paths and gradients by a hash of the chart instead of a random GUID. `ops/validation` (gate evidence, 14 files) is ignored, not tracked; each run archives a copy under `BuildArtifacts\ecosystem-validation`. Two Ecosystem notebooks load the packaged PlotSharp by path (`consumer-cache/plotsharp/<version>/...`): when the pinned version in `ops/VerifyNet11Packages.cs` moves, change that path in `01-signal-spectrum` and `03-indexed-charts` too, or they keep running the stale DLL. | done |
| MA-13 | The logo that read "MàthemaNet" (the old name) was removed from the tree on 2026-10-30 and replaced by the ScienceSharp logo set; the old image stays in the history (the author's own picture). The README logo block is generated, see `ScienceSharp/branding`. | done |
