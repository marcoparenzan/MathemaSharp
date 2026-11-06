# MathemaSharp — Open points

## Current common-core step 2: lane kernels for reductions and scans (2026-11-02)

New library `MathemaSharp.Kernels` 0.1.0-net11-preview.2 (BCL only, no dependency on Core or NDSharp; not packed, not in the feed).
`Lane(Length, Inner, Outer)` describes how a contiguous buffer is visited: a MATLAB column-major dimension and a NumPy row-major axis
give the same triple, so one kernel serves both. Semantic choices are parameters: `NanPolicy` (Propagate, Omit), `SumMethod` (Sequential,
Compensated, Pairwise = `numpy.add.reduce`), `VarianceMethod` (Welford, TwoPass = NumPy), `ddof`, `ScanOp`, `ScanDirection`, the index base is left to the adapters.

| Static class | Methods |
|---|---|
| `Reductions` | `Sum`, `SumOfSquares`, `Product`, `Mean`, `Variance`, `StandardDeviation`, `Range`, `Min`, `Max` (with 0-based indices), `Median`, `Any`, `All`, `CountNonZero` |
| `Scans` | `Scan` (sum, product, max, min; forward or reverse), `Difference` (order n) |

Tests: `tests/MathemaSharp.Kernels.Tests`, 38 cases (six floating-point formats against an independent gather, NaN/infinity/signed-zero rules, the
pairwise algorithm checked against its textbook form for 8–128 elements and above, two-pass variance, strided lanes, empty lanes, contract violations,
allocation-free warm calls). Consumers: the MatSharp bridge (`MatSharpLib.NDSharp`, MATLAB defaults) and `PySharpLib.Numpy` (NumPy defaults).
Benchmarks: `benchmarks/MathemaSharp.Benchmarks/ReductionBenchmarks.cs` (see ARCHITECTURE).

Not done in step 2: mode, quantile, covariance/correlation, gradient, trapezoid, histogram, a selection-based median (it sorts a copy), integer element
types for mean/min/max/median, complex ordering, `nan*` aliases, multi-axis reductions. The kernels are not in `MathemaSharp.Core`'s namespace on purpose.

## Current independent binary power/logarithm corpus

Pow and Log(x,base) now have 78 finite exact-input binary64 mpmath references,
with rounding stable at 160/200 digits and two byte-identical regenerations.
Measured maxima: Pow 0 ULP, LogBase 1 ULP in the ordered-bit metric (including
signed-zero distinction). Mathematical zero is canonical +0 in this oracle;
signed-zero BCL contracts remain tested separately. Acceptance stays 1 ULP.
Run `dotnet run --file ops/VerifyTrigOracle.cs -- --binary` from MathemaSharp;
external pinned mpmath prerequisite is unchanged. Default trig mode was rerun,
with all 110 references unchanged and still passing. No production API change.
Release baseline: 672 passed (Core 365); after: 674 passed (Core 367), zero failures/skips;
build zero warnings/errors. Full post-change gate passed 33/33 with `--no-images`:
`D:\Dev\BuildArtifacts\ecosystem-validation\20261102-001532`. No notebook output changed.
Independent evidence remains sampled/current-platform, not universal correct rounding.
Evidence: `D:\Dev\BuildArtifacts\validation\binary-oracle\report.json` and
`D:\Dev\BuildArtifacts\handoff\core-step1-binary-oracle`.

## Current binary power and arbitrary-base logarithm

