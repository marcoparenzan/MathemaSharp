# MathemaSharp.Geometry — implementation plan

Status: **phases 1–2 implemented and Release-tested; phases 3–5 pending; phase 6 excluded by default**. Original plan dated 2026-11-06. Geometry has 25 tests across double, float, Half and Decimal64; the solution rerun has 882 passing tests. The first phase-2 solution run had an unchanged kernel allocation-test failure (8 bytes); isolated and complete reruns passed without changes, cause unknown. Numerical conditioning, performance and ecosystem rendering are not certified. Owner: Marco Parenzan. Written to be handed to an agent that will do the work.

## 0. Before you start

Read these first, they are binding:

- `C:\Users\marco\.claude\CLAUDE.md` (global rules: .NET 11 only, `net11.0`; MIT licence, Copyright (c) 2026 Marco Parenzan; English in code, comments, documents and commit messages; all generated output under `D:\Dev\BuildArtifacts`; local NuGet feed `D:\Dev\NuGetLocalFeed` only; never delete from the feed or `BuildArtifacts`, move to `D:\Dev\BuildArtifacts\trash`; no publishing; report what failed, what was skipped and what was not tested).
- `D:\Dev\Science\ScienceSharp\docs\DEPENDENCIES.md` (the dependency rules of the suite).
- `D:\Dev\Science\MathemaSharp\ROADMAP.md`, last sections (2026-11-06): how the previous moves into the engine were done and verified.

Working rules that apply to this task:

- **Reusable mathematics goes as high in the stack as possible, into MathemaSharp.** Libraries above it (PlotSharp, NDSharp, PySharp, MatSharp, the articles) keep only what is theirs (drawing, arrays, language bindings) and call the engine.
- **Everything is Generic Math**: `where T : IFloatingPointIeee754<T>` (add `struct` when `T?` is needed). Test with `double`, `float`, `Half` and `System.Numerics.Decimal64` (a .NET 11 BCL type).
- **MathemaSharp never depends on NDSharp, PlotSharp or anything above it.** `MathemaSharp.Geometry` may reference `MathemaSharp.Core` and `MathemaSharp.Kernels` only (BCL otherwise).
- **Before moving existing code, record a SHA-256 fingerprint of its outputs; after the move the fingerprint must be identical with `T = double`.** Only then delete the old code. New code (no predecessor) is checked by hand-computed values and by properties.
- Record every test run with `D:\Dev\Science\ScienceSharp\Log-TestRun.ps1 -Repo <repo> -What <what> -Command <command> -Result <result>` (log: `D:\Dev\BuildArtifacts\logs\test-runs.log`).
- Commit locally, one commit per repository per finished step, message ending with the attribution line the harness gives you. **Do not push and do not merge**: Marco decides.
- Scratch scripts (fingerprint harnesses) go in `D:\Dev\BuildArtifacts\tmp`, never in a repository.
- The shell is Git Bash on Windows. Several heredocs with apostrophes in one command have broken repeatedly: write files with the file-writing tool, not with `cat <<EOF`.

## 1. What exists today

| Where | What | Nature |
| --- | --- | --- |
| `PlotSharp/src/PlotSharp/PlotSharp/FieldOverlays.cs` | `MarchingSquares(double[,] field, double level, bool[,]? mask, bool preserveDegenerateSegments)` → segments; exact-vertex and saddle rules | **our code, to be moved** |
| `PlotSharp/src/PlotSharp.Tests/PlotSharp.Tests/NavierContourTests.cs` | contour tests that fix today's behaviour | the oracle for the move |
| `NDSharp/src/NDSharp.Plot/Plot3D.cs`, `Artists.cs` | 3-D plotting: projection, rotation, polygons | our code, **not read yet**; inspect before deciding |
| `NDSharp/src/NDSharp.Image/Cv.*.cs` | `FindHomography`, `Rodrigues`, `TriangulatePoints`, `GetAffineTransform`, `GetPerspectiveTransform`, `ContourArea`, `ArcLength`, `Moments`, `HuMoments`, ... | **thin wrappers over OpenCvSharp (native OpenCV)**, not our code |
| `MathemaSharp/src/libs/MathemaSharp.Fluids/LatticeBoltzmannD2Q9.cs` | the solid mask of the cylinder (point in circle) | a one-line candidate for phase 3 |
| MathemaSharp | no vectors, transforms, quaternions or polygons | — |

