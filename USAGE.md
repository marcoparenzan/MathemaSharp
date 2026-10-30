# Usage

## Installation

MathemaSharp is a set of **libraries** plus the global tool **`mathema`**. Requires the **.NET 11 SDK**.

```powershell
dotnet add package MathemaSharp.Core          --source D:\Dev\NuGetLocalFeed --prerelease
dotnet add package MathemaSharp.LinearAlgebra --source D:\Dev\NuGetLocalFeed --prerelease
dotnet add package MathemaSharp.TimeSeries    --source D:\Dev\NuGetLocalFeed --prerelease
dotnet add package MathemaSharp.Quantum       --source D:\Dev\NuGetLocalFeed --prerelease
dotnet add package MathemaSharp.NDSharp       --source D:\Dev\NuGetLocalFeed --prerelease   # bridge to NDArray
```

### The `mathema` tool

```powershell
dotnet tool install --global MathemaSharp.Cli --add-source D:\Dev\NuGetLocalFeed --prerelease
mathema stats data.csv --column v
mathema acf data.csv --column v --lags 12
mathema diff data.csv --column v --lag 1
mathema forecast data.csv --column v --method ses --horizon 6    # naive | mean | drift | seasonal-naive | ses
mathema selftest
```

It reads one numeric column of a CSV (empty and non-numeric cells are skipped). Exit codes: 0 ok, 1 unknown command, 2 usage or
input error. Build and test from source: `dotnet test src/tests/MathemaSharp.Cli.Tests`.

This repository also holds the **ecosystem gate**, which validates the whole suite:

```powershell
dotnet run --file ops/ValidateEcosystem.cs -- --no-images    # 32 steps plus skip-report, over 10 minutes
```

Notebooks run in JupyterSharp: install the tool with `D:\Dev\Science\ScienceSharp\Install-Suite.ps1 -Tools`, or the gate uses the
CLI built from source. The remaining sections are the day-to-day instructions and a dated record of the verification tranches (translated from Italian on 2026-10-30; where they mention JupyterNet, that tool has since been removed from the suite and replaced by JupyterSharp).

## Full baseline after NDSharp sampler and bands

`dotnet run --file ops/ValidateEcosystem.cs -- --no-images` rerun after RectilinearFieldSampler and ContourBands: D:\Dev\BuildArtifacts\ecosystem-validation\20261028-215324, 31/31 steps exit 0. Build/test/audit, package/source kernel paths, real notebook, UI/browser and complete numerical selftest, no check skipped.
No new screenshot/review image file; functional PNG outputs remain embedded in the notebook, QA raster run in memory. Only JSON in the *no-images folders, recent evidence archived. Known build warnings and suspended tests remain declared. It prevails over the previous targeted notes; not a complete migration of NDSharp.Plot nor a release.

## Filled contour bands — shared selection

ContourBands keeps NDSharp's inclusive lookup: an exact inner level goes to the last matching band, final limit included, external/NaN/infinity values without a band. Finite non-decreasing levels (duplicates allowed), snapshot copy; a single level contains no bands. Midpoint/full-range normalised with the original arithmetic, not reassociated.
Palette/alpha/bitmap/Skia remain NDSharp. Targeted build and tests PlotSharp227/227, ND19/19 passed; kernel source rebuilt and real lines/bands320×240 notebook rechecked, no review image file. DLL metadata requires ContourBands. Pre-existing kernel warnings visible; not a new full gate/release.

## Physical sampler of filled contours — targeted verification

PlotSharp's RectilinearFieldSampler shares the old NDSharp Sample, with binary search and four-term arithmetic. The 81 queries frozen before the change coincide exactly, including stops/ULP and extrapolation. NDSharp DrawFilled keeps bands, palette/alpha and Skia; no change to the Navier uniform RGB sampler.
Finite strictly increasing axes min2×2, consistent row-major data and copied snapshot; NaN/Infinity in the data propagate to the caller. Finite query coordinates, without new clamping. Inputs/mutations not validated before are not guaranteed compatible; linear contours separately keep the support for descending axes.
Build/test: PlotSharp211/211, NDSharp.Plot19/19. Kernel source-plots rebuilt and real contour notebook verified with two embedded PNGs320×240, no review image file. DLL metadata includes the new sampler. Pre-existing kernel warnings not suppressed; not a new full gate/release.

## NDSharp contour in the real kernel and current complete baseline

```sh
dotnet run --file ops/BuildPySharpKernel.cs -- --source-plots
dotnet run --file ops/RunNDSharpContourNotebook.cs
dotnet run --file ops/ValidateEcosystem.cs -- --no-images
```

The Matplotlib binding keeps the default preview package; ScienceUseSourcePlots=true enables only the NDSharp.Plot source reference. Separate kernel in D:/Dev/BuildArtifacts/PySharp/kernel-source-plots; default in the kernel unchanged. The runner verifies DLL metadata (PlotSharp reference and RectilinearContours type), clears the outputs and starts the real JupyterNet/PySharp plugin on .NET11.
Notebook: notebooks/NDSharp/pysharp/02-contours-kernel.ipynb, not a JupyterSharp pilot. Lines+bands produce two embedded PNGs320×240, verified without saving them as review image files. Report: ops/validation/ndsharp-contour-notebook/report.json; saved-output tests and opt-in source policy distinct from the real execution.
Last gate after these changes: D:\Dev\BuildArtifacts\ecosystem-validation\20261028-203820, 31/31 exit0. Pre-existing warnings of the PySharp projects remain declared; no suppression/release. Not a CPython pixel proof, native JupyterSharp or complete migration of the family.

## NDSharp.Plot: effective migration of linear contours

The DrawLines geometry now delegates to PlotSharp.RectilinearContours/ContourKernel. Physical/non-uniform X/Y coordinates, edge direction, pairing and skip of the non-finite corner sum preserved. Extreme levels/AutoLevels, paint/styles/Skia, Axes and contourf remain ND. The previous index-based kernel uses the same engine with its own convention, without changing the defaults.
Targeted build and tests: NDSharp.Plot18/18, PlotSharp204/204, no review images. Updated report: ops/inventory/ndsharp-plot-contract.json, LineContourGeometryDelegated=true and FamilyMigrationComplete=false. New ProjectReference, no complete migration or published preview; reconfirmation of the full gate/notebook and distribution of the DLLs still missing.

