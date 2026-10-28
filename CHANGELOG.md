# MàthemaSharp — registro delle modifiche

Registro ricostruito del lavoro documentato in MathemaSharp e nei repository collegati sotto `D:\Dev\Science`.

## Come leggere questo documento

- Le sezioni seguono l’ordine ricostruibile delle tranche, dalla più recente alla più antica; non sono una cronologia Git certificata.
- Non sono state inventate date, commit o versioni per le attività prive di questi dati. I timestamp dei report sono identificatori delle esecuzioni, non date assegnate alle funzionalità.
- **Verificato** significa che è documentata una build, un test o un’esecuzione pertinente. Non significa supporto universale di ogni API o tipo numerico.
- Un test sugli output salvati di un notebook **non riesegue** il kernel. Le due verifiche sono riportate separatamente.
- I risultati storici non si sommano: le suite crescono e cambiano perimetro. Il conteggio corrente prevale sui conteggi precedenti.
- Questo registro descrive risultati e cambiamenti. Le attività future sono in [ROADMAP.md](ROADMAP.md); organizzazione e decisioni in [ARCHITECTURE.md](ARCHITECTURE.md); dettagli della migrazione in [NET11-MIGRATION.md](NET11-MIGRATION.md).

## Stato verificato più recente

| Perimetro | Ultima evidenza disponibile |
|---|---|
| Gate completo prima delle ultime estensioni | Tutti gli step verdi; 2624 test passati, zero falliti, 13 sospesi PySharp |
| MathemaSharp in quel gate | 311 test passati |
| NDSharp in quel gate | 564 test passati |
| PlotSharp in quel gate | 70 test passati |
| JupyterSharp in quel gate | 44 test passati |
| MatSharp dopo l’aggiunta del solver typed | Build riuscita, 133 test passati |
| PySharp dopo i tipi CLR generici | Build riuscita, 1507 test passati, zero falliti, 13 sospesi |
| Consumer NuGet dopo le ultime estensioni | Restore, build ed esecuzione riusciti: include solver MatSharp e contenitore CLR generico PySharp |
| Secondo notebook Ecosystem | Preparato, non ancora eseguito; gate non ancora rieseguito dopo questa aggiunta |

Il gate completo verde è documentato in `D:\Dev\BuildArtifacts\ecosystem-validation\20261027-223603`. Non va presentato come verifica della working tree successiva alla preparazione del secondo notebook.

## 1. MatSharp: solver di matrici tipizzate

**Aggiunto**
- `A \ b` per matrici reali typed tramite adattatore ai kernel MathemaSharp.
- LU per sistemi quadrati, con riuso della fattorizzazione per più colonne RHS.
- QR/minimi quadrati per sistemi sovradeterminati.
- Copie esplicite tra storage column-major MATLAB e matrici dei kernel, senza passaggio da `double`.
- Registrazione opt-in del solver nel runtime; la libreria del linguaggio resta separata dai kernel matematici.

**Verificato**
- Tre nuovi test: sei formati, RHS multiple, sistema tall, preservazione dell’input, singolarità e caso sottodeterminato.
- Esecuzione nel linguaggio con conservazione dell’incremento Decimal128 `1e-25`.
- Notebook `notebooks/MatSharp/01-decimal-scalars.ipynb` esteso ed eseguito; marker `typed_left_divide_precision: OK` controllato dal test sugli output salvati.
- Build e 133 test MatSharp passati; consumer NuGet separato eseguito.

**Distribuzione e limiti**
- Pacchetti MatSharp aggiornati a `0.1.1-net11-preview.3` nel feed comune.
- Sistemi sottodeterminati rifiutati: non è implementato un solver minimum-norm.
- Nessun nuovo supporto a complessi typed, N-D o divisione matriciale destra.

## 2. PySharp: costruzione esplicita dei tipi CLR generici

**Aggiunto**
- Selezione `ClrType[TypeArguments]` per chiudere una definizione CLR generica.
- Controlli di arità, natura degli argomenti e constraint CLR.
- Costruzione e uso di `List<Decimal128>` con un tipo aperto iniettato dall’host.

**Verificato**
- Due nuovi test su precisione e tipi/constraint invalidi.
- Incremento `1e-25` conservato attraverso inserimento e lettura del contenitore.
- Build, 1507 test passati e 13 sospesi; consumer NuGet eseguito sul nuovo binder.

