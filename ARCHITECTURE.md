# Recap e architettura

## Evidenze senza salvataggio immagini

EvidenceOptions (net10.0, testata) separa il salvataggio dalle verifiche: --no-images è inoltrato dal gate solo ai tre tool visuali. Il rasterizer verifica PNG in memoria con signature/hash; i browser verificano DOM/layout senza cattura screenshot; il tool Navier non salva SVG/PNG. Gli output notebook e i test restano eseguiti.
Report separati da ops/visual, con SaveImages esplicito, per non attribuire nuova provenienza alle immagini vecchie. EvidenceReport valida JSON/GeneratedUtc nella finestra del processo e politica immagini. Solo su passo riuscito, il gate copia il report recente nell'archivio timestamped; errori di evidenza diventano fallimenti. Sette report JSON sono archiviati oltre ai log.
La baseline completa a 29 passi è stata rieseguita senza nuove immagini di revisione. Non equivale a una verifica visiva umana, di tutti i browser o a pubblicazione.

## Nuovo consumatore — NavierStokes

La sorgente attiva è articoli/2026.10 NavierStokes, non lo snapshot PlotSharp/src/imports. Core → PlotSharp per line/scatter; CLI/Web → Core. Nessun riferimento inverso nella libreria grafica. Solo la suite di test PlotSharp riferisce il consumatore per integrazione. Campi/PNG/contour restano residui locali dichiarati.
Le app MathemaSharp Lab/Docs non mostrano renderer .cs/.razor nel perimetro ispezionato: report lessicale generato dal tool di smoke (2/11 file). Non sono state aggiunte dipendenze fittizie per dichiararle migrate. Asset mostrati dal viewer e materiali importati restano categorie separate.
Il gate costruisce CLI/Web NavierStokes e avvia il selftest fisico originale completo; il tool Edge separato controlla tre report su fixture ridotte. Smoke e accuratezza numerica non sono intercambiabili, e una build Web non prova la UI nel browser.


## Scenari e presentazione — perimetro corrente

Otto notebook source-build, 43 SVG: aggiunti i due scenari A1/D1. `Presentation.csx` espone classi HTML mathema-kpi/mathema-quote senza cambiare stili o testi; renderer legacy e bootstrap PlotSharp continuano a condividere lo stesso helper. Il manifest dichiara i conteggi attesi, verificati in esecuzione e nei test degli output salvati. Il magazzino richiede anche un gap nei path.
La verifica Edge assembla gli output HTML degli scenari in una pagina desktop (1280 px), controlla gruppi KPI, risposte non vuote, tile presenti e assenza di overflow orizzontale. Screenshot completi osservati; non include markdown/celle del notebook o UI VS Code. Il report separa Charts e Dashboards. Nessuna modifica degli algoritmi o della logica di risposta dei due scenari.


## Manifest corrente e adozione STL/tour

Core/03 e TimeSeries/06–10 usano il bootstrap PlotSharp: sei notebook, 38 SVG. Il runner valida `requireTable` oltre al gap; il gate genera i passi di esecuzione dal manifest. Test degli output e del report browser ricavano lo stesso perimetro anziché fissare il totale precedente a 27.
STL/NDSharp mantiene il loader del bridge dopo il bootstrap numerico/grafico: nessun nuovo riferimento a PlotSharp nei kernel matematici o nel bridge. Il tour conserva calcoli e stampa numerica; la concordanza dei notebook gemelli resta protetta dai test esistenti.
Raster e browser seguono il manifest; 11 nuovi screenshot Edge osservati. Una validazione SVG individuale non equivale a una prova del layout dell'intero notebook, delle tabelle nel browser o dell'estensione VS Code.

## Verifica browser — perimetro operativo

ops/VerifyBrowserCharts.cs è un file-based tool net10.0, Playwright 1.56.0, Edge Chromium headless installato. Il JSON reflection è abilitato solo per il tool (necessario al protocollo Playwright); nessuna policy runtime delle librerie modificata. Carica singoli SVG da output notebook del manifest, blocca rete, controlla bounding-box testo/titolo e hit-testing DOM, salva screenshot/report. Verifica almeno un target tooltip raggiungibile per chart, non tutti quelli sovrapposti né i popup nativi. Test separati controllano il report salvato. Il gate invoca il browser dopo l'esecuzione reale e la rasterizzazione.
SeparateLegendRow riserva 18 px in più per legende multiserie, opt-in nel core e attivo in IndexedCharts; default legacy e layout monoseries invariati. Tre collisioni rilevate nel browser sono state corrette. Integrazione VS Code e matrice browser restano fuori dalla prova corrente.

## Rendering condiviso — gap e presentazione

Core/03 e TimeSeries/06,07,08 usano il bootstrap PlotSharp source-build. Il manifest ops/migrated-notebooks.json registra id, path, conteggi SVG (2/6/6/13) e obbligo del gap; runner e test degli output lo condividono. La facade Chart delega Lines/Panels/Stems; SpectrumChart usa l'overload source-build con etichetta k, separato dalla facade dei piloti su pacchetti preview precedenti. NaN Y interrompe i path, la scala usa Y finiti; X preserva tutti gli indici. Infinity e serie totalmente mancanti sono rifiutati. Il default di Charts.LineChart resta rigoroso; stems e bar non accettano gap.
Table/Kpi/Tile sono estratti senza modifiche semantiche in Presentation.csx. Sia Charts.csx legacy sia il bootstrap PlotSharp caricano lo stesso file: non si duplicano gli helper e nessuna dipendenza kernel viene introdotta nella libreria grafica.
Il runner cancella output prima dell'esecuzione e controlla SVG/XML e gap; test separati controllano gli output salvati. Svg.Skia rasterizza gli artefatti per review e registra hash; non è un backend PNG PlotSharp né una prova del browser. La revisione preliminare aveva trovato clipping dei marker ai bordi: corretto per i marker campione con PreserveBoundaryMarkers opt-in. Gli overlay sono esterni al clip geometrico e includono solo centri finiti entro il dominio chiuso; scale e clip di linee/barre non cambiano. Default core legacy preservato, IndexedCharts opt-in; highlight non modificati. Due spettri rigenerati osservati dopo la correzione.

