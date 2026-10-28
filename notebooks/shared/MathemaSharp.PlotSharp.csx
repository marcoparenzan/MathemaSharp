// Opt-in bootstrap for finite-double indexed charts. Build MathemaSharp and PlotSharp Release first.
#load "MathemaSharp.Numerics.csx"
#r "D:/Dev/BuildArtifacts/PlotSharp/bin/PlotSharp/release/PlotSharp.dll"
#load "Kernel.csx"
#load "PlotSharpCharts.csx"
#load "Presentation.csx"
#load "PlotSharpSpectrum.csx"
// The legacy Charts.csx renderer must not be loaded in the same session.
