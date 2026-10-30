# MàthemaSharp — roadmap

MathemaSharp is the code and the name of the packages; **MàthemaSharp** is the brand. Objective: a modular mathematics library based on Generic Math, with .NET 11 and support for the new Decimal32/64/128 where compatible with the algorithms.

This roadmap replaces the previous aggregated items that gave migration and verification of the notebooks as completed. A successful build does not prove that every API supports every format.

## Current roadmap — updated state

This section prevails over all the historical tables and counts reported further on. It does not attribute to the latest changes a complete baseline that has not yet been rerun. Performance optimisations postponed: first correctness, contracts, notebooks, interoperability and distribution.

### Baseline reconfirmed — 2026-10-29

`--no-images` gate rerun without code changes: 31 steps plus `skip-report`, all exit 0, report `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-230541`. PySharp: 1507 passed, 0 failed, 13 suspended (12 PostgreSQL unreachable, 1 opt-in pretrained download); the suspended ones are not counted as passed. `PlotSharp.Tests` reverified separately: 227/227. The gate takes over 10 minutes: run it at the consolidation points. The repository has been under git since 2026-10-29 (baseline commit `13341d3`). This reconfirmation prevails over the baseline `20261028-215324` below, which remains valid as history. The perimeter and the leftovers of the graphics consolidation are in `Science`'s `ROADMAP.md` (R01–R16).

### Previous complete baseline — NDSharp filled sampler/bands

Gate --no-images D:\Dev\BuildArtifacts\ecosystem-validation\20261028-215324: 31/31 exit0 after the latest extractions. Solution build/test/audit, isolated package and source kernels, lines/bands notebook, browser/UI/cancellation and complete numerical selftest rerun. Shared suite227 cases, ND19 and compatibility fixtures/queries reconfirmed in the gate; suspended tests and warnings remain declared.
Zero new review files/screenshots, notebook output PNGs embedded as expected and QA encoding in memory. no-images folders only JSON, recent reports/policy verified and archived. It prevails over the notes of full gate not rerun. D03 remains partial (palette/alpha/Skia/other categories/consumers/distribution), complete accessibility and native JupyterSharp not certified. No release.

### Previous targeted verification — contourf band selection

ContourBands delegated by DrawFilled: indices/inclusive edges and duplicates preserved, external/non-finite without a band, singleton without bands. Snapshot of finite non-decreasing levels, midpoint normalisation with the original order and non-reassociated overflow. Palette/alpha/bitmap/Skia remain ND.
Build/suite shared227/227, ND19/19 green; kernel source rebuilt, ContourBands metadata verified and lines/bands320×240 notebook rechecked. Zero review image files; known kernel warnings visible. Not a full gate/publication, D03 partial and the previous complete baseline31 to be reconfirmed.

### Previous verification — NDSharp filled-contour sampler

Only the physical Sample shared in RectilinearFieldSampler: binary search and four terms unchanged, 81 queries frozen before the change and all equal. Snapshot of axes/data, increasing/finite axes min2×2, consistent shape/finite queries; non-finite data propagate. Mutation during draw and invalid inputs not guaranteed. Separate descending line-contours not modified; bands/palette/alpha/Skia local.
Build and suite shared211/211, NDSharp.Plot19/19 passed. Kernel source-plots rebuilt, new sampler metadata verified and the real lines/bands notebook produces two embedded PNGs320×240; no review file/screenshot. Pre-existing kernel warnings remain. Not a new full gate or release; previous baseline31 to be reconfirmed.

### Previous complete baseline — NDSharp contour in the real kernel

Gate --no-images D:\Dev\BuildArtifacts\ecosystem-validation\20261028-203820: 31/31 steps exit0 after the new notebook and source profile. Default package kernel build and separate source variant, new DLL metadata verified, notebook really run with JupyterNet/PySharp on .NET11. Lines and bands: two embedded PNG320×240 outputs, not review/screenshot files. Recent report archived together with the other JSON/logs.
Before, the binding still used the previous NDSharp.Plot package: introduced the opt-in ScienceUseSourcePlots, compatible default and kernel-source-plots distinct from kernel. Notebook under NDSharp/pysharp because Ecosystem is reserved for the JupyterSharp pilots; test not weakened. Two new tests, Core100/100 verified separately and in the gate; pre-existing PySharp warnings not suppressed.
This baseline prevails over the previous non-full-gate notes. D03 remains partial (contourf/styles/Skia/other consumers/distribution), native JupyterSharp and CPython pixel-oracle not demonstrated. No publication.

### Previous targeted verification — NDSharp contour-line migration

Geometry of ContourArtist.DrawLines effectively delegated to PlotSharp.RectilinearContours and the common kernel. Distinct conventions preserved: ND uses physical/non-uniform coordinates, forward top/left edges, null segments and skip of the non-finite corner sum; the index-based core keeps its own convention/default/mask/bounds. Levels, Skia/paint/Axes and ND contourf not modified.
New reference NDSharp.Plot→PlotSharp; build and e18/18 ND tests passed, shared204/204 passed with geometry compared exactly with pre-migration arithmetic. No image. Hashes/deps report updated with geometry delegated/family incomplete. Not a new full gate, end-to-end notebook or publication; D03 now partial, previous complete baseline distinct.

### Previous verification — NDSharp.Plot inventory

Active source NDSharp/src/NDSharp.Plot inspected, not snapshot: net11.0, NDSharp + Skia/native3.119.0, Figure/Axes/Artists and DejaVu fonts. Hashes/dependencies report in ops/inventory/ndsharp-plot-contract.json. Only the linear contour geometry is a candidate, which uses physical/non-uniform Xs/Ys, skip of non-finite cells and suppression of extreme levels; not a global conversion of PlotSharp indices.
Four baseline tests added without renderer changes, PNG in memory only. Build and NDSharp.Plot18/18 suite passed, inventory tool compiled/run. Contourf/AutoLevels/Ticks/styles/Skia/raster must stay separate until verified. No migration/PlotSharp dependency/image/publication, not a new full gate.

### Previous complete baseline — after FieldPlot consolidation

ops/ValidateEcosystem.cs -- --no-images, D:\Dev\BuildArtifacts\ecosystem-validation\20261028-185732: 29/29 steps exit 0. All the verifications of solution, audit, notebooks/pilots, CLI/Web, calculator/mobile UI, quadro/whistle cancellation, complete original numerical selftest, in-memory raster and browser rerun after the extractions. No skip of the checks, suspended tests always declared in the suites/TRX.
FieldPlot renderer now source-shared with the Navier facade; structural/pixel fixtures and PlotSharp suite198 cases reconfirmed in the gate. Notebook outputs updated; no new review image/screenshot. Three *no-images folders contain only JSON; seven recent reports/image policy verified and archived with logs.
This baseline prevails over the previous notes of gate not rerun. It does not close all the families, Navier/original notebook consumers, complete accessibility, licences or distribution. No package published/overwritten.

### Previous targeted verification — shared FieldPlot assembly

Charts.NodeFieldPlot composes header/styles and consolidated components; Navier keeps FieldPlotOptions/signature through a facade and removes unused private helpers. Numerics/scenario/UI not moved; cell-based Heatmap distinct. Four production outputs frozen before: structural markup and RGB pixel hashes coincide (only GUID and PNG normalised in the comparison).
New overlay validation same-shape, contour0–32, finite obstacles/radius≥0, out-of-contract input incompatibilities declared. Build/suite PlotSharp198/198 passed; Web Release/quadro/smoke --no-images rechecked. Zero images, not a new full gate or release. D02 still requires overall verification/notebook consumers/licences; full29 baseline to be reconfirmed.