## NDSharp.Plot: inventory and baseline, not migration

`dotnet run --file ops/InventoryNDSharpPlot.cs` (after building the NDSharp.Plot tests) records dependencies/hashes and reruns the targeted tests without images. Report/log: ops/inventory/ndsharp-plot-contract.json and ndsharp-plot-tests.log.
Active source NDSharp/src/NDSharp.Plot, net11.0, NDSharp + SkiaSharp/native assets 3.119.0 and DejaVu fonts. Only the linear contour geometry is a candidate; Xs/Ys are non-uniform physical coordinates, extreme levels not drawn, non-finite cells skipped. AutoLevels/Ticks, banded contourf and Figure/Axes/PNG/styles remain separate.
Four new in-memory baseline tests: physical coordinates, NaN-cell skip, endpoint suppression, filled bands. Build and suite18/18 passed. Production renderers and dependencies not modified, no adoption declared, no full gate or publication.

## Current complete baseline, without review images

`dotnet run --file ops/ValidateEcosystem.cs -- --no-images` was rerun after the complete delegation of the FieldPlot renderer. Report: D:\Dev\BuildArtifacts\ecosystem-validation\20261028-185732, 29/29 steps exit 0. Build/test, audit, notebooks/pilots, CLI/Web, UI/mobile/cancellation, numerical selftest, in-memory raster and browser verified in sequence; no check skipped.
No new review image/screenshot, only JSON in the *no-images folders and seven recent reports archived with the logs. Notebook outputs updated as expected, previous images not regenerated. This baseline prevails over the targeted notes further on; it does not certify all the accessibility, every input/consumer or the distribution.

## FieldPlot: shared SVG assembly

Charts.NodeFieldPlot/NodeFieldOptions assemble the node raster fields using shared layout, limits, RGB palette, sampler/PNG, contour, vectors and colorbar. Navier FieldPlot keeps the options type/signature and translates in a facade; unused private rendering helpers removed. Cell-based Heatmap remains a distinct renderer.
Before the change four structural outputs (gradient, contour, vectors, obstacle) were frozen with decoded RGB hashes: only GUIDs and PNG payload normalised in the comparison, not in the real output. Markup and pixels coincide; previous fixtures remain green. Overlays now require the same shape as the field, contour count0–32 and finite non-negative obstacles: out-of-contract inputs not promised compatible.
Build/suite PlotSharp198/198, Web Release/quadro/smoke --no-images rechecked. Zero images, not a new full gate/publication. Scenario/UI/numerics remain in the consumers; not a universal RGBA/Figure-Axes backend and D02 not yet certified complete.

## Shared node frame and projection

NodeFieldLayout centralises Navier margins/proportions, the historical Math.Round of the height, colorbar position and X/Y projection (row zero at the bottom). It does not replace the Heatmap cell-centre map. Finite indices outside the grid are extrapolated as before, not clipped; non-finite/overflow indices rejected.
Create requires at least2×2/max100000 nodes, width1–10000 and projected height1–10000. Proportions that collapse to zero now fail before the raster. FieldPlot keeps assembly/styles/header, uses the layout for frame and contour.
Build and PlotSharp suite 192/192 passed, including even rounding, rectangular proportions, corners/fractional indices and previous fixtures. Web Release/quadro/smoke --no-images rechecked, zero images. Not a new full gate/publication: the last complete baseline remains earlier than these extractions.

## Compatible gradient colorbar

GradientColorbar.Render shares the Navier fragment: 11 RGB samples, 3 ticks, bottom→top gradient, rotated label with legacy integer half height, previous escaping and geometry. FieldPlot keeps the positioning; the 128-stripe Heatmap colorbar does not change.
Three markups frozen before the change (normalised only for random ids) coincide, including the odd-height case. New colour callback requires #RRGGBB; finite and ordered limits/projections. Build/suite PlotSharp 178/178 passed; Web Release/quadro/smoke --no-images rechecked, zero images.
Not a new full gate or publication; overall frame and contour projection still local. Not a proof of equivalence for inputs not validated before or of the visuals on all browsers.

## Compatible arrows on node grids

NodeVectorOverlay.PathData centralises the Navier geometry: normalisation to the global maximum (floor 1e-12), stride/2 offset, relative threshold 0.02, node projection with inverted Y and open heads. It does not modify the Heatmap quiver (cells/physical coordinates/filled heads). SVG style and field frame remain Navier's.
Four FieldPlot paths frozen before the migration cover rectangle, four directions, zero and threshold. Shared validation requires U/V of the same shape, min2×2/max100k, finite values ±1e12, positive stride and valid projection. Inputs not validated before are not guaranteed compatible; legacy arrows not clipped at the edges.
Build/suite PlotSharp 168/168 passed; Web Release/quadro smoke and adoption --no-images rechecked, including geometry in the pressure report. Zero images, not a new full gate/publication. Colorbar rendering and contour projection remain local.

## Field colour limits — shared legacy selection

FieldColorLimits.Resolve centralises min/max, truncated-index percentiles, symmetry and legacy fallback: if the range is <1e-12, max=min+1. NaNs ignored in the statistics; an all-NaN field requires both limits explicit. It does not replace ColorScale's constant policy (centre 0.5).
Resolve contract: 1–100000 samples, finite ±1e12 or NaN, finite clipping in [0,0.5], input limits finite ±1e12 and ordered. The legacy expansion can exceed the input edge by 1. The directly built record/NormalizeLegacy keeps the caller's arithmetic, including a zero denominator; it does not have the Resolve guards.
Eight fixtures read from the FieldPlot colorbar labels before the change, plus edge cases; build and PlotSharp suite 154/154 passed. Web Release/quadro/smoke adoption --no-images rechecked. Zero images and no full gate or publication; layout/projection, colorbar rendering and arrows local.

## Shared bilinear RGB sampling

