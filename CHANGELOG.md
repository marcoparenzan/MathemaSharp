# MàthemaSharp — change log

Reconstructed log of the work documented in MathemaSharp and in the linked repositories under `D:\Dev\Science`.

## How to read this document

- The sections follow the reconstructible order of the tranches, from the most recent to the oldest; they are not a certified Git chronology.
- No dates, commits or versions were invented for the activities that lack such data. The timestamps of the reports are identifiers of the runs, not dates assigned to the features.
- **Verified** means that a relevant build, test or execution is documented. It does not mean universal support of every API or numeric type.
- A test on the saved outputs of a notebook **does not rerun** the kernel. The two verifications are reported separately.
- The historical results do not add up: the suites grow and change perimeter. The current count prevails over the previous counts.
- This log describes results and changes. Future activities are in [ROADMAP.md](ROADMAP.md); organisation and decisions in [ARCHITECTURE.md](ARCHITECTURE.md); migration details in [NET11-MIGRATION.md](NET11-MIGRATION.md).

## Most recent verified state

| Perimeter | Latest evidence available |
|---|---|
| Complete gate before the latest extensions | All steps green; 2624 tests passed, zero failed, 13 PySharp suspended |
| MathemaSharp in that gate | 311 tests passed |
| NDSharp in that gate | 564 tests passed |
| PlotSharp in that gate | 70 tests passed |
| JupyterSharp in that gate | 44 tests passed |
| MatSharp after adding the typed solver | Build successful, 133 tests passed |
| PySharp after the generic CLR types | Build successful, 1507 tests passed, zero failed, 13 suspended |
| NuGet consumer after the latest extensions | Restore, build and execution successful: includes the MatSharp solver and PySharp's generic CLR container |
| Second Ecosystem notebook | Prepared, not yet run; gate not yet rerun after this addition |

The green complete gate is documented in `D:\Dev\BuildArtifacts\ecosystem-validation\20261027-223603`. It must not be presented as a verification of the working tree following the preparation of the second notebook.

## 1. MatSharp: typed matrix solvers

**Added**
- `A \ b` for typed real matrices through an adapter to the MathemaSharp kernels.
- LU for square systems, with reuse of the factorisation for multiple RHS columns.
- QR/least squares for overdetermined systems.
- Explicit copies between MATLAB column-major storage and kernel matrices, without passing through `double`.
- Opt-in registration of the solver in the runtime; the language library stays separate from the mathematics kernels.

**Verified**
- Three new tests: six formats, multiple RHS, tall system, preservation of the input, singularity and underdetermined case.
- Execution in the language with preservation of the Decimal128 increment `1e-25`.
- Notebook `notebooks/MatSharp/01-decimal-scalars.ipynb` extended and run; marker `typed_left_divide_precision: OK` checked by the saved-outputs test.
- Build and 133 MatSharp tests passed; separate NuGet consumer run.

**Distribution and limits**
- MatSharp packages updated to `0.1.1-net11-preview.3` in the common feed.
- Underdetermined systems rejected: a minimum-norm solver is not implemented.
- No new support for typed complex, N-D or right matrix division.

## 2. PySharp: explicit construction of generic CLR types

**Added**
- Selection `ClrType[TypeArguments]` to close a generic CLR definition.
- Checks of arity, nature of the arguments and CLR constraints.
- Construction and use of `List<Decimal128>` with an open type injected by the host.

**Verified**
- Two new tests on precision and invalid types/constraints.
- Increment `1e-25` preserved through insertion and reading of the container.
- Build, 1507 tests passed and 13 suspended; NuGet consumer run on the new binder.

**Distribution and limits**
- `PySharp.Interpreter` `2.6.1-net11-preview.2`.
- Tool `PySharp` `2.5.1-net11-preview.4`.
- `JupyterNet.Kernels.PySharp` `0.1.1-net11-preview.3`.
- It does not demonstrate automatic inference, import of all the generic families, typed NumPy arrays or `Span`/byref support.

**Prepared, not verified**
- `notebooks/Ecosystem/02-typed-language-interop.ipynb` and `ops/RunLanguageInteropPilot.cs`: C# notebook embedding the two languages.
- Not yet run nor included in the gate; it is not proof of native PySharp/MatSharp plugins for JupyterSharp.

## 3. Closure of the Matplotlib defect and of the complete gate