So most of `Geometry` is new code; the marching squares is the one real move.

## 2. Project layout

- `MathemaSharp/src/libs/MathemaSharp.Geometry/MathemaSharp.Geometry.csproj` — copy the shape of `MathemaSharp.Fluids.csproj` (the target framework and common properties come from `Directory.Build.props`; give `Description` and `Version` `0.1.0-net11-preview.1`).
- `MathemaSharp/src/tests/MathemaSharp.Geometry.Tests/` — copy the shape of `MathemaSharp.Fluids.Tests` (a csproj with only the project reference; xUnit comes from `src/tests/Directory.Build.props`).
- Register both in `MathemaSharp/src/MathemaSharp.slnx` next to the other libs and tests.
- File header on every `.cs`: `// Copyright (c) 2026 Marco Parenzan` / `// Licensed under the MIT License. See the LICENSE file in the project root.`

## 3. Phases

Each phase ends with: build of `MathemaSharp.slnx` green, its tests green, the run logged, ROADMAP updated, local commit. Phases 4 and 5 also touch PlotSharp / NDSharp: build and test those solutions too.

### Phase 1 — primitives (new code)

- `Vector2<T>`, `Vector3<T>`: readonly structs; `+ − ×scalar ÷scalar`, `Dot`, `Cross` (3-D; the 2-D cross is the scalar z), `Length`, `LengthSquared`, `Normalize`, `Distance`, `Angle`, `Lerp`, projection onto a vector, equality.
- `Matrix3<T>` (2-D homogeneous) and `Matrix4<T>` (3-D homogeneous): identity, translation, rotation (2-D angle; 3-D about x, y, z and about an axis), scale, composition, transpose, determinant, inverse (fail clearly on a singular matrix), transform of points vs directions.
- `Quaternion<T>`: from axis-angle, from a rotation matrix and back, multiplication, conjugate, normalisation, rotation of a vector, SLERP.
- Tests: hand-computed values; properties (R·Rᵀ = I, det R = 1, q·q* = |q|², rotation preserves length); in `double` compare with `System.Numerics.Vector3/Matrix4x4/Quaternion` (float) within float tolerance.

### Phase 2 — polygons and shapes (new code)

