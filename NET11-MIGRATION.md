# Migration of the ecosystem to .NET 11

The active projects of MathemaSharp, NDSharp, PlotSharp, JupyterSharp, PySharp and MatSharp were updated to `net11.0`, including the test projects and the operational scripts containing earlier targets. The `imports` snapshots are excluded. SDK available and selected in MathemaSharp: `11.0.100-rc.3.26522.148` (prerelease, not GA).

## Release verification

| Repository | Solution build | Tests |
|---|---|---|
| MathemaSharp | OK | 310 passed |
| NDSharp | OK | 563 passed |
| PlotSharp | OK | 70 passed |
| JupyterSharp | OK | 44 passed |
| PySharp | OK | 1503 passed, 1 failed, 13 suspended |
| MatSharp | OK | 130 passed |

The verification concerns the main solution of each repository; samples or tools not included in the solutions have the target updated but are not certified by this run.

The BL0016 requirement was fixed in Blazor: the JS calls now handle the disconnection of the circuit.

## Open points

- PySharp: implemented `sys._getframe` using the real thread-local stack already maintained by the interpreter. Exposed module globals, snapshot of the functions' locals, name/file/line and the f_back chain. Two new tests verify nested callers, real locals/globals and invalid depth/arguments. The representation is a snapshot at the time of the request (not a certification of the complete CPython semantics of persistent frames, lambdas, class bodies or async).
- Implemented `inspect.getsource` for modules, functions and methods of Python with a real file. Builtins produce TypeError, unavailable source OSError. Two new tests verify actual content and errors. The extraction of functions is by indentation blocks: lambdas, classes as a direct object, multi-line decorators, multi-line strings and complex continuations remain uncertified; it is not a complete CPython implementation.
- Implemented `compile` of strings for exec/eval modes: PyCompiledCode object with a real reusable AST, accepted by the respective exec/eval and recognised by inspect.iscode. Two tests verify distinct namespaces, explicit locals, reuse and syntax/flag errors. Single mode, AST/bytes input, flags, optimize and the complete semantics of code objects remain excluded; unsupported forms are rejected. It is not equivalent to compilation into CPython bytecode.
- Added a first public mutable AST: AST, Module, Expression, Assign, Expr, Name, Constant, BinOp, Load/Store and the operators Add/Sub/Mult/Div. `ast.parse` and `compile(..., ast.PyCF_ONLY_AST)` produce real nodes; `compile` converts the current fields of the nodes back into an executable AST. A test that modifies constants shows that exec/eval use the modifications and not the original source. It is not a complete model: visits, complete validation of contexts/positions and most of the nodes are missing.
- Added Import, ImportFrom and alias, with export and recompilation of names, aliases and relative level. The new test modifies the aliases in the tree, runs the compiled code and verifies the new keys in the namespace. The semantics of relative levels and star import are not yet covered by dedicated tests.
- Preliminary inventory run on the files available under site-packages/fastapi, starlette, anyio and pydantic: 26 files parsed, no parsing error. The report is in D:\Dev\BuildArtifacts\net11-migration\ast-inventory.txt. These are parser node counts, not a verification of compatibility with the CPython AST nor a scan of all the installations of the fixtures.
- Added the control-flow/expressions group: If, While, synchronous For, Return, Pass, Break, Continue, Attribute, Call without unpacking, Compare, BoolOp, UnaryOp, List, Tuple and Subscript. The test verifies roundtrip of loops and comparisons, modification of the condition and a call in a boolean expression; not every variant is certified.
- Added Dict, Set, Slice, IfExp, non-generator FunctionDef/AsyncFunctionDef, ClassDef and arguments/arg. The roundtrip preserves the supported parameters/defaults/annotations, bodies and decorators; generators and positional-only parameters remain excluded. A real test modifies a default and a dictionary value, recompiles and invokes the function/method, also verifying slice and conditional. It is not a certification of the async/decorator/kwargs variants.
- Verified the version actually installed by the non-pinned fixture: AnyIO 4.15.1, with lazy exports. Sources and evidence metadata are kept outside the repository in D:\Dev\BuildArtifacts\PySharp\dependency-evidence\anyio.
- Implemented PEP 562 for module lookup and `from ... import ...`: the module's real __getattr__ is called after the normal lookup; AttributeError indicates absence, other exceptions propagate. An independent test verifies lazy exports, precedence of existing attributes and propagation of errors.
- Implemented relative names in importlib.import_module, with explicit package, level resolution and reuse of the Importer's cache. A test verifies module identity, relative levels and errors beyond the package.
- Verified the whole MutableMapping snippet with CPython: KeyError prints the quotes correctly. Only the test's expectation was corrected. Evidence in D:\Dev\BuildArtifacts\net11-migration\keyerror-cpython.txt.
- P5_layout also run with CPython/Matplotlib on the same host: the saved oracle is confirmed. Unrounded probe: PySharp Y=0.1531963984, Matplotlib Y=0.1550479917. The fixture's double rounding amplifies the difference to 0.1/0.2; no arbitrary numerical correction was applied nor was the golden altered. PySharp's renderer still uses NDSharp.Plot 2.6.2 via NuGet.
- Resolved an intermittent collision in the temporary Pandas CSV: unique name for the oracle run. The oracle collection is now non-parallel, because the runner modifies the process working directory.
- Last Release build successful; full suite 1503 passed, 1 failed (Matplotlib P5_layout), 13 suspended. FastAPI/AnyIO and MutableMapping pass. The total number of tests also increased compared with the previous runs; not all of it is attributed to the tests added in these tranches.
- AST for exceptions/comprehensions/generators, visitor and complete validation remain. The relationship of the previous gaps with the .NET migration has not been demonstrated.
- The Matplotlib comparison `P5_layout` fails on the vertical position of the image axis after colorbar and tight_layout (expected 0.2, obtained 0.1, with fixture rounding). A test on `get_position` did not resolve the defect and was cancelled; the oracle was not modified.
- After the cancellation the complete verification was rerun: all builds pass, PySharp keeps 1416 passed, 10 failed and 13 suspended. No test was disabled.
- Warnings remain in the JupyterSharp and PySharp repositories; the first NDSharp compilation reported 15 warnings.
- Reran 24 notebooks of the main baseline, 9 additional C#/PySharp runs (tour/scenarios/precision/operators/generics) and the MatSharp embedding notebook. The C# tour is also present in the baseline. PySharp kernel recompiled on .NET 11. These runs precede the change of the package versions; build and tests were reconfirmed after the change.
- Generated 37 packages in the six repositories with new `-net11-preview.1` versions, without overwriting the previous versions. MathemaSharp 0.2.1-net11-preview.1, NDSharp 2.8.1-net11-preview.1; the others increment the patch of the previous version. Internal NuGet references updated to the generated versions.
- Separate consumer without ProjectReference and with an isolated cache: restore, build and execution successful. Verified Decimal128, typed NDArray, Bluestein FFT, bridge solvers and PySharp operators/compile. MatSharp, PlotSharp and JupyterSharp are restored in the same consumer but their behaviour is not exercised by this smoke test.
- Resolved the consumer's NuGet advisories: Microsoft.PowerShell.SDK 7.6.6 introduced Pkcs/Xml 10.0.10. The JupyterSharp.Kernels.PowerShell project now explicitly requires both at 10.0.13, the corrected version according to the GitHub/Microsoft advisories GHSA-846f-f764-76mx, GHSA-7hqp-7c5m-4vq6, GHSA-c399-9853-6jmj and GHSA-r955-8q5j-3h83 (GHSA-2fmv-rv2c-59x7 also checked).
- JupyterSharp regenerated into nine packages 0.3.1-net11-preview.2, without overwriting preview.1. Release build and 44 tests passed. Consumer updated: restore/build/run successful, zero warnings; `dotnet list ... package --vulnerable --include-transitive` finds no vulnerabilities with the current sources. No audit suppression.
- The verification concerns the consumer's package graph: it does not certify the complete security of the application, SDK/runtime or other consumers. The packages remain previews, not a certified production release. The previous preview.1 packages remain in the feed for reproducibility and are not to be preferred over the new JupyterSharp packages.
- Verify separately the samples/tools outside the solutions and the runtime support in the dynamic kernels.