**Fixed**
- Y ticks of the NDSharp renderer: `center_baseline` alignment instead of the geometric centre of the text rectangle.
- Margin measurement and drawing based on the font's ascent/descent.
- Double rounding eliminated in the `P5_layout.py` snippet; CPython golden kept unchanged.
- Test added on the unrounded coordinates, with a declared tolerance `5e-4` in normalised coordinates for subpixel differences of the renderers.
- Missing PySharp artifact configuration and sample paths in the tests, now independent of the position of the DLLs.
- Local PySharp outputs generated during the inspection removed; build outputs kept in the external root.

**Verified**
- Y error of the examined case reduced from about `0.00185` to about `0.00029` normalised.
- Snippet rerun with CPython/Matplotlib on the same host.
- All the gate's steps completed with exit code 0: build/test, audit, baseline, PySharp kernel, PySharp/MatSharp notebooks and NuGet pilot.
- 2624 tests passed, zero failed, 13 suspended; no waiver introduced for the layout.

**Distribution**
- `NDSharp.Plot` `2.6.3-net11-preview.2`.
- `PySharp.Matplotlib` `2.5.1-net11-preview.2`.
- New previews of the tools/consumers concerned; the previous versions were not overwritten.

The comparison certifies the regression case, not the complete equivalence between Skia and Matplotlib.

## 4. Real JupyterSharp–MathemaSharp–PlotSharp integration

**Added**
- `notebooks/Ecosystem/01-signal-spectrum.ipynb`.
- Execution through the JupyterSharp CLI installed locally from the packages, without global installation.
- Five-cycle signal, MathemaSharp FFT, PlotSharp chart of the signal and of the spectrum.
- Two SVGs saved through the kernel's HTML output.
- Test of the saved outputs; structure test extended to the JupyterSharp notebooks.
- `ops/RunEcosystemPilot.cs` and `ops/ValidateEcosystem.cs`.

**Verified**
- FFT peak at bin 5 and normalised amplitude 0.5.
- Two HTML outputs containing SVG and no cell error.
- Pilot run with packages restored in an isolated cache, without `ProjectReference` or JupyterNet execution.

**Limits**
- The pilot intentionally uses `double` data and coordinates.
- It does not certify Decimal128 plotting, all the renderers or native language plugins.
- No visual verification in the browser documented.

## 5. Audit and fixes of vulnerable dependencies

**JupyterSharp**
- Identified the chain Microsoft.PowerShell.SDK 7.6.6 → Pkcs/Xml 10.0.10.
- Explicit references `System.Security.Cryptography.Pkcs` and `System.Security.Cryptography.Xml` at 10.0.13 in the PowerShell kernel, verified against the Microsoft/GitHub advisories.
- Build and 44 tests passed; nine JupyterSharp packages `0.3.1-net11-preview.2` generated.
- Isolated consumer restored, compiled and run without audit warnings.

**PySharp/Pyodbc**
- Audit extended: PySharp → Pyodbc → Microsoft.Data.SqlClient 7.0.2 → Pkcs 9.0.13.
- Explicit Pkcs 10.0.13 reference in Pyodbc.
- New PySharp/Pyodbc packages `2.5.1-net11-preview.2` generated in that tranche; the PySharp tool was later versioned again.

**Automation**
- `ops/AuditSciencePackages.cs` checks the JSON reports of the six solutions.
- Non-zero exit code on vulnerabilities or errors: the exit code of `dotnet list package` alone is not a sufficient gate.
- No advisory suppressed. Audit green with the current sources in the last complete baseline.

The audit does not certify application security, installed runtimes, old packages in the feed or projects outside the solution.

## 6. .NET 11 migration and distribution

**Changed**
- Target of the active projects in the six repositories: MathemaSharp, NDSharp, PlotSharp, JupyterSharp, PySharp and MatSharp → `net11.0`.
- MathemaSharp SDK pinned to `11.0.100-rc.3.26522.148`.
- Framework identifiers updated in the relevant operational scripts; imported snapshots excluded.
- Blazor portal: handling of circuit disconnection in JS calls, required by the BL0016 analyser.

**Verified and distributed**
- Build of the six solutions and tests, with later fixes of the PySharp gaps described below.
- Baseline of 24 notebooks, nine additional C#/PySharp runs and the MatSharp embedding notebook rerun; some runs overlap, they are not counts of all-distinct notebooks.
- First collection of 37 preview packages generated in `D:\Dev\NuGetLocalFeed`, with version increments and the `net11-preview` suffix.
- NuGet consumer without source references: Decimal128, NDArray, Bluestein FFT, bridge solvers and PySharp verified; then extended to the new features.
- Pack and consumer logs in `D:\Dev\BuildArtifacts\net11-pack`.

