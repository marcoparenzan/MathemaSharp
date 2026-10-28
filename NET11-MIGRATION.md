# Migrazione dell’ecosistema a .NET 11

I progetti attivi di MathemaSharp, NDSharp, PlotSharp, JupyterSharp, PySharp e MatSharp sono stati aggiornati a `net11.0`, compresi i progetti di test e gli script operativi contenenti target precedenti. Gli snapshot `imports` sono esclusi. SDK disponibile e selezionato in MathemaSharp: `11.0.100-rc.3.26522.148` (prerelease, non GA).

## Verifica Release

| Repository | Build soluzione | Test |
|---|---|---|
| MathemaSharp | OK | 310 passati |
| NDSharp | OK | 563 passati |
| PlotSharp | OK | 70 passati |
| JupyterSharp | OK | 44 passati |
| PySharp | OK | 1503 passati, 1 fallito, 13 sospesi |
| MatSharp | OK | 130 passati |

La verifica riguarda la soluzione principale di ciascun repository; i sample o tool non inclusi nelle soluzioni hanno il target aggiornato ma non sono certificati da questa esecuzione.

Corretto in Blazor il requisito BL0016: le chiamate JS gestiscono ora la disconnessione del circuito.

## Punti aperti

- PySharp: implementato `sys._getframe` usando lo stack thread-local reale già mantenuto dall’interprete. Esposti globals del modulo, snapshot dei locals delle funzioni, nome/file/linea e catena f_back. Due nuovi test verificano chiamanti annidati, locals/globals reali e profondità/argomenti invalidi. La rappresentazione è uno snapshot al momento della richiesta (non una certificazione della completa semantica CPython dei frame persistenti, lambda, class body o async).
- Implementato `inspect.getsource` per moduli, funzioni e metodi Python con file reale. I builtin producono TypeError, il sorgente indisponibile OSError. Due nuovi test verificano contenuto effettivo e errori. L’estrazione di funzioni è per blocchi di indentazione: lambda, classi come oggetto diretto, decorator multilinea, stringhe multilinea e continuazioni complesse restano non certificati; non è una completa implementazione CPython.
- Implementato `compile` di stringhe per modalità exec/eval: oggetto PyCompiledCode con AST reale riutilizzabile, accettato dai rispettivi exec/eval e riconosciuto da inspect.iscode. Due test verificano namespace distinti, locals espliciti, riuso ed errori di sintassi/flag. Modalità single, input AST/bytes, flag, optimize e semantica completa dei code object restano esclusi; le forme non supportate sono rifiutate. Non equivale alla compilazione in bytecode CPython.
- Aggiunto un primo AST pubblico mutabile: AST, Module, Expression, Assign, Expr, Name, Constant, BinOp, Load/Store e operatori Add/Sub/Mult/Div. `ast.parse` e `compile(..., ast.PyCF_ONLY_AST)` producono nodi reali; `compile` riconverte i campi attuali dei nodi in AST eseguibile. Test di modifica di costanti dimostra che exec/eval usano le modifiche e non il sorgente originale. Non è un modello completo: mancano visite, validazione completa di contesti/posizioni e gran parte dei nodi.
- Aggiunti Import, ImportFrom e alias, con esportazione e ricompilazione di nomi, alias e livello relativo. Il nuovo test modifica gli alias nel tree, esegue il codice compilato e verifica le nuove chiavi nel namespace. La semantica dei livelli relativi e star import non è ancora coperta da test dedicati.
- Inventario preliminare eseguito sui file disponibili sotto site-packages/fastapi, starlette, anyio e pydantic: 26 file parsati, nessun errore di parsing. Il report è in D:\Dev\BuildArtifacts\net11-migration\ast-inventory.txt. Sono conteggi dei nodi del parser, non una verifica della compatibilità con l’AST CPython né una scansione di tutte le installazioni delle fixture.
- Aggiunto gruppo controllo di flusso/espressioni: If, While, For sincrono, Return, Pass, Break, Continue, Attribute, Call senza unpacking, Compare, BoolOp, UnaryOp, List, Tuple e Subscript. Il test verifica roundtrip di cicli e confronti, modifica della condizione e chiamata in un’espressione booleana; non ogni variante è certificata.
- Aggiunti Dict, Set, Slice, IfExp, FunctionDef/AsyncFunctionDef non generatori, ClassDef e arguments/arg. Il roundtrip preserva i parametri/default/annotation supportati, corpi e decorator; generatori e parametri positional-only restano esclusi. Test reale modifica un default e un valore di dizionario, ricompila e invoca funzione/metodo, verificando anche slice e condizionale. Non è una certificazione delle varianti async/decorator/kwargs.
- Verificata la versione realmente installata dalla fixture non pinned: AnyIO 4.15.1, con lazy exports. Sorgenti e metadati di evidenza vengono conservati fuori dal repository in D:\Dev\BuildArtifacts\PySharp\dependency-evidence\anyio.
- Implementato PEP 562 per il lookup dei moduli e `from ... import ...`: viene chiamato il vero __getattr__ del modulo dopo il lookup normale; AttributeError indica assenza, altre eccezioni propagano. Test indipendente verifica export lazy, precedenza di attributi esistenti e propagazione degli errori.
- Implementati nomi relativi in importlib.import_module, con package esplicito, risoluzione dei livelli e riuso della cache dell’Importer. Test verifica identità dei moduli, livelli relativi ed errori oltre il package.
- Verificato l’intero snippet MutableMapping con CPython: KeyError stampa correttamente gli apici. Corretta soltanto l’aspettativa del test. Evidenza in D:\Dev\BuildArtifacts\net11-migration\keyerror-cpython.txt.
- Eseguito anche P5_layout con CPython/Matplotlib sullo stesso host: l’oracolo salvato è confermato. Probe non arrotondato: PySharp Y=0.1531963984, Matplotlib Y=0.1550479917. Il doppio arrotondamento della fixture amplifica la differenza a 0.1/0.2; non è stata applicata una correzione numerica arbitraria né alterato il golden. Il renderer di PySharp usa ancora NDSharp.Plot 2.6.2 via NuGet.
- Risolta una collisione intermittente nel CSV temporaneo Pandas: nome univoco per l’esecuzione oracle. La collezione oracle è ora non parallela, perché il runner modifica la working directory di processo.
- Ultima build Release riuscita; suite completa 1503 passati, 1 fallito (Matplotlib P5_layout), 13 sospesi. FastAPI/AnyIO e MutableMapping passano. Il totale dei test è aumentato anche rispetto alle esecuzioni precedenti; non viene attribuito tutto ai test aggiunti in queste tranche.
- Restano AST per eccezioni/comprensioni/generatori, visitor e validazione completa. Il rapporto dei gap precedenti con la migrazione .NET non è stato dimostrato.
- Il confronto Matplotlib `P5_layout` fallisce nella posizione verticale dell’asse immagine dopo colorbar e tight_layout (atteso 0.2, ottenuto 0.1, con arrotondamento della fixture). Una prova su `get_position` non ha risolto il difetto ed è stata annullata; l’oracolo non è stato modificato.
- Dopo l’annullamento è stata rieseguita la verifica completa: tutte le build passano, PySharp mantiene 1416 passati, 10 falliti e 13 sospesi. Nessun test è stato disabilitato.
- Restano warning nei repository JupyterSharp e PySharp; la prima compilazione NDSharp ha riportato 15 warning.
- Rieseguiti 24 notebook della baseline principale, 9 esecuzioni aggiuntive C#/PySharp (tour/scenari/precisione/operatori/generics) e il notebook MatSharp di embedding. Il tour C# è presente anche nella baseline. Kernel PySharp ricompilato su .NET 11. Queste esecuzioni precedono il cambio delle versioni dei pacchetti; build e test sono stati riconfermati dopo il cambio.
- Generati 37 pacchetti nei sei repository con nuove versioni `-net11-preview.1`, senza sovrascrivere le versioni precedenti. MathemaSharp 0.2.1-net11-preview.1, NDSharp 2.8.1-net11-preview.1; gli altri incrementano il patch della versione precedente. Aggiornati riferimenti NuGet interni alle versioni generate.
- Consumatorе separato senza ProjectReference e con cache isolata: restore, build ed esecuzione riusciti. Verificati Decimal128, NDArray typed, FFT Bluestein, solver del ponte e PySharp operatori/compile. MatSharp, PlotSharp e JupyterSharp sono ripristinati nello stesso consumer ma il loro comportamento non è esercitato da questo smoke.
- Risolti gli advisory NuGet del consumer: Microsoft.PowerShell.SDK 7.6.6 introduceva Pkcs/Xml 10.0.10. Il progetto JupyterSharp.Kernels.PowerShell ora richiede esplicitamente entrambi a 10.0.13, versione corretta secondo gli advisory GitHub/Microsoft GHSA-846f-f764-76mx, GHSA-7hqp-7c5m-4vq6, GHSA-c399-9853-6jmj e GHSA-r955-8q5j-3h83 (controllato anche GHSA-2fmv-rv2c-59x7).
- JupyterSharp rigenerato in nove pacchetti 0.3.1-net11-preview.2, senza sovrascrivere preview.1. Build Release e 44 test passati. Consumer aggiornato: restore/build/run riusciti, zero warning; `dotnet list ... package --vulnerable --include-transitive` non trova vulnerabilità con le fonti correnti. Nessuna soppressione audit.
- La verifica riguarda il grafo dei pacchetti del consumer: non certifica sicurezza completa dell’applicazione, SDK/runtime o altri consumatori. I pacchetti restano preview, non un rilascio produzione certificato. Le preview.1 precedenti rimangono nel feed per riproducibilità e non vanno preferite ai nuovi pacchetti JupyterSharp.
- Verificare separatamente sample/tool esterni alle soluzioni e supporto del runtime nei kernel dinamici.