- On `ReadOnlySpan<Vector2<T>>`: signed area (shoelace), centroid, perimeter, orientation (clockwise / counter-clockwise), point in polygon (winding number; state what happens on the boundary), bounding box, convex hull (Andrew's monotone chain; state how collinear points are treated), Douglas-Peucker simplification.
- Tests: squares, triangles, L-shapes, degenerate inputs (fewer than 3 points, collinear points, repeated points), orientation flips.

### Phase 3 — intersections and distances (new code)

- Segment-segment (with the parallel and collinear cases), line-circle, circle-circle, point-segment distance, point-polygon distance, inside tests for circle and ellipse.
- Optional: make the solid mask of `LatticeBoltzmannD2Q9<T>` use the circle test — **only if its Navier-Stokes fingerprint stays identical** (see section 4).

### Phase 4 — iso-contours (move from PlotSharp)

1. Write a fingerprint harness in `D:\Dev\BuildArtifacts\tmp` that calls `PlotSharp.Charts.MarchingSquares` / `FieldOverlays.MarchingSquares` on many fields (smooth fields, fields with exact level values at vertices, saddles, masks, constant fields, NaN where allowed, every `preserveDegenerateSegments` value) and hashes the segment coordinates (bits) and counts. Run it twice to check it is deterministic; keep the hash.
2. Implement `IsoContours.MarchingSquares<T>(T[,] field, T level, bool[,]? mask, bool preserveDegenerateSegments)` in `MathemaSharp.Geometry`, returning segments of `Vector2<T>` (or a `Segment2<T>` record struct), with the same order of operations as PlotSharp's code.
3. Make PlotSharp call it (PlotSharp gets a project reference to `MathemaSharp.Geometry`; this direction, PlotSharp → MathemaSharp, is allowed). Keep PlotSharp's public signatures as overloads: prebuilt packages (NDSharp.Plot and others in the feed) are compiled against them, and removing a signature breaks them at run time (`MissingMethodException`).
4. Rerun the harness: identical hash, and `PlotSharp.Tests` (including `NavierContourTests`) green. Only then remove PlotSharp's own algorithm.
5. Update `ScienceSharp/docs/DEPENDENCIES.md` (PlotSharp now depends on MathemaSharp.Geometry) and check there is no cycle.

### Phase 5 — projection and camera (inspect first)

- Read `NDSharp/src/NDSharp.Plot/Plot3D.cs` and `Artists.cs`. List what is mathematics (view rotation by azimuth and elevation, perspective and orthographic projection, depth sorting of polygons, clipping) and what is drawing.
- Move the mathematics into `MathemaSharp.Geometry` (`Camera<T>` / `Projection<T>`) with the same fingerprint procedure as phase 4 (hash the projected coordinates and the painting order produced by NDSharp.Plot today).

### Phase 6 — OpenCV pieces in pure C# (only if Marco confirms)

- Candidates, short and verifiable: `Rodrigues` (rotation vector ↔ matrix), `GetAffineTransform` (3 point pairs), `GetPerspectiveTransform` (4 point pairs), contour area and arc length, image moments and Hu moments.
- Oracle: the OpenCV results that `NDSharp.Image` produces today (within tolerance, not bits: different arithmetic).
- `NDSharp.Image` keeps OpenCV for everything else.

## 4. Verification reference

How the previous moves were verified (reuse the pattern):

- `D:\Dev\BuildArtifacts\tmp\linalg-fingerprint.cs`, `wavelet-fingerprint.cs`, `navier-fingerprint.cs`: file-based C# programs (`dotnet run --file x.cs`, with a `#:project "<path to csproj>"` line, quoted) that run many cases, append the bits of every result (`BitConverter.DoubleToInt64Bits`) and error messages to a string and print its SHA-256.
- Navier-Stokes: `navier-fingerprint.cs` must keep the hash it had when the numerics ran in double (`78D435BB…`) only while the article's `Real` alias is `double`. **The article now computes in `Decimal64`** (`Articoli/2026.10 NavierStokes/src/NavierStokes.Core/Numerics.cs`), so for phase 3 compare against the generic solvers in double, or switch the alias temporarily. Its own check is `dotnet run --project src/NavierStokes.Cli -c Release -- selftest` (all checks must pass).
- Test counts at the start of this plan: Kernels 163, LinearAlgebra 21, Fluids 15 (MathemaSharp); NDSharp 700; PySharp 1673 (13 skipped).

## 5. Risks

- Degenerate cases (self-intersecting polygons, collinear points, singular matrices): every function documents what it does and has a test for it.
- Tolerances in narrow formats: thresholds written for double are scaled by the epsilon of `T` for narrower formats (see `Tol<T>` in `MathemaSharp.LinearAlgebra/DenseLinearAlgebra.cs`).
- Marching squares: its exact-vertex rules matter to charts already declared compatible; move it only with an identical fingerprint.
- Binary compatibility of PlotSharp and NDSharp public APIs (see phase 4, step 3).

## 6. Decisions pending from Marco

Ask before starting the phase concerned; the default applies if Marco says "go" without choosing.

1. **Order.** Default: phase 1 first (primitives), then 2, 3, 4, 5. Alternative: phase 4 (the only real move) first.
2. **Phase 6.** Default: not done; `NDSharp.Image` stays on OpenCV.
3. **Point type.** Default: one `Vector2<T>` / `Vector3<T>` for both points and vectors (no separate `Point2<T>`).

## 7. Deliverables

- `MathemaSharp.Geometry` and its tests in the MathemaSharp solution.
- PlotSharp (phase 4) and NDSharp.Plot (phase 5) calling it, with their tests green.
- Documentation: `MathemaSharp/ROADMAP.md` (one section per phase), `MathemaSharp/README.md` and `ARCHITECTURE.md` (the new library), a `USAGE.md` section with installation first, `ScienceSharp/docs/DEPENDENCIES.md`, OPENPOINTS of the repositories touched.
- A final report to Marco: what was done, what failed, what was skipped, what was not tested.