**Distribuzione e limiti**
- `PySharp.Interpreter` `2.6.1-net11-preview.2`.
- Tool `PySharp` `2.5.1-net11-preview.4`.
- `JupyterNet.Kernels.PySharp` `0.1.1-net11-preview.3`.
- Non dimostra inferenza automatica, import di tutte le famiglie generiche, array NumPy typed o supporto `Span`/byref.

**Preparato, non verificato**
- `notebooks/Ecosystem/02-typed-language-interop.ipynb` e `ops/RunLanguageInteropPilot.cs`: notebook C# di embedding dei due linguaggi.
- Non ancora eseguito né incluso nel gate; non è prova di plugin nativi PySharp/MatSharp per JupyterSharp.

## 3. Chiusura del difetto Matplotlib e del gate completo

**Corretto**
- Tick Y del renderer NDSharp: allineamento `center_baseline` anziché centro geometrico del rettangolo del testo.
- Misura dei margini e disegno basati su ascent/descent del font.
- Doppio arrotondamento eliminato nello snippet `P5_layout.py`; golden CPython mantenuto invariato.
- Test aggiunto sulle coordinate non arrotondate, con tolleranza dichiarata `5e-4` in coordinate normalizzate per differenze subpixel dei renderer.
- Configurazione artifact PySharp mancante e percorsi dei sample nei test, ora indipendenti dalla posizione delle DLL.
- Rimossi gli output locali PySharp generati durante l’ispezione; build output conservati nella radice esterna.

**Verificato**
- Errore Y del caso esaminato ridotto da circa `0.00185` a circa `0.00029` normalizzati.
- Snippet rieseguito con CPython/Matplotlib sullo stesso host.
- Tutti gli step del gate completati con exit code 0: build/test, audit, baseline, kernel PySharp, notebook PySharp/MatSharp e pilota NuGet.
- 2624 test passati, zero falliti, 13 sospesi; nessuna deroga introdotta per il layout.

**Distribuzione**
- `NDSharp.Plot` `2.6.3-net11-preview.2`.
- `PySharp.Matplotlib` `2.5.1-net11-preview.2`.
- Nuove preview dei tool/consumatori interessati; le versioni precedenti non sono state sovrascritte.

Il confronto certifica il caso di regressione, non l’equivalenza completa tra Skia e Matplotlib.

## 4. Integrazione reale JupyterSharp–MathemaSharp–PlotSharp

**Aggiunto**
- `notebooks/Ecosystem/01-signal-spectrum.ipynb`.
- Esecuzione tramite CLI JupyterSharp installata localmente dai pacchetti, senza installazione globale.
- Segnale a cinque cicli, FFT MathemaSharp, grafico del segnale e dello spettro PlotSharp.
- Due SVG salvati tramite output HTML del kernel.
- Test degli output salvati; test di struttura esteso ai notebook JupyterSharp.
- `ops/RunEcosystemPilot.cs` e `ops/ValidateEcosystem.cs`.

**Verificato**
- Picco FFT al bin 5 e ampiezza normalizzata 0.5.
- Due output HTML contenenti SVG e nessun errore di cella.
- Pilota eseguito con pacchetti ripristinati in cache isolata, senza `ProjectReference` o esecuzione JupyterNet.

**Limiti**
- Il pilota usa dati e coordinate `double` intenzionalmente.
- Non certifica plotting Decimal128, tutti i renderer o plugin linguistici nativi.
- Nessuna verifica visuale nel browser documentata.

## 5. Audit e correzioni delle dipendenze vulnerabili

**JupyterSharp**
- Individuata la catena Microsoft.PowerShell.SDK 7.6.6 → Pkcs/Xml 10.0.10.
- Riferimenti espliciti `System.Security.Cryptography.Pkcs` e `System.Security.Cryptography.Xml` a 10.0.13 nel kernel PowerShell, verificati contro gli advisory Microsoft/GitHub.
- Build e 44 test passati; nove pacchetti JupyterSharp `0.3.1-net11-preview.2` generati.
- Consumer isolato ripristinato, compilato ed eseguito senza warning di audit.

