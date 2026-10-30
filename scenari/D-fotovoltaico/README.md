# Scenario D · Photovoltaic park

> A 1 MWp park (600 kW AC inverters) sells energy on the day-ahead market. The SCADA records every hour irradiance, temperatures, power and inverter state; a weather service sends, the day before, the **irradiance forecast** with its error. The customer's (the trader's) question: **"how much energy do you offer on the market tomorrow?"**, and the answer must be given **with an uncertainty**, because producing less than offered costs more than producing more.

| | |
|---|---|
| C# notebook | [`D1-fotovoltaico-csharp.ipynb`](D1-fotovoltaico-csharp.ipynb): generic API |
| PySharp notebook | [`D1-fotovoltaico-pysharp.ipynb`](D1-fotovoltaico-pysharp.ipynb): `Interop` facade and `matplotlib` |
| Agreement | same analyses, **same numbers**: verified by `ScenarioDNotebooksAgreeAcrossLanguages` |
| Data | [`data/`](data/): `impianto_orario.csv` (56 hourly days; impianto orario = hourly plant), `verita.json` (truth) |
| Generator | [`generate-data.cs`](generate-data.cs): `dotnet run scenari/D-fotovoltaico/generate-data.cs` |
| Run | `pwsh scripts/run-notebooks.ps1 -Filter 'D1-*'` |

**The data is synthetic.** The generator uses the position of the sun at 45°N, three types of day (clear 45 %, variable 35 %, overcast 20 %), an error of the weather service (σ = 0.12 on the day's clearness degree), soiling of the modules (0.09 % per day) reset by a rain on day 30, an inverter failure on day 38 (5 hours) and the 600 kW limit. 49 days serve for training, 7 (14–20 September) for the test.

## What is found (true results, from the executed notebooks)

### 1 · Plant KPIs
| KPI | Value |
|---|---|
| Performance ratio before the cleaning (days 24–29) | **0.780** |
| Performance ratio after the cleaning (31–36) | **0.809**: the dirt cost ~3.6 % of the energy |
| Performance ratio on the day of the failure | 0.368 |
| Availability (sunshine hours with the inverter in service) | 99.3 % |
| Hours at the inverter's maximum power (clipping) | 17 |

### 2 · Power forecast, knowing the forecast weather
Regression $P\approx\beta_0+\beta_1G_{\text{prev}}+\beta_2G_{\text{prev}}T_{\text{aria}}$ with $\beta=(10.99,\;0.807,\;-0.0027)$, limited to 0…600 kW, trained with QR excluding the failure hours (prev = forecast, aria = air).

| | MASE (scale: seasonal naive at 24 h) |
|---|---|
| **Model with weather covariate** | **0.511** |
| Persistence (yesterday at the same hour) | 1.362 |

The **MAPE does not exist**: at night the power is zero and the library declares it (`mape_definita: 0`).

### 3 · Intervals: quantiles of the residuals, hour by hour
Pinball loss: P10 = 4.86, P50 = 11.59, P90 = 4.67 (kW). **P10–P90 coverage on the sunshine hours: 84.6 %** (expected 80 %). On *all* the hours it seems 91.7 %, but it is an illusion: at night the interval is [0, 0] and always contains the value.

### 4 · The energy of the whole day: trajectories, not sums of quantiles
| Day | Actual kWh | P10 | P50 | P90 | Inside P10–P90 |
|---|---|---|---|---|---|
| Mon 14 | 1415 | 914 | 1749 | 2443 | yes |
| Tue 15 | 4037 | 2433 | 3269 | 3964 | no (above) |
| Wed 16 | 3909 | 2803 | 3640 | 4334 | yes |
| Thu 17 | 3150 | 1481 | 2315 | 3010 | no (above) |
| Fri 18 | 3722 | 2835 | 3670 | 4365 | yes |
| Sat 19 | 1192 | 1196 | 2028 | 2722 | no (by 4 kWh below) |
| Sun 20 | 3001 | 2309 | 3139 | 3834 | yes |

**Summing the hourly P10s does not give the day's P10.** The band from the trajectories is on average **1529 kWh** wide against **1884** obtained by summing the hourly quantiles (which contains all 7 days, because assuming that all the hours go wrong together in the worst way is excessively prudent). The trajectories give **4 days out of 7** inside the band against the expected 80 %: with only 7 days it is compatible with chance (seeing 4 or fewer out of 7 with probability 0.8 happens about 15 times out of 100). **It is not a calibration.**

### 5 · How much to offer: the quantile that minimises the cost
If the shortfall costs $a=3$ and the excess $b=1$ per kWh, the optimal bid is the quantile of level $b/(a+b)=0.25$. Cost over the test week:

| Bid | Cost |
|---|---|
| P10 | 6470 |
| **P25** | **5094** |
| P50 (median) | 5845 |
| P90 | 13593 |

Level 0.25 wins, in agreement with the theory; with seven days it is a confirmation, not a proof.

### 6 · Communicate
Five KPI tiles, two charts and two ready texts: the answer to the trader ("for 20 September we offer about 2 thousand kWh…", with median and interval) and the note for O&M (the PR that rose after the cleaning, the failure on day 38).

## What is missing (and where it is in the book)

| What would be needed | Why | Status |
|---|---|---|
| **Conformal prediction** (*model prediction set*, intervals with guaranteed coverage) | intervals from the quantiles of the residuals do not guarantee coverage on new data | described in the book, not implemented |
| **Rolling origin** | one test week is not enough; the evaluation should be repeated on many origins | same |
| **Conditioning on an uncertain future** (propagating the weather error) | today the weather service's error enters the residuals, but is not modelled explicitly | same |
| **Comparison between forecasts** (Diebold-Mariano, Clark-West…) | say whether the model *really* beats persistence | same |