## Interoperabilità typed — stato corrente

MatSharp mantiene `GenericScalar<T>` come storage reale 2D column-major. Il runtime espone una registrazione per la divisione sinistra, separata dai kernel; il modulo `MatSharpLib.NDSharp` registra adattatori per Half, float, double e Decimal32/64/128. La registrazione è statica per tipo e conserva il primo adattatore: non è una policy diversa per ogni istanza di interprete. Il solver copia i dati in T, riusa LU sulle quadrate e chiama QR per ogni RHS delle sovradeterminate. Gli underdetermined sono rifiutati, non risolti con una formula approssimativa.

PySharp distingue `ClrType`, `ClrObject` e `ClrMethod`. `ClrBinder.SelectGenericType` chiude una definizione mediante MakeGenericType e controlla arità/constraint; GetItem applica il binder a ClrType. Questo si aggiunge alla selezione dei metodi generici, non la sostituisce. I test verificano i tipi iniettati dall'host e una lista Decimal128; non certificano import, inferenza o marshalling universali.

Inventari rigenerabili: `ops/api-declarations.csv` per MathemaSharp e `ops/interop-api-declarations.csv` per le dichiarazioni dei file del ponte/CLR interessati. Comando: `dotnet run --file ops/RefreshApiDeclarations.cs`. Sono indici lessicali delle righe public, non una ricostruzione semantica completa delle firme multilinea, degli accessori o dei constraint.

I tre notebook Ecosystem sono eseguiti dalla CLI JupyterSharp dai pacchetti. Il terzo carica la facade PlotSharpCharts.csx; il notebook applicativo Core/03 usa MathemaSharp.PlotSharp.csx e il vero host JupyterNet con roll-forward .NET 11. PlotSharp è costruito separatamente, non introdotto nei kernel numerici.

Il parser TRX operational è in src/tools/Ecosystem.Validation (net10.0) con sei test xUnit dedicati: evidenza vuota/incompleta rifiutata, skipped separati dai passati, nomi e motivazioni conservati. Il gate produce TRX timestamped e conserva il report accanto al TRX e in ops/validation/pysharp-skips.md. Il secondo esegue MatEngine/PyEngine dentro C#: resta distinto dall'implementazione di plugin nativi per i due linguaggi. Nessuna dipendenza PlotSharp introdotta nei kernel matematici.


## Import di riferimento da NDSharp

Importata una copia selettiva di **68 file** da `D:\Dev\2026\repos\NDSharp` in `src/imports/NDSharp`. È uno snapshot locale, non una dipendenza attiva: nessun progetto importato è aggiunto a `MathemaSharp.slnx`. Il repository sorgente non è stato modificato. Licenza MIT, copyright 2026 Marco Parenzan: testo e intestazioni originali conservati. Non è stato registrato un commit sorgente: non trattare lo snapshot come una release upstream identificata.

Inclusi nucleo NDSharp, test del nucleo e fixture, wavelet, notebook e quattro generatori di oracoli numerici; esclusi Frame, Plot, Image, applicazione di esecuzione notebook, output di build e metadati di sessione. Alcuni notebook importati fanno riferimento ai moduli esclusi: sono materiale di riferimento, non notebook eseguibili garantiti in MathemaSharp.

| Materiale esaminato | Destinazione candidata | Lavoro necessario |
|---|---|---|
| `np.LinearAlgebra.cs`: LU e solver densi | `MathemaSharp.LinearAlgebra` | Separare conversioni NDArray/batch dal kernel numerico; confrontare con QR esistente |
| `Eig.cs`: bilanciamento, Hessenberg e QR complesso | `MathemaSharp.LinearAlgebra` | Estrarre kernel, test di residuo/convergenza e casi difettivi; oggi lavora in double/Complex |
| `Fft.cs`: radix-2 e Bluestein | Modulo segnali Numbers | Separare wrapper NDArray da trasformata; verificare normalizzazione e lunghezze arbitrarie |
| `GeneratorDistributions.cs`, `np.Stats.cs` | Modulo statistica/probabilità | Analisi dettagliata ancora da fare; mantenere separate API RNG e riduzioni per asse |
| `NDSharp.Wavelets/Dwt.cs` e filtri | Modulo segnali Numbers | Rimuovere dipendenza dalle eccezioni NDSharp; verificare provenienza dei filtri e convenzioni PyWavelets |
| Shape, stride, slicing, broadcasting, dtype | NDSharp | Non duplicare in Numbers; mantenere il ponte `MathemaSharp.NDSharp` |

**Import non significa migrazione:** il codice conservato non è ancora API Numbers, non è stato reso Generic Math e i suoi test non sono eseguiti dalla soluzione MathemaSharp. Prima dell’estrazione verificare anche le attribuzioni di codice e dati generati, oltre alla licenza del repository.

Lo script `ops/import-ndsharp.proj` documenta la selezione e rifiuta di sovrascrivere uno snapshot esistente. I file importati sono preservati senza riscritture.

## Mappa dei tipi

| Tipo | Modello / precisione indicativa | Uso e caveat |
|---|---|---|
| byte…long, UInt128/Int128 | Interi a larghezza fissa | Esatti nel range; decidere checked o wrap |
| BigInteger | Interi a precisione arbitraria | Esatti, limitati da memoria/tempo; possibili allocazioni |
| Half | IEEE binary16, circa 3 cifre decimali | Storage compatto; errori di accumulo importanti |
| float | IEEE binary32, circa 6–9 cifre | Grafica, dati compatti |
| double | IEEE binary64, circa 15–17 cifre | Calcolo scientifico generale |
| decimal | Coefficiente 96 bit × 10^-scala, scala 0–28 | 28–29 cifre; finanza con policy esplicita; overflow/divisione per zero generano eccezioni |
| Complex | Due double | Non ordinato, fase in radianti, branch cut nelle funzioni complesse |
| Decimal32 (.NET 11) | IEEE decimal, 7 cifre significative | 32 bit; NaN, infinito, signed zero, esponente variabile |
| Decimal64 (.NET 11) | IEEE decimal, 16 cifre significative | 64 bit; non equivale a double |
| Decimal128 (.NET 11) | IEEE decimal, 34 cifre significative | 128 bit; non equivale a System.Decimal |
| Complex<T> (.NET 11) | Due componenti T | T: IFloatingPointIeee754<T>, IMinMaxValue<T>; anche decimal IEEE, non System.Decimal |