**PySharp/Pyodbc**
- Audit esteso: PySharp → Pyodbc → Microsoft.Data.SqlClient 7.0.2 → Pkcs 9.0.13.
- Riferimento esplicito Pkcs 10.0.13 in Pyodbc.
- Nuovi pacchetti PySharp/Pyodbc `2.5.1-net11-preview.2` generati in quella tranche; il tool PySharp è stato successivamente versionato ancora.

**Automazione**
- `ops/AuditSciencePackages.cs` controlla i report JSON delle sei soluzioni.
- Exit code non zero su vulnerabilità o errori: il solo exit code di `dotnet list package` non costituisce un gate sufficiente.
- Nessun advisory soppresso. Audit verde con le fonti correnti nell’ultima baseline completa.

L’audit non certifica sicurezza applicativa, runtime installati, vecchi pacchetti nel feed o progetti fuori soluzione.

## 6. Migrazione e distribuzione .NET 11

**Modificato**
- Target dei progetti attivi nei sei repository: MathemaSharp, NDSharp, PlotSharp, JupyterSharp, PySharp e MatSharp → `net11.0`.
- SDK MathemaSharp fissato a `11.0.100-rc.3.26522.148`.
- Aggiornati identificatori dei framework negli script operativi pertinenti; snapshot importati esclusi.
- Portale Blazor: gestione della disconnessione del circuito nelle chiamate JS, richiesta dall’analizzatore BL0016.

**Verificato e distribuito**
- Build delle sei soluzioni e test, con correzioni successive dei gap PySharp descritte sotto.
- Baseline di 24 notebook, nove esecuzioni C#/PySharp aggiuntive e notebook MatSharp di embedding rieseguiti; alcune esecuzioni si sovrappongono, non sono conteggi di notebook tutti distinti.
- Prima raccolta di 37 pacchetti preview generata in `D:\Dev\NuGetLocalFeed`, con incrementi di versione e suffisso `net11-preview`.
- Consumer NuGet senza riferimenti sorgente: Decimal128, NDArray, FFT Bluestein, solver del ponte e PySharp verificati; poi esteso alle nuove funzionalità.
- Log di pack e consumer in `D:\Dev\BuildArtifacts\net11-pack`.

Le preview non sono un rilascio GA o una certificazione di produzione. Alcuni sample/tool fuori soluzione hanno il target aggiornato ma richiedono verifica dedicata.

## 7. PySharp: recupero della compatibilità Python richiesta da FastAPI/AnyIO

**Aggiunto progressivamente**
- `sys._getframe` sullo stack reale thread-local: globals, snapshot locals, nome/file/linea e catena `f_back`.
- `inspect.getsource` per moduli, funzioni e metodi con file reale; errori espliciti quando il sorgente manca.
- `compile` di stringhe exec/eval in oggetti con AST riutilizzabile; integrazione con exec/eval e inspect.iscode.
- AST pubblico mutabile e flag `PyCF_ONLY_AST`: inizialmente assegnazioni/espressioni, poi import/alias, controllo di flusso, chiamate, collezioni, funzioni e classi nei casi supportati.
- Ricompilazione dai campi modificati dei nodi, non semplice riesecuzione del sorgente originale.
- PEP 562: lookup lazy tramite `__getattr__` dei moduli anche per `from ... import ...`.
- Nomi relativi in `importlib.import_module`, con package esplicito e identità dei moduli conservata nella cache.

**Corretto nei test**
- Aspettativa KeyError: CPython conferma gli apici; modificata l’aspettativa, non il runtime.
- Collisione del CSV temporaneo Pandas: nome univoco per esecuzione.
- Collezione oracle isolata dal parallelismo, perché il runner modifica la working directory di processo.

**Evidenze e limiti**
- Regressioni per frame, sorgente, codice compilato, mutazione AST, export lazy e import relativi.
- I nove fallimenti FastAPI/AnyIO iniziali sono stati recuperati.
- Evidenza della fixture AnyIO 4.15.1 conservata fuori dal repository.
- AST, introspezione e compile restano parziali: non equivalgono a una completa implementazione CPython; visitor, generatori/comprensioni, varianti sintattiche e semantiche non coperte restano dichiarati.

## 8. MatSharp: primo percorso Generic Math nel linguaggio MATLAB