`Pow(x, y)` and `Log(x, newBase)` now have scalar and equal-input-length span forms.
Weakest IPowerFunctions/ILogarithmicFunctions constraints forward dedicated BCL calls,
without conversion or log-ratio rewrite. Six IEEE formats verify special-value matrices,
analytic anchors, scalar/span bits, in-place on either/both inputs, pre-write length
and overlap rejection, safe readonly overlap, unchanged tails/inputs and warmed-double
zero managed allocations. Custom T may allocate or throw without rollback.
Release baseline: 665 passed (Core 358); after: 672 passed (Core 365), zero failures/skips.
Build: zero warnings/errors. Core preview.35 is source-only. Full-family gate passed
33/33 with `--no-images`: `D:\Dev\BuildArtifacts\ecosystem-validation\20261101-235736`.
Core/12 added only four generated binary declarations; code cells were unchanged.
Independent accuracy, complex shapes and builtin adoption remain pending;
common-core step 1 is partial.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-binary-math`.

## Current base-two/base-ten small-argument variants

`Exp2M1`, `Exp10M1`, `Log2P1` and `Log10P1` now have scalar and span forms.
Dedicated BCL operations are used; zero inputs are returned unchanged to preserve
sign, matching existing ExpM1/LogP1 policy. Six IEEE formats verify domains, special
values, subnormals, near-zero and boundary samples, scalar/span bit parity, in-place,
untouched tails/inputs and pre-write dimension/overlap rejection. Warmed double
calls allocate zero managed bytes on this runtime; custom T may throw without rollback.
Baseline first run failed an existing inverse-trig allocation assertion (+8000 bytes),
before code changes. Three isolated runs and full solution rerun passed unchanged;
cause remains unknown, not fixed or waived. Green baseline: 652 passed (Core 345).
After: 665 passed (Core 358), zero failures/skips; build zero warnings/errors.
Core preview.34 is source-only. Full-family gate passed 33/33 with `--no-images`:
`D:\Dev\BuildArtifacts\ecosystem-validation\20261101-234051`.
Core/12 added only eight generated declarations; code cells were unchanged.
Independent accuracy, complex shapes and builtin adoption remain pending; step 1 partial.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-base-variants`.

## Current independent binary64 trigonometric accuracy corpus

Seven existing functions (Sin/Cos/Tan/Asin/Acos/Atan/Atan2) now have 110 independent
finite binary64 references, reconstructed from exact input ratios and generated by
external pinned mpmath 1.3.0 at 160/200 digits. Both precisions round to identical
reference bits; two complete regenerations have identical SHA256. Measured maximum
on this runtime: 0 ULP for all seven; acceptance remains 1 ULP, not a universal bound.
Tests consume the frozen fixture without Python. Release baseline: 645 passed
(Core 338); after: 652 passed (Core 345), zero failures/skips; build zero warnings/errors.
First verifier failed because reflection JSON serialization is disabled; report writer
corrected to Utf8JsonWriter and verifier rerun successfully, with no tolerance change.
Run `dotnet run --file ops/VerifyTrigOracle.cs` from MathemaSharp; prerequisite is
mpmath 1.3.0 under `D:\Dev\BuildArtifacts\tools\mpmath-1.3.0` or SCIENCE_MPMATH_PATH.
No production API change. Full post-change gate passed 33/33 with `--no-images`:
`D:\Dev\BuildArtifacts\ecosystem-validation\20261101-232324`. No notebook output changed.
Other formats, pi-scaled/hyperbolic/root accuracy and builtin adoption remain pending.
Evidence: `D:\Dev\BuildArtifacts\validation\trig-oracle\report.json` and
`D:\Dev\BuildArtifacts\handoff\core-step1-trig-oracle`.

## Current complete pi-scaled trigonometric family

