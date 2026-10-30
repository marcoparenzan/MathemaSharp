#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\MatSharp";
File.AppendAllText(Path.Combine(root,"ARCHITECTURE.md"),"""

## Alignment with Science / MathemaSharp

Build output: D:\Dev\BuildArtifacts\MatSharp; packages: D:\Dev\NuGetLocalFeed.
MatSharpLib.NDSharp uses NDSharp 2.8.0 and a source reference to the sibling library MathemaSharp.LinearAlgebra.
The language representation remains Mat with a column-major double buffer; it does not yet support Decimal32/64/128.
`NDSharpFunctions.Register` also registers `mathema_solve(A,b)` and `mathema_rcond(A,b)`: real-double adapters to the shared solver with diagnostics.
A must be 2D, b a column, inputs real. The copy explicitly preserves the column-major order. They do not change the semantics of backslash, 1-based indexing or A(:).
`mathema_rcond` requires b because it uses the complete diagnostics; it is not a universal substitute for MATLAB's rcond function.
Last verification: Release build without warnings/errors and 121 tests passed, including 3 new tests.
Open: generic numeric storage and operators in the language, Decimal, CLR binding, PlotSharp adapter, notebook kernel and NuGet consumption of the bridge.
""");
