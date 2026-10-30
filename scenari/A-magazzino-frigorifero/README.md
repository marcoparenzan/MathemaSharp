# Scenario A · Cold-storage warehouse (cold chain)

> A warehouse at +2 °C holds a customer's merchandise. The PLC sends the historian temperature, door, defrost, current, pressure and vibration every minute; a meter sends the kWh every hour. Whoever works on the data must **certify the cold chain**, **forecast the energy** and **notice in time that the compressor is failing**. The customer's question: **"was my merchandise always below +4 °C?"**

| | |
|---|---|
| C# notebook | [`A1-magazzino-frigorifero-csharp.ipynb`](A1-magazzino-frigorifero-csharp.ipynb): generic API |
| PySharp notebook | [`A1-magazzino-frigorifero-pysharp.ipynb`](A1-magazzino-frigorifero-pysharp.ipynb): `Interop` facade and `matplotlib` |
| Agreement | same analyses, **same 38 numbers**: verified by `ScenarioANotebooksAgreeAcrossLanguages` |
| Data | [`data/`](data/): `cella_1min.csv` (72 h at one minute), `energia_oraria.csv` (6 weeks), `verita.json` (cella = chamber, energia oraria = hourly energy, verita = truth) |
| Generator | [`generate-data.cs`](generate-data.cs): `dotnet run scenari/A-magazzino-frigorifero/generate-data.cs` |
| Run | `pwsh scripts/run-notebooks.ps1 -Filter 'A1-*'` |

**The data is synthetic.** The generator knows the truth (`verita.json`: the true minutes above the threshold, when the vibration changes): it is the only way to know whether the pipeline is right. With real data this verification does not exist.

## The signals

| Signal | Period | Generator notes |
|---|---|---|
| `temp_cella_c` | 1 min | thermostat with ±0.5 °C hysteresis around +2 °C, sensor noise σ = 0.08 °C, **6 communication gaps** (70 minutes, 1.6 %) |
| `porta_aperta`, `sbrinamento` | 1 min | door open during loading hours; defrost at 3, 11 and 19 for 25 minutes; **Tuesday 14:00 a door left open for 22 minutes** |
| `corrente_compressore_a` | 1 min | ~17.5 A on, ~1.8 A off |
| `pressione_aspirazione_bar` | 1 min | low with the compressor on |
| `vibrazione_mm_s` | 1 min | level per state + GARCH(1,1) shock; **after 48 h ω triples** (a bearing failing) |
| `kwh`, `temp_esterna_c` | 1 h | daily and weekly cycle, effect of the weather, defrosts |

## What is found (true results, from the executed notebooks)

### 1 · Acquire: data quality
6 gaps, 70 missing minutes, the longest 20 minutes. **A gap is "suspect"** if the temperature at its edges differs by more than 1 °C: there is one, of 12 minutes.

### 2 · Kalman and the HACCP KPI
The process variance $q$ is chosen on the log-likelihood: $q=0.01$ (2689 against −646 for $q=0.001$).

| Measure | Minutes above +4 °C |
|---|---|
| Observed in the data | **7** |
| Kalman, level only | 8 |
| Kalman, level + slope | 9 |
| **Honest interval** (observed … observed + suspect gaps) | **7 – 19** |
| **Truth** (known only to the generator) | **11** ✓ inside the interval |

> **The rise happens entirely inside a communication gap** (the temperature goes from 2.4 °C to 4.5 °C between two samples): no filter can see it. The filter does not "guess": it keeps the last level (or extends the slope) and then jumps. This is why the answer to the customer is not "always below +4 °C" but **"from 7 to 19 minutes above the threshold in a single excursion"**, with the recommendation to check the door and the gateway.

### 3 · Energy forecast (one test week)

| Model | MASE (m = 168) | Week total | Error on the total |
|---|---|---|---|
| seasonal naive 24 h | 1.871 | 6479 kWh | −12.4 % |
| **seasonal naive 168 h** | **0.918** | 7120 kWh | −3.7 % |
| Holt-Winters 24 h | 1.904 | 6361 kWh | −14.0 % |
| Holt-Winters 168 h | 0.934 | **7415 kWh** | **+0.3 %** |
| *actual* | | *7396 kWh* | |

**Two criteria, two answers**: the hourly profile is done better by the weekly seasonal naive, the total (the budget) by Holt-Winters. And with **a single week** choice and evaluation coincide: it is optimistic. Serious validation (*rolling origin*) is a theory of the book **not yet implemented**.

### 4 · GARCH on the vibration
Estimate on the first 48 h: $(\omega,\alpha,\beta)=(0.0019,\,0.083,\,0.809)$ against the true $(0.002,\,0.10,\,0.80)$. Minutes in alarm (conditional σ > 1.5 σ normal): **0.5 %** on the first day, 0.2 % on the second, **22.7 %** on the third. The sustained alarm (≥ 8 minutes in the last hour) goes off **71 minutes after** the true change, with no false alarms before. GARCH does not say *what* is failing, it says that the behaviour has changed.

### 5 · VAR: the thermostat, rediscovered from the data
On 5-minute means, in the temperature equation the current weighs **−0.033** (compressor on ⇒ it cools down); in the current equation the temperature weighs **+9.06 A/°C** (warm chamber ⇒ the thermostat turns the compressor on).

### 6 · Communicate
A row of KPI tiles (with a symbol and a word for the state, never the colour alone), three charts and two ready texts with the numbers: the answer to the customer and the note for maintenance.

## What is missing (and where it is in the book)

| What would be needed | Why | Status |
|---|---|---|
| **Change point** (PELT, BOCPD) | date the change of vibration or setpoint without knowing it | described in the book, not implemented |
| **Matrix profile** (motif, discord) | recognise the defrost cycle and the anomalies between cycles | same |
| **Rolling origin** | evaluate the energy models on many origins, not one week | same |
| **Prediction intervals** | give a range to the kWh, not a single number | same (planned with scenario D) |