**Aggiunto**
- Collegamento iniziale `mathema_solve`/`mathema_rcond` in double ai kernel condivisi, senza modificare il precedente operatore backslash.
- `GenericScalar<T>`: storage reale typed, inizialmente scalare e poi matriciale 2D column-major.
- Costruttori decimal32/64/128; operatori e confronti senza conversioni intermedie a double.
- Concatenazione, indicizzazione e assegnazione, copie, trasposta, broadcasting e prodotti.
- Rifiuto delle conversioni implicite fra formati Decimal o da double frazionari nei percorsi verificati.
- Notebook C# che esegue MATLAB attraverso MatEngine, non un kernel MATLAB dedicato.

Questa base è stata estesa dal solver typed descritto nella sezione 1. Il nome GenericScalar<T> resta da normalizzare perché il contenitore ospita anche matrici.

## 9. PySharp: interoperabilità numerica con MathemaSharp

**Recuperato**
- Kernel e loader dopo rinomina/spostamento.
- Tour TimeSeries e scenari frigorifero/fotovoltaico eseguiti nelle coppie C#/PySharp.
- Percorsi CSV corretti; tre test di concordanza riattivati.

**Esteso**
- Prima facciata Decimal128Interop: roundtrip scalar/array e Complex<Decimal128> hermitiano con differenze `1e-25`.
- Dispatch degli operatori CLR: aritmetica, confronti e operatori unari nei casi verificati.
- Modulo builtin `mathema`, con costruttori decimal32/64/128 da stringhe, interi e valori CLR dello stesso tipo.
- Binding esplicito dei metodi CLR generici: `method[ClrType](args)` e applicazione dei constraint.
- Notebook precisione, operatori e metodi generici realmente eseguiti.

**Limiti**
- Facciata TimeSeries legacy intenzionalmente double.
- I literal float già arrotondati non recuperano cifre decimali perse.
- Nessuna garanzia generale su marshalling di ogni tipo CLR, NumPy typed, inferenza e Span/byref.

## 10. FFT/IFFT Generic Math e prime misure

**Aggiunto in MathemaSharp.Core**
- FFT radix-2 per potenze di due.
- Bluestein per altre lunghezze, con convoluzione radix-2 e fasi ridotte mediante aritmetica intera.
- Forward non normalizzata, inversa divisa per N; input preservato e controlli di finitezza/rappresentabilità.

**Verificato**
- Sei formati: confronto DFT, roundtrip, impulso/costante e casi Decimal128.
- Riferimenti analitici indipendenti: trasformata di [1,2,3,4], tono complesso e Parseval.
- Fixture mpmath a 200 cifre per segnali complessi di lunghezza 7, 16, 31 e 64 su float, double, Decimal64 e Decimal128.
- Roundtrip double su 1024 punti radix-2 e 1009 punti Bluestein.
- `notebooks/Core/06-fft.ipynb` eseguito e incluso nella baseline.

**Misurato, non ottimizzato**
- Sei benchmark forward/inverse double su 64, 1009 e 1024 punti.
- BenchmarkDotNet ShortRun in-process per il limite della toolchain .NET 11 RC: misure preliminari, non benchmark definitivi.
- Costo elevato dei buffer Bluestein osservato; ottimizzazioni rimandate su richiesta.

Fixture/generatore: `ops/oracles/fft-mpmath.json`, `ops/generate-fft-oracles.py`. Non certificano ogni lunghezza o Half/Decimal32 sugli oracoli generali.

## 11. NDSharp: storage generico e ponte

**Aggiunto/corretto**
- Lettura dei dtype runtime nativi verso T senza intermedi double.
- Eliminato Convert.ToDouble dalle identità delle riduzioni, con regressioni su interi oltre 2^53 e ulong.MaxValue.
- NDArray<T> affiancato al contenitore runtime: shape, stride, indicizzazione e copie in ordine logico.
- Trasposizione e slicing come viste, inclusi passi negativi.
- Broadcasting Add sugli assi finali, scalari e dimensioni vuote.
- Sum, Dot e MatMul 2D, con rispetto degli stride e risultati indipendenti.
- Conversioni typed verso il ponte LU, QR e diagnostica MathemaSharp, senza duplicare decomposizioni.
- Notebook `notebooks/NDSharp/01-generic-math.ipynb` eseguito e incluso nella baseline.