Cifre significative non sono posti dopo la virgola. Base 10 rappresenta 0.1, ma non 1/3. Più precisione non elimina errori algoritmici né implica maggiore performance. `double.Epsilon` è il minimo positivo subnormale; ULP a 1 è circa 2.22e-16. NaN/infinito devono essere trattati secondo il dominio applicativo.

## Generic Math: scegliere il constraint

- Static abstract interface members consentono operatori e costanti su T senza `dynamic`, reflection o boxing obbligatorio.
- `INumberBase<T>`: somma, prodotto, Horner; accetta anche Complex.
- `INumber<T>`: numeri ordinabili, Min/Max/Clamp; non usarlo per un algoritmo che deve accettare Complex.
- `IBinaryInteger<T>`: GCD, bit e divisione intera.
- `IFloatingPoint<T>`: media frazionaria e rounding; include decimal.
- `IFloatingPointIeee754<T>`: Kahan qui, NaN, infinito, funzioni scientifiche; non include System.Decimal.
- Nuovo `IDecimalFloatingPointIeee754<T>` in 11: operazioni specifiche decimal IEEE.
- `CreateChecked`, `CreateSaturating`, `CreateTruncating`: policy di conversione, non promesse di assenza di perdita di precisione nelle conversioni floating-point.

Generic Math riusa algoritmi, non garantisce associatività, stabilità, vettorizzazione o tutte le identità matematiche in aritmetica finita.

## Layout

```text
nuget.config                feed nuget.org + D:\Dev\NuGetLocalFeed (NDSharp, JupyterNet), con package source mapping
.config/dotnet-tools.json   tool jupyternet
artifacts/                  output unico di build e pack (UseArtifactsOutput), escluso da Git
notebooks/                  notebook JupyterNet C# (01–10) e PySharp (pysharp/) + shared/*.csx
scripts/                    run-notebooks.ps1, pack.ps1
scenari/                    scenari di IoT industriale (A magazzino frigorifero, D parco fotovoltaico): dati sintetici, notebook C# e PySharp
refs/timeseries/            il libro (formule, libreria, atlante delle 91 teorie) e le 18 infografiche di partenza
src/
  Directory.Build.props     net11.0, nullable, warning come errori; metadati NuGet per src/libs
  (global.json             SDK 11 RC2 fissato alla radice)
  Directory.Packages.props  versioni centralizzate
  libs/                     librerie pacchettizzabili
    MathemaSharp.Core            Generic Math, precisione, DFT, SIMD. Nessuna dipendenza
    MathemaSharp.Quantum         matematica quantistica a dimensione finita (QUANTUM.md)
    MathemaSharp.TimeSeries      fondamenti statistici del forecasting + Interop (facciata su double[] per PySharp)
    MathemaSharp.NDSharp         ponte NDArray <-> double[]/double[,]/Complex[]
  apps/MathemaSharp.Lab          console con topic, Microsoft.Extensions.Hosting
  tests/MathemaSharp.*.Tests     un progetto xUnit per libreria; tests/Directory.Build.props aggiunge xUnit
  benchmarks/               BenchmarkDotNet
```

Dipendenze: Quantum e TimeSeries dipendono da Core. NDSharp dipende solo dal pacchetto NDSharp. Core, Quantum e TimeSeries non dipendono da NDSharp.

## Numbers e NDSharp

NDSharp (repository PySharp, pacchetto 2.5.0) è un ndarray con semantica NumPy: dtype a runtime, viste strided, broadcasting, linalg, FFT, random PCG64. Numbers è il livello tipizzato: Generic Math e `Span<T>`, controlli a compile-time, nessuna dipendenza. Si è scelto di **non fondere** le due librerie: hanno contratti diversi (dtype dinamico contro statico). `MathemaSharp.NDSharp` le collega:
- verso Numbers copia in ordine C con cast, perché le viste strided non sono esponibili come Span;
- verso NDSharp avvolge un `double[]` senza copia.

Le duplicazioni attuali (QR in `MathemaSharp.LinearAlgebra.MatrixAlgebra`, DFT didattica in Core contro la FFT di NDSharp) sono piccole e intenzionali. Se crescono, vanno estratte in un nucleo comune: è una decisione da prendere quando la duplicazione sarà reale.

## Contratti degli algoritmi

Somma vuota = Zero. Dot su lunghezze diverse genera ArgumentException; dot Complex generico bilineare, HermitianDot coniuga a sinistra. Horner usa coefficienti crescenti; polinomio vuoto = Zero. Media vuota non definita. GCD(0,0)=0; Abs(MinValue) signed può generare overflow. Factorial accetta n>=0, senza garanzie di costo per n enorme. Kahan è didattico per dati finiti, non una riduzione esatta universale. DFT forward non normalizzata O(N²); array vuoto produce output vuoto. SIMD gestisce la coda ma riordina l'accumulo. InvoiceTotal distingue rounding per riga dal totale; non implementa IVA/cambio o normative fiscali.

## Fonti consultate

Documentazione descritta come .NET 11 RC2, SDK locale 11 RC2: lo stato finale delle API può cambiare.

- https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/overview
- https://learn.microsoft.com/en-us/dotnet/api/system.numerics.decimal32?view=net-11.0
- https://learn.microsoft.com/en-us/dotnet/api/system.numerics.complex-1?view=net-11.0
- https://learn.microsoft.com/en-us/dotnet/standard/generics/math

Le prime tre pagine sono state lette durante l'implementazione; l'ultima è un riferimento aggiuntivo. Non si presume supporto hardware decimal IEEE né si pubblicano rapporti di velocità non misurati.

