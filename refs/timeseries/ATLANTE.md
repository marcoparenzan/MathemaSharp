# Atlante delle teorie

> Le **91 teorie** delle 18 infografiche di [`refs/timeseries`](.), con **che cosa esiste nel repository** per ciascuna. Le infografiche descrivono idee e cronologia, non una classifica di modelli: qui si dice solo cosa è stato **realizzato**.

## Legenda dello stato

| | Significato |
|---|---|
| ✅ | **implementata** nella libreria, con test che ne verificano le proprietà |
| ◐ | **in parte**: un sottoinsieme, un mattone di base, o soltanto nel codice di un notebook |
| ⬜ | **non implementata**. Se compare un testo, è quello della Parte I solo per ciò che serve a capirla; la scheda di approfondimento della teoria è **ancora da scrivere** |

Colonna **Testo**: il capitolo del libro che spiega formule e realizzazione ([Parte I](00-formule-e-libreria/README.md)). Colonne **C#** e **PySharp**: i notebook che la mostrano in funzione ([`notebooks/`](../../notebooks/), [`scenari/`](../../scenari/README.md)). Colonna **Libreria**: la classe o la funzione ([API](01-libreria/02-uso-e-funzioni.md)).

Abbreviazioni dei notebook: **06–09** = [`notebooks/06…09`](../../notebooks/) (C#); **10** = il [giro della libreria](01-libreria/04-esempi.md), in C# e in PySharp; **A1**, **D1** = scenari [A](../../scenari/A-magazzino-frigorifero/README.md) (magazzino frigorifero) e [D](../../scenari/D-fotovoltaico/README.md) (fotovoltaico), in C# e in PySharp.

**Riepilogo:** 13 ✅ · 12 ◐ · 66 ⬜ (su 91, contati sulle tabelle di questa pagina).

---

## Fondamenti statistici — [`time-series-forecasting-0-01`](time-series-forecasting-0-01/README.md)

**Schede complete** (descrizione teorica + racconto a fumetti) per tutte e 9 le teorie. Per le altre infografiche l'atlante è ancora l'unico indice.

| Teoria | Anni | Stato | Testo | C# | PySharp | Libreria |
|---|---|---|---|---|---|---|
| [Previsioni di riferimento: mean, naive, seasonal naive](time-series-forecasting-0-01/01-previsioni-di-riferimento/README.md) | — | ✅ | [cap. 3](00-formule-e-libreria/03-riferimenti-e-metriche.md) | 06, 07, A1, D1 | 10, A1, D1 | `ReferenceForecasts`, `Accuracy` |
| [Autoregressione e innovazioni (Yule; Wold)](time-series-forecasting-0-01/02-autoregressione-e-innovazioni/README.md) | 1927–38 | ✅ | [cap. 2](00-formule-e-libreria/02-statistica-di-base.md), [4](00-formule-e-libreria/04-ar-arima.md) | 06 | 10 | `ArModel`, `SeriesStatistics` |
| [Smoothing esponenziale: SES, Holt, Holt-Winters](time-series-forecasting-0-01/03-smoothing-esponenziale/README.md) | 1950s–60 | ✅ | [cap. 5](00-formule-e-libreria/05-smoothing.md) | 07, A1 | 10, A1 | `ExponentialSmoothing` |
| [Filtro di Kalman](time-series-forecasting-0-01/04-filtro-di-kalman/README.md) | 1960 | ✅ | [cap. 6](00-formule-e-libreria/06-kalman.md) | 07, A1 | 10, A1 | `KalmanFilter` |
| [Box-Jenkins / ARIMA](time-series-forecasting-0-01/05-box-jenkins-arima/README.md) | 1970 | ◐ solo Hannan-Rissanen, non la massima verosimiglianza esatta | [cap. 4](00-formule-e-libreria/04-ar-arima.md) | 08 | 10 | `ArimaModel` |
| [VAR in macroeconomia](time-series-forecasting-0-01/06-var/README.md) | 1980 | ✅ | [cap. 7](00-formule-e-libreria/07-var.md) | 08, A1 | 10, A1 | `VarModel` |
| [ARCH / GARCH](time-series-forecasting-0-01/07-arch-garch/README.md) | 1982/86 | ◐ solo GARCH(1,1) gaussiano (ARCH(1) = β = 0) | [cap. 8](00-formule-e-libreria/08-garch.md) | 08, A1 | 10, A1 | `Garch11` |
| [Damped trend](time-series-forecasting-0-01/08-damped-trend/README.md) | 1985 | ✅ | [cap. 5](00-formule-e-libreria/05-smoothing.md) | 07 | 10 | `ExponentialSmoothing.Holt(…, phi)` |
| [Decomposizione STL](time-series-forecasting-0-01/09-stl/README.md) | 1990 | ✅ | [cap. 9](00-formule-e-libreria/09-stl-loess.md) | 09 | 10 | `Stl` |

## Automazione e modelli neurali — [`time-series-forecasting-0-02`](time-series-forecasting-0-02/README.md)

**Schede complete** (descrizione teorica + racconto a fumetti) per tutte e 9 le teorie.

| Teoria | Anni | Stato | Note |
|---|---|---|---|
| [LSTM](time-series-forecasting-0-02/01-lstm/README.md) | 1997 | ⬜ | rete ricorrente a memoria con porte |
| [Quadro ETS e Auto ARIMA](time-series-forecasting-0-02/02-ets-e-auto-arima/README.md) | 2002/08 | ⬜ | Holt-Winters additivo coincide con ETS(A,A,A) nelle previsioni puntuali; manca la stima per verosimiglianza e la selezione automatica (nel notebook 08 il confronto fra ordini è manuale, con AIC) |
| [TBATS](time-series-forecasting-0-02/03-tbats/README.md) | 2011 | ⬜ | stagionalità multiple e non intere |
| [Alberi su feature di ritardo](time-series-forecasting-0-02/04-alberi-su-feature-di-ritardo/README.md) | anni 2010 | ⬜ | la regressione su ritardi e covariate esiste (`MatrixAlgebra.LeastSquares`, usata in D1) ma non gli alberi |
| [Prophet](time-series-forecasting-0-02/05-prophet/README.md) | 2017 | ⬜ | trend + stagionalità + festività; STL è la parte "decomposizione" |
| [DeepAR](time-series-forecasting-0-02/06-deepar/README.md) | 2017 | ⬜ | |
| [Transformer](time-series-forecasting-0-02/07-transformer/README.md) | 2017 | ⬜ | |
| [N-BEATS](time-series-forecasting-0-02/08-n-beats/README.md) | 2019 | ⬜ | |
| [Temporal Fusion Transformer](time-series-forecasting-0-02/09-temporal-fusion-transformer/README.md) | 2019 | ⬜ | |

## Transformer e baseline forti — `time-series-forecasting-0-03`

| Teoria | Anni | Stato | Note |
|---|---|---|---|
| Informer | 2020 | ⬜ | |
| Autoformer | 2021 | ⬜ | usa una decomposizione tendenza/stagionalità come STL, ma dentro la rete |
| N-HiTS | 2022 | ⬜ | |
| DLinear | 2022 | ⬜ | decomposizione + due mappe lineari: i mattoni (`Stl`, `LeastSquares`) ci sono, il modello no |
| PatchTST | 2022 | ⬜ | |
| iTransformer | 2023 | ⬜ | |

## Modelli linguistici e pre-addestramento — `time-series-forecasting-0-04`

| Teoria | Data | Stato |
|---|---|---|
| Time-LLM | ott 2023 | ⬜ |
| TimeGPT-1 | ott 2023 | ⬜ |
| LLMTime | ott 2023 | ⬜ |
| TimesFM | ott 2023 | ⬜ |
| Moirai | feb 2024 | ⬜ |
| Chronos | mar 2024 | ⬜ |

## Modelli efficienti e multivariati — `time-series-forecasting-0-05`

| Teoria | Data | Stato |
|---|---|---|
| Time-MoE | set 2024 | ⬜ |
| Chronos-Bolt | nov 2024 | ⬜ |
| Chronos-2 | ott 2025 | ⬜ (il suo uso di covariate è il tema dello scenario D, con una regressione) |
| Moirai 2.0 | nov 2025 | ⬜ |
| TimesFM-3 | ago 2026 | ⬜ |

## Diagnostica: punti di cambiamento — `time-series-diagnostic-01`

| Teoria | Stato | Note |
|---|---|---|
| PELT (offline) | ⬜ | |
| BOCPD (online) | ⬜ | |
| ClaSP (offline) | ⬜ | |

*Il cambiamento non è l'allarme*: il concetto (ritardo di rilevazione, falsi allarmi) è mostrato con il GARCH nello scenario A (allarme 71 minuti dopo il cambio vero): ◐ come dimostrazione, non come algoritmo.

## Diagnostica: matrix profile — `time-series-diagnostic-02`

| Teoria | Stato |
|---|---|
| Nearest neighbor (matrix profile) | ⬜ |
| Motif e discord | ⬜ |
| FLUSS / FLOSS | ⬜ |

## Validazione delle previsioni, 1982–2000 — `time-series-forecast-validation-01`

| Teoria | Anni | Stato | Note |
|---|---|---|---|
| Competizione M | 1982 | ◐ | benchmark di riferimento e MASE (`ReferenceForecasts`, `Accuracy.Mase`) |
| Principio prequenziale | 1984 | ⬜ | |
| Competizione M2 | 1987–89 | ⬜ | |
| h-block CV | 1994 | ⬜ | |
| hv-block CV | 2000 | ⬜ | |
| Rolling-origin | 2000 | ✅ | `RollingOrigin.Evaluate`: training espandente, gap, MAE/RMSE per origine e test di isolamento del passato. Gli scenari esistenti non sono ancora migrati al backtest multiplo |

## Validazione: confronto fra previsioni, 2000–2012 — `time-series-forecast-validation-02`

| Teoria | Anni | Stato |
|---|---|---|
| Reality Check (White) | 2000 | ⬜ |
| Test SPA (Hansen) | 2005 | ⬜ |
| Capacità condizionale (Giacomini-White) | 2006 | ⬜ |
| Clark-West | 2007 | ⬜ |
| Model Confidence Set | 2011 | ⬜ |
| CV per serie temporali (Bergmeir-Benítez) | 2012 | ⬜ |

## Validazione: oltre un singolo punteggio, 2012–2025 — `time-series-forecast-validation-03`

| Teoria | Anni | Stato | Note |
|---|---|---|---|
| Test robusti alla finestra | 2012 | ⬜ | |
| Diagrammi di Murphy | 2016 | ⬜ | `Accuracy.PinballLoss` è la perdita elementare dei quantili: ◐ come mattone |
| LFO approssimato | 2019/20 | ⬜ | |
| Confronto sempre valido (*anytime-valid*) | 2021 | ⬜ | |
| Instabilità di breve durata | 2024 | ⬜ | |
| Model prediction set (conforme) | 2025 | ⬜ | |

## Valutare i modelli pre-addestrati, 2024–2026 — `time-series-forecast-validation-04`

| Teoria | Anni | Stato |
|---|---|---|
| GIFT-Eval | 2024 | ⬜ |
| fev-bench | 2025 | ⬜ |
| TIME | 2026 | ⬜ |

## Mappa delle architetture — `time-series-forecasting-1-01-a map of model architectures`

| Teoria | Stato | Note |
|---|---|---|
| Lineare / MLP | ◐ | una mappa lineare dalla finestra al futuro è una regressione: `MatrixAlgebra.LeastSquares` (scenario D), non un MLP |
| CNN temporale | ⬜ | |
| RNN / LSTM | ⬜ | |
| Transformer | ⬜ | |
| Modello in spazio degli stati | ◐ | il filtro di Kalman è il caso lineare (`KalmanFilter`); non quelli con dinamica appresa |
| Rete neurale su grafo | ⬜ | |

## Rappresentare la storia — `time-series-forecasting-1-02-representing the history`

| Teoria | Stato | Testo | Libreria |
|---|---|---|---|
| Valori singoli | ✅ | — | la serie stessa (`T[]`) |
| Patch | ◐ | [Analisi e uso](../../TIMESERIES.md) | `HistoryPatches.Extract`: finestre complete con stride, senza padding o embedding appreso |
| Componenti (trend + stagionale + resto) | ✅ | [cap. 9](00-formule-e-libreria/09-stl-loess.md) | `Stl` |
| Dominio della frequenza | ◐ | | `Signals.Dft` (didattica, $O(N^2)$); in PySharp `np.fft` |
| Scale multiple | ⬜ | | |

## Come il passato arriva alla previsione — `time-series-forecasting-1-03-how the past reaches the forecast`

| Teoria | Stato | Note |
|---|---|---|
| Convoluzione temporale | ◐ | la media mobile (filtro passa-basso di STL) è una convoluzione; non c'è una CNN |
| Stato ricorrente | ◐ | lo stato del Kalman e le ricorsioni di AR/ARIMA portano l'informazione nel tempo; non una RNN |
| Auto-attenzione | ⬜ | |

## Un passo o l'intero orizzonte — `time-series-forecasting-1-one step or the whole horizon`

| Teoria | Stato | Note |
|---|---|---|
| Ricorsiva | ✅ | `ArModel.Forecast`, `ArimaModel.Forecast`, smoothing: le previsioni a $h$ passi sono ricorsive |
| Diretta | ⬜ | si può fare con una `LeastSquares` per orizzonte, ma non è una funzione della libreria |
| MIMO | ⬜ | |
| Rectify | ⬜ | |

## Oltre ricorsivo contro diretto — `time-series-forecasting-1-beyond recursive vs direct`

| Teoria | Anno | Stato |
|---|---|---|
| Stratify | 2025 | ⬜ |
| QDF | 2026 | ⬜ |
| FlowTime | 2025 | ⬜ |
| TSFlow | 2025 | ⬜ |

## Condizionare a un futuro incerto — `time-series-forecasting-1-conditioning on an uncertain future`

| Teoria | Stato | Note |
|---|---|---|
| Condizionare a un input noto | ◐ | scenario D: regressione sull'irraggiamento previsto (`LeastSquares`) |
| Propagare l'incertezza dell'input | ⬜ | nello scenario D l'errore meteo è assorbito dai residui, non modellato a parte |
| Intervenire (osservare ≠ fare) | ⬜ | |

## Dai punti ai futuri possibili — `time-series-forecasting-1-from points to possible futures`

| Teoria | Stato | Testo | Libreria / note |
|---|---|---|---|
| Previsione puntuale | ✅ | tutta la Parte I | tutte le previsioni |
| Quantili e intervalli | ✅ | [API](01-libreria/02-uso-e-funzioni.md) | `SeriesStatistics.Quantile`, `Accuracy.PinballLoss`, `Accuracy.IntervalCoverage`; D1 |
| Distribuzione predittiva | ◐ | | l'insieme delle traiettorie campionate di D1 la approssima |
| Traiettorie campionate | ◐ | | ricampionamento di blocchi di residui in D1 (nel codice del notebook, non una funzione della libreria) |

---

## Prossime implementazioni naturali

Per rapporto fra utilità negli scenari e costo, in ordine:

1. **Rolling origin** e **Diebold-Mariano / Clark-West**: servono a dire se un modello batte davvero un altro (A e D usano una sola settimana di prova).
2. **Previsione conforme** e **intervalli per ARIMA/VAR**: intervalli con copertura garantita (oggi per quantili dei residui, senza garanzia).
3. **Matrix profile (STOMP)**: cicli di sbrinamento, anomalie del ciclo di lavorazione.
4. **PELT** e **BOCPD**: datare cambi di setpoint o di regime.
5. **ETS in spazio degli stati con selezione AIC** e ARIMA per massima verosimiglianza esatta.

I modelli neurali, i *foundation model* e i benchmark (una quarantina di voci di questo atlante) richiedono addestramento e dati che questa libreria non intende fornire: restano descritti, non implementati.