**Verificato**
- Test di precisione Decimal128 `1e-25`, viste/copie, input invalidi e sei formati nei percorsi coperti.
- Ponte verificato anche tramite pacchetti; mapping del feed MathemaSharp.* corretto.

NDSharp non è completamente convertita. Storage runtime, algoritmi importati, complessi e prodotti batched restano distinti dal percorso NDArray<T> implementato.

## 12. Notebook e baseline riproducibile

**Creato ed eseguito**
- Quattro notebook di consolidamento Usage/Reference, uno per Core, LinearAlgebra, TimeSeries e Quantum.
- Quattro cataloghi API da reflection con firme e constraint.
- Riesecuzione dei notebook Core precedenti, LU, Quantum e TimeSeries C# selezionati.
- Notebook oracoli numerici, diagnostica, NDSharp, FFT e integrazioni linguistiche.

**Automazione**
- Runner selettivi e manifest esplicito per evitare esecuzioni involontarie di notebook fuori perimetro.
- Baseline inizialmente di 22, poi 23 e 24 notebook; log/report esterni, SDK/runtime ed esclusioni registrati.
- Modalità rapida build/test separata dall’esecuzione del kernel.
- Test degli output salvati, distinti dalla reale riesecuzione dei notebook.
- Gate ecosistema aggiunto successivamente, con controlli per tutti i repository e pilota NuGet.

I cataloghi reflection non sostituiscono contratti ed esempi API-per-API. I notebook non selezionati non sono automaticamente certificati.

## 13. Oracoli indipendenti e diagnostica dei sistemi lineari

**Validazione**
- Fixture mpmath a 200 cifre per gamma, log-gamma e code equivalenti del chi-quadro, fino all’ordine di 1e-306 sui tipi appropriati.
- Sistemi di Hilbert 3, 5 e 8 per LU/QR: soluzione, residuo e determinante.
- Controesempio Hilbert 12 double: residuo circa 1.31e-16 ma errore della soluzione circa 0.00595; il controllo dei pivot non certifica il condizionamento.
- MatrixExp su cinque matrici, incluse non normali e complesse; tre spettri hermitiani sbilanciati.

**Correzione**
- Autovalore 1e-40 perso su diag(1,1e-40): diagonali trattate direttamente; recupero del piccolo autovalore dal determinante e dall’altro autovalore, con FMA per ridurre cancellazione.

**Nuova API**
- LinearSystemDiagnostics.Solve<T>: soluzione, residuo relativo, errore backward normwise, stima del condizionamento infinito e reciproco, avviso IsIllConditioned.
- Diagnostica opt-in, più costosa del semplice solve; non è certificazione dell’errore forward e non include refinement.
- Notebook oracoli e diagnostica eseguiti, reference aggiornata.

## 14. Consolidamento numerico dei kernel attivi

**Core**
- NearlyEqual: default limitato a min(4096 × epsilon, sqrt(epsilon)), correggendo la tolleranza eccessiva su Half.
- Test nativi in T per confronti, valori speciali, somme/Kahan, Horner, prodotti e DFT.

**TimeSeries**
- LOESS: ascisse centrate, soglie legate alla precisione di T, costanti in T e validazione dati/pesi.
- GARCH: griglia con indici interi per evitare mancato avanzamento e accumulo floating-point; validazione e likelihood finita.
- Simulation: overload con Func<Random,T> per innovazioni native; default Random/Box–Muller precedente conservato.
- Funzioni speciali: coefficienti Lanczos letti direttamente in T, dominio/valori speciali e mancata convergenza espliciti. Non è un algoritmo accurato a 34 cifre per ogni input.

**LinearAlgebra**
- QR/minimi quadrati: normalizzazione, norme Hypot, riflettori unitari e soglie per tipo.

**Quantum**
- Norm e Normalize robuste tramite Hypot/scaling.
- Diagonalizzazione hermitiana 2×2 normalizzata e autovettori stabili.
- CheckEigenstate tramite residuo relativo e scaling separato.
- MatrixExp con Hypot, copia e dimezzamenti nello scaling, validazione termini/intermedi; controlli TimeEvolution.

Ogni intervento è accompagnato da test multiformato/casi limite documentati. Una prima passata di consolidamento non certifica matrici arbitrarie, tutte le code o ogni algoritmo.