## Librerie: Reference

Inventario dei tipi pubblici estratto dai sorgenti. Per firme, vincoli e commenti completi consultare i file collegati e la documentazione XML prodotta dalla build. Non costituisce una conversione aggiuntiva dei kernel NDSharp.

### MathemaSharp.Core — Reference

| Tipo pubblico | Sorgente |
|---|---|
| `NumericAlgorithms` | [NumericAlgorithms.cs](src/libs/MathemaSharp.Core/NumericAlgorithms.cs) |
| `Precision` | [Precision.cs](src/libs/MathemaSharp.Core/Precision.cs) |
| `Signals` | [Signals.cs](src/libs/MathemaSharp.Core/Signals.cs) |

### MathemaSharp.LinearAlgebra — Reference

| Tipo pubblico | Sorgente |
|---|---|
| `LuFactorization` | [LuFactorization.cs](src/libs/MathemaSharp.LinearAlgebra/LuFactorization.cs) |
| `MatrixAlgebra` | [MatrixAlgebra.cs](src/libs/MathemaSharp.LinearAlgebra/MatrixAlgebra.cs) |

### MathemaSharp.Quantum — Reference

| Tipo pubblico | Sorgente |
|---|---|
| `ComplexMatrix` | [ComplexMatrix.cs](src/libs/MathemaSharp.Quantum/ComplexMatrix.cs) |
| `Pauli` | [Pauli.cs](src/libs/MathemaSharp.Quantum/Pauli.cs) |
| `QubitStates` | [Pauli.cs](src/libs/MathemaSharp.Quantum/Pauli.cs) |
| `QuantumAlgorithms` | [QuantumAlgorithms.cs](src/libs/MathemaSharp.Quantum/QuantumAlgorithms.cs) |

### MathemaSharp.TimeSeries — Reference

| Tipo pubblico | Sorgente |
|---|---|
| `ArModel` | [Arima.cs](src/libs/MathemaSharp.TimeSeries/Arima.cs) |
| `ArModel` | [Arima.cs](src/libs/MathemaSharp.TimeSeries/Arima.cs) |
| `ArimaModel` | [Arima.cs](src/libs/MathemaSharp.TimeSeries/Arima.cs) |
| `ArimaModel` | [Arima.cs](src/libs/MathemaSharp.TimeSeries/Arima.cs) |
| `SmoothingResult` | [ExponentialSmoothing.cs](src/libs/MathemaSharp.TimeSeries/ExponentialSmoothing.cs) |
| `ExponentialSmoothing` | [ExponentialSmoothing.cs](src/libs/MathemaSharp.TimeSeries/ExponentialSmoothing.cs) |
| `HistoryPatches` | [ForecastEvaluation.cs](src/libs/MathemaSharp.TimeSeries/ForecastEvaluation.cs) |
| `ForecastFold` | [ForecastEvaluation.cs](src/libs/MathemaSharp.TimeSeries/ForecastEvaluation.cs) |
| `RollingOrigin` | [ForecastEvaluation.cs](src/libs/MathemaSharp.TimeSeries/ForecastEvaluation.cs) |
| `Garch11` | [Garch.cs](src/libs/MathemaSharp.TimeSeries/Garch.cs) |
| `Garch11` | [Garch.cs](src/libs/MathemaSharp.TimeSeries/Garch.cs) |
| `LevinsonResult` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `LjungBoxResult` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `KalmanResult` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `Statistics` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `Forecasts` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `Metrics` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `Smoothing` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `Arima` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `Kalman` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `Var` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `Garch` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `Decomposition` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `Linear` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `Simulate` | [Interop.cs](src/libs/MathemaSharp.TimeSeries/Interop.cs) |
| `KalmanFilter` | [KalmanFilter.cs](src/libs/MathemaSharp.TimeSeries/KalmanFilter.cs) |
| `KalmanFilter` | [KalmanFilter.cs](src/libs/MathemaSharp.TimeSeries/KalmanFilter.cs) |
| `ReferenceForecasts` | [ReferenceForecasts.cs](src/libs/MathemaSharp.TimeSeries/ReferenceForecasts.cs) |
| `Accuracy` | [ReferenceForecasts.cs](src/libs/MathemaSharp.TimeSeries/ReferenceForecasts.cs) |
| `SeriesStatistics` | [SeriesStatistics.cs](src/libs/MathemaSharp.TimeSeries/SeriesStatistics.cs) |
| `Simulation` | [Simulation.cs](src/libs/MathemaSharp.TimeSeries/Simulation.cs) |
| `SpecialFunctions` | [SpecialFunctions.cs](src/libs/MathemaSharp.TimeSeries/SpecialFunctions.cs) |
| `StlDecomposition` | [Stl.cs](src/libs/MathemaSharp.TimeSeries/Stl.cs) |
| `Stl` | [Stl.cs](src/libs/MathemaSharp.TimeSeries/Stl.cs) |
| `VarModel` | [VectorAutoregression.cs](src/libs/MathemaSharp.TimeSeries/VectorAutoregression.cs) |
| `VarModel` | [VectorAutoregression.cs](src/libs/MathemaSharp.TimeSeries/VectorAutoregression.cs) |

### MathemaSharp.NDSharp — Reference

| Tipo pubblico | Sorgente |
|---|---|
| `NDArrayBridge` | [NDArrayBridge.cs](src/libs/MathemaSharp.NDSharp/NDArrayBridge.cs) |

## Inventario Generic Math — baseline dopo la rinomina

Ambito: solo librerie attive in `src/libs`, non applicazioni, benchmark o snapshot NDSharp. L'indice [api-declarations.csv](ops/api-declarations.csv) registra file, riga e dichiarazioni pubbliche (inclusi tipi/proprietà/operatori); i 268 record **non sono un conteggio semantico di metodi** (l'indice testuale include anche membri public di tipi non pubblici) né una percentuale di conversione. Rigenerazione: `dotnet msbuild ops/inventory-api.proj -t:Inventory`.