The previews are not a GA release or a production certification. Some samples/tools outside the solution have the target updated but require dedicated verification.

## 7. PySharp: recovery of the Python compatibility required by FastAPI/AnyIO

**Added progressively**
- `sys._getframe` on the real thread-local stack: globals, snapshot locals, name/file/line and the `f_back` chain.
- `inspect.getsource` for modules, functions and methods with a real file; explicit errors when the source is missing.
- `compile` of exec/eval strings into objects with a reusable AST; integration with exec/eval and inspect.iscode.
- Public mutable AST and `PyCF_ONLY_AST` flag: initially assignments/expressions, then import/alias, control flow, calls, collections, functions and classes in the supported cases.
- Recompilation from the modified fields of the nodes, not a simple rerun of the original source.
- PEP 562: lazy lookup through the modules' `__getattr__` also for `from ... import ...`.
- Relative names in `importlib.import_module`, with explicit package and module identity preserved in the cache.

**Fixed in the tests**
- KeyError expectation: CPython confirms the quotes; the expectation was changed, not the runtime.
- Collision of the temporary Pandas CSV: unique name per run.
- Oracle collection isolated from parallelism, because the runner modifies the process working directory.

**Evidence and limits**
- Regressions for frames, source, compiled code, AST mutation, lazy exports and relative imports.
- The nine initial FastAPI/AnyIO failures were recovered.
- Evidence of the AnyIO 4.15.1 fixture kept outside the repository.
- AST, introspection and compile remain partial: they are not equivalent to a complete CPython implementation; visitors, generators/comprehensions, uncovered syntactic and semantic variants remain declared.

## 8. MatSharp: first Generic Math path in the MATLAB language

**Added**
- Initial link `mathema_solve`/`mathema_rcond` in double to the shared kernels, without modifying the previous backslash operator.
- `GenericScalar<T>`: typed real storage, initially scalar and then 2D column-major matrix.
- decimal32/64/128 constructors; operators and comparisons without intermediate conversions to double.
- Concatenation, indexing and assignment, copies, transpose, broadcasting and products.
- Rejection of implicit conversions between Decimal formats or from fractional doubles in the verified paths.
- C# notebook that runs MATLAB through MatEngine, not a dedicated MATLAB kernel.

This base was extended by the typed solver described in section 1. The name GenericScalar<T> remains to be normalised because the container also hosts matrices.

## 9. PySharp: numerical interoperability with MathemaSharp

**Recovered**
- Kernel and loader after renaming/moving.
- TimeSeries tour and refrigerator/photovoltaic scenarios run in the C#/PySharp pairs.
- CSV paths fixed; three agreement tests reactivated.

**Extended**
- First Decimal128Interop facade: scalar/array roundtrip and Hermitian Complex<Decimal128> with differences `1e-25`.
- CLR operator dispatch: arithmetic, comparisons and unary operators in the verified cases.
- Builtin module `mathema`, with decimal32/64/128 constructors from strings, integers and CLR values of the same type.
- Explicit binding of generic CLR methods: `method[ClrType](args)` and application of the constraints.
- Precision, operators and generic-methods notebooks actually run.

**Limits**
- Legacy TimeSeries facade intentionally double.
- Float literals already rounded do not recover lost decimal digits.
- No general guarantee on the marshalling of every CLR type, typed NumPy, inference and Span/byref.

## 10. Generic Math FFT/IFFT and first measurements

**Added in MathemaSharp.Core**
- Radix-2 FFT for powers of two.
- Bluestein for other lengths, with radix-2 convolution and phases reduced through integer arithmetic.
- Unnormalised forward, inverse divided by N; input preserved and finiteness/representability checks.

**Verified**
- Six formats: DFT comparison, roundtrip, impulse/constant and Decimal128 cases.
- Independent analytic references: transform of [1,2,3,4], complex tone and Parseval.
- 200-digit mpmath fixture for complex signals of length 7, 16, 31 and 64 on float, double, Decimal64 and Decimal128.
- Double roundtrip on 1024 radix-2 points and 1009 Bluestein points.
- `notebooks/Core/06-fft.ipynb` run and included in the baseline.