BilinearRgbSampler prepares RGB8/filter-0 scanlines from a node field (minimum 2×2), with row zero at the bottom and top-down output. The colour callback receives the interpolated value; masks receive sample-index coordinates and require an explicit colour. The legacy arithmetic order and the pixel-centre coordinates are preserved.
Navier FieldToPng delegates sampling/encoding, keeping signature, normalisation and ColorMap policy. The three frozen PNG references continue to coincide on scanlines/dimensions and compressed hashes on the recorded platform. New tests on centre, orientation, masks/callback, unchanged inputs and invalid shapes: build and PlotSharp suite 138/138 passed.
Web Release/quadro smoke rechecked, zero images. Normalisation, projection/colorbar/arrows remain local; not a universal raster or a new full gate/publication.

## RGB8 PNG: shared encoder, not a complete raster

PlotSharp's RgbPngEncoder encodes top-down RGB8 scanlines with filter 0, positive dimensions/lengths verified and immutable static CRC. Navier FieldToPng keeps bilinear sampling, masks, palette/normalisation and signature, delegating only the PNG bytes. No new support for alpha, other filters or Figure/Axes rendering.
Before the migration three small fields (gradient, diverging, masked) were frozen in ops/compatibility/png-baseline.json, as scanline hex/hash, not image files. The tests always compare pixels/dimensions/CRC; the compressed hash is compared only on the recorded runtime/OS, because ZLib can vary between platforms. Invalid inputs and concurrency covered.
Build and PlotSharp suite 127/127 passed; Web Release, quadro and adoption smoke --no-images rechecked. No review image and no full gate or publication. The previous 29-step complete baseline must be reconfirmed before attributing it to the current code.

## Shared RGB palettes — byte compatibility verified

PlotSharp's ColorPalette now centralises RGB and truncation with immutable presets: CompactSequential (4 stops, PlotSharp default), Sequential (8, Navier) and Diverging (9). The interval calculations remain explicit to preserve the historical bytes; the palettes were not substituted for one another. Navier ColorMap is a facade with legacy policy NaN→first colour and infinity→extreme; the shared/default PlotSharp API rejects non-finite values.
Before the change 323 inputs (dense grid, adjacent stops/ULP, colorbar samples) were frozen in ops/compatibility/palette-baseline.json, with hashes of the previous sources. The tests copy the fixture, they do not regenerate it. CapturePaletteBaseline keeps the existing file; pre-migration extension allowed only if the legacy hashes still match.
Build and PlotSharp suite 117/117 passed, all three palettes match the references; Web Release and quadro smoke rechecked, zero images. PNG, field normalisation, colorbar/projection/arrows remain local. New source-build API only: not a new full gate nor publication.

## NavierStokes contour: shared geometry, targeted verification

NavierStokes MarchingSquares now delegates to PlotSharp keeping the tuple signature, sample coordinates and fixed pairing of the saddle cases. The compatible mode keeps the null segments at the vertices exactly on the level; PlotSharp's historical default continues to eliminate them. NavierStokes SVG projection, PNG, colorbar and arrows remain local.
Shared contract: non-empty field, max 100000 samples, finite values/level within ±1e12. The current UI grids fall within the contract; inputs not validated/unbounded before are not guaranteed compatible. The core buffers the segments of each level; it is not a promise of streaming/performance equal to the old iterator.
Targeted PlotSharp build/tests: 109/109; Web Release built and --quadro rechecked with two cycles, exit 0, no image. Not a new full gate or numerical convergence benchmark. New overload source-build only, no publication.

## Full verification without new review images

```sh
dotnet run --file ops/ValidateEcosystem.cs -- --no-images
```