### Previous verification — node frame/projection

NodeFieldLayout shared for proportions/margins, colorbar position and contour X/Y. Math.Round height to-even, bottom origin and fractional indices preserved, not cell-centre; finite extrapolations not clipped. Create validates shape min2×2/max100k, width/height1–10000 and collapses before the raster.
Build/suite PlotSharp 192/192 passed, previous fixtures and new corner/rounding/proportions; Web Release/quadro/smoke --no-images rechecked, zero images. FieldPlot assembly/header/styles remain local. Not a new full gate, benchmark or publication; previous complete baseline to be reconfirmed.

### Previous targeted verification — gradient colorbar

Navier colorbar fragment shared in GradientColorbar: k/10 samples, 3 ticks, rect/gradient/anchor/escaping preserved, including the legacy integer half height. Three production markups frozen before the migration coincide, normalised only for GUIDs. Default striped Heatmap unchanged; inputs validated (finite limits/projection, #RRGGBB colours).
Build/suite PlotSharp 178/178 passed, Web Release/quadro/smoke --no-images rechecked. Zero images and no full gate/publication. FieldPlot positioning/frame and contour projection still local, D02 partial. Full29 baseline earlier than the latest extractions to be reconfirmed.

### Previous targeted verification — Navier arrow geometry

NodeVectorOverlay vector geometry shared with global normalisation, stride/threshold/node projection and legacy open heads. Previous Heatmap quiver unchanged; style and frame local. Four FieldPlot paths frozen before the migration coincide. New explicit finite/max100k/shapes/stride/projection validation, previously unvalidated inputs not guaranteed.
Build and PlotSharp suite 168/168 passed: four directions, unsampled maximum, zero/threshold, unchanged inputs and real pressure report. Web Release/quadro and smoke --no-images rechecked, zero images. Not a new full gate or benchmark, no publication. Contour projection/colorbar rendering and complete D02 remain open; full29 baseline to be reconfirmed.

### Previous targeted verification — field colour limits

Min/max, floor-index percentiles, symmetry and legacy min+1 fallback shared in FieldColorLimits.Resolve; ColorScale constant default unchanged. Eight fixtures from the original FieldPlot labels frozen before the migration. NaN omitted; all-NaN with explicit limits supported. Resolve now rejects out-of-contract inputs (empty/max100k, infinity/±1e12, clip [0,0.5], inverted limits); behaviour of previously unvalidated inputs not guaranteed. Direct legacy normalisation keeps the callers' arithmetic.
Build and PlotSharp suite 154/154 passed, including zero/symmetry, clipping, near-constants, NaN and expanded edge. Web Release/quadro/smoke adoption --no-images rechecked, zero images. Not a new full gate/publication; layout/projection/colorbar rendering/arrows local. 29-step complete baseline earlier than the latest extractions, to be reconfirmed.

### Previous targeted verification — bilinear RGB sampling

Node sampler shared in PlotSharp, Navier FieldToPng facade keeps normalisation/ColorMap and grey mask. Original pixel centres, top-down orientation and bilinear order preserved; fields min 2×2 explicit, callback handles non-finite values, masks in sample coordinates with required colour.
Build and PlotSharp suite 138/138 passed, including frozen PNG fixtures (scanline/dimensions always, compressed hashes on the recorded platform) and sampler cases. Web Release and quadro smoke rechecked, no image or complete benchmark. Normalisation/projection/colorbar/arrows remain local, D02 partial.
Not a new full gate or release; the 29-step complete baseline is earlier than the latest extractions and must be reconfirmed.

### Previous targeted verification — RGB PNG encoder

Only RGB8/filter-0 encoding shared in PlotSharp: Navier keeps sampling/masking and the FieldToPng signature, delegates chunk/CRC/ZLib and uses anti-overflow buffer sizing. Three pre-encoder fixtures frozen as raw hex/hash, not images. Tests always compare scanline/dimensions/CRC, compressed hashes on the recorded runtime/OS; invalid inputs and concurrency verified. Not a complete raster backend.
Build and PlotSharp suite 127/127 passed; Web Release built, quadro with two cycles and adoption smoke --no-images exit 0. No review image or complete benchmark. The 29-step baseline is earlier than contour/palette/encoder and not reconfirmed on all these latest changes. Projection, sampling, normalisation/colorbar/arrows remain local; no publication.

### Previous targeted verification — shared RGB palettes

Immutable ColorPalette centralises RGB with 4/8/9-stop presets; PlotSharp default 4 and Navier 8/9 remain distinct. Historical spacing/truncation preserved, ColorMap facade keeps legacy NaN/infinity, shared API strict on non-finite. PNG/normalisation/colorbar/projection/arrows not migrated.
Before the change: 323 inputs and hashes of the two sources frozen in ops/compatibility/palette-baseline.json, including adjacent stops and colorbar samples. Extension allowed only before the change with matching hashes; fixture copied into the tests, not regenerated. All three palettes coincide on the expected bytes.
Build and PlotSharp suite 117/117 passed; Web Release built and quadro smoke exit 0 with two cycles. No image, new full gate or publication. 29-step complete baseline earlier than contour/palette, to be reconfirmed before attributing it to the whole working tree.

### Previous targeted verification — NavierStokes contour geometry

Only MarchingSquares consolidated: Navier tuple facade → PlotSharp core with a compatibility mode for null segments at the vertices. Coordinates, row order and fixed saddle pairing preserved; previous core default unchanged. Explicit core limits (non-empty field, max100000 samples, finite/level ±1e12) applied, without a promise of compatibility for unbounded inputs. Segments buffered per level.
PlotSharp build and suite 109/109 successful: 16 binary configurations compared with expected geometry, equality, rectangular coordinates, limits and exact SVG path. Web Release built, --quadro rechecked with two cycles, exit 0. No image and no full gate rerun after this change to the graphics kernel.
PNG, FieldPlot/projection, colorbar and arrows remain local; D02 partial and no publication. The 29-step complete baseline below is earlier and must be reconfirmed before attributing it to these latest changes.

### Previous complete verification — consolidation without new images

`ops/ValidateEcosystem.cs -- --no-images`, report D:\Dev\BuildArtifacts\ecosystem-validation\20261028-150530: 29/29 steps exit 0 after all the changes. Build/test, audit, notebooks, NuGet pilot, CLI/Web, calculator/mobile UI, quadro/whistle cancellation and complete numerical selftest rerun. No verification skipped or failure ignored; suspended tests still recorded in the suites/TRX.
The mode prevents saving of screenshots and SVG/PNG in the three visual tools, but keeps in-memory rasterisation/PNG encoding and real XML/DOM/browser checks. Only JSON in the *no-images folders; notebook outputs updated, old images preserved and not declared regenerated. Any temporary images needed by the tests are not suppressed.
Six image-policy tests and ten time-report tests added: operational suite 66/66 verified separately. The gate rejects missing, obsolete/future, malformed reports or with inconsistent SaveImages and archives seven recent JSONs together with the logs. Legacy default of the single tools preserved; recommended use with --no-images.
This complete baseline prevails over the previous targeted notes. It does not close distribution, field renderers, other families or complete accessibility; no package published/overwritten and no new manual visual review.

### Previous targeted verification — contrast of support/accent texts

Real CSS measurements in Edge for direct HTML samples on solid backgrounds: accents before 4.05–4.30:1, muted texts below 4.5. Fix of --accent and --ink-3 of the Web only; SVG series and algorithms not modified. After Web Release build, --mobile exit 0 with 17 samples home/pipe/wind, min about 4.97:1. Layout, keyboard check and calculations keep passing; desktop rechecked, zero images.
Seven new cases of the sRGB contrast calculation: build successful and operational suite 50/50. Report/log in ops/validation/navier-mobile-smoke. Selected texts only, not a complete WCAG audit or of SVG/states/phones; backgrounds with images/gradients and group opacity are unsupported and generate a flag. Future 29-step gate not completely rerun, previous complete baseline distinct.

### Previous targeted verification — controls size and keyboard

Initial measurement min 16 px in the pipe/wind inputs/selects. CSS fix of the NavierStokes Web below 600 px: minimum height 44 px and visible focus. Release build successful; --mobile exit 0 at 360×800: min height 44 px, no document overflow, SVG texts min 11 px; native right arrow updated wind 180→181 km/h, force 1926→1948 N, focus indicator present. Desktop rechecked, exit 0. Zero images and no simulation.
Eight new cases for minimum height: build successful, operational suite 43/43. Report/log in ops/validation/navier-mobile-smoke. Does not certify real touch/hit-testing, contrast, complete accessibility or mobile simulations. Future 29-step gate not completely rerun, previous complete baseline distinct.

### Previous targeted verification — SVG text size and scrolling

Initial measurement of the pipe/wind charts at 360×800: min 5.85 effective px. Local fix in the NavierStokes Web: below 600 px native SVGs/min 560 px in a scrollable container, grid with safe min-width, focusable Chart with name and visible focus. PlotSharp core and data not modified.
Web Release build successful; mobile check exit 0 with min 11 px, no document overflow, keyboard right arrow and end edge of the three charts reached. Desktop pipe/wind rechecked, exit 0. Zero images. Eight new cases of the text-size criterion: build successful and operational suite 35/35 passed. Report/log in ops/validation/navier-mobile-smoke.
The 10 px test threshold is not equivalent to complete readability/accessibility, contrast, screen reader or touch; mobile simulations excluded. Future 29-step gate unchanged and full gate not rerun; previous complete baseline remains distinct.

### Previous targeted verification — narrow viewport, without images

ops/VerifyNavierWeb.cs -- --mobile exit 0, Kestrel+Edge at 360×800: home/pipe/wind, 360 px document without overflow and controls/panels/charts within the horizontal limits. Five pipe controls and three wind, numerical updates verified. No CSS change needed and zero images. Desktop pipe/wind rechecked, exit 0.
Limits validator with eight new xUnit cases (including 1 px tolerance and whole overflow): build successful, operational suite 27/27 passed. Report/log in ops/validation/navier-mobile-smoke. Future 29-step gate compiled, not a new full gate run; previous 25-step complete baseline remains distinct. Does not certify real phones, touch, chart texts, mobile simulations or deployment.

### Previous targeted verification — whistle cancellation, without images

ops/VerifyNavierWeb.cs -- --fischio exit 0: Kestrel/Edge, minimum LBM grid 140×50, two cycles with observed progress, stop and controls restored. --quadro mode rechecked after the sharing of the logic, exit 0. Zero images generated, separate report/log in ops/validation/navier-fischio-smoke.
Two CylinderLbm cancellation/resumption unit tests added: build successful and PlotSharp 98/98 tests passed. No change to the algorithms or the renderers. The gate now plans 28 steps, compilation verified but full gate not rerun (previous 25-step complete baseline remains distinct). Periodicity/Strouhal, convergence/accuracy, mobile, native popups and deployment not certified.

### Previous targeted verification — quadro cancellation

ops/VerifyNavierWeb.cs -- --quadro exit 0: real Kestrel/Edge, grid 31, two cycles with observed progress, parameters disabled during the execution, stop and controls restored. No image generated; log/report in ops/validation/navier-quadro-smoke. Previous pipe/wind parser/calculators rerun, exit 0. Two core cancellation tests added: PlotSharp 96/96 with successful build.
Tool integrated into the gate, now 27 planned steps; full gate not rerun in this tranche (last complete baseline remains the 25-step one). No proof of convergence/accuracy of the simulation, whistle, mobile or deploy. The temporary server is closed by the tool.

### Previous targeted verification — NavierStokes Web, without images

Web Release build successful, Ecosystem.Validation.Tests suite 19/19 (13 new UI parser cases) passed; ops/VerifyNavierWeb.cs exit 0. Real Kestrel server on loopback and Edge: navigation home/pipe/wind, range inputs and Blazor update, shared SVGs and absence of errors. Observed through the DOM: pressure 0.42 → 0.84 bar; force 963 → 3853 → 1926 N for doubled speed and halved area, within the tolerances of the rounded displays.
No image generated. Report/log in ops/validation/navier-web-smoke; server/browser closed by the tool. A possible --screenshot produces a single image. Tool added to the gate: 26 planned steps, **not completely rerun in this tranche**; previous 25-step complete baseline remains distinct. This test does not cover quadro/whistle, mobile, native popups, editorial content or deployment.

### Last previous complete verification — new NavierStokes consumer, 2026-10-28

Complete gate after the changes: `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-104324`, 25/25 steps exit 0. PlotSharp 94/94 tests passed separately (seven new NavierStokes cases); NavierStokes CLI/Web built and the complete original numerical selftest passed. Three linear reports on reduced fixtures verified in Edge and screenshots observed; smoke fixtures are not a proof of regime accuracy.

NavierStokes.Core → PlotSharp for line/scatter through a facade with preserved API. Field/PNG/contour and local utilities remain unchanged and not migrated. New strict validation, clip/layout differences declared and wider Ghia report; Web UI and the article's notebook still to be verified E2E. D02 partial, not completed.

MathemaSharp app inventory: 2 Lab files and 11 Docs .cs/.razor without a local SVG renderer; no fictitious dependencies added. Lab numerical console and Docs Markdown/asset viewer. Lexical scan, not a certification of every asset. D01 remains partial for inventory/API/assets, other families and distribution remain open. No package published or overwritten.

### Previous verification — scenarios and dashboards, 2026-10-28

Gate after all the changes: `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-101452`, 21/21 steps exit 0. MathemaSharp Core: 98/98 verified separately, then reconfirmed in the gate; numerical agreement of the A/D C#/PySharp scenarios protected by the existing tests.

A1 warehouse and D1 photovoltaic migrated: 3/2 SVGs and one KPI group/two answers each. Eight notebooks, 43 SVGs run and XML-validated. The manifest declares the required gap for the warehouse, KPI/quote counts; runner and tests check them. Presentation.csx adds semantic classes without changing styles/texts or introducing host dependencies into PlotSharp.

Edge: 43 individual charts and two HTML-output dashboards assembled at 1280 px desktop width, five tiles/two non-empty answers each and no horizontal overflow. Two full screenshots observed. It does not include markdown, cells, VS Code UI, mobile layout or OS tooltip popup; overlapping targets keep the declared limit. D01 remains partial for apps/other paths, other families still open. No preview published or overwritten.

### Previous verification — STL/tour and manifest, 2026-10-28

Gate after all the changes: `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-075021`, 19/19 steps exit 0. MathemaSharp Core verified separately: 96/96. PlotSharp not modified in this tranche, suite confirmed in the gate. C#/PySharp tour agreement tests active and green.

Migrated TimeSeries/09 (STL/NDSharp, 6 SVGs and tables preserved) and 10 (tour, 5 SVGs); six application notebooks, 38 SVGs really run in JupyterNet and XML-validated. Shared manifest extended with requireTable; the gate automatically generates the migrated steps and runner/tests verify SVGs and required tables. Browser and rasteriser derive the same perimeter from the manifest. Edge verified 38 charts; the 11 new screenshots observed, panels/outliers/legends readable.

D01 remains partial: scenarios, apps and other consumers not all migrated. No complete equivalence of the whole notebook/VS Code or guarantee of individual hit targets in dense charts; OS popups and other families open. Local source-build, no preview package published or overwritten.

### Previous verification — browser and review, 2026-10-28

Gate after all the changes: `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-065717`, 17/17 steps exit 0. PlotSharp 87/87 tests, MathemaSharp Core 94/94, verified separately. New test on the saved browser report, distinct from the real execution.

The rasteriser follows the whole manifest (27 PNGs). The 19 new 06/08 rasters observed; Edge Chromium detected three title/legend collisions on 27 SVGs. Fixed through opt-in SeparateLegendRow, active in IndexedCharts; tooltips added to the stems. Edge rerun: 27/27 with texts in the SVG viewport, no title/text collision, at least one reachable title hit target per chart. Three browser screenshots of the corrected cases observed. Screenshots/report in ops/visual/browser, browser in the gate, requires Edge installed.

Individual reachability of all overlapping targets, native OS popups, VS Code renderer and other browsers not verified. The DOM checks do not replace an exhaustive visual review of the whole UI. Distribution, other families and completion of D01 remain open; no preview published or overwritten.

### Previous tranche — markers and manifest, 2026-10-28

This summary prevails over the historical counts further on. Complete gate: `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-061520`, 16/16 steps exit 0, after all the changes. PlotSharp verified separately: 83/83 tests; MathemaSharp Core: 93/93 tests. PySharp suspended tests still recorded in the TRX, not turned into passed.

Four source-build application notebooks migrated: Core/03 and TimeSeries/06,07,08. Respectively 2/6/6/13 SVGs run in the real JupyterNet host and XML-validated (27 in total); tables preserved. Runner and tests read `ops/migrated-notebooks.json`; smoothing keeps the gap verification. JupyterSharp pilots on packages remain separate.

Clipping of the sample markers fixed through opt-in PreserveBoundaryMarkers: overlay only for finite centres in the closed domain, geometric clip/scales unchanged; legacy core default preserved. IndexedCharts enables the fix. Two regenerated and observed spectra show whole markers at zero and at the maxima. Highlights not modified. Review of 06/08 and browser still open; no promises of tooltip interaction or PNG backend.

D01 MathemaSharp remains partial: four notebooks are not equivalent to a completely migrated family. New APIs only in the local sources, no preview package overwritten or published. The Ecosystem.Validation operational projects remain net10.0; existing scientific libraries net11.0.

### Completed and verified

- [x] net11.0 target for the active projects of the six repositories; .NET 11 RC3 SDK, not GA. Imported snapshots excluded. Exception verified on 2026-10-29: only `Ecosystem.Validation` and `Ecosystem.Validation.Tests` (the gate's operational tools) remain `net10.0`; keeping or migrating is an open decision (R06), not an implicit retargeting.
- [x] MathemaSharp kernels consolidated and validated on multi-format cases and independent oracles: Core, LinearAlgebra, TimeSeries and Quantum.
- [x] NDArray<T>: typed storage, views, slicing, Add broadcasting, Sum, Dot and MatMul; LU/QR/diagnostics bridge without an intermediate double.
- [x] PySharp: Decimal32/64/128 constructors, CLR operators, explicit binding of generic methods, recovery of the twin notebooks and FastAPI/AnyIO compatibility in the tests.
- [x] PySharp: explicit closure of generic CLR types (host-injected List<Decimal128> example), constraints and arity verified. Not equivalent to inference, import of all the generic families or typed NumPy.
- [x] MatSharp: typed real 2D column-major scalars and matrices, indexing, copies, transpose, concatenation and products.
- [x] MatSharp: typed A \\ b through an opt-in adapter: LU on square, QR on overdetermined, multiple RHS columns; tests on the six formats and preservation of 1e-25 in Decimal128. Underdetermined systems explicitly rejected.
- [x] Real JupyterSharp → MathemaSharp FFT → PlotSharp SVG pilot, run through NuGet packages: notebooks/Ecosystem/01-signal-spectrum.ipynb.
- [x] Preview packages generated in the common feed without overwriting; isolated consumer verified also on the latest PySharp APIs and on the MatSharp solver.
- [x] Matplotlib defect: center_baseline of the Y ticks fixed; double rounding eliminated in the snippet, CPython golden unchanged and unrounded test added.
- [x] NuGet audit of the six solutions and repeatable gate with non-zero exit code on findings/failures. No suppression of the problems.

### Verifications: distinguishing baseline and latest changes

Complete green historical baseline: ops/ValidateEcosystem.cs, report in D:\Dev\BuildArtifacts\ecosystem-validation\20261027-223603. 2624 tests passed, zero failed, 13 PySharp suspended; 24 baseline notebooks, PySharp/MatSharp paths and NuGet pilot verified.

Update 2026-10-28: complete gate rerun after the changes, exit 0 for all nine steps. Report: D:\Dev\BuildArtifacts\ecosystem-validation\20261028-005251. Includes build/test of the six solutions, audit, baseline, PySharp kernel/notebooks, MatSharp notebooks and three pilots. Core.Tests also verified separately: 86/86 passed. Do not reuse the previous total 2624 as the count of the new run; the detailed counts are in the suite logs.

The second Ecosystem notebook is now run and verified in the gate, with a precision-preservation marker for Python/MATLAB. The third pilot `03-indexed-charts.ipynb` loads a shared opt-in facade: Lines/Panels/Stems delegate to PlotSharp, four XML-validated SVGs. Previous outputs cleared by the runner before the execution. xUnit test added on the saved outputs, distinct from the real execution. Legacy renderer kept: no equivalence declared for non-finite gaps, layout/tick/tooltip or visual verification. Complete migration of the consumers still open.

Last complete verification after the following tranche: `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-010433`, 12/12 steps exit 0. First application migration: Core/03 uses the PlotSharp bootstrap, two SVGs run in JupyterNet and XML-validated, previous outputs cleared. A dedicated test protects bootstrap/output. MathemaSharp solution also verified separately: 320 tests passed (87 Core and 6 new TRX parser tests included), zero failed/suspended. PySharp: 1507 passed, 13 suspended; report regenerable and kept in the gate. Of the 13 suspended, 12 require a reachable PostgreSQL and one theory requires an opt-in pretrained download. Do not declare the migration of the families or the visual verification completed.

### P0 — close the current tranche

- [x] Run notebooks/Ecosystem/02-typed-language-interop.ipynb and verify references/outputs.
- [x] Second and third pilot in the gate; tests on the saved outputs distinct from the real execution.
- [ ] Complete the API inventory; Usage already contains PySharp generic types and the MatSharp typed solver.
- [x] Rerun the complete gate after these changes: all twelve steps exit 0.
- [x] Register of the reasons of the suspended tests, regenerated from a fresh TRX in every gate; no suspended test deleted or counted as passed.
- [ ] Verify the consistency of the assembly/kernel versions with respect to the packages, without reusing obsolete outputs.

### P1 — complete interoperability and Generic Math

- [ ] PySharp: import/selection of the generic type families, controlled inference, typed arrays and NDArray<T> integration.
- [ ] PySharp: explicit strategy for Span/byref, numerical conversions and typed NumPy; no promises of universal marshalling.
- [ ] MatSharp: typed complex numbers, N-D arrays, exact large integer literals; normalise the name GenericScalar<T>, now also used for matrices.
- [ ] MatSharp: underdetermined/minimum-norm systems and further solver contracts; keep matrix division distinct from element-wise division.
- [ ] NDSharp: runtime/typed cohabitation, checked conversions, complex numbers, batched products and adapters to the shared algorithms.
- [ ] FFT/eig, wavelets and imported distributions: migration by family and tests, avoiding duplication of the kernels in NDSharp.
- [ ] Python AST: complete nodes, visitor, validation and semantics of compile/exec/eval within the declared limits.

### P2 — shared visualisation and execution

- [ ] Migrate the duplicated renderers to PlotSharp by category, keeping compatibility and tests before removing the previous code.
- [ ] Decimal adapters: normalisation in the type before the graphical projection; the first pilot intentionally uses double.
- [ ] Verify native PySharp and MatSharp plugins in JupyterSharp. The C# embedding notebooks are not this proof.
- [ ] Update loaders and application notebooks to distribution through packages, without graphics dependencies in the mathematics kernels.

### P3 — numerical coverage and documentation

- [ ] API-by-API inventory of constraints, verified types, precision and residual conversions.
- [ ] Contracts: domain, overflow, tolerances, mutability/copies/views, representability and cost of the diagnostics.
- [ ] Extend oracles to Half/Decimal32 and further matrices/signals/tails; do not confuse residual and accuracy of the solution.
- [ ] Complete Usage/Reference per library and rerun the notebooks not included in the current selection.
- [ ] Portal: formulas, compiled CSS, MàthemaSharp logo, accessibility, responsive and print verified in the browser.

### P4 — release and operations

- [ ] Reproducible CI, RC/GA policy, metadata/licences and API compatibility of the packages.
- [ ] Make the runners and the versioning manifest-driven: the current scripts still have fixed versions and paths.
- [ ] Audit of projects outside the solution, tools and old packages; application/runtime security distinct from the NuGet audit.
- [ ] Document the update and retirement of the old previews, without indiscriminate deletions or overwrites.

### P5 — performance, explicitly after

- [ ] Reduction of Bluestein allocations, workspace, caching, SIMD and parallelism only after functional stabilisation.
- [ ] Reliable out-of-process benchmarks, comparisons per format, memory and cost of diagnostics/Solve.
- [ ] BLAS/GPU only as separate objectives, without universal speed guarantees.

## Previous picture — historical, not current state

The following tables and checkboxes keep the states of the previous tranches and may contain items that are superseded today. Refer exclusively to the current roadmap above for priorities and completion.

### State of the libraries

| Library/project | Verified state | Limits and open work |
|---|---|---|
| MathemaSharp.Core | Generic Math, precision, DFT and radix-2/Bluestein FFT/IFFT; FFT notebooks and oracles | FFT only in the sources, not in the 0.2.0 packages; widen oracles and contracts |
| MathemaSharp.LinearAlgebra | Matrices, norms, QR/least squares, LU, inverse and diagnostics; multi-format and Hilbert tests | No general SVD/Cholesky or general eigenvalues; conditioning estimate not certified, no refinement |
| MathemaSharp.TimeSeries | Consolidated generic kernels, forecasting and statistics; selected C# notebooks run | Lanczos does not guarantee 34 digits; default Gaussians from double; scenarios and PySharp comparisons not closed |
| MathemaSharp.Quantum | Complex<T>, norm, normalisation, eigenstates, 2x2 Hermitian, MatrixExp; independent oracles | Taylor/scaling-squaring not guaranteed on every matrix; extend extreme cases and dimensionality |
| MathemaSharp.NDSharp | Improved runtime bridge and typed towards LU/QR/diagnostics; 16 tests also via the 0.2.0 package | Runtime path towards NDArray still float64; runtime and generic types separate; local dependency on the sibling repository in the sources |
| NDSharp | NDArray<T>: storage, views, slicing, Add broadcasting, Sum, Dot, MatMul; complete suite 563 tests at the last verification | Not a complete conversion: FFT/eig/wavelet/distributions and runtime APIs not migrated; no batched MatMul; target net10.0 |
| PlotSharp | Last verification: build successful, 70 tests; library, Gallery, Inventory present | Reassess the state of the dedicated session; MathemaSharp/JupyterSharp integration and migration of the charts not verified here |
| MatSharp | Double and typed real 2D column-major matrices, NDSharp 2.8.0 and LinearAlgebra bridge; GenericScalar<T> now includes matrix storage, concatenation, broadcast, products, transpose and assignment; build without warnings and 130 tests passed; Decimal128 notebook run | Typed solver, complex numbers, N-D, large exact literals and packaging open; Plot remains NDSharp.Plot; C# embedding notebook, not a MATLAB kernel. Name GenericScalar<T> to normalise because it now also contains matrices |
| JupyterSharp | Last verification: build successful, 44 tests, 60 warnings | Classify warnings; the MathemaSharp runner still uses the JupyterNet tool: not proof of integration with JupyterSharp |
| Docs portal | Interactive Blazor Server, Markdown, navigation and CSS; existing tests | Tailwind CDN, formulas, accessibility and visual verification to complete |

State updated after the migration: all the active projects of the six repositories target net11.0 (SDK RC3). MathemaSharp: 310 tests passed, none suspended; baseline of 24 notebooks, plus PySharp paths and MatSharp embedding rerun. MathemaSharp 0.2.1-net11-preview.1 packages include the FFTs; NDSharp 2.8.1-net11-preview.1. JupyterSharp 0.3.1-net11-preview.2; PySharp/Pyodbc 2.5.1-net11-preview.2 fix cryptographic dependencies. PySharp: 1503 passed, 1 failed (Matplotlib layout), 13 suspended. NuGet audit of the six solutions without vulnerabilities found from the current sources. This summary and NET11-MIGRATION.md prevail over the historical limits/versions reported in the table.

### Open points — priority before performance

- [ ] Update API inventory/constraints and native coverage per type after the new typed APIs and FFT; avoid global declarations of support.
- [ ] API-by-API contracts: precision, overflow, domain, mutability/copies/views, tolerances and memory costs.
- [x] .NET 11 preview packages generated in the common feed without overwriting; FFT/IFFT and bridge verified in an isolated NuGet consumer.
- [x] Repeatable NuGet audit of the six solutions, with non-zero exit code on findings/errors: `dotnet run --file ops/AuditSciencePackages.cs`.
- [ ] Fix the Matplotlib layout P5_layout, without altering the golden confirmed by CPython.
- [ ] Extend the audit to projects outside the solution and consumers of the tool packages; the previous previews in the feed remain historical.
- [ ] Metadata and licences of the MathemaSharp packages; eliminate obsolete Numbers descriptions, establish versioning and API compatibility.
- [ ] Make the release repeatable: the current operational scripts contain fixed versions and paths; avoid silent repack of the same version.
- [ ] CI and fixed SDK/runtime, RC/stable policy, residual net10.0 targets made explicit.
- [ ] Rerun the NuGet consumer mode when the packages change; keep it separate from the tests on the sources.
- [ ] Oracles on further families of signals/matrices and large lengths; Half/Decimal32 not included in the general mpmath FFT oracles.
- [ ] Extend analysis of conditioning, stability, intermediate overflow and special functions/extreme tails; do not confuse residual with forward error.
- [ ] Reference and Usage updated in every library; other notebooks and scenarios not selected remain unverified.
- [x] PySharp reconnected to the MathemaSharp assemblies: kernel compiled externally, tour and A/D scenarios run in both languages, three agreement tests reactivated and passed. Last BuildAndTest: 304 passed, zero suspended.
- [x] First PySharp precision path: Decimal128Interop in Core, notebook run on .NET 11 with scalar 1+1e-25, CLR array/list and Hermitian Complex<Decimal128>. CLR objects preserved by the existing marshaller, explicit non-generic facade; 307 tests passed, zero suspended.
- [x] First CLR operator dispatch in the PySharp runtime: arithmetic, comparisons and unary; notebook run on the three Decimals with Decimal128 increment 1e-25 and mixed int. Four targeted CLR tests passed, MathemaSharp 308 tests green.
- [x] PySharp builtin module `mathema`: decimal32/64/128 importable, strings/integers/values of the same type, implicit floats rejected. Notebook without local helpers run; 8 targeted PySharp tests green with .NET 11 roll-forward. Requires the .NET 11 runtime to build the IEEE Decimals.
- [x] Explicit binding of CLR methods `method[ClrType](args)`: closure of generics and constraints; MathemaSharp notebook Identity/Determinant2x2/Transpose in Decimal128 run without facades. 12 targeted PySharp tests green; MathemaSharp 309 tests green.
- [ ] Generic inference, construction of generic types, Span/byref parameters, typed numpy and new packages: open. The current binder excludes Span/byref from reflection; do not declare support for all the Generic Math APIs. Floats and different numeric CLR types are not implicitly converted. The general marshalling of decimal/float that existed before was not modified; do not declare universal preservation for any CLR numeric object.
- [ ] Complete PySharp suite and wider regressions of the dispatch: in this tranche only the four new tests and the Decimal notebook were run.
- [ ] NDSharp: decide runtime and typed integration/cohabitation, APIs for the operations still missing, complex numbers, explicit checked conversions; no duplication of the MathemaSharp kernels.
- [ ] NDSharp: FFT/eig, wavelets, distributions and batched products are separate functional work, not completed.
- [x] End-to-end pilot notebook through NuGet: `notebooks/Ecosystem/01-signal-spectrum.ipynb`, real JupyterSharp CLI, MathemaSharp FFT and two PlotSharp SVGs; run and outputs saved. Command: `dotnet run --file ops/RunEcosystemPilot.cs`.
- [x] Ecosystem gate: `dotnet run --file ops/ValidateEcosystem.cs`, external report and no waiver on failures. Last run: audit, baseline, kernel, PySharp/MatSharp notebooks and pilot green; build/test step red because of the PySharp legacy layout.
- [ ] Decimal graphics adapters: the pilot intentionally uses double for data and coordinates; it does not certify the visualisation of Decimal128 differences.
- [ ] Migrate duplicated charts to PlotSharp only after functional verification, keeping the mathematics kernels independent of the renderer.
- [ ] Recheck scripts and configurations of all the repositories for old paths and local outputs; centralised build output does not automatically cover external tools, publish, custom reports and caches.
- [ ] Portal: MàthemaSharp logo, compiled CSS, formulas, responsive, accessibility and print verified in the browser.
- [ ] Sources of the infographics and boundary between implemented APIs and purely conceptual models.

### Performance — explicitly after

- [ ] Reduce Bluestein buffers/allocations and evaluate workspace reuse, caching, SIMD or parallelism only after contracts and tests.
- [ ] Out-of-process benchmarks on .NET 11 recognised by the toolchain; the current measurements are in-process ShortRun on double, not definitive.
- [ ] Comparisons per type and kernel, cost of diagnostics vs Solve, scalability, memory and real scenarios.
- [ ] Evaluate BLAS/GPU only as separate objectives; no universal promise of speed.

## Historical state of the initial tranches (see the current picture above)

- [x] Solution, projects, namespaces and references renamed to `MathemaSharp.*`.
- [x] Separate libraries: Core, LinearAlgebra, TimeSeries, Quantum and NDSharp bridge.
- [x] Notebooks organised in the root `notebooks/`, by library.
- [x] Blazor documentation portal with Markdown navigation, images and table styles.
- [x] Linear algebra extracted from TimeSeries; generic LU, solver, determinant and general inverse added with tests.
- [x] Historical patches and rolling-origin evaluation implemented and tested.
- [x] Last verification after the renaming: build without warnings/errors; 124 tests passed, 3 suspended.
- [x] Initial PlotSharp collection: 148 reference copies and provenance manifest. It is not yet a verified active library.
- [ ] All the projects on the same .NET 11 target: some, including the portal, remain net10.0.
- [ ] Complete conversion of the imported NDSharp material.
- [ ] Rerun of the notebooks after moves and renaming; restoration of the 3 suspended tests.
- [x] Graphic logo updated: the renamed file still contained the old name (MàthemaNet); replaced on 2026-10-30 by the ScienceSharp logo set.

## P0 — inventory and reproducible baseline

- [ ] Survey public APIs, constraints, targets, conversions to double and per-type coverage of each library.
- [ ] Classify each API: verified generic, unverified generic, runtime wrapper, to migrate or out of scope.
- [ ] Verify dependencies and licences of the imported snapshots, keeping attributions and provenance.
- [ ] Fix the available .NET 11 SDK/runtime and the policy for preview/stable updates. Migrate the residual targets only when the environment's constraints allow it.
- [ ] Make build and tests reproducible in CI, separating fast tests, integration and notebooks.

**Completion criterion:** API-by-API inventory and green pipeline; no conversion percentage without an explicit denominator.

## P1 — complete the Generic Math core

### LinearAlgebra
- [x] First QR/least squares tranche: normalised system, Hypot norms, unit reflectors, threshold min(4096 epsilon, sqrt(epsilon)), check of finite data and non-representable intermediates. Ten tests added: six formats, scales 1e-200/1e200, Decimal128 coefficient 1e-25 and invalid inputs. Build without warnings/errors, 175 tests passed and 3 suspended.
- [ ] Widen further the tests of products, norms, QR and LU: ill-conditioned matrices, relative errors/residuals and independent oracles; the QR tranche is not a general certification.
- [ ] Convert imported eigenvalues: balancing, Hessenberg and complex QR; document convergence and edge cases.
- [ ] Evaluate Cholesky and SVD as separate tranches, without declaring them available before implementation and tests.
- [ ] Move the algebra tests still hosted in the TimeSeries project into a dedicated project.

### Signals and transforms
- [ ] Decide boundaries and name of the signals module before extracting the kernels.
- [x] Radix-2 FFT/IFFT in Core: DFT comparison and round trip on the six formats, analytic impulse/constant cases, Decimal128 1e-25, input and overflow. Radix-2 path for powers of two, now accompanied by Bluestein; no general certification.
- [x] Bluestein for non-power-of-two lengths: radix-2 convolution, phases reduced with integer arithmetic, five new tests on prime/composite lengths and six formats. FFT notebook updated and run.
- [ ] Independent high-precision FFT oracles on general signals, large lengths and benchmarks. The small analytic cases are not a general certification.
- [x] NuGet bridge 0.2.0: 16 tests run in UsePackagedBridge=true mode with dependencies from the local feed; MathemaSharp.* mapping corrected.
- [ ] Wavelets and filters: conversion of constants, normalisation, boundary conditions and reconstruction.

### Statistics and probability
- [ ] Inventory the imported distributions, generators and special functions.
- [ ] Extract generic kernels without hidden conversions to double; distinguish numerical computation and random source.
- [ ] Tests of moments, quantiles, domain and reproducibility; non-fragile statistical tests.

### NDSharp — conversion resumed on request
- [x] `NDArray<T>` storage with `INumber<T>`, shape, strides, views, slicing and broadcasting.
- [x] Generic Sum, Add, Dot and MatMul without double intermediates.
- [x] Bridge towards the existing LU, QR and diagnostics; verifications on the six formats and Decimal128 1e-25.
- [x] Usage/Reference notebook run and included in the baseline manifest.
- [ ] Complete conversion of the runtime APIs, FFT, wavelets and distributions: not completed.
- [ ] Batched products and optimisations: not implemented.

NDSharp manages arrays and views; MathemaSharp the mathematics kernels. The new container sits alongside the runtime one. The imported snapshots are not automatically converted.

### Non-NDSharp interoperability
- [ ] Document the non-generic wrappers needed by PySharp, separating them from the mathematical APIs.

**Completion criterion:** every migrated kernel has tests on all the declared types, documented errors and comparison with fixtures/oracles. Generic Math does not mean imposing the same constraint on every operation.

### Quantum — consolidation
- [x] Norm with Hypot on the components, Normalize scaled on the maximum finite component. A finite state can be normalised even when its original norm exceeds the type's range.
- [x] Seven NativeStateNormTests tests: six formats, maximum/subnormal amplitudes, preservation of the inputs, zero/NaN/infinity rejected and Decimal128 component 1e-25. Build without warnings/errors, 182 tests passed and 3 suspended.
- [x] Normalised 2×2 Hermitian diagonalisation, Hypot for the radius, fixed 1e-12 threshold removed, eigenvectors without unstable subtractions and orthonormal complement even in degeneracy. Eight native tests: six formats, maximum/subnormal scales, residuals, zero/identity, Decimal128 coupling 1e-25 and input/overflow. Baseline: 190 tests passed, 3 suspended, build without warnings/errors.
- [x] CheckEigenstate: state/operator scaled separately, Rayleigh quotient and residual relative to the operator's Frobenius norm, validation of tolerance/dimensions/components. Seven tests on the six formats: maximum/subnormal amplitudes, small operators, complex eigenvalue and Decimal128 perturbation 1e-25. Baseline: 197 tests passed and 3 suspended.
- [x] MatrixExp: Frobenius with Hypot, explicit copy and scaling by halvings without 2^s, positive Taylor terms required and finite intermediates verified. TimeEvolution validates finite t and positive finite hbar. Eight tests on the six formats: zero, diagonal, large nilpotent, unitary evolution, Decimal128 term 1e-25 and overflow/input. Build green, 205 tests passed and 3 suspended.
- [ ] Validate MatrixExp with oracles and non-normal matrices/extreme scales: 24 terms give no general guarantee, underflow and loss of small terms during the scaling remain possible. Widen the diagonalisation tests to nearly singular matrices and very unbalanced eigenvalues. The global normalisation can lose components below the range of T; the recovery of the small eigenvalue from the determinant was added in the oracle tranche, but does not eliminate every conditioning/underflow problem.

## P2 — notebooks, Usage and Reference

- [x] Four notebooks `notebooks/<Core|LinearAlgebra|TimeSeries|Quantum>/11-consolidation.ipynb` with focused Usage/Reference, checks in T and real outputs on the six formats, run with JupyterNet/.NET 11. Numerical bootstrap without Charts/NDSharp and runner `dotnet msbuild ops/consolidation-notebooks.proj -t:Run`.
- [x] Fixed the solution path in the runner and the compatibility of the path resolution with Windows PowerShell. Four tests added for evidence of the saved outputs (not rerun).
- [x] Four notebooks `12-api-reference.ipynb` run: catalogue from reflection of exported types and declared public members, signatures and constraints. Core 3 types, LinearAlgebra 4 (including diagnostics), Quantum 4, TimeSeries 36 (including Interop wrappers). Runner: `dotnet msbuild ops/reference-notebooks.proj -t:Run`. Four tests for the catalogue outputs; they do not certify the behaviour of the APIs.
- [x] Core 01–05 rerun on .NET 11 RC3 with real outputs: Generic Math, precision, Complex/DFT, SIMD environment and new Decimal/partial parsing. Contracts added to notebooks 01–03, five tests of the saved outputs and selective runner `dotnet msbuild ops/core-notebooks.proj -t:Run`. The performance notebook does not run statistical benchmarks.
- [x] LinearAlgebra/01-lu and Quantum/01-operators rewritten and rerun: contracts, examples with checks, Reference links, real .NET 11 outputs. LU/residual/inverse/reuse, Decimal128 QR; extreme normalisation, eigenstates, density and Decimal128 TimeEvolution with error ≤1e-28 in the specific case. Two tests on the outputs. Runner: `dotnet msbuild ops/algebra-quantum-notebooks.proj -t:Run`.
- [x] C# TimeSeries 06–08 and tour 10: contracts/limits and Reference links added, rerun with real .NET 11 outputs. Runner `dotnet msbuild ops/timeseries-notebooks.proj -t:Run`, top-level selection and PowerShell wildcards. Four tests of the outputs; the tour has at least 24 saved numerical labels.
- [ ] Complete the remaining contracts and end-to-end numerical verifications; scenarios and PySharp not rerun, three comparisons still suspended. 09 (STL + NDSharp bridge) remains excluded as long as NDSharp is parked.

- [ ] Inventory the console examples and transfer the didactic ones into the notebooks; evaluate whether to keep the console only as a harness.
- [ ] For each library create Usage and Reference notebooks with an index of the APIs, constraints, examples, limits and links to the documentation.
- [ ] Rerun notebooks on documented kernels/runtimes and verify relative paths from their actual position.
- [ ] Restore the suspended C#/PySharp comparisons; distinguish executed, structurally valid and unverified notebooks.
- [ ] Prefer versioned packages to references to artifacts for the examples intended for users.
- [ ] Align README, USAGE and ARCHITECTURE with the real state of the modules.

**Completion criterion:** every notebook published as executed is reproducible; every public API is traceable in the Reference.

## P3 — PlotSharp, external dependency in preparation

The standalone project is in `D:\Dev\2027\repos\PlotSharp`; the extraction objectives are in its `OBJECTIVES.md`. The initial collection does not guarantee all the dependencies and does not prove compatibility or compilation.

- [ ] Wait for the inventory, licence verification and first stable API with renderer/tests in PlotSharp.
- [ ] Survey the local Charts implementations and their users before removing code.
- [ ] Integrate PlotSharp in a pilot notebook, then migrate the other examples progressively.
- [ ] Keep the mathematical notebooks in MathemaSharp; the gallery of the charts belongs to PlotSharp.
- [ ] Limit the graphics dependency to the samples and the adapters: the mathematics kernels must not depend on the renderer.
- [ ] Remove Charts duplications only after functional equivalence and visual verification.

**Completion criterion:** migrated examples verified and no graphics dependency introduced into the kernels. NDSharp, JupyterSharp and NavierStokes are updated in authorised tranches in their respective repositories.

## Consolidation started — Core

- [x] NearlyEqual fixed: default tolerance min(4096 epsilon, sqrt(epsilon)), to avoid a default of about 4 on Half. It is a behavioural change for the low-precision formats; use the explicit overload when an application policy is needed.
- [x] Seven NativePrecisionTests tests added: comparisons/special values and small examples Sum, Mean, Kahan, Horner, VectorDot, DFT and HermitianDot on Half, float, double, Decimal32/64/128, with fixtures and comparisons in T.
- [x] Decimal128 test that preserves a difference of 1e-25 in sum/dot product and detects it with an explicit tolerance, without passing through double.
- [x] First TimeSeries tranche: LOESS with centred abscissae, margin 999/1000 computed in T, thresholds tied to epsilon and validation of observations/weights/abscissa.
- [x] Nine NativeLoessTests tests: lines on six formats, Decimal128 trend 1e-25, large indices and invalid inputs. Baseline: build green, 140 tests passed and 3 suspended.
- [x] GARCH variance targeting: grid with integer indices, stationarity margin computed in T and bounded by the predecessor of 1, invalid inputs rejected and finite likelihood required. Seven native tests: recurrence/forecast/reproducible fit on the six formats and wrong inputs. Baseline: 147 tests passed, 3 suspended, build without warnings/errors.
- [x] Simulation: ARMA, random walk, GARCH and VAR overloads with a Func<Random,T> innovation source; the defaults keep double Box-Muller. Nine tests: recurrences and call count on six formats, Decimal128 differences 1e-25, default compared with an explicit legacy source and invalid inputs. Baseline: build green, 156 tests passed, 3 suspended. A high-precision Gaussian RNG was not implemented.
- [x] First SpecialFunctions tranche: Lanczos coefficients and threshold from invariant strings in T with cache, explicit domain/NaN/infinity, identities for LogGamma(1/2,1,2) and Q(1,x), flagging of non-convergence/overflow. Nine tests: identities and recurrences on the six formats, Decimal128 difference 1e-25 in the exponential case, iteration limit and overflow. Baseline: 165 tests passed, 3 suspended, build without warnings/errors.
- [ ] Wider validation of the special functions (extreme tails, independent oracles, quality of the approximation and [0,1] clamp), simulation parameters/overflow of the recurrences and LinearAlgebra/Quantum coverage; also verify the cost of the repeated LOESS validation inside STL.
- [ ] Usage/Reference notebooks and rerun: not run in this tranche.

## Independent validation — first tranche

- [x] mpmath 1.3.0 fixture: 200 working digits, 160 saved, generator independent of MathemaSharp and isolated Python dependency. Versionable fixture in ops/oracles/numerical-reference.json; no Python needed for dotnet test.
- [x] 12 gamma Q cases and 7 LogGamma compared on float/double/Decimal64/Decimal128; chi-square verified for the equivalent parameters with integer degrees. Relative tolerance 0.002 for float and 1e-10 for the others; float underflow explicitly distinguished. Half/Decimal32 not certified on these tails.
- [x] Hilbert 3/5/8: LU and QR compared with an independent solution on double/Decimal128, residuals and determinants. Allowed solution error depends on the conditioning, not a universal tolerance.
- [x] Hilbert12 double limit documented: LU accepts the system with relative residual about 1.31e-16 but maximum error of the solution about 0.00595; QR rejects the numerical rank. The LU threshold was not arbitrarily raised.
- [x] Notebook 13-independent-oracles run, with maximum relative errors on the gamma fixtures about 1.46e-12 double and 2.46e-13 Decimal128; no promise of 34 Lanczos digits.
- [x] Quantum oracles: 5 MatrixExp from mpmath.expm (Jordan 2/3, stiffer and complex case) on float/double/Decimal64/128; 3 Hermitian spectra from eigh on double/Decimal128, including eigenvalues 1e-40. Six numerical tests and a notebook test added.
- [x] Fixed a defect found by the oracles: diagonalisation returned zero on diag(1,1e-40). Diagonals handled directly, eigenvalue subject to cancellation derived from the determinant with FMA and from the other eigenvalue. Notebook Quantum/13 run: maximum scaled MatrixExp error about 1.98e-14 double and 3.38e-32 Decimal128 on the 5 fixtures.
- [x] Opt-in diagnostics LinearSystemDiagnostics.Solve<T>: LU solution, infinity relative residual, normwise backward error, condition/rcond and warning rcond ≤ sqrt(epsilon). n additional solves for the norm of the inverse, without storing it; it is not a cheap iterative condition estimator or a forward certification. Native tests on the six formats and condition against mpmath; notebook LinearAlgebra/13 run and References regenerated.
- [ ] Extend further: harder non-normal matrices, nearly zero determinants, rounded inputs, gamma cases near cancellation/underflow, noisy overdetermined regressions; component-wise diagnostics, refinement and cheap estimate of the conditioning.

## P4 — numerical validation and performance

- [ ] Targeted Decimal32/64/128 tests: precision, conversions, NaN/infinity, rounding, parsing and transcendental functions.
- [ ] Widen Complex<T> tests, relative/absolute errors and extreme cases per algorithm.
- [ ] Direct benchmarks of the formats, FFT, LU and other kernels, without parsing/reflection in the loops.
- [ ] Run reliable BenchmarkDotNet jobs, documenting hardware, SDK/runtime, allocations and configuration.
- [ ] Separate Dry smoke tests from measurements usable for comparisons; no universal promise of speed.

## P5 — deepening and scenarios

- [ ] Apply the already implemented rolling-origin to the refrigerator and photovoltaic scenarios with comparable windows and without leakage.
- [ ] Build scenarios B (compressors), C (aqueduct), E (CNC).
- [ ] Evaluate Diebold-Mariano/Clark-West, conformal prediction, matrix profile, PELT/BOCPD.
- [ ] Extend ARIMA with exact likelihood, Kalman with q/r estimation and RTS smoother, state-space ETS and ARIMA/VAR intervals.
- [ ] Complete the cards of the atlas theories and verify the primary sources; distinguish concepts, baselines and complete models.
- [ ] Update logo and brand in the portal; verify responsive rendering, print, accessibility and formulas.
- [ ] Replace Tailwind CDN with compiled CSS before productive use.

## Automated baseline

- [x] Single command `dotnet msbuild ops/validate.proj -t:Baseline`: Release build, tool restore, 22 explicit notebooks, Release tests and JSON/log report with SDK/runtime and counts. Run successfully: 272 tests passed, 3 suspended; 7 notebooks excluded with reasons.
- [x] BuildAndTest mode records notebooks not executed and FullBaseline=false. Manifest validated by two new tests, no automatic selection of NDSharp/PySharp/scenarios.
- [ ] CI with fixed SDK/runtime and reproducible feeds; statistical benchmarks separate from the functional baseline.

## P6 — release

- [ ] Define versioning, API compatibility and changelog.
- [ ] Verify packaging, licences, metadata and consumption of the packages from a clean external project.
- [ ] Publish/install only on request, after build, tests and verification of the examples.

## Out of scope for these tranches

Complete symbolic algebra, regulatory finance, alternative implementations of the Decimal types, a complete custom INumber, optimised GPU/BLAS and complete Transformers/foundation models. SVD, eigenvalues and FFT are objectives to evaluate/implement, not capabilities already guaranteed.

## Recommended next tranche

1. Update inventory, contracts and documentation to the current code.
2. Release a new local version containing FFT/IFFT and verify it via NuGet.
3. Verify a pilot notebook with JupyterSharp and PlotSharp.

Performance postponed. NDSharp was resumed; complete runtime conversion, general eigenvalues and wavelets remain open. The FFTs in Core are implemented and verified, but not yet in the 0.2.0 packages.