`SinPi`, `CosPi`, `TanPi`, `AsinPi`, `AcosPi` and `AtanPi` now have scalar and
span overloads with `ITrigonometricFunctions<T>`. Dedicated BCL methods are used,
not multiplication/division by rounded pi. Six IEEE formats verify exact analytic
anchors, special values, half-integers/pole neighbours, domain-boundary neighbours,
BCL bit parity, in-place, unchanged tails/inputs and pre-write overlap/length rejection.
Warmed double span calls allocate zero managed bytes on this runtime. Custom T may
allocate/throw without rollback; no new universal accuracy guarantee.
Release baseline: 632 passed (Core 325); after: 645 passed (Core 338), zero failures/skips.
Build: zero warnings/errors. Core preview.33 is source-only, not packaged.
Full-family post-change gate passed 33/33 with `--no-images`:
`D:\Dev\BuildArtifacts\ecosystem-validation\20261101-230539`.
Core/12 added only twelve generated pi-scaled declarations; code cells were unchanged.
Independent accuracy, complex shapes and builtin migration remain pending;
common-core step 1 is partial.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-pi-trig-family`.

## Current complete root-family continuation

`Cbrt`, `Hypot` and `RootN` now have scalar and span overloads with `IRootFunctions<T>`.
All forward BCL math in the original type. RootN preserves BCL degree-zero/negative
and extreme-int degree behaviour; no facade degree restriction. Hypot spans require
equal input lengths and validate destination overlap against both before writes.
Six IEEE formats verify scalar bit parity, special values, subnormals, degree extremes,
in-place, untouched tails/inputs and pre-write rejection. Warmed double calls allocate
zero managed bytes on this runtime. Custom T may throw without rollback.
Release baseline: 619 passed (Core 312); after: 632 passed (Core 325), zero failures/skips.
Build: zero warnings/errors. Core preview.32 is source-only. Independent accuracy,
complex shapes and builtin migration remain pending; common-core step 1 is partial.
Full-family ecosystem gate passed 33/33 with `--no-images`:
`D:\Dev\BuildArtifacts\ecosystem-validation\20261101-221816`.
Core/12 added only six generated root-family declarations; code cells were unchanged.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-roots-family`.

## Current binary Atan2 span continuation