**Measured, not optimised**
- Six double forward/inverse benchmarks on 64, 1009 and 1024 points.
- BenchmarkDotNet ShortRun in-process because of the limit of the .NET 11 RC toolchain: preliminary measurements, not definitive benchmarks.
- High cost of the Bluestein buffers observed; optimisations postponed on request.

Fixture/generator: `ops/oracles/fft-mpmath.json`, `ops/generate-fft-oracles.py`. They do not certify every length or Half/Decimal32 against the general oracles.

## 11. NDSharp: generic storage and bridge

**Added/fixed**
- Reading of the native runtime dtypes towards T without double intermediates.
- Convert.ToDouble eliminated from the identities of the reductions, with regressions on integers beyond 2^53 and ulong.MaxValue.
- NDArray<T> alongside the runtime container: shape, strides, indexing and copies in logical order.
- Transposition and slicing as views, including negative steps.
- Add broadcasting on trailing axes, scalars and empty dimensions.
- Sum, Dot and 2D MatMul, respecting the strides and with independent results.
- Typed conversions towards the MathemaSharp LU bridge, QR and diagnostics, without duplicating decompositions.
- Notebook `notebooks/NDSharp/01-generic-math.ipynb` run and included in the baseline.

**Verified**
- Decimal128 precision tests `1e-25`, views/copies, invalid inputs and six formats in the paths covered.
- Bridge also verified through packages; mapping of the MathemaSharp.* feed corrected.

NDSharp is not completely converted. Runtime storage, imported algorithms, complex numbers and batched products remain distinct from the implemented NDArray<T> path.

## 12. Notebooks and reproducible baseline

**Created and run**
- Four Usage/Reference consolidation notebooks, one each for Core, LinearAlgebra, TimeSeries and Quantum.
- Four API catalogues from reflection with signatures and constraints.
- Rerun of the previous Core notebooks, LU, Quantum and selected C# TimeSeries.
- Numerical oracle, diagnostics, NDSharp, FFT and language integration notebooks.

**Automation**
- Selective runners and explicit manifest to avoid involuntary runs of notebooks outside the perimeter.
- Baseline initially of 22, then 23 and 24 notebooks; external logs/reports, SDK/runtime and exclusions recorded.
- Quick build/test mode separate from the kernel execution.
- Tests of the saved outputs, distinct from the real rerun of the notebooks.
- Ecosystem gate added later, with checks for all the repositories and the NuGet pilot.

The reflection catalogues do not replace API-by-API contracts and examples. The notebooks not selected are not automatically certified.

## 13. Independent oracles and diagnostics of linear systems

**Validation**
- 200-digit mpmath fixture for gamma, log-gamma and equivalent chi-square tails, down to the order of 1e-306 on the appropriate types.
- Hilbert systems 3, 5 and 8 for LU/QR: solution, residual and determinant.
- Hilbert 12 double counterexample: residual about 1.31e-16 but solution error about 0.00595; the pivot check does not certify the conditioning.
- MatrixExp on five matrices, including non-normal and complex ones; three unbalanced Hermitian spectra.

**Fix**
- Eigenvalue 1e-40 lost on diag(1,1e-40): diagonals handled directly; recovery of the small eigenvalue from the determinant and the other eigenvalue, with FMA to reduce cancellation.

**New API**
- LinearSystemDiagnostics.Solve<T>: solution, relative residual, normwise backward error, estimate of the infinity condition number and reciprocal, IsIllConditioned warning.
- Opt-in diagnostics, more expensive than the plain solve; it is not a certification of the forward error and does not include refinement.
- Oracle and diagnostics notebooks run, reference updated.

## 14. Numerical consolidation of the active kernels

**Core**
- NearlyEqual: default limited to min(4096 × epsilon, sqrt(epsilon)), fixing the excessive tolerance on Half.
- Native tests in T for comparisons, special values, sums/Kahan, Horner, products and DFT.

**TimeSeries**
- LOESS: centred abscissae, thresholds tied to the precision of T, constants in T and validation of data/weights.
- GARCH: grid with integer indices to avoid lack of progress and floating-point accumulation; validation and finite likelihood.
- Simulation: overload with Func<Random,T> for native innovations; previous Random/Box–Muller default preserved.
- Special functions: Lanczos coefficients read directly in T, explicit domain/special values and non-convergence. It is not a 34-digit accurate algorithm for every input.

