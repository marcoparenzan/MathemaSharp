#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\MatSharp";
File.AppendAllText(Path.Combine(root,"ARCHITECTURE.md"),"""

## Allineamento Science / MathemaSharp

Output di compilazione: D:\Dev\BuildArtifacts\MatSharp; pacchetti: D:\Dev\NuGetLocalFeed.
MatSharpLib.NDSharp usa NDSharp 2.8.0 e un riferimento sorgente alla libreria sorella MathemaSharp.LinearAlgebra.
La rappresentazione del linguaggio resta Mat con buffer double column-major; non supporta ancora Decimal32/64/128.
`NDSharpFunctions.Register` registra anche `mathema_solve(A,b)` e `mathema_rcond(A,b)`: adattatori real-double al solver con diagnostica condiviso.
A deve essere 2D, b colonna, input reali. La copia preserva esplicitamente l'ordine column-major. Non modificano la semantica di backslash, indicizzazione da 1 o A(:).
`mathema_rcond` richiede b perché usa la diagnostica completa, non è un sostituto universale della funzione MATLAB rcond.
Ultima verifica: build Release senza warning/errori e 121 test passati, inclusi 3 test nuovi.
Aperti: storage e operatori numerici generici nel linguaggio, Decimal, binding CLR, adattatore PlotSharp, kernel notebook e consumo NuGet del ponte.
""");
