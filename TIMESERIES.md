# MathemaSharp.TimeSeries — fondamenti statistici del forecasting

> **Questa pagina è un riassunto.** La trattazione completa — formule, realizzazione, uso funzione per funzione, overlap con NumPy in PySharp, esempi in C# e PySharp, scenari industriali — è nel **[libro](refs/timeseries/README.md)**.

Libreria (pacchetto NuGet) in `src/libs/MathemaSharp.TimeSeries`. Dipende solo da `MathemaSharp.Core`, target net11.0, **generica su `T : IFloatingPointIeee754<T>`** (float, double, Half, Decimal32/64/128). Segue la cronologia di [refs/timeseries/time-series-forecasting-0-01/time-series-forecasting-0-01.jpg](refs/timeseries/time-series-forecasting-0-01/time-series-forecasting-0-01.jpg): *una cronologia di idee, non una classifica di modelli*. Ogni tappa ha un'implementazione, test che ne verificano le proprietà matematiche e un notebook eseguito.

## Analisi delle infografiche 03–05 e nuovi mattoni

Analisi visiva delle tre immagini originali in `refs/timeseries/`:

- **03 — Transformer e baseline forti:** Informer seleziona query e distilla la sequenza; Autoformer separa componenti e aggrega ritardi; N-HiTS combina risoluzioni. DLinear applica due mappe lineari a trend e resto (non è STL seguito da ARIMA). PatchTST usa una finestra di un canale come token, iTransformer la storia di una variabile. Cambia quindi l'asse dell'attenzione, non solo la lunghezza della finestra.
- **04 — Linguaggio e pre-addestramento:** Time-LLM adatta ingressi a un LLM congelato; LLMTime serializza numeri in testo. TimeGPT, TimesFM, Moirai e Chronos apprendono da serie temporali. L'immagine distingue patch continue, encoder mascherato e quantizzazione in token: non sono pipeline intercambiabili. Zero-shot richiede comunque validazione locale.
- **05 — Efficienza e multivariato:** routing sparso (Time-MoE), quantili diretti (Chronos-Bolt), contesto condiviso (Chronos-2), decoder semplificato (Moirai 2.0) e attenzione temporale/fra variabili (TimesFM-3). Le date e le etichette sono quelle della figura, **non verificate qui su fonti primarie**; i diagrammi non dimostrano superiorità prestazionale.

### Implementazione verificabile

`HistoryPatches.Extract<T>(history, length, stride)` estrae copie di finestre complete, senza padding. È il mattone di rappresentazione illustrato da PatchTST, **non** implementa embedding, attenzione o addestramento.

`RollingOrigin.Evaluate<T>` rifà fit/previsione a origini successive con training espandente. Ogni callback riceve solo una copia del passato. Il gap richiede di prevedere anche i passi intermedi, che vengono esclusi dal punteggio. Restituisce MAE/RMSE per origine; con finestre sovrapposte i punteggi sono dipendenti. Il callback deve stimare anche normalizzazione e iperparametri esclusivamente sul training (eventuale selezione in validazione interna).

```csharp
var patches = HistoryPatches.Extract<double>(series, length: 24, stride: 12);
var folds = RollingOrigin.Evaluate<double>(series, initialTraining: 168,
    horizon: 24, fitForecast: (past, h) =>
        ReferenceForecasts.SeasonalNaive<double>(past, 24, h), step: 24);
foreach (var fold in folds)
    Console.WriteLine($"Origine {fold.Origin}: MAE={fold.Mae}, RMSE={fold.Rmse}");
```

Confrontare baseline e modello sulle **stesse origini**, mantenendo un test finale intatto. Questo backtest non è un test di significatività e non dà garanzie di copertura. Test automatici: patch complete e indipendenti, isolamento del passato, allineamento del gap, errori analitici di drift/naive e input invalidi. Transformer, DLinear addestrato e foundation model restano non implementati; nessun benchmark di accuratezza è stato prodotto da questa analisi.

## Dalla cronologia all'API

| Anni | Idea | API | Notebook |
|---|---|---|---|
| — | Mean, naive, seasonal naive, drift; MAE/RMSE/MAPE/MASE | `ReferenceForecasts`, `Accuracy` | 06 |
| 1927–1938 | AR di Yule, decomposizione di Wold | `SeriesStatistics` (ACF, PACF, Levinson-Durbin, Ljung-Box), `ArModel.FitYuleWalker` | 06 |
| 1950s–1960 | SES, Holt (1957), Holt-Winters (1960) | `ExponentialSmoothing.Ses/Holt/HoltWinters`, `FitSesAlpha` | 07 |
| 1960 | Filtro di Kalman | `KalmanFilter` (`LocalLevel`, `LocalLinearTrend`, `LocalLevelSteadyStateGain`) | 07 |
| 1985 | Damped trend | parametro `phi` di `Holt`/`HoltWinters` | 07 |
| 1970 | Box-Jenkins / ARIMA | `ArimaModel.FitHannanRissanen`, `Residuals`, `Forecast` | 08 |
| 1980 | VAR | `VarModel.Fit/Forecast` | 08 |
| 1982/1986 | ARCH/GARCH | `Garch11` (varianze condizionali, log-verosimiglianza, previsione, `FitVarianceTargeting`) | 08 |
| 1990 | STL | `Stl.Decompose`, `Stl.Loess` | 09 |