## Audit esteso alle sei soluzioni

L’audit globale ha individuato anche PySharp/Pyodbc → Microsoft.Data.SqlClient 7.0.2 → Pkcs 9.0.13, non presente nel precedente consumer. Aggiunto riferimento esplicito Pkcs 10.0.13 in PySharp.Pyodbc. Nuovi pacchetti PySharp e PySharp.Pyodbc 2.5.1-net11-preview.2 generati nel feed; versioni precedenti non sovrascritte.

Build Release PySharp riuscita; suite 1503 passati, 1 fallito (layout), 13 sospesi. Audit finale delle sei soluzioni: zero pacchetti vulnerabili rilevati dalle fonti correnti e nessun errore di report. Report in D:\Dev\BuildArtifacts\security-audit\20261027-210253. I progetti fuori soluzione e i vecchi pacchetti conservati nel feed non sono inclusi in questa verifica.

```sh
dotnet run --file ops/AuditSciencePackages.cs
```

Il gate controlla il JSON NuGet e restituisce exit code 1 su vulnerabilità o errori, perché l’exit code del solo comando `dotnet list package` non basta. Non disabilita né sopprime l’audit.

## Pilota e gate end-to-end

Eseguito `notebooks/Ecosystem/01-signal-spectrum.ipynb` con la CLI JupyterSharp 0.3.1-net11-preview.2 installata localmente sotto BuildArtifacts, non come tool globale. Carica MathemaSharp.Core e PlotSharp dai pacchetti ripristinati in cache isolata, calcola un segnale a 5 cicli e la FFT e salva due SVG tramite Display.Html. Verificati picco FFT, ampiezza e due output HTML/SVG. Nessun ProjectReference né esecuzione JupyterNet in questo pilota; nessuna verifica visuale nel browser.