`Atan2(y, x, destination)` requires equal-length read-only inputs, without broadcasting.
Its loop preserves scalar bits on six IEEE formats, including the signed-zero and
infinity cases in the 13-by-13 input matrix. Destination length and overlap with both
inputs are checked before writes. Exact in-place on either/both is supported;
shifted input-input overlap alone is safe with separate output. Only the written
output prefix participates in overlap checks; tails and separate inputs stay unchanged.
Warmed double calls allocate zero managed bytes on this runtime. Custom T may throw
without rollback. Release baseline: 612 passed (Core 305); after: 619 passed (Core 312),
zero failures/skips. Build: zero warnings/errors. Core preview.31 is source-only.
Post-change ecosystem gate passed 33/33 with `--no-images`:
`D:\Dev\BuildArtifacts\ecosystem-validation\20261101-213531`.
Core/12 added only the two generated Atan2 declarations; code cells were unchanged.
Independent accuracy and complex shapes remain pending; no builtin migration and
common-core step 1 remains partial.
Gate handoff: `D:\Dev\BuildArtifacts\handoff\core-step1-atan2-gate`.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-atan2-spans`.

## Current scalar Atan2 continuation

`GenericMath.Atan2(y, x)` forwards to `T.Atan2` without division or conversion.
The required BCL member is on `IFloatingPointIeee754<T>`, not the trigonometric
interface alone. Six IEEE formats verify argument order, four quadrants, signed-zero
axes/origin, infinities and NaN, including bit parity for a 13-by-13 input matrix.
Release baseline: 606 passed (Core 299); after: 612 passed (Core 305), zero failures/skips.
Build: zero warnings/errors. Core preview.30 is source-only, not packaged.
Binary span/complex shapes, independent accuracy and post-change ecosystem gate
remain pending. No language builtin migration; common-core step 1 remains partial.
Evidence: `D:\Dev\BuildArtifacts\handoff\core-step1-atan2-scalar`.

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

### Geometry remaining work
Phase 1 vectors implemented only. Matrices and quaternions, shape/intersection algorithms, fingerprinted PlotSharp and NDSharp migrations remain pending. No rendering, Release run, ecosystem gate or narrow-format extremes were verified in this slice.

## Geometry — Matrix3 implementation
Matrix3<T> adds row-major storage with column-vector transforms (A*B applies B first), identity, translation, rotation, scale, composition, transpose, determinant, inverse and point/direction transforms. Nonfinite or singular inverses and invalid homogeneous coordinates throw InvalidOperationException; directions require affine matrices. Inverse is not a conditioning guarantee. Geometry source version is preview.2.
Release geometry baseline 5/5; first new run 9 passed/1 failed (Half inverse rounding), corrected without changing tolerance; rerun 10/10. Release solution build: zero warnings/errors; solution tests: 867 passed, zero skipped/failed. Phase 1 still needs Matrix4 and Quaternion; phases 2–5 pending; phase 6 excluded. No consumer migration, publication, rendering or ecosystem gate. Extreme conditioning/scaled inverse paths need additional tests.

## Geometry — Matrix4 implementation
Matrix4<T> adds 3-D homogeneous transforms with row-major storage acting on column vectors: identity, translation, scale, rotations X/Y/Z and normalized axis-angle, multiplication, transpose, determinant, partially pivoted Gauss-Jordan inverse, and point/direction transforms. Zero pivots/nonfinite inverse results throw; no conditioning guarantee. Determinant and inverse currently allocate scratch arrays. Source preview.3, not packed. Release geometry baseline 10/10, after 15/15; solution build zero warnings/errors, solution tests 872 passed, zero failures/skips. No failed run in this slice. Quaternion remains to complete phase 1; phases 2–5 pending, phase 6 excluded. No rendering, consumer migration, ecosystem gate, performance or ill-conditioned/extreme-format corpus tested.

## Geometry phase 1 — primitives implemented
Quaternion<T> completes the planned primitive family with (x,y,z,w) storage, Hamilton product, conjugate, scaled normalization, axis-angle creation, rotation matrix conversion, vector rotation and shortest-path SLERP for t in [0,1]. Rotation matrix import rejects nonfinite, translated, reflected, scaled and nonorthogonal inputs (orthogonality tolerance 64 spacings at one). Matrix3/Matrix4 conventions are column vectors and right-hand composition first; quaternion equality is component equality, not rotational equivalence. Geometry is BCL-only, source preview.4, not packed.
Release geometry baseline 15/15, after 20/20; solution build zero warnings/errors, solution tests 877 passed, zero failures/skips. All four requested types are exercised; matrix conversion branches, antipodal/coincident SLERP, composition, length and conjugate properties, and float Numerics references pass. No failures in this slice. Phase 1 functionality is implemented; performance, ill-conditioning/extreme-input corpus and ecosystem rendering remain unverified. Phases 2–5 pending; phase 6 excluded. No existing algorithm moved, so no migration fingerprints yet.

## Geometry phase 2 — polygons and shapes
Polygons adds signed shoelace area, algebraic centroid, closed perimeter, orientation, winding-number LocatePoint (Outside/Boundary/Inside), BoundingBox2<T>, Andrew monotone-chain hull and iterative Douglas-Peucker open-polyline simplification. Finite coordinates required; boundary tests use exact T comparisons. Empty area/perimeter are zero; empty bounds and fewer-than-three centroid throw; zero-area centroid throws. Hull removes duplicates and collinear intermediate points and is CCW without a closing duplicate. Self-intersections use algebraic area and nonzero winding, not union geometry. Simplification preserves endpoints but is not polygon-topology preserving. Narrow-format overflow/ill-conditioned predicates remain unverified. Source preview.5, not packed.
Geometry Release baseline 20/20, after 25/25. Solution Release build zero warnings/errors. First solution run: 881 passed, 1 failed in unchanged Kernels WarmDoubleReductionsAllocateNoManagedBytes (8 bytes); isolated rerun 1/1; complete rerun 882 passed, no failures/skips, without code changes. Cause remains unknown, not fixed. Phase 2 implemented; phase 3 intersections/distances next, phases 4–5 migrations pending, phase 6 excluded. No consumer migration, performance checks, rendering or ecosystem gate.

## Geometry phase 3 — intersections and distances
Intersections provides closed segment/segment intersections (including point segments and collinear overlap), infinite line/circle and circle/circle intersections, point/segment distance, filled-polygon distance and closed circle/axis-aligned ellipse membership. Segment2<T> describes endpoints; Intersection2<T> discriminates None, Point, TwoPoints, Overlap and Coincident. Coordinates/radii must be finite; circle radius may be zero, ellipse axes must be positive, line direction nonzero. Boundaries included; predicates use exact T comparisons, not tolerance snapping. Empty polygon distance rejected. Coincident positive-radius circles have infinitely many intersections; coincident zero-radius circles are one point. Results ordering documented in API comments. Source preview.6, not packed.
Release Geometry baseline 25/25, after 29/29; solution Release build zero warnings/errors; solution 886 passed, no failures/skips. Four-format cases include tangency, internal/external circles, overlap, degenerate segments, filled-polygon distances and invalid input. No failures this phase. Optional LBM mask change was deliberately not performed; article selftest and its fingerprint were not run. Extreme conditioning/overflow, performance, rendering and ecosystem gate not tested. Phases 1–3 implemented; fingerprinted PlotSharp and NDSharp migrations (4–5) remain pending; phase 6 excluded. Previous intermittent kernel allocation failure remains unexplained, not fixed by this green run.

## Geometry phase 4 — contour migration
PlotSharp delegates its shared contour traversal to MathemaSharp.Geometry.IsoContours; Charts.MarchingSquares overloads and Segment/Segment<T> return types are unchanged. RectilinearContours<T> retains INumber<T> (including decimal); the engine exposes a legacy-compatible Trace plus the IFloatingPointIeee754 MarchingSquares API returning Segment2<T>. Fixed edge order, summed-corner policy, exact-level segments and arithmetic order preserved. Geometry source preview.7; PlotSharp preview.3; neither packed. Direction PlotSharp -> Geometry -> BCL, no cycle.
Fingerprint: 2922 cases, two pre-change runs and two post-change runs identical: 69B10DD0254A1AE95D1B0881D341E5A34EF6E2397ED6E994B6EDF22C0714FEB6. Harness in BuildArtifacts/tmp/geometry-contour-fingerprint.cs, logs in BuildArtifacts/logs/geometry-contours.log. Old algorithm removed only after fingerprint match and green tests.
Initial PlotSharp baseline could not compile NavierContourTests because the article had already removed Charts.MarchingSquares. Tests now call PlotSharp compatibility overload through a tuple adapter; frozen expected coordinates and assertions unchanged. This is a repaired test target, not an independent article oracle. Post-removal solution build and tests pass: PlotSharp 242 + CLI 10; MathemaSharp 891 (Geometry 34). Both builds zero warnings/errors. Public signatures preserved by source inspection; prebuilt NuGet consumer runtime loading, full NDSharp tests, article selftest, rendering/UI and ecosystem gate not executed. PlotSharp solution references currently build article/engine dependencies in Debug even under Release; article output still follows its preexisting local bin configuration. Phase 5 camera/projection pending; phase 6 excluded.

## Geometry phase 5 — camera and painter ordering
NDSharp.Plot delegates perspective projection, world/view/projection matrices, aspect normalization, projected-box fit, stable back-to-front sorting and average depth to Camera<T>/DepthOrdering in MathemaSharp.Geometry. Public Plot3D.Project stays unchanged. Artists, Skia paths/paints, frame panes, grid/ticks/text and viewport acquisition remain in NDSharp. Inspected Plot3D.cs and Artists.cs: no existing orthographic projection or 3-D clipping to migrate; the latter contains separate 2-D annotation clipping.
Fingerprint on 2304 projected points plus scatter/polygon order and ties, including inverted limits, viewport sizes and near-pole/pole views: 5B92443FDF5A7BBD506E3693B9C8F67E05CAE66D822B3A65ADDEBF48324ABF7C. Two baseline runs, post-route run and post-removal engine-order run identical. Original and engine-order harnesses in BuildArtifacts/tmp. Legacy arithmetic retained (including IEEE results for singular views/ranges); no new validation or clipping semantics. Generic constants round from the predecessor's double literals.
Geometry 39/39 across four formats; MathemaSharp solution 896 passed, zero warnings/errors in build. NDSharp solution 862 passed (core 700, Plot 46), no skipped/failed; build 15 nullable warnings in unchanged Image/Frame files. All runs logged through Log-TestRun.ps1; details in BuildArtifacts/logs/geometry-contours.log. Full pre-change NDSharp suite was not run; migration baseline is the repeated fingerprint. Geometry preview.8, NDSharp.Plot preview.3; no packages/push/install. Source phases 1–5 are implemented in inspected scope; phase 6 excluded. Full ecosystem gate, prebuilt consumers, independent matplotlib accuracy, performance and broad conditioning remain unverified.