## Audit extended to the six solutions

The global audit also found PySharp/Pyodbc → Microsoft.Data.SqlClient 7.0.2 → Pkcs 9.0.13, not present in the previous consumer. Added an explicit Pkcs 10.0.13 reference in PySharp.Pyodbc. New PySharp and PySharp.Pyodbc packages 2.5.1-net11-preview.2 generated in the feed; previous versions not overwritten.

PySharp Release build successful; suite 1503 passed, 1 failed (layout), 13 suspended. Final audit of the six solutions: zero vulnerable packages found from the current sources and no report errors. Report in D:\Dev\BuildArtifacts\security-audit\20261027-210253. Projects outside the solution and the old packages kept in the feed are not included in this verification.

```sh
dotnet run --file ops/AuditSciencePackages.cs
```

The gate checks the NuGet JSON and returns exit code 1 on vulnerabilities or errors, because the exit code of the `dotnet list package` command alone is not enough. It does not disable or suppress the audit.

## Pilot and end-to-end gate

Run `notebooks/Ecosystem/01-signal-spectrum.ipynb` with the JupyterSharp CLI 0.3.1-net11-preview.2 installed locally under BuildArtifacts, not as a global tool. It loads MathemaSharp.Core and PlotSharp from the packages restored in an isolated cache, computes a 5-cycle signal and the FFT and saves two SVGs through Display.Html. Verified FFT peak, amplitude and two HTML/SVG outputs. No ProjectReference nor JupyterNet execution in this pilot; no visual verification in the browser.

Added a test of the saved outputs; it does not run the kernel. MathemaSharp now has 311 passing tests. The main baseline keeps the 24 previous notebooks; the pilot is run separately from the gate.

```sh
dotnet run --file ops/RunEcosystemPilot.cs
dotnet run --file ops/ValidateEcosystem.cs
```

Complete gate rerun after the fixes to the structure/output tests: security, baseline, PySharp kernel, PySharp/MatSharp notebooks and NuGet pilot pass. The build/test step remains non-zero because of the legacy Matplotlib test; it is not ignored nor classified as a success. Report under D:\Dev\BuildArtifacts\ecosystem-validation. The gate does not run an automatic pack nor change versions. The PlotSharp/JupyterSharp projects/packages are integrated on the C# path; the PySharp plugin for JupyterSharp is not certified by the pilot.

## Repeatability

```sh
dotnet run --file ops/UpgradeNet11.cs -- --verify
```

The script returns a non-zero code if a build or test suite fails. `--apply` applies the target update; it must not be used to automatically fix code incompatibilities.

External logs and backups: `D:\Dev\BuildArtifacts\net11-migration`. Compilation output and package destination remain centralised as already configured.
