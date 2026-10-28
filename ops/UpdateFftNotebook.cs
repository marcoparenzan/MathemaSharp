#:property TargetFramework=net11.0
#:property PublishAot=false
#:property JsonSerializerIsReflectionEnabledByDefault=true
using System.Text.Json;
using System.Text.Json.Nodes;
var path="notebooks/Core/06-fft.ipynb";
var doc=JsonNode.Parse(File.ReadAllText(path))!;
var cells=doc["cells"]!.AsArray();
foreach(var cell in cells) {
 var source=cell!["source"]!.AsArray();
 for(int i=0;i<source.Count;i++)source[i]=source[i]!.GetValue<string>()
 .Replace("FFT radix-2 — Usage e Reference","FFT radix-2 e Bluestein — Usage e Reference")
 .Replace("Zero o potenza di due; altrimenti ArgumentException","Qualunque lunghezza, nei limiti di memoria e rappresentabilità di T")
 .Replace("Bluestein, SIMD e benchmark non sono inclusi.","Bluestein usa una convoluzione radix-2 con padding >= 2N-1. SIMD e benchmark non sono inclusi.");
}
var code="""
var prime = new Complex<double>[] { new(1,0), new(0,0), new(0,0), new(0,0), new(0,0), new(0,0), new(0,0) };
var primeSpectrum = Signals.Fft<double>(prime);
Require(primeSpectrum.All(v => double.Hypot(v.Real-1,v.Imaginary)<1e-12), "bluestein_prime_oracle");
var primeBack = Signals.Ifft<double>(primeSpectrum);
Require(primeBack.Zip(prime).All(p => double.Hypot(p.First.Real-p.Second.Real,p.First.Imaginary-p.Second.Imaginary)<1e-12), "bluestein_prime_roundtrip");
""";
cells.Add(JsonSerializer.SerializeToNode(new {cell_type="markdown",metadata=new {},source=new[]{"## Bluestein: lunghezza prima 7\n","L'impulso ha spettro unitario: riferimento analitico indipendente."}}));
cells.Add(JsonSerializer.SerializeToNode(new {cell_type="code",metadata=new {},execution_count=(int?)null,outputs=Array.Empty<object>(),source=new[]{code}}));
File.WriteAllText(path,doc.ToJsonString(new JsonSerializerOptions {WriteIndented=true}));
