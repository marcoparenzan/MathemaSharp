"""Independent direct DFT using mpmath, not MathemaSharp. Inputs are exact integers."""
import json
from pathlib import Path
import mpmath as mp
mp.mp.dps = 200
cases=[]
for n in (7, 16, 31, 64):
    samples=[mp.mpc((i*7)%19-9, (i*5)%13-6) for i in range(n)]
    result=[sum((samples[j]*mp.exp(-2*mp.pi*mp.j*k*j/n) for j in range(n)), mp.mpc(0)) for k in range(n)]
    cases.append({'length':n,'input':[[str(int(z.real)),str(int(z.imag))] for z in samples], 'output':[[mp.nstr(z.real,100),mp.nstr(z.imag,100)] for z in result]})
path=Path(__file__).parent/'oracles'/'fft-mpmath.json'
path.parent.mkdir(parents=True,exist_ok=True)
path.write_text(json.dumps({'generator':'mpmath direct DFT','mpmathVersion':mp.__version__,'precisionDigits':200,'storedDigits':100,'inputPolicy':'exact integer complex components','cases':cases},indent=2),encoding='utf-8')
print(path)