### Target effettivi

Core, TimeSeries, Quantum e NDSharp ereditano `net11.0` da `src/Directory.Build.props`. LinearAlgebra sovrascrive il target a **net10.0**: usa interfacce Generic Math e può essere consumata dai progetti net11.0; i test con Decimal32/64/128 sono nei test TimeSeries net11.0. Target della libreria e tipo usato dal consumatore sono due aspetti distinti.

### Classificazione delle API attive

| Famiglia / API | Constraint e aritmetica | Stato |
|---|---|---|
| Core: Sum, SumChecked, Dot, Horner | `INumberBase<T>`, operazioni in T | Generiche; Dot complesso è bilineare |
| Core: Mean | `IFloatingPoint<T>` | Generica, include System.Decimal |
| Core: KahanSum | `IFloatingPointIeee754<T>` | Generica IEEE |
| Core: Gcd | `IBinaryInteger<T>` | Generica intera; MinValue signed può traboccare |
| Core: Factorial | Input int, risultato BigInteger | Specializzazione intenzionale, non wrapper double |
| Precision: NearlyEqual, entrambi gli overload | IEEE, tolleranze in T | Generica; default min(4096 epsilon, sqrt(epsilon)), verificato sui sei formati nei casi descritti sotto |
| Precision: InvoiceTotal, DecimalScale | System.Decimal | Specializzazioni non generiche; scala legata al layout di decimal |
| Signals: Dft, HermitianDot | IEEE + `IMinMaxValue<T>`, `Complex<T>` | Generiche; DFT O(N²), non FFT |
| Signals: VectorDot | IEEE, fallback scalare se Vector<T> non supportato | Generica; SIMD non garantita da constraint/tipo |
| MatrixAlgebra: Multiply (2 overload), Identity | `INumber<T>` | Generiche, interi e decimal ammessi; non Complex<T> |
| MatrixAlgebra: Transpose | Nessun constraint numerico | Generica strutturale |
| MatrixAlgebra: NormL1/L2/Infinity, FrobeniusNorm, Determinant2x2, Inverse2x2, LeastSquares | IEEE | Generiche in T; QR normalizzata con Hypot e riflettori unitari; constraint talvolta più forte del necessario |
| LuFactorization<T>: Create, Solve, Determinant, Inverse | IEEE sul tipo | Generiche in T, pivoting parziale |
| TimeSeries: SeriesStatistics, ReferenceForecasts, Accuracy, modelli AR/ARIMA, smoothing, Kalman, VAR, GARCH, STL | API numeriche IEEE; parametri dimensionali int | Kernel generici; copertura per formato non uniforme |
| TimeSeries: HistoryPatches.Extract, RollingOrigin.Evaluate, ForecastFold<T> | Extract e record senza constraint; Evaluate IEEE | Struttura/backtest generici; test di leakage non certificano tutti i tipi |
| TimeSeries: Num.C<T>(double) | `INumberBase<T>`, double → T | Helper di classe internal, non API pubblica; costante di origine double |
| TimeSeries: SpecialFunctions | IEEE; coefficienti Lanczos da stringhe invariant → T, cache per tipo | Nessun passaggio dei coefficienti da double; accuratezza ancora limitata dall'approssimazione |
| TimeSeries: Simulation | Ricorrenze IEEE; default Box-Muller double → T oppure Func<Random,T> iniettata | Default parzialmente generici; nuovi overload senza conversione del rumore fornito in T |
| TimeSeries.Interop: Stats, Forecast, Metrics, Smooth, ARIMA, Kalman, Var, Garch, Decomposition, Linear, Simulate e record risultato | Double esplicito, delega ai kernel | Facciata non generica intenzionale per host/reflection |
| NDArrayBridge: ToVector/ToMatrix<T>, ToNDArray<T> | IEEE, passaggio da float64 | Firme generiche, **non preservano precisione/dtype T** |
| NDArrayBridge: overload double e Complex<double> | float64/complex128 | Specializzazioni esplicite; manca ponte Complex<T> generale |
| Quantum: ComplexMatrix<T>, Pauli<T>, QubitStates<T>, QuantumAlgorithms | IEEE + MinMax; aritmetica Complex<T> | Generiche; Norm con Hypot e Normalize scalata; diagonalizzazione hermitiana 2×2, non solver spettrale generale |

### Tipi effettivamente esercitati nei test

Questa tabella descrive i test trovati, **non garantisce l'intera superficie di ogni libreria**. I tipi ammessi dai constraint ma non elencati sono non verificati per quella famiglia.

| API / scenario | Tipi esercitati | Evidenza |
|---|---|---|
| Sum | int, decimal, BigInteger (vuoto), System.Numerics.Complex | Core.Tests/NumericTests.cs |
| SumChecked; Gcd | int | NumericTests.cs |
| Horner | int (vuoto), double, Complex | NumericTests.cs |
| Mean Core | decimal; double solo errore input vuoto | NumericTests.cs |
| KahanSum | double, Decimal64 | NumericTests.cs; Core.Tests/GenericFormatTests.cs |
| NearlyEqual | float, double, Decimal64, Decimal128 | NumericTests.cs; GenericFormatTests.cs; non test sistematico Half/Decimal32 |
| DFT | float, double, Decimal64, Decimal128 | GenericFormatTests.cs; niente copertura Half/Decimal32 |
| HermitianDot | double | NumericTests.cs, incluso contratto e differenza dal bilineare |
| VectorDot | float, double, Decimal64 fallback | GenericFormatTests.cs e test coda SIMD; non tutti i sei formati |
| Norme/2×2 | prevalentemente double; L2 float | TimeSeries.Tests/LinearAlgebraInfographicTests.cs |
| LU: solve, determinante, inversa, pivoting | Half, float, double, Decimal32, Decimal64, Decimal128 | TimeSeries.Tests/GenericLuTests.cs, un problema piccolo per tipo |
| Prodotti matriciali | int, decimal e formati LU | GenericLuTests.cs; non coverage universale degli overload |
| LeastSquares, SES↔ARIMA, Levinson, HoltWinters, STL, gain Kalman, chi-quadro | float, double, Decimal64, Decimal128 | TimeSeries.Tests/GenericFormatTests.cs |
| Mean TimeSeries | Half, Decimal32 | GenericFormatTests.cs, casi semplici |
| Naive | Decimal32 | GenericFormatTests.cs |
| AR con stesso rumore, GARCH/VAR | Decimal128 e confronti double dove presenti | GenericFormatTests.cs; non tutti i formati per ogni metodo/stimatore |
| Quantile | float, Decimal128 oltre ai test double | TimeSeriesTests.cs |
| Quantum: Pauli, commutatore, normalizzazione, expectation, density/Bloch | float, double, Decimal64, Decimal128 | Quantum.Tests/GenericFormatTests.cs |
| Quantum: TimeEvolution/Apply con errore < 1e-28 | Decimal128 | GenericFormatTests.cs; nessun certificato analogo Half/Decimal32 |
| Ponte NDArray generico | float, Decimal64, Decimal128 su valori rappresentabili | NDSharp.Tests/NDArrayBridgeTests.cs; verifica conversione, non conservazione della precisione |