**LinearAlgebra**
- QR/least squares: normalisation, Hypot norms, unit reflectors and per-type thresholds.

**Quantum**
- Norm and Normalize robust through Hypot/scaling.
- Normalised 2×2 Hermitian diagonalisation and stable eigenvectors.
- CheckEigenstate through relative residual and separate scaling.
- MatrixExp with Hypot, copy and halvings in the scaling, validation of terms/intermediates; TimeEvolution checks.

Each intervention is accompanied by documented multi-format tests/edge cases. A first consolidation pass does not certify arbitrary matrices, all the tails or every algorithm.

## 15. Identity, structure and boundaries of the libraries

**Reorganised**
- Numbers → MathemaNet → MathemaSharp for projects, namespaces, references and notebooks.
- MàthemaSharp brand with an accent on the first a; technical names without accent.
- Notebooks moved to the root, split by library; outputs initially cleared and then rerun in the documented tranches.
- Linear algebra extracted from TimeSeries into an independent library.
- Generic LU with pivoting, solver, determinant and inverse; basic operations widened to the appropriate numeric constraints.

**Boundaries**
- MathemaSharp: mathematics kernels.
- NDSharp: arrays/storage/views.
- PySharp and MatSharp: language semantics.
- PlotSharp: visualisation.
- JupyterSharp: notebooks/kernel/output.

**Import and preparation**
- Initial selective copy of 68 NDSharp files, with licence and numerical materials, outside the solution.
- Initial PlotSharp collection of 148 files from the four contexts, with provenance and hash; not equivalent to an active consumer migration.
- PlotSharp subsequently verified as a compilable library with tests, Gallery and Inventory.
- Repositories moved to the common root D:\Dev\Science.

**Operations**
- Package feed D:\Dev\NuGetLocalFeed.
- Build, test, benchmark and custom reports progressively moved under D:\Dev\BuildArtifacts.
- Fixes of the loaders and of the tests that depended on the old position of the DLLs.
- Temporary import scripts removed in the documented clean-up tranche; sources, fixtures, notebooks and snapshots kept.

The Numbers → MathemaSharp renaming is a breaking change. The renamed graphic logo kept the previous brand (MàthemaNet); it was replaced on 2026-10-30 by the ScienceSharp logo set.

## 16. Mathematical documentation, infographics and portal

**Documented/implemented**
- Analysis of the TimeSeries infographics: patches, decompositions, temporal/cross-variable attention and foundation models, distinguishing concepts from implemented APIs.
- HistoryPatches.Extract<T> and RollingOrigin.Evaluate<T> backtest, with isolation of the past and optional gap.
- Linear algebra for AI infographic integrated into LINEAR-ALGEBRA.md; norms, determinant and 2×2 inverse added before the extraction of the library.
- Blazor Docs portal with Server interactivity, Markdown, search, images/internal links and path protection.
- CSS for typography, code, images, print and tables with alternating rows, bold headers and padding.
- Table of contents of the headings and browser JavaScript functions.

**Not declared as completed**
- Complete Transformers/foundation models, general eigenvalues/SVD or accuracy of the models deduced from the infographics.
- Chronological sources of the images not all verified against primary sources.
- Tailwind compiled for production, formula rendering, accessibility audit and complete visual verification of the site.

## Where to find the evidence and how to maintain the log

| Evidence | Location |
|---|---|
| Objectives received | PROMPTS.md, managed automatically; not proof of completion. Removed from the repository on 2026-10-30 (kept in the git history) |
| State and priorities | ROADMAP.md, first current section |
| Decisions/API/constraints | ARCHITECTURE.md and ops/api-declarations.csv |
| .NET 11 migration and fixes | NET11-MIGRATION.md |
| Notebooks run | Saved outputs in notebooks/*, with the limits indicated above |
| MathemaSharp baseline | D:\Dev\BuildArtifacts\MathemaSharp\baseline |
| Ecosystem gate | D:\Dev\BuildArtifacts\ecosystem-validation |
| Audit | D:\Dev\BuildArtifacts\security-audit |
| Pack and isolated consumer | D:\Dev\BuildArtifacts\net11-pack |
| Versioned packages | D:\Dev\NuGetLocalFeed |

For each new tranche add a section at the top with: project/API, observable change, tests or notebooks actually run, package version, available report and limits. Do not turn prepared activities into completed ones; do not overwrite historical results with new ones.