## 15. Identità, struttura e confini delle librerie

**Riorganizzato**
- Numbers → MathemaNet → MathemaSharp per progetti, namespace, riferimenti e notebook.
- Marchio MàthemaSharp con accento sulla prima a; nomi tecnici senza accento.
- Notebook spostati nella root, suddivisi per libreria; output inizialmente azzerati e poi rieseguiti nelle tranche documentate.
- Algebra lineare estratta da TimeSeries in libreria indipendente.
- LU generica con pivoting, solver, determinante e inversa; operazioni di base ampliate ai constraint numerici appropriati.

**Confini**
- MathemaSharp: kernel matematici.
- NDSharp: array/storage/viste.
- PySharp e MatSharp: semantiche dei linguaggi.
- PlotSharp: visualizzazione.
- JupyterSharp: notebook/kernel/output.

**Import e preparazione**
- Copia selettiva iniziale di 68 file NDSharp, con licenza e materiali numerici, fuori soluzione.
- Raccolta iniziale PlotSharp di 148 file dai quattro contesti, con provenienza e hash; non equivalente a migrazione attiva dei consumer.
- PlotSharp successivamente verificata come libreria compilabile con test, Gallery e Inventory.
- Repository spostati nella radice comune D:\Dev\Science.

**Operatività**
- Feed pacchetti D:\Dev\NuGetLocalFeed.
- Build, test, benchmark e report personalizzati portati progressivamente sotto D:\Dev\BuildArtifacts.
- Correzioni dei loader e dei test che dipendevano dalla vecchia posizione delle DLL.
- Script temporanei di importazione rimossi nella tranche di pulizia documentata; sorgenti, fixture, notebook e snapshot conservati.

La rinomina Numbers → MathemaSharp è una breaking change. Il logo grafico rinominato conservava il marchio precedente: non risulta documentata qui una sua rigenerazione definitiva.

## 16. Documentazione matematica, infografiche e portale

**Documentato/implementato**
- Analisi delle infografiche TimeSeries: patch, decomposizioni, attenzione temporale/fra variabili e foundation model, distinguendo concetti da API implementate.
- HistoryPatches.Extract<T> e backtest RollingOrigin.Evaluate<T>, con isolamento del passato e gap opzionale.
- Infografica di algebra lineare per AI integrata in LINEAR-ALGEBRA.md; norme, determinante e inversa 2×2 aggiunti prima dell’estrazione della libreria.
- Portale Docs Blazor con interattività Server, Markdown, ricerca, immagini/link interni e protezione dei percorsi.
- CSS per tipografia, codice, immagini, stampa e tabelle con righe alternate, header in grassetto e padding.
- Indice dei titoli e funzioni JavaScript del browser.

**Non dichiarato come completato**
- Transformer/foundation model completi, autovalori/SVD generali o accuratezza dei modelli dedotta dalle infografiche.
- Fonti cronologiche delle immagini non tutte verificate su fonti primarie.
- Tailwind compilato per produzione, rendering formule, audit accessibilità e verifica visuale completa del sito.

## Dove trovare le prove e come mantenere il registro

| Prova | Posizione |
|---|---|
| Obiettivi ricevuti | PROMPTS.md, gestito automaticamente; non è prova di completamento |
| Stato e priorità | ROADMAP.md, prima sezione corrente |
| Decisioni/API/constraint | ARCHITECTURE.md e ops/api-declarations.csv |
| Migrazione .NET 11 e correzioni | NET11-MIGRATION.md |
| Notebook eseguiti | Output salvati in notebooks/*, con i limiti indicati sopra |
| Baseline MathemaSharp | D:\Dev\BuildArtifacts\MathemaSharp\baseline |
| Gate ecosistema | D:\Dev\BuildArtifacts\ecosystem-validation |
| Audit | D:\Dev\BuildArtifacts\security-audit |
| Pack e consumer isolato | D:\Dev\BuildArtifacts\net11-pack |
| Pacchetti versionati | D:\Dev\NuGetLocalFeed |

Per ogni nuova tranche aggiungere una sezione in alto con: progetto/API, cambiamento osservabile, test o notebook realmente eseguiti, versione dei pacchetti, report disponibile e limiti. Non trasformare attività preparate in attività completate; non sovrascrivere risultati storici con quelli nuovi.