Aggiornamento consolidamento Core: `NativePrecisionTests.cs` aggiunge casi completamente in T per **tutti i sei formati** su Sum, Mean, KahanSum, Horner, VectorDot, NearlyEqual (inclusi valori speciali), HermitianDot e DFT impulso/costante. Aggiunge inoltre un caso Decimal128 con differenza 1e-25 mantenuta da somma e prodotto scalare e rilevata da tolleranza esplicita. La tabella precedente descrive la baseline iniziale: per queste famiglie la copertura ora include Half e Decimal32, ma resta circoscritta a casi piccoli, non una certificazione generale di accuratezza.

Molti altri test multifomato creano fixture da double e alcuni riconvertono risultati a double per confrontarli. Questo dimostra coerenza a quelle tolleranze, non tutte le cifre di Decimal128. Il test Quantum a 1e-28 e la verifica LU con confronti in T danno evidenze più forti, ma su casi circoscritti. I tre test notebook sospesi non forniscono evidenza di esecuzione.

### Dove resta double, e perché

| Sorgente | Confine | Conseguenza / azione |
|---|---|---|
| NDArrayBridge.cs:15–27, 43–59, 73–77 | NDArray → double → T; T → double → float64 | Perdita potenziale di cifre, range e dtype. Priorità: mapping dtype nativo e conversioni esplicite |
| NDArrayBridge.cs:34–39, 65–70 | Complex non generico ↔ Complex<double> | Non preserva un componente Decimal128; progettare adattatore separato |
| Num.cs:12 e chiamanti C<T> | Costanti double → T | Non è conversione dei dati T a double. Numeri interi/mezzi sono esatti, 0.05/0.9/0.999 e coefficienti non necessariamente |
| SpecialFunctions.cs: Coefficients<T> | Lanczos e soglia circa 1e-16 letti da stringhe direttamente in T | Eliminato double intermedio; coefficienti/approssimazione non sufficienti a promettere 34 cifre Decimal128 |
| Simulation.cs: StandardNormal e overload default | NextDouble + Math di Box-Muller → T | Default invariato; overload con innovationSource permettono rumore in T. Non viene fornito un nuovo RNG ad alta precisione |
| Interop.cs | API e risultati double dichiarati | Non un bug del kernel: wrapper intenzionale. Documentare o separare in adattatore |
| Arima.cs:114 | Log10/Ceiling su lunghezza per scegliere ordine preliminare | Calcolo euristico strutturale, non downcast delle osservazioni |
| Stl.cs:156 | Math.Ceiling per parametro strutturale | Da distinguere dalla precisione delle serie; non è aritmetica del segnale |

Nella scansione dei kernel Core, LinearAlgebra e Quantum non sono emersi downcast dei valori T a double. Nei kernel TimeSeries il problema principale è l'origine double di rumore/costanti, non una ricorrenza eseguita interamente in double. Aggiornamento GARCH: FitVarianceTargeting non usa più slack 1e-12 o avanzamento floating-point `a += step`: i candidati sono ottenuti da indici interi, con 21 punti per asse nelle refinement. Il limite di persistenza è min(9999/10000, BitDecrement(1)), calcolato in T; il confronto ammette il limite stesso. Cambiano quindi i candidati rispetto alla vecchia griglia e i parametri stimati possono differire. Empty, zero/overflow della media dei quadrati, shock non finiti e parametri di griglia invalidi sono rifiutati; vengono accettate solo likelihood finite. NativeGarchTests aggiunge ricorrenze, forecast e fit riproducibile su tutti i sei formati e test di errori. È ancora uno stimatore a griglia con precisione dipendente dal tipo, non una garanzia di convergenza al massimo globale. Nessun benchmark nuovo.

Aggiornamento LOESS: ascisse centrate in x, margine 999/1000 ottenuto con interi in T e soglia di degenerazione 16 epsilon max(1, secondo momento centrato). Rimossa la soglia fissa 1e-12 da LOESS. Aggiunta validazione di dati finiti, ascissa finita, lunghezza dei pesi e pesi non negativi. NativeLoessTests verifica rette sui sei formati, variazioni Decimal128 1e-25, un caso con indice circa 100000 e input errati. La formula centrata riduce la cancellazione ma non garantisce accuratezza su ogni dataset. La validazione scansiona l'input ad ogni chiamata: il costo nell'uso STL ripetuto deve ancora essere misurato; nessun miglioramento prestazionale è dichiarato.

### Snapshot importato e prossime azioni

`src/imports/NDSharp` non è nella soluzione: FFT, autovalori, wavelet e altre API dello snapshot **non sono capacità attive di MathemaSharp**. Qui non vengono conteggiate come convertite né certificate dai 124 test della soluzione.