Di supporto: `MatrixAlgebra.LeastSquares` (QR di Householder), `SpecialFunctions` (log Γ di Lanczos, gamma incompleta, sopravvivenza χ²), `Simulation` (ARMA, random walk, GARCH, VAR con seme esplicito).

## Identità verificate dai test

Sono il cuore didattico: le famiglie di modelli non sono isole.

- **SES ≡ ARIMA(0,1,1) con θ = α − 1.** Da ℓ_t = y_t − (1−α)e_t segue ŷ_{t+1} = y_t + (α−1)e_t. Previsioni identiche a 1e-9.
- **SES ≡ filtro di Kalman stazionario del local level.** La Riccati P = P − P²/(P+r) + q dà P* = (q + √(q²+4qr))/2 e K* = P*/(P*+r). Dopo il transitorio le previsioni coincidono a 1e-9; α dipende solo da q/r.
- **Levinson-Durbin ≡ soluzione diretta di Yule-Walker** Γφ = γ. Sull'ACF teorica di un AR(1), la PACF vale φ al lag 1 ed è esattamente 0 dopo.
- **Damped trend**: le previsioni convergono a ℓ_T + φ b_T/(1−φ); con φ = 1 è Holt lineare, che riproduce le rette con SSE nullo.
- **Holt-Winters esatto su retta + stagionalità a media nulla**, grazie all'inizializzazione centrata: la media del primo ciclo è il livello al tempo (m−1)/2, non a m−1.
- **STL esatta su retta + stagionalità periodica a media nulla**: il LOESS lineare riproduce le rette (anche estrapolando) e la media mobile di un periodo annulla la stagionalità. Con le iterazioni robuste un outlier finisce nel resto con peso ≈ 0.
- **GARCH**: la previsione di σ² decade verso ω/(1−α−β) con ragione esattamente α + β.
- **VAR(1)**: le previsioni convergono a (I − A)⁻¹c.
- **Stime su dati simulati** con parametri noti: Yule-Walker per AR(2), Hannan-Rissanen per ARMA(1,1) (residui bianchi secondo Ljung-Box), OLS per VAR(1), variance targeting per GARCH(1,1).

## Contratti e scelte

- Serie come `ReadOnlySpan<double>`; più serie (VAR) come `double[tempo, serie]`. Per dati NDSharp: `MathemaSharp.NDSharp` (`ToVector`, `ToMatrix`).
- Autocovarianza con divisore n: matrice di Toeplitz semidefinita positiva, quindi |PACF| ≤ 1 e AR di Yule-Walker sempre stazionario.
- `ArimaModel`: residui condizionali (CSS), con e_t = 0 prima del primo valore stimabile. Hannan-Rissanen è lineare, senza ottimizzazione e consistente, ma meno efficiente della massima verosimiglianza. Stazionarietà e invertibilità **non** sono imposte.
- Smoothing: parametri in [0,1] (`ArgumentOutOfRangeException` altrimenti), inizializzazioni deterministiche non stimate. `FitSesAlpha` usa la sezione aurea e presuppone un SSE unimodale.
- `KalmanFilter`: osservazione scalare (S è un numero, nessuna inversione); covarianza in forma di Joseph; `NaN` = dato mancante (si salta l'update); log-verosimiglianza per decomposizione dell'errore di previsione.
- `Garch11`: shock a media nulla; σ²_0 = media di ε²; la stima per griglia raffinata con variance targeting è robusta ma non efficiente.
- `Stl`: fedele all'algoritmo di Cleveland et al., non al Fortran (niente *jump*, solo LOESS di grado 1); n_s dispari, almeno due periodi.
- Regressioni con QR di Householder: colonne dipendenti → `ArgumentException`.

## Non incluso

Massima verosimiglianza esatta per ARIMA, smoother di Kalman, ETS moltiplicativo e intervalli di previsione, selezione automatica degli ordini, VAR strutturali, GARCH asimmetrici. Vedi [ROADMAP.md](ROADMAP.md).

## Esecuzione

```sh
dotnet test src/tests/MathemaSharp.TimeSeries.Tests -c Release
dotnet run --project src/apps/MathemaSharp.Lab -- --topic timeseries
pwsh scripts/run-notebooks.ps1 -Filter '0[6-9]*'
```

Riferimenti: Hyndman & Athanasopoulos, *Forecasting: Principles and Practice* (2ª ed.), più le pubblicazioni originali (Yule 1927, Wold 1938, Holt 1957, Winters 1960, Kalman 1960, Box & Jenkins 1970, Sims 1980, Engle 1982, Hannan & Rissanen 1982, Gardner & McKenzie 1985, Bollerslev 1986, Cleveland et al. 1990).