Aggiunto test degli output salvati; non esegue il kernel. MathemaSharp ora ha 311 test passati. La baseline principale conserva i 24 notebook precedenti; il pilota viene eseguito separatamente dal gate.

```sh
dotnet run --file ops/RunEcosystemPilot.cs
dotnet run --file ops/ValidateEcosystem.cs
```

Gate completo rieseguito dopo le correzioni ai test di struttura/output: sicurezza, baseline, kernel PySharp, notebook PySharp/MatSharp e pilota NuGet passano. Lo step build/test resta non zero a causa del test Matplotlib legacy; non viene ignorato né classificato come successo. Report sotto D:\Dev\BuildArtifacts\ecosystem-validation. Il gate non esegue pack automatico né modifica versioni. I progetti/package PlotSharp/JupyterSharp sono integrati sul percorso C#; il plugin PySharp per JupyterSharp non è certificato dal pilota.

## Ripetibilità

```sh
dotnet run --file ops/UpgradeNet11.cs -- --verify
```

Lo script restituisce un codice non zero se una build o suite di test fallisce. `--apply` applica l’aggiornamento dei target; non va usato per correggere automaticamente incompatibilità del codice.

Log e backup esterni: `D:\Dev\BuildArtifacts\net11-migration`. Output di compilazione e destinazione dei pacchetti rimangono centralizzati come già configurato.
