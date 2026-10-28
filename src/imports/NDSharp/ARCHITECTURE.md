# Architecture

## Repository layout

```text
src/
  NDSharp.slnx             solution
  NDSharp/                numerical array core
  NDSharp.Frame/          tabular and time-series model
  NDSharp.Plot/           plotting model and renderer
  NDSharp.Image/          OpenCvSharp integration
  NDSharp.Wavelets/       wavelet filter banks and transforms
  NDSharp.Tests/          core xUnit tests and fixtures
  NDSharp.Frame.Tests/    tabular and temporal xUnit tests
  NDSharp.Plot.Tests/     rendering xUnit tests
  NDSharp.Image.Tests/    native conversion xUnit tests
tools/oracle/             optional Python code/data generators
artifacts/                local NuGet package output
```

Documentation lives at the repository root. There is no standalone executable or deployment pipeline in the current tree. Each companion library references `NDSharp`; the core does not reference the companions.

## Array model

`NDArray` holds a typed CLR array buffer together with a dtype, shape, element strides, and offset. Strides are expressed in **elements**, not bytes. This allows slices and other views to reuse backing storage without allocating a full copy. Arrays expose rank, size, and contiguity information.

`DType` and related helpers describe numeric types and promotion/casting rules. The `np` partial class forms the NumPy-shaped public facade, split into creation, shape operations, elementwise operations, products, statistics, and linear algebra files. Implementation helpers handle broadcasting, indexing, assignment, casting, and row traversal.

The core also contains complex and temporal operations, reduction kernels, FFT, eigendecomposition, and PCG64-based random generation. It has no external package references in its project file and no Python runtime dependency. Some implementation paths use unsafe code; the project explicitly enables it.

Views are an ownership consideration: consumers should not assume a new array-shaped result always owns independent storage. `Copy()` provides an explicit isolation boundary.

## Companion libraries

### Frame

`Column` models typed column data and missing-value behavior. `Series` couples values with an index; `DataFrame` holds columns and row/column labels. Dedicated modules implement alignment and arithmetic, grouping, merges, reshaping, reductions, rolling/window operations, categorical/binning behavior, and CSV/JSON/HTML I/O.

Date parsing, offsets, periods, time zones, and time-series operations are separated into focused source files. Formatting is implemented within Frame rather than delegated to Python. Label alignment and missing-value promotion are important semantic differences from position-only array arithmetic.

### Plot

`Figure` contains `Axes`, which contain artists such as lines, images, and quiver arrows. Plotting modules also handle ticks, text, colormaps, contours, and 3D projection. `RenderPng()` produces encoded PNG bytes using SkiaSharp.

DejaVu Sans fonts are embedded resources for rendering. Their license is included in the package. SkiaSharp native asset references cover Linux and macOS in addition to assets supplied through SkiaSharp itself; native behavior still needs validation on each deployment platform.

### Image

`Cv` is split across basics, filters, features, and DNN operations. `Mats` bridges managed arrays and OpenCvSharp `Mat` data. OpenCV performs the image processing; it is a native dependency, unlike the array core.

The project explicitly references the Windows OpenCvSharp runtime. Platform support should not be inferred from the portable `net10.0` target alone. Callers must dispose native objects they own.

### Wavelets

`Wavelet` looks up generated discrete filter banks. `Dwt` implements convolution-based forward/inverse one- and two-dimensional transforms and boundary extension rules in double precision. Filter data is generated from PyWavelets, but transforms execute in .NET.

## Tests and compatibility evidence

The solution contains four xUnit test projects:

- **Core:** generated numerical oracle cases, dtype promotion/casting fixtures, formatting, sorting/diagonals, eigenvalues/eigenvectors, norms, RNG, and distributions.
- **Frame:** column/index behavior, missing values, arithmetic alignment, categoricals, date/time operations, periods/time zones, windows, and HTML.
- **Plot:** high-level rendering checks, pixel-based assertions, colorbars, quiver bounds, tick placement, and colormap concurrency.
- **Image:** native library loading, grayscale conversion, dtype/shape/data round trips, and non-contiguous views.

There is currently no dedicated Wavelets test project in this solution. Oracle-backed coverage is evidence for specific behaviors, not a guarantee of complete NumPy/pandas/matplotlib/OpenCV/PyWavelets compatibility.

## Generated sources and fixtures

Generated outputs are committed, so a normal .NET build does not invoke Python. Optional development scripts include:

| Script under `tools/oracle/` | Main output |
| --- | --- |
| `gen_ndsharp_tests.py` | `src/NDSharp.Tests/OracleCases.g.cs` |
| `gen_eig_tests.py` | `src/NDSharp.Tests/EigCases.g.cs` |
| `gen_sortdiag_tests.py` | `src/NDSharp.Tests/SortDiagCases.g.cs` |
| `gen_colormaps.py` | `src/NDSharp.Plot/Colormaps.g.cs` |
| `gen_wavelets.py` | `src/NDSharp.Wavelets/WaveletFilters.g.cs` |

For example, from the repository root:

```powershell
python -m venv tools/oracle/.venv
# Windows:
tools/oracle/.venv/Scripts/python -m pip install numpy matplotlib PyWavelets
tools/oracle/.venv/Scripts/python tools/oracle/gen_wavelets.py
# Linux/macOS use tools/oracle/.venv/bin/python instead.
```

Review each generator before running it; generators overwrite their outputs. Regenerate intentionally, review diffs, and rerun the solution tests. Record the Python package versions used when updating numerical reference data, since upstream changes can change generated results. Do not hand-edit generated files to fix a generator issue.

## Packaging and maintenance

All projects target `net10.0`, enable nullable references and implicit usings, and use the latest available C# language version. Versions and package metadata are maintained per library. `dotnet pack` produces NuGet packages; it does not publish them to a remote feed.

Keep changes close to the relevant library and extend its tests for non-trivial behavior. Native dependency updates deserve runtime tests as well as managed compilation. Existing nullable warnings in Frame and Image are separate maintenance work and should not be mistaken for documentation changes.
