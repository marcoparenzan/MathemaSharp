// Common bootstrap of the notebooks (kernel ≥ 0.3.0): the notebooks load it with #load "shared/MathemaSharp.csx" and the #r below
// are relative to this file. Prerequisite: dotnet build src/MathemaSharp.slnx -c Release.
#r "D:/Dev/BuildArtifacts/MathemaSharp/bin/MathemaSharp.Core/release/MathemaSharp.Core.dll"
#r "D:/Dev/BuildArtifacts/MathemaSharp/bin/MathemaSharp.Quantum/release/MathemaSharp.Quantum.dll"
#r "D:/Dev/BuildArtifacts/MathemaSharp/bin/MathemaSharp.LinearAlgebra/release/MathemaSharp.LinearAlgebra.dll"
#r "D:/Dev/BuildArtifacts/MathemaSharp/bin/MathemaSharp.TimeSeries/release/MathemaSharp.TimeSeries.dll"
#load "Kernel.csx"
#load "Charts.csx"

// The using directives of a file loaded with #load do NOT propagate to the notebook (the types do): each notebook declares its own.