Recommended mode to reduce visual artifacts: all 29 steps run, no verification skipped. The PNGs are rasterised/encoded and checked in memory; screenshots and SVG/PNG files of the three visual tools are not saved. The JSON reports and the updated notebook outputs remain. The previous images are neither deleted nor presented as regenerated; any temporary files used by the tests are not disabled.
The tools RasterizeNotebookEvidence, VerifyBrowserCharts and VerifyNavierAdoption also accept --no-images directly. Separate reports in ops/validation/*no-images; without the flag the legacy saving behaviour remains. The gate verifies GeneratedUtc within the real window of the step and a consistent SaveImages, then copies seven JSON reports into its own timestamped archive. An obsolete/malformed report or a different policy fails the step.
Last complete verification: D:\Dev\BuildArtifacts\ecosystem-validation\20261028-150530, 29/29 exit 0. Operational suite verified separately: 66/66, with tests on mode and freshness. It is not a new manual visual review nor a release.

## Targeted contrast of the Web texts

The --mobile check now also measures supporting/accent HTML texts (.hint, .eyebrow, .card .tag, brand and footer with direct text), using CSS colours and composited solid backgrounds. The initial measurements of the accents were 4.05–4.30:1; the muted texts did not reach 4.5:1. Two Web tokens were darkened: --accent #2467ba and --ink-3 #6b6a65. SVG series and calculations unchanged.
Web Release build and --mobile passed: 17 samples across home/pipe/wind, min about 4.97:1; layout/texts/sizes/keyboard/calculations still green. Desktop rechecked. Seven new cases on the sRGB contrast calculation bring the operational suite to 50/50. No image, report in ops/validation/navier-mobile-smoke.
It is not a full WCAG audit, of all texts/SVGs, of devices or of the buttons in every state: backgrounds with images/gradients and group opacity not supported are flagged, not ignored. Future 29-step gate unchanged, full gate not rerun.

## Narrow controls and keyboard — current verification

The --mobile command also verifies the height of inputs/selects (44 px criterion) and a native keyboard update of the Wind range. The initial minimum measurement was 16 px; below 600 px the Web app's CSS brings input/select to min-height 44 px and adds a visible focus.
After Web Release build, test passed at 360×800: pipe 5 controls and wind 3, minimum 44 px, document 360 px, SVG texts min 11 px. Right arrow: 180→181 km/h and force 1926→1948 N, focus indicator verified through DOM/style. Desktop rechecked, no image.
Eight new cases of the height criterion: operational suite 43/43, build successful. Report/log in ops/validation/navier-mobile-smoke. Element height and keyboard do not certify real hit-testing/touch, contrast or full accessibility; mobile simulations not tested. Future 29-step gate unchanged, full gate not rerun.

## Charts on a narrow viewport: texts and scrolling

The --mobile check now measures SVG texts through CSS font and the getScreenCTM matrix, with a test threshold of 10 effective px. The initial measurement was about 5.85 px; the NavierStokes Web now keeps the chart's native width below 600 px and scrolls only the box. The Chart component is a named, focusable region, with a focus indicator.
After Web Release build, `dotnet run --file ops/VerifyNavierWeb.cs -- --mobile` passed: home/pipe/wind at 360×800, document 360 px, SVG texts minimum 11 px, keyboard right arrow and end edge reachable in the three charts. No image; desktop rechecked. Validator of the text criterion: eight new cases, operational suite 35/35 passed.
Report/log: ops/validation/navier-mobile-smoke. The pixel threshold does not certify all the readability/accessibility, contrast, screen readers or touch. Mobile simulations not verified. Future gate remains 29 steps, full gate not rerun; no change to the data or the PlotSharp core.

## Narrow viewport: targeted check, without images

`dotnet run --file ops/VerifyNavierWeb.cs -- --mobile` (Web already built in Release, Edge installed). Tests home/pipe/wind at 360×800: document width, horizontal limits of the controls/panels/charts and update of the calculations. It does not start simulations. --mobile mode alternative to --quadro/--fischio, separate report in ops/validation/navier-mobile-smoke. Server/browser closed at the end.
Test passed without CSS changes: document 360 px in all the pages, 5 pipe and 3 wind controls within the limits. Eight xUnit cases for the limits validator (1 px tolerance), operational suite 27/27 green. Desktop pipe/wind path rechecked.
Zero screenshots by default, optional --screenshot saves a single image. It does not certify a real phone, touch, readability of the SVG texts, simulation pages or deployment. Gate extended to 29 planned steps and compiled, full gate not rerun.

## Whistle: targeted start/stop, without images

`dotnet run --file ops/VerifyNavierWeb.cs -- --fischio` (after Web Release build, Edge installed). LBM UI grid 140×50; two cycles with real progress, controls locked during the simulation, stop and controls restored. Separate report: ops/validation/navier-fischio-smoke. Temporary server/browser closed at the end.
The --quadro command remains compatible and was rechecked. The two modes are alternative; optional --screenshot produces a single image, none by default. Two new LBM cancellation/resumption cases bring the PlotSharp suite to 98/98 passed. The gate now plans 28 steps and is compiled, but was not completely rerun in this tranche.
It does not verify periodicity, measured frequency/Strouhal, convergence, accuracy, mobile or deployment.

## Quadro: targeted start/stop, without images

```sh
dotnet run --file ops/VerifyNavierWeb.cs -- --quadro
```

Requires NavierStokes.Web already built in Release and Microsoft Edge. Starts a temporary server on loopback, verifies interactivity, selects grid 31 and observes two cycles with effective progress, locking of the parameters during the simulation, stop and restoration of the controls. No benchmark or wait for convergence. Separate report/log: ops/validation/navier-quadro-smoke. Server and browser closed at the end.
Zero images by default; optional `--screenshot` saves a single image. The previous check without --quadro remains pipe/wind and was rerun. Two new core tests on cancellation/resumption: PlotSharp suite 96/96 passed. Gate extended to 27 planned steps but not completely rerun in this tranche; the 25-step complete baseline is distinct. Whistle, mobile and deployment remain open.

## Targeted Web verification, without images by default

After a Release build of NavierStokes.Web, from MathemaSharp:

```sh
dotnet run --file ops/VerifyNavierWeb.cs
```

Starts Kestrel on loopback with a temporary port and an isolated Edge, navigates from the home to pipe/wind and sends input events to the real ranges. Verifies doubling of length/pressure, quadratic speed/force and linear area/force with tolerances for the UI roundings; checks shared SVGs, Blazor and browser errors. Server and browser are closed at the end of the test. Report/log: ops/validation/navier-web-smoke. Numeric parsers covered by 13 new xUnit cases (operational suite total 19).
No screenshot by default; `--screenshot` produces at most one image of the wind page. The check is added to the gate (26 planned steps), but the last complete verification remains the 25-step one: this tranche was verified with targeted build/tests and the live tool, not with a new full gate.
Scope: desktop home/pipe/wind, not quadro/whistle simulations, mobile, editorial content or deployment.

## NavierStokes adoption and app inventory

`dotnet run --file ops/VerifyNavierAdoption.cs` generates three real NavierStokes linear reports on smoke fixtures, XML, Edge screenshots and a report in ops/visual/navier. Requires the checkout `articoli/2026.10 NavierStokes` and Edge installed. It does not measure regime accuracy and does not certify the Web UI.
NavierStokes.Core delegates line/scatter to PlotSharp through a facade; fields/PNG/contour remain local. Gate extended: CLI and Web build, complete original numerical selftest and report smoke. Total: 25 steps. The physical selftest is more expensive than the notebook checks alone.
The MathemaSharp apps had no local renderers in the inspected .cs/.razor sources (Lab: 2 files, Docs: 11): no superfluous graphics dependency added. Lab is a numerical console; Docs is a Markdown/asset viewer. Lexical inventory, not a certification of every external asset. Source-build only, no publication.

## Application scenarios — current state

After a Release build of MathemaSharp and PlotSharp:

```sh
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id cold-storage
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id solar
```

The manifest covers eight notebooks, 43 SVGs. Cold-storage warehouse: 3 SVGs, required gap, one KPI group and two textual answers; photovoltaic: 2 SVGs, one KPI group and two answers. `expectedKpi`/`expectedQuotes` are verified by the runner and by the tests through the semantic classes mathema-kpi/mathema-quote (styles and texts unchanged).
The browser verifies 43 individual SVGs and assembles the HTML outputs of the two scenarios: five tiles per dashboard, two non-empty answers, no horizontal overflow at a 1280 px desktop viewport. The two full-page screenshots were observed. It is not a test of the notebook/markdown interface or of VS Code, nor of mobile layout, native popups or individual access to overlapping samples.
Screenshots/report: ops/visual/browser; gate: 21 steps. This section prevails over the following historical counts.

## Current tranche — STL and tour

The manifest now covers six notebooks: Core/03 and TimeSeries/06–10, 38 SVGs in total. Additional commands, after a Release build of MathemaSharp and PlotSharp:

```sh
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id stl
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id tour
```

STL: 6 SVGs and required tables; tour: 5 SVGs without an obligation of tables. `requireTable` in the manifest is verified by the runner and by the tests, `requireGap` keeps the smoothing check. The gate automatically generates the migrated steps from the manifest, without duplicating the list of ids. The browser and the rasteriser follow the same manifest; 38 charts verified in Edge, the 11 new screenshots observed. Gate: 19 steps. Browser prerequisite: Microsoft Edge installed.
This section prevails over the following historical counts. Limits unchanged: overlapping targets, OS popups, VS Code and distribution not certified.

## Browser and visual review — current verification

`dotnet run --file ops/VerifyBrowserCharts.cs` requires Microsoft Edge installed; it uses Playwright with headless Edge Chromium and inline SVG documents without network requests. Verifies 27 charts: texts in the SVG viewport, no title/text bounding-box collision, at least one reachable hit target with a title per chart. Produces screenshots and report in ops/visual/browser. Included in the gate (now 17 steps).
The browser had detected three title/legend collisions: fixed through the opt-in SeparateLegendRow in the PlotSharp sources, enabled by IndexedCharts. Tooltips added to the stems. The 19 rasters of the new notebooks 06/08 were observed; three Edge screenshots after the fix observed separately. The rasterisation now follows the whole manifest and produces 27 PNGs.
Explicit limits remain: overlapping targets do not guarantee individual access to every sample; the operating system's native popup and the VS Code renderer not verified. The report counts total/reachable targets, without declaring complete coverage. Test on the browser outputs distinct from the real execution.

## Migrated notebooks — shared manifest

`ops/migrated-notebooks.json` records id, path, expected SVG count and gap requirement. From MathemaSharp, after a Release build of MathemaSharp and PlotSharp:

```sh
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id core
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id smoothing
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id autoregression
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id arima
```

Counts: 2, 6, 6, 13 SVGs (27 in total). Tables preserved; all the runners in the gate. The command without arguments remains Core and `--smoothing` remains compatible. Manifest/tests on the outputs do not replace the real execution. The preliminary review includes the eight Core/07 charts and the 19 new 06/08 rasters; the Edge report verifies 27 SVGs and three screenshots of the corrected cases were observed. It is not equivalent to a test of the VS Code renderer.

## Register of suspended tests

The gate produces a new TRX and the report `ops/validation/pysharp-skips.md`, also kept next to the TRX in the gate's timestamped directory. In the current run: 12 PostgreSQL tests without a reachable server and one opt-in pretrained theory. The suspended tests are not passed and are not waivers. Credentials must stay in the environment, never in the sources.
The TRX parser is tested with six xUnit cases: namespace, counts, failures, missing reasons and empty/incomplete evidence. The two new operational projects Ecosystem.Validation target .NET 10; they do not change the .NET 11 targets of the existing numerical libraries.

## Smoothing/Kalman and graphical review

`TimeSeries/07-ts-smoothing-kalman.ipynb` also uses the PlotSharp bootstrap. Runner: `dotnet run --file ops/RunMigratedCoreNotebook.cs -- --smoothing`. Verifies six SVGs and at least one broken line on the gap; tables preserved through `shared/Presentation.csx`, common also to the legacy renderer.
In local builds, NaN Y breaks Lines/Panels; Infinity, non-finite X, entirely missing series and stems/bar with NaN are rejected. The previous preview packages remain finite-only. No implicit pack/distribution.
`dotnet run --file ops/RasterizeNotebookEvidence.cs` produces 27 review PNGs and a hash manifest in ops/visual, from real outputs. The gate regenerates the artifacts but does not run an automatic visual review. Eight images were observed: gap, curves, titles and legends readable. The clipping of the sample markers was fixed in local builds through an overlay for centres within the domain; two regenerated and observed spectra show complete circles. Highlight and browser keep separate checks. The label of the spectra is now k, not Lag. The Svg.Skia raster is a review tool, not a PlotSharp PNG backend nor a browser proof.

## Migrated application notebook

`notebooks/Core/03-complex-signals.ipynb` now uses `shared/MathemaSharp.PlotSharp.csx`, without the legacy renderer. Run the Release build of both libraries, then `dotnet run --file ops/RunMigratedCoreNotebook.cs`. The runner sets the .NET 11 roll-forward for JupyterNet and verifies two SVGs after clearing the previous outputs. The baseline also builds PlotSharp before the execution. This path is JupyterNet, distinct from the JupyterSharp pilots.

## Opt-in graphics migration

After `ops/RunEcosystemPilot.cs`, run `dotnet run --file ops/RunIndexedChartsPilot.cs`.
The notebook `03-indexed-charts.ipynb` loads `shared/PlotSharpCharts.csx`: Chart.Lines/Panels/Stems API delegated to PlotSharp.IndexedCharts, four XML-verified SVGs and previous outputs cleared before the execution. The runner is included in the gate.
Do not load the facade and Charts.csx together: they expose the same names. Finite double data are supported; layout/tick/tooltip differ and non-finite gaps remain with the legacy renderer. It is not a complete migration nor a visual approval.

## Ecosystem — updated verification

```sh
dotnet run --file ops/ValidateEcosystem.cs
```

Runs build/test of the six solutions, NuGet audit, baseline of 24 notebooks, PySharp kernel build, twin/precision notebooks, MatSharp notebooks, three JupyterSharp NuGet pilots and four application notebooks migrated to PlotSharp. Includes TRX reports and review raster: 16 steps; rasterisation is not equivalent to a visual review. The Edge verification of the 27 SVGs adds the seventeenth step, after the raster. Logs and reports: `D:\Dev\BuildArtifacts\ecosystem-validation`. It does not run benchmarks nor pack and does not ignore failures. Suspended tests remain visible in the suites.

For the separate pilots:

```sh
dotnet run --file ops/RunEcosystemPilot.cs
dotnet run --file ops/RunLanguageInteropPilot.cs
```

The first restores the packages in an isolated cache and installs the JupyterSharp CLI locally; the second uses that CLI and those packages. Real outputs saved in `notebooks/Ecosystem/01-signal-spectrum.ipynb` and `02-typed-language-interop.ipynb`. The first shows mathematics and SVG; the second runs Python/MATLAB through C# embedding, not native language plugins.

### Typed MatSharp: use of A \\ b

```matlab
A = [decimal128(1) decimal128(0); decimal128(0) decimal128(1)];
b = [decimal128('1.0000000000000000000000001'); decimal128(2)];
x = A \\ b;
x(1) - decimal128(1)  % 1e-25
```

In the host first register `MatSharpLib.NDSharpBridge.NDSharpFunctions.Register(engine)`. LU for square, QR for overdetermined; RHS with compatible rows and one or more columns. Inputs copied, result column-major in the same type. Underdetermined, singular systems or types without an adapter are rejected; no new right matrix division. Thresholds/rank are those of the shared kernels, not a universal MATLAB certification.

### PySharp: selection of generic CLR types

The host can inject `typeof(List<>)` as `ClrList` and `typeof(Decimal128)` as `D128` through `PyEngine.SetVariable`. In the language:

```python
from mathema import decimal128
List128 = ClrList[D128]
values = List128()
values.Add(decimal128('1.0000000000000000000000001'))
assert values[0] - 1 == decimal128('1e-25')
```

The selection requires an open CLR definition and CLR arguments of the correct arity; the constraints are applied. It does not infer the type, does not turn Python lists into typed NumPy and does not automatically resolve all the generic families from the imports. The mathema module accepts strings/integers and Decimal values of the same format; it does not recover precision from float literals already rounded.


## FFT benchmark — first .NET 11 measurement

```sh
dotnet run --file ops/RunFftBenchmarks.cs
```

BenchmarkDotNet 0.15.8 does not recognise .NET 11 RC in the out-of-process toolchain: the runner uses in-process ShortRun, 3 warm-ups and 3 iterations, one launch. Preliminary measurement, not a definitive comparison between algorithms or formats. Results and logs: `D:\Dev\BuildArtifacts\MathemaSharp\benchmarks`.

Host: Windows 11, Intel i7-11370H, .NET 11 RC3 x64. Complex double data.

| Operation | N | Mean | Allocations/op |
|---|---:|---:|---:|
| FFT | 64 | 1.462 µs | 1.02 KB |
| IFFT | 64 | 1.609 µs | 1.02 KB |
| FFT Bluestein | 1009 | 206.236 µs | 191.70 KB |
| IFFT Bluestein | 1009 | 216.258 µs | 207.49 KB |
| FFT radix-2 | 1024 | 27.478 µs | 16.02 KB |
| IFFT radix-2 | 1024 | 30.097 µs | 16.02 KB |

The BenchmarkDotNet report contains confidence intervals and deviations. Different lengths: the ratio of the times is not a like-for-like comparison. Do not replace a transform of length 1009 with padding to 1024 without evaluating the different semantics. Out-of-process measurements, other types and comparison with the DFT remain.

## Full baseline — one command

From Windows, in the root:

```sh
dotnet msbuild ops/validate.proj -t:Baseline
```

Compiles Release, restores JupyterNet, runs the **22 notebooks explicitly selected** in ops/baseline-notebooks.json updating the outputs in place, then runs the Release tests. Saves SDK, installed runtimes, test counts, notebook outcomes, exclusions and logs in `ops/artifacts/baseline/<timestamp-id>/report.json` and `summary.md`. Non-zero exit code on errors; the report is also produced in case of a handled failure.

NDSharp, PySharp and scenarios are not in the selection: 7 notebooks excluded with reasons in the report. The 3 C#/PySharp tests remain suspended; the solution's build/test continues to include the existing NDSharp tests, without modifying their code. It does not run benchmarks, Python, publish or migration of the consumers. The notebook runtime uses LatestMajor roll-forward with prerelease; reproducibility requires compatible SDK/runtime and feeds, not a completely pinned environment image.

Only build and tests, **without declaring the notebooks executed**:

```sh
dotnet msbuild ops/validate.proj -t:BuildAndTest
```

This mode records FullBaseline=false and notebooks NotExecuted; the output tests read those already saved. Last complete baseline verified: build without warnings/errors, 22 notebooks run, 272 tests passed and 3 suspended.

## Consolidation notebooks — run

| Library | Verified Usage | Public API catalogue |
|---|---|---|
| Core | [11-consolidation](notebooks/Core/11-consolidation.ipynb) | [12-api-reference](notebooks/Core/12-api-reference.ipynb) |
| LinearAlgebra | [11-consolidation](notebooks/LinearAlgebra/11-consolidation.ipynb) | [12-api-reference](notebooks/LinearAlgebra/12-api-reference.ipynb) |
| TimeSeries | [11-consolidation](notebooks/TimeSeries/11-consolidation.ipynb) | [12-api-reference](notebooks/TimeSeries/12-api-reference.ipynb) |
| Quantum | [11-consolidation](notebooks/Quantum/11-consolidation.ipynb) | [12-api-reference](notebooks/Quantum/12-api-reference.ipynb) |

The four API catalogues are also run: reflection of the Release assemblies on the .NET 11 runtime, with exported types, declared public members and constraints. Regeneration: `dotnet msbuild ops/reference-notebooks.proj -t:Run`. The catalogues do not certify behaviour or coverage and do not reproduce all the source annotations (nullable, tuples, ref/readonly).

The four notebooks were run with JupyterNet on the .NET 11 runtime, saving the outputs of the six formats: Half, float, double, Decimal32/64/128. The References are focused on the consolidated kernels, not on the whole surface of the libraries. Fixtures and comparisons are in T; the simple cases do not certify universal accuracy.

To rerun them from the root (requires the JupyterNet tool restorable from the configured feeds):

```sh
dotnet msbuild ops/consolidation-notebooks.proj -t:Run
```

The command compiles Release, sets the .NET 11 roll-forward and updates the outputs in place. Bootstrap `notebooks/shared/MathemaSharp.Numerics.csx`, without Charts/NDSharp dependencies. The xUnit tests verify structure and presence of the outputs; they do not run the kernel. This verification does not extend the execution state to the other notebooks nor restore the three suspended C#/PySharp comparisons.

### Previous Core examples — rerun

The Core notebooks 01–05 were rerun on the .NET 11 RC3 runtime. 05 includes Decimal32/64/128, Complex<T>, NaN/infinity and partial parsing; 04 checks environment and SIMD correctness, it does not produce statistical performance measurements. Notebooks 01–03 include contracts and links to the Reference.

```sh
dotnet msbuild ops/core-notebooks.proj -t:Run
```

The runner supports `-Module Core|LinearAlgebra|TimeSeries|Quantum` to limit the selection. With Module Core it does not run other modules or PySharp; with TimeSeries the recursive selection may include the PySharp notebooks. The other notebooks are not declared rerun by this tranche.

### LinearAlgebra and Quantum — introductory examples rerun

- [LU and least squares](notebooks/LinearAlgebra/01-lu.ipynb): pivoting, residual, inverse, reuse, Decimal128 QR and norms.
- [Quantum operators](notebooks/Quantum/01-operators.ipynb): normalisation on extreme amplitudes, eigenstates, diagonalisation, density and Decimal128 evolution.

Both contain contracts and executable checks, with .NET 11 outputs saved. The Quantum case verifies error ≤1e-28 for H=X, t=π/2, not for every matrix.

```sh
dotnet msbuild ops/algebra-quantum-notebooks.proj -t:Run
```

### C# TimeSeries — examples rerun

Notebook 06 (baseline/AR), 07 (smoothing/Kalman), 08 (ARIMA/VAR/GARCH) and tour 10: .NET 11 outputs saved, contracts and Reference links updated.

```sh
dotnet msbuild ops/timeseries-notebooks.proj -t:Run
```

The runner uses `-Module TimeSeries -TopLevelOnly` and selects only 06–08 and 10. It does not run scenarios, PySharp or 09 (NDSharp bridge). `-Filter` uses PowerShell wildcards, also ranges such as `0[6-8]*`. The C# execution does not demonstrate agreement with the PySharp twin; the related tests remain suspended.

### Independent numerical oracles

[Validation notebook](notebooks/TimeSeries/13-independent-oracles.ipynb): gamma Q and the Hilbert12 counterexample, with real outputs saved.

```sh
# Consumes the saved fixtures: does not require Python
dotnet msbuild ops/oracle-notebook.proj -t:Run
dotnet test src/tests/MathemaSharp.TimeSeries.Tests --filter FullyQualifiedName~IndependentOracleTests

# Regenerate the fixtures on Windows: requires Python 3 and access to the pip feed
dotnet msbuild ops/numerical-oracles.proj -t:Setup,Generate
```

[Quantum oracles](notebooks/Quantum/13-independent-oracles.ipynb): non-normal/complex MatrixExp and very unbalanced eigenvalues, with real outputs. Run: `dotnet msbuild ops/quantum-oracle-notebook.proj -t:Run`. Test: `dotnet test src/tests/MathemaSharp.Quantum.Tests --filter FullyQualifiedName~IndependentQuantumOracleTests`.

The generator uses mpmath 1.3.0 with 200 digits, isolated in ops/.venv-oracles (ignored by Git). The fixtures contain 160-digit strings. The C# tests do not invoke Python or MathemaSharp to compute the references. Tolerances and limits are in ARCHITECTURE and ROADMAP; a small residual does not certify accuracy on ill-conditioned matrices.

### Solution with diagnostics

```csharp
using MathemaSharp.LinearAlgebra;

var report = LinearSystemDiagnostics.Solve<double>(
    new double[,] { { 2, 0 }, { 0, 4 } }, [2, 8]);
// Solution=[1,2], ConditionInfinityEstimate=2, IsIllConditioned=false
```

[Executed notebook](notebooks/LinearAlgebra/13-solve-diagnostics.ipynb). Launch: `dotnet msbuild ops/diagnostics-notebook.proj -t:Run`.

`NormwiseBackwardErrorInfinity` (alias `NormwiseBackwardError`) and `RelativeResidualInfinity` are computed in T with scaling/FMA. `ConditionInfinityEstimate` requires n additional solves: use it when needed, it does not replace the fast Solve. The `IsIllConditioned` flag is a warning based on rcond ≤ sqrt(epsilon), not a guarantee on the accuracy of the solution.

## Console

From the root:

```sh
dotnet run --project src/apps/MathemaSharp.Lab -- --topic generic
dotnet run --project src/apps/MathemaSharp.Lab -- --topic precision
dotnet run --project src/apps/MathemaSharp.Lab -- --topic decimal
dotnet run --project src/apps/MathemaSharp.Lab -- --topic complex
dotnet run --project src/apps/MathemaSharp.Lab -- --topic integers
dotnet run --project src/apps/MathemaSharp.Lab -- --topic simd
dotnet run --project src/apps/MathemaSharp.Lab -- --topic quantum
dotnet run --project src/apps/MathemaSharp.Lab -- --topic timeseries
```

The `quantum` topic shows inner product, Pauli expectation values, commutator, eigenvalues, time evolution and density matrix; see [QUANTUM.md](QUANTUM.md). The `timeseries` topic goes through benchmark, Yule-Walker, Hannan-Rissanen, Kalman/SES, GARCH and STL on simulated data with known parameters; see [TIMESERIES.md](TIMESERIES.md).

Without options `all` is selected. Unknown topic: exit code 2 and error log. Configuration through Microsoft.Extensions.Hosting: also the environment variable `topic`; the command line prevails. Numeric output with invariant culture.

Significant results: `0.1+0.2` double = `0.30000000000000004`; decimal = `0.3`. `[1e16,1,1,-1e16]`: naive sum 0, Kahan 2. Three rows of 0.005: ToEven per row 0.00, on the total 0.02. For z=3+4i: Hermitian dot 25, bilinear -7+24i.

## Benchmarks

```sh
dotnet run -c Release --project src/benchmarks/MathemaSharp.Benchmarks -- --list flat
dotnet run -c Release --project src/benchmarks/MathemaSharp.Benchmarks -- --filter '*DotBenchmarks*' --job Dry --artifacts ops/artifacts/benchmarks
dotnet run -c Release --project src/benchmarks/MathemaSharp.Benchmarks -- --filter '*SumBenchmarks*' --job Short --artifacts ops/artifacts/benchmarks
dotnet run -c Release --project src/benchmarks/MathemaSharp.Benchmarks -- --filter '*IntegerBenchmarks*' --artifacts ops/artifacts/integers
dotnet run -c Release --project src/benchmarks/MathemaSharp.Benchmarks -- --filter '*ComplexBenchmarks*' --artifacts ops/artifacts/complex
```

`Dry`: verification only; `Short`: exploration; default job: more robust measurements. Do not compare with a debugger or from Stopwatch in a notebook. Close concurrent loads and record the effective runtime, CPU, GC and JIT. CSV/HTML/Markdown produced automatically. Comparable rows have the same N; different types have different precision contracts. Arrays prepared outside the measurement; the DFT instead deliberately allocates the output. SIMD and LINQ can change the order of the sum.

The local Dry run executed all four Dot combinations (N=31,4099). The cold-start means in the order of milliseconds include non-representative warm-up/JIT: they do not mean that SIMD is slower. No measurements are available on the new .NET 11 IEEE decimals.

## Notebooks (JupyterNet)

> **Note (2026-10-30):** JupyterNet has been removed from the suite; notebooks run in JupyterSharp (`jupytersharp`), see the Installation section. The description below is kept as the record of how the notebooks were originally wired.

JupyterNet kernel (C#, Roslyn scripting, repository `D:\Dev\2026\repos\JupyterNet`), tool `jupyternet` from the local feed `D:\Dev\NuGetLocalFeed` (manifest in `.config/dotnet-tools.json`, `dotnet tool restore`). The notebooks are saved **executed**, with output and SVG charts.

```powershell
./scripts/run-notebooks.ps1                    # Release build + runs all the notebooks (C# and PySharp, also those of the scenarios)
./scripts/run-notebooks.ps1 -Filter '0[6-9]*'  # time series only
```

Interactive: VS Code with the JupyterNet extension, notebook type `jupyternet-notebook`.

How they are linked to the libraries (requires JupyterNet ≥ 0.3.0, version pinned in the tool manifest):
- every notebook starts with `#load "shared/MathemaSharp.csx"`, relative to the notebook's folder; that file references `artifacts/bin/*/release/*.dll` with `#r` relative to itself. No absolute path: the repository can be moved;
- the `using` directives of a `#load` do not propagate: each notebook declares its own;
- `Console.WriteLine` ends up in the notebook (the C# kernel captures it since 0.3.0), grouped by cell and in order with respect to the charts;
- `shared/Kernel.csx` sets the invariant culture also as the default of the threads, because each cell can run on a different thread;
- `shared/Charts.csx`: static SVG charts (`Chart.Lines`, `Chart.Panels`, `Chart.Stems`) and `Table.Show`, via `Display.Html`;
- notebook 05 runs on the .NET 11 RC2 runtime via `DOTNET_ROLL_FORWARD=LatestMajor` + `DOTNET_ROLL_FORWARD_TO_PRERELEASE=1`: Roslyn compiles against the assemblies of the running runtime, so Decimal32/64/128 and Complex<T> are available.

## Packages

```powershell
./scripts/pack.ps1            # artifacts/package/release/*.nupkg
./scripts/pack.ps1 -Publish   # also copies into the local feed (increase <Version> first)
```

## Libraries: Usage

The didactic samples are in `notebooks/<lib>/`; the shared helpers are in `notebooks/shared/`. The outputs were cleared during the migration: rerun the notebooks before considering them verified. The Lab console remains a verification harness, not the catalogue of the samples.

### MathemaSharp.Core — Usage

Namespace: `MathemaSharp.Core`. Notebooks: `notebooks/Core/`.

- [01-generic-math](notebooks/Core/01-generic-math.ipynb)
- [02-precision-decimal](notebooks/Core/02-precision-decimal.ipynb)
- [03-complex-signals](notebooks/Core/03-complex-signals.ipynb)
- [04-performance](notebooks/Core/04-performance.ipynb)
- [05-dotnet11-decimals](notebooks/Core/05-dotnet11-decimals.ipynb)

### MathemaSharp.LinearAlgebra — Usage

Namespace: `MathemaSharp.LinearAlgebra`. Notebooks: `notebooks/LinearAlgebra/`.

- [01-lu](notebooks/LinearAlgebra/01-lu.ipynb)

Use `LuFactorization<T>.Create(matrix)` and `Solve(rhs)` to reuse the same factorisation; nearly singular pivots are rejected.

### MathemaSharp.Quantum — Usage

Namespace: `MathemaSharp.Quantum`. Notebooks: `notebooks/Quantum/`.


### MathemaSharp.TimeSeries — Usage

Namespace: `MathemaSharp.TimeSeries`. Notebooks: `notebooks/TimeSeries/`.

- [06-ts-reference-autoregression](notebooks/TimeSeries/06-ts-reference-autoregression.ipynb)
- [07-ts-smoothing-kalman](notebooks/TimeSeries/07-ts-smoothing-kalman.ipynb)
- [08-ts-arima-var-garch](notebooks/TimeSeries/08-ts-arima-var-garch.ipynb)
- [09-ts-stl-ndsharp](notebooks/TimeSeries/09-ts-stl-ndsharp.ipynb)
- [10-ts-libreria-tour](notebooks/TimeSeries/10-ts-libreria-tour.ipynb)
- [10-ts-libreria-tour](notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb)
- [A1-magazzino-frigorifero-csharp](notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-csharp.ipynb)
- [A1-magazzino-frigorifero-pysharp](notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-pysharp.ipynb)
- [D1-fotovoltaico-csharp](notebooks/TimeSeries/Scenarios/D-fotovoltaico/D1-fotovoltaico-csharp.ipynb)
- [D1-fotovoltaico-pysharp](notebooks/TimeSeries/Scenarios/D-fotovoltaico/D1-fotovoltaico-pysharp.ipynb)

### MathemaSharp.NDSharp — Usage

Namespace: `MathemaSharp.NDSharp`. Notebooks: `notebooks/NDSharp/`.


The bridge uses runtime dtype/storage and still contains conversions via double: it is not an entirely Generic Math kernel. The STL notebook in TimeSeries shows the integration.