Aggiornamento Simulation: ARMA, random walk, GARCH e VAR accettano anche `Func<Random,T> innovationSource`; il callback riceve il Random del chiamante, è invocato durante il burn-in e per ogni componente VAR, e deve restituire valori finiti. Distribuzione, media zero, varianza unitaria e indipendenza sono responsabilità del chiamante. La sorgente non introduce un nuovo RNG nativo Decimal128. Nove test verificano ricorrenze e numero di campioni sui sei formati, un random walk Decimal128 con incrementi 1e-25, errori e uguaglianza dei default con la sorgente legacy esplicita. I parametri del modello e gli overflow delle ricorrenze richiedono ancora validazione più ampia.

1. Copertura Half/Decimal32 dei kernel più semplici e policy di tolleranza; test di precisione nativi T per Decimal128.
2. Rendere espliciti i confini del ponte NDSharp e aggiungere test che mostrino le perdite, prima di promettere round-trip.
3. Separare precisione delle costanti, algoritmi speciali e generazione del rumore dalla genericità delle firme.

SpecialFunctions — consolidamento: coefficienti e soglia letti una volta per tipo con cultura invariant. LogGamma rifiuta NaN/non positivi, accetta +infinito e tratta 1/2, 1 e 2 con identità. Q richiede a positivo finito, rifiuta x NaN/negativo, tratta x=0/+infinito e a=1 con identità; i gradi di libertà del chi-quadro devono essere positivi. Iterazioni oltre 1000 e intermedi non rappresentabili lanciano ArithmeticException. Risultati finiti di Q sono limitati a [0,1]; ciò non certifica accuratezza e richiede validazione su code estreme. Nove test NativeSpecialFunctionTests coprono sei formati con confronti in T, entrambi i rami Q(2,x), un caso Decimal128 1e-25 per Q(1,x), overflow e limite di iterazioni. Non viene promessa precisione Decimal128 completa per Lanczos. Il nuovo comportamento di errore può interrompere chiamanti che prima ricevevano NaN o approssimazioni non convergenti.
4. Migrare FFT/IFFT con fixture indipendenti e confronti in T; poi autovalori/wavelet.
5. Completare la matrice membro × tipo × caso: l'indice testuale non sostituisce un analizzatore Roslyn o un tracciamento semantico della copertura.

LeastSquares — consolidamento: matrix e termine noto normalizzati sulla massima magnitudine dei regressori; norme con Hypot e riflettori Householder unitari. Soglia min(4096 epsilon, sqrt(epsilon)) nella matrice normalizzata, senza floor assoluto di scala 1. Rifiuta input non finiti e segnala proiezioni/soluzioni non rappresentabili. NativeLeastSquaresTests copre sei formati, preservazione degli input, scale globali 1e-200/1e200 per double e differenza di coefficiente Decimal128 1e-25. Cambia la decisione di rango rispetto alla versione precedente: Half non usa più una soglia circa 4 e sistemi piccoli non sono automaticamente rifiutati per la scala assoluta. Non stima il condizionamento e può rifiutare un sistema se y/scala non è rappresentabile, anche se una diversa strategia numerica sarebbe possibile. Dieci test aggiunti, nessun benchmark o notebook eseguito in questa tranche.

Quantum — norme: Norm accumula Hypot su Real e Imaginary, evitando di formare il prodotto interno quadratico. Normalize verifica componenti finite, divide prima per la massima componente assoluta e poi per la norma scalata, lavorando in T. Zero e stato vuoto restano non normalizzabili; NaN/infinito ora sono rifiutati. Norm può restituire infinito se la norma supera il range: Normalize può comunque riuscire per uno stato finito. Sette test NativeStateNormTests sui sei formati coprono scale fino a MaxValue e T.Epsilon, stato di norma non rappresentabile, preservazione input e componente Decimal128 1e-25. Non certifica InnerProduct o MatrixExp su scale estreme; questi richiedono consolidamento distinto.

CheckEigenstate — consolidamento distinto: normalizza il ket e scala l'operatore; usa il quoziente di Rayleigh e verifica ||Au−lambda u||₂/||A||F sulla matrice scalata. Cambia la semantica della tolleranza: non più confronti assoluti/relativi per singola componente. Zero operator è compatibile con qualsiasi ket finito non nullo, anche con tolleranza zero; stato nullo, dimensioni errate, tolleranze negative/non finite e operatori non finiti sono rifiutati. Autovalori/intermedi non rappresentabili lanciano ArithmeticException. Sette test NativeEigenstateTests sui sei formati verificano invarianza rispetto all'ampiezza, operatori molto piccoli, autovalori complessi e rifiuto di perturbazione Decimal128 1e-25 con tolleranza esplicita. Rimangono limiti di range, accumulo e condizionamento; nessun nuovo solver spettrale.

MatrixExp — consolidamento distinto: valida matrice quadrata finita e numero positivo di termini; copia i dati e dimezza componenti finché Frobenius (Hypot) è ≤1/2. Non forma 2^s né il suo reciproco. Taylor rimane a numero di termini scelto dal chiamante (default 24), seguito da squaring con controlli di finitezza; nessuna convergenza adattiva è promessa. Intermedi non finiti e limite scaling 100000 lanciano ArithmeticException. TimeEvolution richiede t finito e hbar positivo finito e segnala un rapporto fuori range. Otto test NativeMatrixExpTests sui sei formati coprono exp della diagonale, zero, nilpotente con entry 32768, rotazione unitaria, termine Decimal128 1e-25 e errori. Restano accumulo nello squaring, underflow e perdita dei termini piccoli nello scaling; input negativo molto grande può legittimamente produrre zero per underflow. Nessuna garanzia generale per matrici non normali o grandi.

## Oracoli indipendenti e limiti osservati

Generatore `ops/generate-numerical-oracles.py`, mpmath 1.3.0 a 200 cifre; fixture `ops/oracles/numerical-reference.json` salvata a 160 cifre. Python non chiama MathemaSharp. Il consumatore C# legge e arrotonda le stringhe direttamente nel tipo T; la rigenerazione è separata dai test.

IndependentOracleTests: 12 coppie (a,x) per Q e 7 valori LogGamma su float, double, Decimal64 e Decimal128. Per a intero/sem-intero verifica anche la sopravvivenza chi-quadro equivalente. Code fino all'ordine di 1e-306: confronto relativo actual/expected−1, non una soglia assoluta che accetterebbe zero. Tolleranze dichiarate 0.002 float e 1e-10 altri; se il riferimento arrotondato a float è zero, il test controlla underflow e non certifica accuratezza relativa. Half/Decimal32 esclusi da questa tranche estrema.

Hilbert 3/5/8: H e b=H·1 ottenuti a precisione alta, poi arrotondati in T; LU/QR controllate con errore della soluzione, residuo relativo e determinante. Tolleranze soluzione double: 1e-12/1e-9/1e-5; Decimal128: 1e-29/1e-26/1e-20. Questi limiti dipendono dal problema mal condizionato e dalla perturbazione d'ingresso, non indicano perdita universale di precisione del tipo.

**Controesempio Hilbert12 double:** LU accetta i pivot, ma errore massimo contro la soluzione esatta 1 circa 0.005954; residuo relativo circa 1.312e-16. QR rifiuta il rango. Dimostra che la soglia LU non è un condition estimator e che un residuo piccolo non basta. Il test documenta il limite, non dichiara il solve accurato né cambia arbitrariamente i pivot.

Notebook TimeSeries/13-independent-oracles eseguito: errore relativo massimo Q sulle fixture circa 1.457e-12 double e 2.463e-13 Decimal128. Lanczos resta limitato dall'approssimazione; questo confronto non promette 34 cifre e non copre tutti i domini. 11 casi IndependentOracleTests e un test dell'evidenza notebook aggiunti; diagnostica del condizionamento ancora da estendere.

### Estensione Quantum dell'oracolo

Stesso generatore indipendente: mpmath.expm su 5 matrici (diagonale, Jordan 2/3, Jordan più rigido, rotazione complessa); eighe su 3 hermitiane, incluse diag(1,1e-40) e [[1,1e-20],[1e-20,2e-40]]. Test MatrixExp su float/double/Decimal64/128 con soglie per elemento |errore|/max(1,|riferimento|) rispettivamente 1e-4/1e-12/1e-12/1e-26. Non è accuratezza relativa garantita per elementi minuscoli. Autovalori controllati con errore relativo su double/Decimal128 a 1e-10/1e-28, oltre a residui e ortogonalità.

Gli oracoli hanno trovato cancellazione concreta: il precedente mean−radius restituiva zero per diag(1,1e-40). Ora le diagonali conservano direttamente gli elementi originali; nel caso accoppiato si sceglie l'autovalore non soggetto a cancellazione e si ricava l'altro da det/lambda, usando FMA per il determinante normalizzato. Restano underflow dei prodotti, determinanti mal condizionati e perturbazioni d'ingresso: nessuna garanzia universale.

Notebook Quantum/13-independent-oracles eseguito: errore MatrixExp scalato massimo circa 1.976e-14 double e 3.38e-32 Decimal128 sulle 5 fixture; lambda piccolo 1e-40 preservato su entrambi i tipi. Sei test numerici e un caso di evidenza notebook aggiunti. NDSharp non coinvolto.

## Diagnostica dei sistemi lineari

API separata opt-in `LinearSystemDiagnostics.Solve<T>(A,b)` e record `LinearSolveReport<T>`, constraint IEEE. Mantiene Solve LU esistente invariato. Restituisce soluzione, ||Ax−b||∞/||b||∞, backward error ||Ax−b||∞/(||A||∞||x||∞+||b||∞), condition infinity approssimata e reciproco. `NormwiseBackwardError` è alias di `NormwiseBackwardErrorInfinity`.

A e vettori sono scalati separatamente; residuo accumulato con FMA in T. Serve a evitare molti overflow intermedi, non a calcolare il residuo esatto: rounding e underflow possono restare e una soluzione esatta può avere un residuo misurato non nullo. Zero rhs e zero residuo hanno indicatori zero. Soluzioni/intermedi non rappresentabili sono segnalati.

ConditionInfinityEstimate usa ||Abar||∞ e n colonne dell'inversa normalizzata ottenute con la stessa LU; conserva solo le somme per riga, non una matrice inversa completa. Costo n solve aggiuntivi, memoria O(n²) per dati normalizzati. È una norma dell'inversa approssimata, non un estimator iterativo LAPACK, un bound rigoroso o una stima del condizionamento dei singoli componenti. Se la stima trabocca, condition=+infinito e rcond=0; se le colonne non sono rappresentabili, errore esplicito.

Flag IsIllConditioned: rcond ≤ sqrt(epsilon), policy di avviso di potenziale perdita di almeno metà delle cifre. False non certifica accuracy; true non implica che ogni termine noto darà una soluzione imprecisa. Test su sei formati, zero rhs, scale 1e±200, input errati, condition Hilbert contro mpmath e Hilbert12 con residuo piccolo ma avviso true. Decimal128 Hilbert8 non attiva il flag grazie alla diversa epsilon: non è un giudizio universale sul dataset.

Notebook LinearAlgebra/13-solve-diagnostics eseguito; quattro Reference rigenerate per includere i nuovi tipi. Non esegue refinement e non stima l'errore forward vero senza riferimento indipendente.

DiagonalizeHermitian2 — consolidamento distinto: verifica componenti finite e hermitianità, scala globale, raggio Hypot, nessuna soglia assoluta 1e-12 per il coupling. Il ramo autovettore sceglie radius+abs(halfDiff) e costruisce il secondo vettore come complemento ortonormale; zero e identità hanno basi indipendenti. Autovalori fuori range lanciano ArithmeticException. Otto test NativeHermitianTests coprono tutti i sei formati, componenti complesse, residui sulla matrice normalizzata, scale MaxValue/4 e T.Epsilon, coupling Decimal128 1e-25 e input errati. Fasi degli autovettori possono cambiare. Restano limiti: normalizzazione può annullare componenti estremamente piccole rispetto alla scala e mean±radius può cancellare l'autovalore piccolo in spettri molto sbilanciati; non è un solver generale.

