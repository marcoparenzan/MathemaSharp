# Uso quotidiano

## Baseline completa dopo sampler e bande NDSharp

`dotnet run --file ops/ValidateEcosystem.cs -- --no-images` rieseguito dopo RectilinearFieldSampler e ContourBands: D:\Dev\BuildArtifacts\ecosystem-validation\20261028-215324, 31/31 passi exit0. Build/test/audit, percorsi package/source kernel, notebook reale, UI/browser e selftest numerico completi, nessun controllo saltato.
Nessun nuovo screenshot/file immagine di revisione; output funzionali PNG restano incorporati nel notebook, raster QA eseguito in memoria. Solo JSON nelle cartelle *no-images, evidenze recenti archiviate. Warning build noti e test sospesi restano dichiarati. Prevale sulle note mirate precedenti; non migrazione completa di NDSharp.Plot o release.

## Bande contour riempiti — selezione condivisa

ContourBands conserva lookup inclusivo NDSharp: un livello interno esatto va all'ultima banda corrispondente, limite finale incluso, valori esterni/NaN/infinito senza banda. Livelli finiti non decrescenti (duplicati ammessi), copia snapshot; un solo livello non contiene bande. Punto medio/full-range normalizzato con aritmetica originale, non riassociata.
Palette/alpha/bitmap/Skia restano NDSharp. Build e test mirati PlotSharp227/227, ND19/19 passati; kernel source ricostruito e notebook vero linee/bande320×240 ricontrollato, nessun file immagine di revisione. Metadata DLL richiede ContourBands. Warning kernel preesistenti visibili; non nuovo full gate/release.

## Sampler fisico dei contour riempiti — verifica mirata

RectilinearFieldSampler PlotSharp condivide la vecchia Sample NDSharp, con binary search e aritmetica a quattro termini. Le 81 interrogazioni congelate prima della modifica coincidono esattamente, inclusi stop/ULP ed extrapolazione. NDSharp DrawFilled mantiene bande, palette/alpha e Skia; nessun cambiamento al sampler uniforme RGB Navier.
Assi finiti strettamente crescenti min2×2, dati row-major coerenti e snapshot copiato; NaN/Infinity nei dati propagano al chiamante. Coordinate di query finite, senza nuovo clamp. Input/mutazioni prima non validati non garantiti compatibili; contour lineari conservano separatamente il supporto agli assi discendenti.
Build/test: PlotSharp211/211, NDSharp.Plot19/19. Kernel source-plots ricostruito e notebook contour reale verificato con due PNG incorporati320×240, nessun file immagine di revisione. Metadata DLL include sampler nuovo. Warning kernel preesistenti non soppressi; non nuovo full gate/release.

## Contour NDSharp nel kernel reale e baseline completa corrente

```sh
dotnet run --file ops/BuildPySharpKernel.cs -- --source-plots
dotnet run --file ops/RunNDSharpContourNotebook.cs
dotnet run --file ops/ValidateEcosystem.cs -- --no-images
```

Il binding Matplotlib mantiene il pacchetto preview predefinito; ScienceUseSourcePlots=true abilita solo il riferimento source NDSharp.Plot. Kernel separato in D:/Dev/BuildArtifacts/PySharp/kernel-source-plots; default in kernel invariato. Il runner verifica metadata DLL (riferimento PlotSharp e tipo RectilinearContours), pulisce gli output e avvia il vero plugin JupyterNet/PySharp su .NET11.
Notebook: notebooks/NDSharp/pysharp/02-contours-kernel.ipynb, non un pilota JupyterSharp. Linee+bande producono due PNG320×240 incorporati, verificati senza salvarli come file immagine di revisione. Report: ops/validation/ndsharp-contour-notebook/report.json; test saved-output e policy source opt-in distinti dall'esecuzione reale.
Ultimo gate dopo queste modifiche: D:\Dev\BuildArtifacts\ecosystem-validation\20261028-203820, 31/31 exit0. Warning preesistenti dei progetti PySharp restano dichiarati; nessuna soppressione/release. Non prova pixel CPython, native JupyterSharp o migrazione completa della famiglia.

## NDSharp.Plot: migrazione effettiva contour lineari

La geometria DrawLines delega ora a PlotSharp.RectilinearContours/ContourKernel. Coordinate X/Y fisiche/nonuniformi, direzione degli edge, pairing e skip della somma dei corner non finita preservati. Livelli estremi/AutoLevels, paint/stili/Skia, Axes e contourf restano ND. Il kernel precedente a indici usa lo stesso motore con la propria convenzione, senza cambiare i default.
Build e test mirati: NDSharp.Plot18/18, PlotSharp204/204, niente immagini di revisione. Report aggiornato: ops/inventory/ndsharp-plot-contract.json, LineContourGeometryDelegated=true e FamilyMigrationComplete=false. Nuovo ProjectReference, nessuna migrazione completa o preview pubblicata; manca ancora riconferma full gate/notebook e distribuzione delle DLL.

## NDSharp.Plot: inventario e baseline, non migrazione

`dotnet run --file ops/InventoryNDSharpPlot.cs` (dopo build dei test NDSharp.Plot) registra dipendenze/hashes e riesegue i test mirati senza immagini. Report/log: ops/inventory/ndsharp-plot-contract.json e ndsharp-plot-tests.log.
Sorgente attiva NDSharp/src/NDSharp.Plot, net11.0, NDSharp + SkiaSharp/native assets 3.119.0 e font DejaVu. Candidata la sola geometria contour lineare; Xs/Ys sono coordinate fisiche nonuniformi, livelli estremi non disegnati, celle non finite saltate. AutoLevels/Ticks, contourf a bande e Figure/Axes/PNG/stili restano separati.
Quattro nuovi baseline test in-memory: coordinate fisiche, NaN-cell skip, endpoint suppression, filled bands. Build e suite18/18 passate. Renderer e dipendenze di produzione non modificati, nessuna adozione dichiarata, nessun full gate o pubblicazione.

## Baseline completa corrente, senza immagini di revisione

`dotnet run --file ops/ValidateEcosystem.cs -- --no-images` è stato rieseguito dopo la delega completa del renderer FieldPlot. Report: D:\Dev\BuildArtifacts\ecosystem-validation\20261028-185732, 29/29 passi exit 0. Build/test, audit, notebook/piloti, CLI/Web, UI/mobile/cancellazione, selftest numerico, raster in memoria e browser verificati in sequenza; nessun controllo saltato.
Nessuna nuova immagine di revisione/screenshot, solo JSON nelle cartelle *no-images e sette report recenti archiviati con i log. Output notebook aggiornati come previsto, precedenti immagini non rigenerate. Questa baseline prevale sulle note mirate più avanti; non certifica tutta l'accessibilità, ogni input/consumer o la distribuzione.

## FieldPlot: assemblaggio SVG condiviso

Charts.NodeFieldPlot/NodeFieldOptions assemblano i campi raster di nodi usando layout, limiti, palette RGB, sampler/PNG, contour, vettori e colorbar condivisi. Navier FieldPlot mantiene tipo opzioni/firma e traduce in una facade; helper privati di rendering inutilizzati rimossi. Heatmap a celle resta un renderer distinto.
Prima della modifica congelati quattro output strutturali (gradiente, contour, vettori, ostacolo) con hash RGB decodificati: normalizzati solo GUID e payload PNG nel confronto, non nell'output reale. Markup e pixel coincidono; precedenti fixture restano verdi. Overlay ora richiedono stessa forma del campo, contour count0–32 e ostacoli finiti non negativi: input fuori contratto non promessi compatibili.
Build/suite PlotSharp198/198, Web Release/quadro/smoke --no-images ricontrollati. Zero immagini, non nuovo full gate/pubblicazione. Scenario/UI/numerica restano nei consumatori; non backend universale RGBA/Figure-Axes e D02 non ancora certificata completa.

## Frame e proiezione di nodi condivisi

NodeFieldLayout centralizza margini/proporzioni Navier, Math.Round storico dell'altezza, posizione colorbar e proiezione X/Y (riga zero in basso). Non sostituisce la mappa cell-center Heatmap. Gli indici finiti fuori griglia vengono extrapolati come prima, non clippati; indici non finiti/overflow rifiutati.
Create richiede almeno2×2/max100000 nodi, larghezza1–10000 e altezza proiettata1–10000. Proporzioni che collassano a zero ora falliscono prima del raster. FieldPlot conserva assemblaggio/stili/header, usa layout per frame e contour.
Build e suite PlotSharp 192/192 passate, inclusi rounding pari, proporzioni rettangolari, corner/indici frazionari e fixture precedenti. Web Release/quadro/smoke --no-images ricontrollati, zero immagini. Non nuovo full gate/pubblicazione: l'ultima baseline completa resta precedente a queste estrazioni.

## Colorbar a gradiente compatibile

GradientColorbar.Render condivide il frammento Navier: 11 campioni RGB, 3 tick, gradiente basso→alto, etichetta ruotata con metà altezza intera legacy, escaping e geometria precedenti. FieldPlot mantiene il posizionamento; la colorbar Heatmap a 128 strisce non cambia.
Tre markup congelati prima della modifica (normalizzati solo id casuali) coincidono, incluso il caso di altezza dispari. Nuovo callback colore richiede #RRGGBB; limiti/proiezioni finiti e ordinati. Build/suite PlotSharp 178/178 passate; Web Release/quadro/smoke --no-images ricontrollati, zero immagini.
Non nuovo full gate o pubblicazione; frame complessivo e proiezione contour ancora locali. Non prova di equivalenza per input prima non validati o di visuale su tutti i browser.

## Frecce compatibili su griglie di nodi

NodeVectorOverlay.PathData centralizza la geometria Navier: normalizzazione al massimo globale (floor 1e-12), offset stride/2, soglia relativa 0.02, proiezione nodi con Y invertita e teste aperte. Non modifica il quiver Heatmap (celle/coordinate fisiche/teste riempite). Stile SVG e frame del campo restano Navier.
Quattro path FieldPlot congelati prima della migrazione coprono rettangolo, quattro direzioni, zero e soglia. Validazione condivisa richiede U/V stessa forma, min2×2/max100k, valori finiti ±1e12, stride positivo e proiezione valida. Input prima non validati non garantiti compatibili; frecce legacy non clippate ai bordi.
Build/suite PlotSharp 168/168 passate; Web Release/quadro smoke e adozione --no-images ricontrollati, compresa geometria nel report pressione. Zero immagini, non nuovo full gate/pubblicazione. Rendering colorbar e proiezione contour restano locali.

## Limiti dei colori dei campi — selezione legacy condivisa

FieldColorLimits.Resolve centralizza min/max, percentili a indice troncato, simmetria e fallback legacy: se l'intervallo è <1e-12, max=min+1. NaN ignorati nelle statistiche; campo tutto NaN richiede entrambi i limiti espliciti. Non sostituisce la politica costante di ColorScale (centro 0.5).
Contratto Resolve: 1–100000 campioni, finiti ±1e12 o NaN, clipping finito in [0,0.5], limiti input finiti ±1e12 e ordinati. L'espansione legacy può superare di 1 il bordo input. Il record costruito direttamente/NormalizeLegacy mantiene l'aritmetica del chiamante, compreso zero denominatore; non ha le guardie Resolve.
Otto fixture lette dai label colorbar del FieldPlot prima della modifica, più casi limite; build e suite PlotSharp 154/154 passate. Web Release/quadro/smoke adozione --no-images ricontrollati. Zero immagini e nessun full gate o pubblicazione; layout/proiezione, rendering colorbar e frecce locali.

## Campionamento bilineare RGB condiviso

BilinearRgbSampler prepara scanline RGB8/filter-0 da un campo di nodi (minimo 2×2), con riga zero in basso e output top-down. Callback colore riceve il valore interpolato; maschere ricevono coordinate degli indici campione e richiedono colore esplicito. L'ordine aritmetico legacy e le coordinate dei centri pixel sono conservati.
Navier FieldToPng delega campionamento/encoding, mantenendo firma, normalizzazione e politica ColorMap. I tre riferimenti PNG frozen continuano a coincidere su scanline/dimensioni e hash compressi sulla piattaforma registrata. Nuovi test su centro, orientamento, maschere/callback, input immutati e forme invalide: build e suite PlotSharp 138/138 passate.
Web Release/quadro smoke ricontrollati, zero immagini. Normalizzazione, proiezione/colorbar/frecce restano locali; non è un raster universale o un nuovo full gate/pubblicazione.

## PNG RGB8: encoder condiviso, non raster completo

RgbPngEncoder PlotSharp codifica scanline RGB8 top-down con filtro 0, dimensioni positive/lunghezze verificate e CRC statico immutabile. Navier FieldToPng conserva il campionamento bilineare, maschere, palette/normalizzazione e firma, delegando solo i byte PNG. Nessun supporto nuovo ad alpha, altri filtri o rendering Figure/Axes.
Prima della migrazione sono stati congelati tre campi piccoli (gradiente, divergente, mascherato) in ops/compatibility/png-baseline.json, come scanline hex/hash, non file immagine. I test confrontano sempre pixel/dimensioni/CRC; hash compresso confrontato soltanto sul runtime/OS registrato, perché ZLib può variare fra piattaforme. Input invalidi e concorrenza coperti.
Build e suite PlotSharp 127/127 passate; Web Release, quadro e smoke adozione --no-images ricontrollati. Nessuna immagine di revisione e nessun full gate o pubblicazione. La baseline completa precedente a 29 passi va riconfermata prima di attribuirla al codice attuale.

## Palette RGB condivise — compatibilità byte verificata

ColorPalette PlotSharp ora centralizza RGB e troncamento con preset immutabili: CompactSequential (4 stop, default PlotSharp), Sequential (8, Navier) e Diverging (9). I calcoli degli intervalli restano espliciti per preservare i byte storici; le palette non sono state sostituite l'una con l'altra. ColorMap Navier è una facade con politica legacy NaN→primo colore e infinito→estremo; l'API condivisa/default PlotSharp rifiuta i non finiti.
Prima della modifica sono stati congelati 323 input (griglia fitta, stop/ULP adiacenti, campioni colorbar) in ops/compatibility/palette-baseline.json, con hash dei sorgenti precedenti. I test copiano la fixture, non la rigenerano. CapturePaletteBaseline mantiene il file esistente; estensione pre-migrazione consentita solo se gli hash legacy corrispondono ancora.
Build e suite PlotSharp 117/117 passate, tutte e tre le palette combaciano con i riferimenti; Web Release e quadro smoke ricontrollati, zero immagini. PNG, normalizzazione field, colorbar/proiezione/frecce restano locali. Nuova API source-build solo: non nuovo full gate né pubblicazione.

## Contour NavierStokes: geometria condivisa, verifica mirata

MarchingSquares NavierStokes ora delega a PlotSharp conservando firma tuple, coordinate dei campioni e pairing fisso dei casi a sella. La modalità compatibile conserva i segmenti nulli ai vertici esattamente sul livello; il default storico di PlotSharp continua a eliminarli. Proiezione SVG, PNG, colorbar e frecce NavierStokes restano locali.
Contratto condiviso: campo non vuoto, max 100000 campioni, valori/livello finiti entro ±1e12. Le griglie UI correnti rientrano nel contratto; input prima non validati/unbounded non sono garantiti compatibili. Il core bufferizza i segmenti di ogni livello; non è una promessa di streaming/performance uguale al vecchio iterator.
Build/test mirati PlotSharp: 109/109; Web Release costruita e --quadro ricontrollato con due cicli, exit 0, nessuna immagine. Non nuovo full gate o benchmark numerico di convergenza. Nuovo overload solo source-build, nessuna pubblicazione.

## Verifica completa senza nuove immagini di revisione

```sh
dotnet run --file ops/ValidateEcosystem.cs -- --no-images
```

Modalità consigliata per ridurre gli artefatti visivi: tutti i 29 passi eseguiti, nessuna verifica saltata. I PNG vengono rasterizzati/encodati e controllati in memoria; screenshot e file SVG/PNG dei tre tool visuali non vengono salvati. Restano i report JSON e gli output dei notebook aggiornati. Le precedenti immagini non vengono cancellate né presentate come rigenerate; eventuali file temporanei usati dai test non sono disabilitati.
I tool RasterizeNotebookEvidence, VerifyBrowserCharts e VerifyNavierAdoption accettano anche --no-images direttamente. Report separati in ops/validation/*no-images; senza flag rimane il comportamento legacy di salvataggio. Il gate verifica GeneratedUtc entro la finestra reale del passo e SaveImages coerente, quindi copia sette report JSON nel proprio archivio timestamped. Report obsoleto/malformato o politica diversa fanno fallire il passo.
Ultima verifica completa: D:\Dev\BuildArtifacts\ecosystem-validation\20261028-150530, 29/29 exit 0. Suite operational verificata separatamente: 66/66, con test su modalità e freschezza. Non è una nuova revisione visiva manuale né una release.

## Contrasto mirato dei testi Web

Il controllo --mobile misura ora anche testi di supporto/accento HTML (.hint, .eyebrow, .card .tag, brand e footer con testo diretto), usando colori CSS e sfondi solidi compositati. Le misure iniziali degli accenti erano 4.05–4.30:1; i testi attenuati non raggiungevano 4.5:1. Due token Web sono stati scuriti: --accent #2467ba e --ink-3 #6b6a65. Serie SVG e calcoli invariati.
Build Release Web e --mobile passati: 17 campioni tra home/tubo/vento, min circa 4.97:1; layout/testi/dimensioni/tastiera/calcoli ancora verdi. Desktop ricontrollato. Sette nuovi casi sul calcolo sRGB del contrasto portano la suite operational a 50/50. Nessuna immagine, report in ops/validation/navier-mobile-smoke.
Non è audit completo WCAG, di tutti i testi/SVG, dei dispositivi o dei pulsanti in ogni stato: sfondi con immagini/gradienti e opacità di gruppo non supportati vengono segnalati, non ignorati. Gate futuro 29 passi invariato, non full gate rieseguito.

## Controlli stretti e tastiera — verifica corrente

Il comando --mobile verifica anche altezza degli input/select (criterio 44 px) e un aggiornamento nativo da tastiera del range Vento. La misura iniziale minima era 16 px; sotto 600 px il CSS della Web app porta input/select a min-height 44 px e aggiunge focus visibile.
Dopo build Release Web, prova passata a 360×800: tubo 5 controlli e vento 3, minimo 44 px, documento 360 px, testi SVG min 11 px. Freccia destra: 180→181 km/h e forza 1926→1948 N, indicatore di focus verificato tramite DOM/stile. Desktop ricontrollato, nessuna immagine.
Otto nuovi casi del criterio di altezza: suite operational 43/43, build riuscita. Report/log in ops/validation/navier-mobile-smoke. Altezza elemento e tastiera non certificano hit-testing/touch reali, contrasto o accessibilità completa; simulazioni mobile non provate. Gate futuro 29 passi invariato, non full gate rieseguito.

## Grafici su viewport stretto: testi e scorrimento

Il controllo --mobile misura ora i testi SVG tramite font CSS e matrice getScreenCTM, con soglia di test 10 px effettivi. La misura iniziale era circa 5.85 px; la Web NavierStokes ora mantiene la larghezza nativa del grafico sotto 600 px e fa scorrere il solo riquadro. Il componente Chart è una regione nominata e focusabile, con indicatore di focus.
Dopo build Release Web, `dotnet run --file ops/VerifyNavierWeb.cs -- --mobile` è passato: home/tubo/vento a 360×800, documento 360 px, testi SVG minimo 11 px, freccia destra da tastiera e bordo finale raggiungibili nei tre grafici. Nessuna immagine; desktop ricontrollato. Validatore del criterio testuale: otto nuovi casi, suite operational 35/35 passata.
Report/log: ops/validation/navier-mobile-smoke. La soglia pixel non certifica tutta la leggibilità/accessibilità, contrasto, lettori di schermo o touch. Simulazioni mobile non verificate. Gate futuro resta 29 passi, non full gate rieseguito; nessun cambiamento ai dati o al core PlotSharp.

## Viewport stretto: controllo mirato, senza immagini

`dotnet run --file ops/VerifyNavierWeb.cs -- --mobile` (Web già costruita Release, Edge installato). Testa home/tubo/vento a 360×800: larghezza documento, limiti orizzontali dei controlli/pannelli/grafici e aggiornamento dei calcoli. Non avvia simulazioni. Modalità --mobile alternativa a --quadro/--fischio, report separato in ops/validation/navier-mobile-smoke. Server/browser chiusi al termine.
Prova passata senza modifiche CSS: documento 360 px in tutte le pagine, 5 controlli tubo e 3 vento entro i limiti. Otto casi xUnit per il validatore dei limiti (tolleranza 1 px), suite operational 27/27 verde. Ricontrollato il percorso desktop tubo/vento.
Zero screenshot per default, --screenshot opzionale salva una sola immagine. Non certifica telefono reale, touch, leggibilità dei testi SVG, pagine delle simulazioni o deployment. Gate esteso a 29 passi previsti e compilato, non full gate rieseguito.

## Fischio: avvio/stop mirato, senza immagini

`dotnet run --file ops/VerifyNavierWeb.cs -- --fischio` (dopo build Release Web, Edge installato). Griglia UI LBM 140×50; due cicli con progresso reale, controlli bloccati durante la simulazione, stop e comandi ripristinati. Report separato: ops/validation/navier-fischio-smoke. Server/browser temporanei chiusi al termine.
Il comando --quadro resta compatibile ed è stato ricontrollato. Le due modalità sono alternative; opzionale --screenshot produce una sola immagine, nessuna per default. Due nuovi casi di cancellazione/ripresa LBM portano la suite PlotSharp a 98/98 passati. Il gate ora prevede 28 passi ed è compilato, ma non rieseguito completamente in questa tranche.
Non verifica periodicità, frequenza/Strouhal misurato, convergenza, accuratezza, mobile o deployment.

## Quadro: avvio/stop mirato, senza immagini

```sh
dotnet run --file ops/VerifyNavierWeb.cs -- --quadro
```

Richiede NavierStokes.Web già costruita Release e Microsoft Edge. Avvia un server temporaneo su loopback, verifica l'interattività, seleziona griglia 31 e osserva due cicli con progresso effettivo, blocco dei parametri durante la simulazione, stop e ripristino dei comandi. Nessun benchmark o attesa di convergenza. Report/log separati: ops/validation/navier-quadro-smoke. Server e browser chiusi al termine.
Zero immagini per default; opzionale `--screenshot` salva una sola immagine. Il controllo precedente senza --quadro resta tubo/vento ed è stato rieseguito. Due nuovi test core sulla cancellazione/ripresa: suite PlotSharp 96/96 passata. Gate esteso a 27 passi previsti ma non rieseguito completamente in questa tranche; baseline completa a 25 passi distinta. Fischio, mobile e deployment restano aperti.

## Verifica Web mirata, senza immagini per default

Dopo build Release di NavierStokes.Web, da MathemaSharp:

```sh
dotnet run --file ops/VerifyNavierWeb.cs
```

Avvia Kestrel su loopback con porta temporanea e Edge isolato, naviga dalla home a tubo/vento e invia eventi input ai range reali. Verifica raddoppio lunghezza/pressione, velocità/forza quadratica e area/forza lineare con tolleranze per gli arrotondamenti UI; controlla SVG condivisi, errori Blazor e browser. Server e browser vengono chiusi a fine prova. Report/log: ops/validation/navier-web-smoke. Parser numerici coperti da 13 nuovi casi xUnit (suite operational totale 19).
Nessuno screenshot per default; `--screenshot` produce al massimo una immagine della pagina vento. Il controllo è aggiunto al gate (26 passi previsti), ma l'ultima verifica completa resta quella a 25 passi: questa tranche è stata verificata con build/test mirati e il tool live, non con un nuovo full gate.
Scope: home/tubo/vento desktop, non simulazioni quadro/fischio, mobile, contenuti editoriali o deployment.

## Adozione NavierStokes e inventario app

`dotnet run --file ops/VerifyNavierAdoption.cs` genera tre report lineari NavierStokes reali su fixture smoke, XML, screenshot Edge e report in ops/visual/navier. Richiede il checkout `articoli/2026.10 NavierStokes` e Edge installato. Non misura accuratezza di regime e non certifica la UI Web.
NavierStokes.Core delega line/scatter a PlotSharp tramite facade; campi/PNG/contour restano locali. Gate esteso: build CLI e Web, selftest numerico originale completo e smoke report. Totale: 25 passi. Il selftest fisico è più costoso delle sole verifiche notebook.
Le app MathemaSharp non avevano renderer locali nei sorgenti .cs/.razor ispezionati (Lab: 2 file, Docs: 11): nessuna dipendenza grafica superflua aggiunta. Lab è console numerica; Docs è viewer Markdown/asset. Inventario lessicale, non certificazione di ogni asset esterno. Source-build solo, nessuna pubblicazione.

## Scenari applicativi — stato corrente

Dopo build Release di MathemaSharp e PlotSharp:

```sh
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id cold-storage
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id solar
```

Il manifest copre otto notebook, 43 SVG. Magazzino frigorifero: 3 SVG, gap richiesto, un gruppo KPI e due risposte testuali; fotovoltaico: 2 SVG, un gruppo KPI e due risposte. `expectedKpi`/`expectedQuotes` sono verificati dal runner e dai test tramite le classi semantiche mathema-kpi/mathema-quote (stili e testi invariati).
Il browser verifica 43 SVG individuali e assembla gli output HTML dei due scenari: cinque tile per dashboard, due risposte non vuote, nessun overflow orizzontale a viewport desktop 1280 px. I due screenshot full-page sono stati osservati. Non è una prova dell'interfaccia notebook/markdown o di VS Code, né di layout mobile, popup nativi o accesso individuale ai campioni sovrapposti.
Screenshot/report: ops/visual/browser; gate: 21 passi. Questa sezione prevale sui conteggi storici seguenti.

## Tranche corrente — STL e tour

Il manifest ora copre sei notebook: Core/03 e TimeSeries/06–10, 38 SVG totali. Comandi aggiuntivi, dopo build Release di MathemaSharp e PlotSharp:

```sh
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id stl
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id tour
```

STL: 6 SVG e tabelle richieste; tour: 5 SVG senza obbligo di tabelle. `requireTable` nel manifest è verificato dal runner e dai test, `requireGap` mantiene il controllo smoothing. Il gate genera automaticamente i passi migrati dal manifest, senza duplicare la lista degli id. Il browser e il rasterizer seguono lo stesso manifest; 38 grafici verificati in Edge, gli 11 nuovi screenshot osservati. Gate: 19 passi. Prerequisito browser: Microsoft Edge installato.
Questa sezione prevale sui conteggi storici seguenti. Limiti invariati: target sovrapposti, popup OS, VS Code e distribuzione non certificati.

## Browser e revisione visiva — verifica corrente

`dotnet run --file ops/VerifyBrowserCharts.cs` richiede Microsoft Edge installato; usa Playwright con Edge Chromium headless e documenti SVG inline senza richieste di rete. Verifica 27 grafici: testi nel viewport SVG, nessuna collisione bounding-box titolo/testi, almeno un hit target con title raggiungibile per grafico. Produce screenshot e report in ops/visual/browser. Incluso nel gate (ora 17 passi).
Il browser aveva rilevato tre collisioni titolo/legenda: corrette tramite SeparateLegendRow opt-in nei sorgenti PlotSharp, abilitato da IndexedCharts. Tooltip aggiunti alle aste. I 19 raster dei nuovi notebook 06/08 sono stati osservati; tre screenshot Edge dopo la correzione osservati separatamente. La rasterizzazione ora segue tutto il manifest e produce 27 PNG.
Restano limiti espliciti: target sovrapposti non garantiscono accesso individuale a ogni campione; popup nativo del sistema operativo e renderer VS Code non verificati. Il report conta target totali/raggiungibili, senza dichiarare copertura completa. Test sugli output del browser distinto dall'esecuzione reale.

## Notebook migrati — manifest condiviso

`ops/migrated-notebooks.json` registra id, percorso, conteggio SVG atteso e obbligo del gap. Da MathemaSharp, dopo build Release di MathemaSharp e PlotSharp:

```sh
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id core
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id smoothing
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id autoregression
dotnet run --file ops/RunMigratedCoreNotebook.cs -- --id arima
```

Conteggi: 2, 6, 6, 13 SVG (27 totali). Tabelle preservate; tutti i runner nel gate. Il comando senza argomenti resta Core e `--smoothing` rimane compatibile. Manifest/test degli output non sostituiscono l'esecuzione reale. La review preliminare comprende gli otto grafici Core/07 e i 19 nuovi raster 06/08; il report Edge verifica 27 SVG e tre screenshot dei casi corretti sono stati osservati. Non equivale a una prova del renderer VS Code.

## Registro dei test sospesi

Il gate produce un TRX nuovo e il report `ops/validation/pysharp-skips.md`, conservato anche accanto al TRX nella directory timestamped del gate. Nell'esecuzione corrente: 12 test PostgreSQL senza server raggiungibile e una theory pretrained opt-in. I test sospesi non sono passati e non sono deroghe. Le credenziali devono rimanere nell'ambiente, mai nei sorgenti.
Il parser TRX è testato con sei casi xUnit: namespace, conteggi, fallimenti, motivazioni assenti ed evidenza vuota/incompleta. I due nuovi progetti operativi Ecosystem.Validation targettano .NET 10; non cambiano i target .NET 11 delle librerie numeriche esistenti.

## Smoothing/Kalman e revisione grafica

Anche `TimeSeries/07-ts-smoothing-kalman.ipynb` usa il bootstrap PlotSharp. Runner: `dotnet run --file ops/RunMigratedCoreNotebook.cs -- --smoothing`. Verifica sei SVG e almeno una linea interrotta sul gap; tabelle preservate tramite `shared/Presentation.csx`, comune anche al renderer legacy.
Nei build locali, NaN Y interrompe Lines/Panels; Infinity, X non finiti, serie interamente mancanti e stems/bar con NaN sono rifiutati. I pacchetti preview precedenti restano finite-only. Nessun pack/distribuzione implicito.
`dotnet run --file ops/RasterizeNotebookEvidence.cs` produce 27 PNG di revisione e manifest hash in ops/visual, da output reali. Il gate rigenera gli artefatti ma non esegue una revisione visiva automatica. Sono state osservate otto immagini: gap, curve, titoli e legende leggibili. Il clipping dei marker campione è stato corretto nei build locali tramite overlay per centri nel dominio; due spettri rigenerati e osservati mostrano cerchi completi. Highlight e browser mantengono verifiche separate. L'etichetta degli spettri è ora k, non Lag. Raster Svg.Skia è uno strumento di revisione, non backend PNG PlotSharp né prova nel browser.

## Notebook applicativo migrato

`notebooks/Core/03-complex-signals.ipynb` usa ora `shared/MathemaSharp.PlotSharp.csx`, senza il renderer legacy. Eseguire build Release di entrambe le librerie, poi `dotnet run --file ops/RunMigratedCoreNotebook.cs`. Il runner imposta il roll-forward .NET 11 per JupyterNet e verifica due SVG dopo aver cancellato gli output precedenti. Anche la baseline costruisce PlotSharp prima dell'esecuzione. Questo percorso è JupyterNet, distinto dai piloti JupyterSharp.

## Migrazione grafica opt-in

Dopo `ops/RunEcosystemPilot.cs`, eseguire `dotnet run --file ops/RunIndexedChartsPilot.cs`.
Il notebook `03-indexed-charts.ipynb` carica `shared/PlotSharpCharts.csx`: API Chart.Lines/Panels/Stems delegate a PlotSharp.IndexedCharts, quattro SVG verificati XML e output precedenti cancellati prima dell'esecuzione. Il runner è incluso nel gate.
Non caricare insieme la facade e Charts.csx: espongono gli stessi nomi. Sono supportati dati double finiti; layout/tick/tooltip differiscono e i gap non finiti restano al renderer legacy. Non è una migrazione completa né un'approvazione visiva.

## Ecosistema — verifica aggiornata

```sh
dotnet run --file ops/ValidateEcosystem.cs
```

Esegue build/test delle sei soluzioni, audit NuGet, baseline di 24 notebook, build del kernel PySharp, notebook gemelli/precisione, notebook MatSharp, tre piloti NuGet JupyterSharp e quattro notebook applicativi migrati a PlotSharp. Include report TRX e raster di review: 16 passi; rasterizzazione non equivale a revisione visiva. La verifica Edge dei 27 SVG aggiunge il diciassettesimo passo, dopo il raster. Log e report: `D:\Dev\BuildArtifacts\ecosystem-validation`. Non esegue benchmark né pack e non ignora i fallimenti. I test sospesi restano visibili nelle suite.

Per i piloti separati:

```sh
dotnet run --file ops/RunEcosystemPilot.cs
dotnet run --file ops/RunLanguageInteropPilot.cs
```

Il primo ripristina i pacchetti in una cache isolata e installa localmente la CLI JupyterSharp; il secondo usa quella CLI e quei pacchetti. Output reali salvati in `notebooks/Ecosystem/01-signal-spectrum.ipynb` e `02-typed-language-interop.ipynb`. Il primo mostra matematica e SVG; il secondo esegue Python/MATLAB tramite embedding C#, non plugin linguistici nativi.

### MatSharp typed: uso di A \\ b

```matlab
A = [decimal128(1) decimal128(0); decimal128(0) decimal128(1)];
b = [decimal128('1.0000000000000000000000001'); decimal128(2)];
x = A \\ b;
x(1) - decimal128(1)  % 1e-25
```

Nell'host registrare prima `MatSharpLib.NDSharpBridge.NDSharpFunctions.Register(engine)`. LU per quadrate, QR per sovradeterminate; RHS con righe compatibili e una o più colonne. Input copiati, risultato column-major nel medesimo tipo. Sistemi sottodeterminati, singolari o tipi senza adattatore sono rifiutati; nessuna divisione matriciale destra nuova. Soglie/rango sono quelli dei kernel condivisi, non una certificazione MATLAB universale.

### PySharp: selezione dei tipi CLR generici

L'host può iniettare `typeof(List<>)` come `ClrList` e `typeof(Decimal128)` come `D128` tramite `PyEngine.SetVariable`. Nel linguaggio:

```python
from mathema import decimal128
List128 = ClrList[D128]
values = List128()
values.Add(decimal128('1.0000000000000000000000001'))
assert values[0] - 1 == decimal128('1e-25')
```

La selezione richiede una definizione CLR aperta e argomenti CLR di arità corretta; i constraint sono applicati. Non inferisce il tipo, non trasforma liste Python in NumPy typed e non risolve automaticamente tutte le famiglie generiche dagli import. Il modulo mathema accetta stringhe/interi e valori Decimal dello stesso formato; non recupera precisione da literal float già arrotondati.


## Benchmark FFT — prima misura .NET 11

```sh
dotnet run --file ops/RunFftBenchmarks.cs
```

BenchmarkDotNet 0.15.8 non riconosce .NET 11 RC nella toolchain out-of-process: il runner usa ShortRun in-process, 3 warm-up e 3 iterazioni, un lancio. Misura preliminare, non confronto definitivo fra algoritmi o formati. Risultati e log: `D:\Dev\BuildArtifacts\MathemaSharp\benchmarks`.

Host: Windows 11, Intel i7-11370H, .NET 11 RC3 x64. Dati double complessi.

| Operazione | N | Media | Allocazioni/op |
|---|---:|---:|---:|
| FFT | 64 | 1.462 µs | 1.02 KB |
| IFFT | 64 | 1.609 µs | 1.02 KB |
| FFT Bluestein | 1009 | 206.236 µs | 191.70 KB |
| IFFT Bluestein | 1009 | 216.258 µs | 207.49 KB |
| FFT radix-2 | 1024 | 27.478 µs | 16.02 KB |
| IFFT radix-2 | 1024 | 30.097 µs | 16.02 KB |

Il report BenchmarkDotNet contiene intervalli di confidenza e deviazioni. Lunghezze diverse: il rapporto dei tempi non è un confronto a parità di problema. Non sostituire una trasformata di lunghezza 1009 con padding a 1024 senza valutarne la diversa semantica. Restano misure out-of-process, altri tipi e confronto con DFT.

## Baseline completa — un comando

Da Windows, nella root:

```sh
dotnet msbuild ops/validate.proj -t:Baseline
```

Compila Release, ripristina JupyterNet, esegue i **22 notebook esplicitamente selezionati** in ops/baseline-notebooks.json aggiornando gli output in-place, poi esegue i test Release. Salva SDK, runtime installati, conteggi dei test, esiti notebook, esclusioni e log in `ops/artifacts/baseline/<timestamp-id>/report.json` e `summary.md`. Exit code non zero su errori; il report è prodotto anche in caso di fallimento gestito.

NDSharp, PySharp e scenari non sono nella selezione: 7 notebook esclusi con motivazioni nel report. I 3 test C#/PySharp restano sospesi; la build/test della soluzione continua a includere i test NDSharp esistenti, senza modificarne il codice. Non esegue benchmark, Python, publish o migrazione dei consumatori. Il runtime dei notebook usa roll-forward LatestMajor con prerelease; la riproducibilità richiede SDK/runtime e feed compatibili, non un'immagine d'ambiente completamente fissata.

Solo build e test, **senza dichiarare eseguiti i notebook**:

```sh
dotnet msbuild ops/validate.proj -t:BuildAndTest
```

Questa modalità registra FullBaseline=false e notebook NotExecuted; i test degli output leggono quelli già salvati. Ultima baseline completa verificata: build senza warning/errori, 22 notebook eseguiti, 272 test passati e 3 sospesi.

## Notebook di consolidamento — eseguiti

| Libreria | Usage verificata | Catalogo API pubblico |
|---|---|---|
| Core | [11-consolidation](notebooks/Core/11-consolidation.ipynb) | [12-api-reference](notebooks/Core/12-api-reference.ipynb) |
| LinearAlgebra | [11-consolidation](notebooks/LinearAlgebra/11-consolidation.ipynb) | [12-api-reference](notebooks/LinearAlgebra/12-api-reference.ipynb) |
| TimeSeries | [11-consolidation](notebooks/TimeSeries/11-consolidation.ipynb) | [12-api-reference](notebooks/TimeSeries/12-api-reference.ipynb) |
| Quantum | [11-consolidation](notebooks/Quantum/11-consolidation.ipynb) | [12-api-reference](notebooks/Quantum/12-api-reference.ipynb) |

I quattro cataloghi API sono anch'essi eseguiti: reflection degli assembly Release sul runtime .NET 11, con tipi esportati, membri pubblici dichiarati e constraint. Rigenerazione: `dotnet msbuild ops/reference-notebooks.proj -t:Run`. I cataloghi non certificano comportamento o copertura e non riproducono tutte le annotazioni sorgente (nullable, tuple, ref/readonly).

I quattro notebook sono stati eseguiti con JupyterNet sul runtime .NET 11, salvando gli output dei sei formati: Half, float, double, Decimal32/64/128. Le Reference sono focalizzate sui kernel consolidati, non sull'intera superficie delle librerie. Fixture e confronti sono in T; i casi semplici non certificano accuratezza universale.

Per rieseguirli dalla root (richiede tool JupyterNet ripristinabile dai feed configurati):

```sh
dotnet msbuild ops/consolidation-notebooks.proj -t:Run
```

Il comando compila Release, imposta roll-forward .NET 11 e aggiorna gli output in-place. Bootstrap `notebooks/shared/MathemaSharp.Numerics.csx`, senza dipendenze Charts/NDSharp. I test xUnit verificano struttura e presenza degli output; non eseguono il kernel. Questa verifica non estende lo stato di esecuzione agli altri notebook né ripristina i tre confronti C#/PySharp sospesi.

### Esempi Core precedenti — rieseguiti

I notebook Core 01–05 sono stati rieseguiti sul runtime .NET 11 RC3. Il 05 include Decimal32/64/128, Complex<T>, NaN/infinito e parsing parziale; il 04 controlla ambiente e correttezza SIMD, non produce misure prestazionali statistiche. I notebook 01–03 includono contratti e collegamenti alla Reference.

```sh
dotnet msbuild ops/core-notebooks.proj -t:Run
```

Il runner supporta `-Module Core|LinearAlgebra|TimeSeries|Quantum` per limitare la selezione. Con Module Core non esegue altri moduli o PySharp; con TimeSeries la selezione ricorsiva puo' includere i notebook PySharp. Gli altri notebook non sono dichiarati rieseguiti da questa tranche.

### LinearAlgebra e Quantum — esempi introduttivi rieseguiti

- [LU e minimi quadrati](notebooks/LinearAlgebra/01-lu.ipynb): pivoting, residuo, inversa, riuso, QR Decimal128 e norme.
- [Operatori quantistici](notebooks/Quantum/01-operators.ipynb): normalizzazione su ampiezze estreme, autostati, diagonalizzazione, densità ed evoluzione Decimal128.

Entrambi contengono contratti e verifiche eseguibili, con output .NET 11 salvati. Il caso Quantum verifica errore ≤1e-28 per H=X, t=π/2, non per ogni matrice.

```sh
dotnet msbuild ops/algebra-quantum-notebooks.proj -t:Run
```

### TimeSeries C# — esempi rieseguiti

Notebook 06 (baseline/AR), 07 (smoothing/Kalman), 08 (ARIMA/VAR/GARCH) e tour 10: output .NET 11 salvati, contratti e collegamenti Reference aggiornati.

```sh
dotnet msbuild ops/timeseries-notebooks.proj -t:Run
```

Il runner usa `-Module TimeSeries -TopLevelOnly` e seleziona solo 06–08 e 10. Non esegue scenari, PySharp o 09 (ponte NDSharp). `-Filter` usa wildcard PowerShell, anche intervalli come `0[6-8]*`. L'esecuzione C# non dimostra concordanza con il gemello PySharp; i relativi test restano sospesi.

### Oracoli numerici indipendenti

[Notebook di validazione](notebooks/TimeSeries/13-independent-oracles.ipynb): gamma Q e il controesempio Hilbert12, con output reali salvati.

```sh
# Consuma le fixture salvate: non richiede Python
dotnet msbuild ops/oracle-notebook.proj -t:Run
dotnet test src/tests/MathemaSharp.TimeSeries.Tests --filter FullyQualifiedName~IndependentOracleTests

# Rigenera le fixture su Windows: richiede Python 3 e accesso al feed pip
dotnet msbuild ops/numerical-oracles.proj -t:Setup,Generate
```

[Oracoli Quantum](notebooks/Quantum/13-independent-oracles.ipynb): MatrixExp non normale/complessa e autovalori molto sbilanciati, con output reali. Esecuzione: `dotnet msbuild ops/quantum-oracle-notebook.proj -t:Run`. Test: `dotnet test src/tests/MathemaSharp.Quantum.Tests --filter FullyQualifiedName~IndependentQuantumOracleTests`.

Il generatore usa mpmath 1.3.0 con 200 cifre, isolato in ops/.venv-oracles (ignorata da Git). Le fixture contengono stringhe a 160 cifre. I test C# non invocano Python né MathemaSharp per calcolare i riferimenti. Tolleranze e limiti sono in ARCHITECTURE e ROADMAP; un residuo piccolo non certifica l'accuratezza su matrici mal condizionate.

### Soluzione con diagnostica

```csharp
using MathemaSharp.LinearAlgebra;

var report = LinearSystemDiagnostics.Solve<double>(
    new double[,] { { 2, 0 }, { 0, 4 } }, [2, 8]);
// Solution=[1,2], ConditionInfinityEstimate=2, IsIllConditioned=false
```

[Notebook eseguito](notebooks/LinearAlgebra/13-solve-diagnostics.ipynb). Avvio: `dotnet msbuild ops/diagnostics-notebook.proj -t:Run`.

`NormwiseBackwardErrorInfinity` (alias `NormwiseBackwardError`) e `RelativeResidualInfinity` sono calcolati in T con scaling/FMA. `ConditionInfinityEstimate` richiede n solve aggiuntivi: usarla quando serve, non sostituisce il Solve veloce. Il flag `IsIllConditioned` è un avviso basato su rcond ≤ sqrt(epsilon), non una garanzia sulla precisione della soluzione.

## Console

Dalla root:

```sh
dotnet run --project src/apps/MathemaSharp.Lab -- --topic generic
dotnet run --project src/apps/MathemaSharp.Lab -- --topic precision
dotnet run --project src/apps/MathemaSharp.Lab -- --topic decimal
dotnet run --project src/apps/MathemaSharp.Lab -- --topic complex
dotnet run --project src/apps/MathemaSharp.Lab -- --topic integers
dotnet run --project src/apps/MathemaSharp.Lab -- --topic simd
dotnet run --project src/apps/MathemaSharp.Lab -- --topic quantum
dotnet run --project src/apps/MathemaSharp.Lab -- --topic timeseries
```

Il topic `quantum` mostra prodotto interno, valori attesi di Pauli, commutatore, autovalori, evoluzione temporale e matrice densità; vedi [QUANTUM.md](QUANTUM.md). Il topic `timeseries` percorre benchmark, Yule-Walker, Hannan-Rissanen, Kalman/SES, GARCH e STL su dati simulati con parametri noti; vedi [TIMESERIES.md](TIMESERIES.md).

Senza opzioni viene selezionato `all`. Topic sconosciuto: exit code 2 e log di errore. Configurazione via Microsoft.Extensions.Hosting: anche variabile ambiente `topic`; la riga di comando prevale. Output numerico con cultura invariante.

Risultati significativi: `0.1+0.2` double = `0.30000000000000004`; decimal = `0.3`. `[1e16,1,1,-1e16]`: somma naive 0, Kahan 2. Tre righe da 0.005: ToEven per riga 0.00, sul totale 0.02. Per z=3+4i: dot hermitiano 25, bilineare -7+24i.

## Benchmark

```sh
dotnet run -c Release --project src/benchmarks/MathemaSharp.Benchmarks -- --list flat
dotnet run -c Release --project src/benchmarks/MathemaSharp.Benchmarks -- --filter '*DotBenchmarks*' --job Dry --artifacts ops/artifacts/benchmarks
dotnet run -c Release --project src/benchmarks/MathemaSharp.Benchmarks -- --filter '*SumBenchmarks*' --job Short --artifacts ops/artifacts/benchmarks
dotnet run -c Release --project src/benchmarks/MathemaSharp.Benchmarks -- --filter '*IntegerBenchmarks*' --artifacts ops/artifacts/integers
dotnet run -c Release --project src/benchmarks/MathemaSharp.Benchmarks -- --filter '*ComplexBenchmarks*' --artifacts ops/artifacts/complex
```

`Dry`: solo verifica; `Short`: esplorazione; job predefinito: misure più robuste. Non fare confronti con debugger o da Stopwatch in un notebook. Chiudere carichi concorrenti e registrare runtime effettivo, CPU, GC e JIT. CSV/HTML/Markdown prodotti automaticamente. Le righe confrontabili hanno stessa N; diversi tipi hanno contratti di precisione diversi. Array preparati fuori dalla misura; DFT invece alloca deliberatamente l'output. SIMD e LINQ possono cambiare l'ordine della somma.

Il run locale Dry ha eseguito tutte le quattro combinazioni Dot (N=31,4099). Le medie cold-start nell'ordine dei millisecondi includono warm-up/JIT non rappresentativi: non significano che SIMD sia più lento. Non sono disponibili misure sui nuovi decimal IEEE .NET 11.

## Notebook (JupyterNet)

Kernel JupyterNet (C#, Roslyn scripting, repository `D:\Dev\2026\repos\JupyterNet`), tool `jupyternet` dal feed locale `D:\Dev\NuGetLocalFeed` (manifest in `.config/dotnet-tools.json`, `dotnet tool restore`). I notebook sono salvati **eseguiti**, con output e grafici SVG.

```powershell
./scripts/run-notebooks.ps1                    # build Release + esegue tutti i notebook (C# e PySharp, anche quelli degli scenari)
./scripts/run-notebooks.ps1 -Filter '0[6-9]*'  # solo serie temporali
```

Interattivo: VS Code con l'estensione JupyterNet, tipo notebook `jupyternet-notebook`.

Come sono collegati alle librerie (richiede JupyterNet ≥ 0.3.0, versione fissata nel manifest dei tool):
- ogni notebook inizia con `#load "shared/MathemaSharp.csx"`, relativo alla cartella del notebook; quel file referenzia `artifacts/bin/*/release/*.dll` con `#r` relativi a sé stesso. Nessun percorso assoluto: il repository si può spostare;
- le direttive `using` di un `#load` non si propagano: ogni notebook dichiara le proprie;
- `Console.WriteLine` finisce nel notebook (il kernel C# la cattura dalla 0.3.0), raggruppata per cella e nell'ordine rispetto ai grafici;
- `shared/Kernel.csx` imposta la cultura invariante anche come default dei thread, perché ogni cella può girare su un thread diverso;
- `shared/Charts.csx`: grafici SVG statici (`Chart.Lines`, `Chart.Panels`, `Chart.Stems`) e `Table.Show`, via `Display.Html`;
- il notebook 05 gira sul runtime .NET 11 RC2 via `DOTNET_ROLL_FORWARD=LatestMajor` + `DOTNET_ROLL_FORWARD_TO_PRERELEASE=1`: Roslyn compila contro gli assembly del runtime in esecuzione, quindi Decimal32/64/128 e Complex<T> sono disponibili.

## Pacchetti

```powershell
./scripts/pack.ps1            # artifacts/package/release/*.nupkg
./scripts/pack.ps1 -Publish   # copia anche nel feed locale (aumentare prima <Version>)
```

## Librerie: Usage

I sample didattici sono in `notebooks/<lib>/`; gli helper condivisi sono in `notebooks/shared/`. Gli output sono stati azzerati durante la migrazione: rieseguire i notebook prima di considerarli verificati. La console Lab resta un harness di verifica, non il catalogo dei sample.

### MathemaSharp.Core — Usage

Namespace: `MathemaSharp.Core`. Notebook: `notebooks/Core/`.

- [01-generic-math](notebooks/Core/01-generic-math.ipynb)
- [02-precision-decimal](notebooks/Core/02-precision-decimal.ipynb)
- [03-complex-signals](notebooks/Core/03-complex-signals.ipynb)
- [04-performance](notebooks/Core/04-performance.ipynb)
- [05-dotnet11-decimals](notebooks/Core/05-dotnet11-decimals.ipynb)

### MathemaSharp.LinearAlgebra — Usage

Namespace: `MathemaSharp.LinearAlgebra`. Notebook: `notebooks/LinearAlgebra/`.

- [01-lu](notebooks/LinearAlgebra/01-lu.ipynb)

Usare `LuFactorization<T>.Create(matrix)` e `Solve(rhs)` per riutilizzare la stessa fattorizzazione; i pivot quasi singolari sono rifiutati.

### MathemaSharp.Quantum — Usage

Namespace: `MathemaSharp.Quantum`. Notebook: `notebooks/Quantum/`.


### MathemaSharp.TimeSeries — Usage

Namespace: `MathemaSharp.TimeSeries`. Notebook: `notebooks/TimeSeries/`.

- [06-ts-reference-autoregression](notebooks/TimeSeries/06-ts-reference-autoregression.ipynb)
- [07-ts-smoothing-kalman](notebooks/TimeSeries/07-ts-smoothing-kalman.ipynb)
- [08-ts-arima-var-garch](notebooks/TimeSeries/08-ts-arima-var-garch.ipynb)
- [09-ts-stl-ndsharp](notebooks/TimeSeries/09-ts-stl-ndsharp.ipynb)
- [10-ts-libreria-tour](notebooks/TimeSeries/10-ts-libreria-tour.ipynb)
- [10-ts-libreria-tour](notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb)
- [A1-magazzino-frigorifero-csharp](notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-csharp.ipynb)
- [A1-magazzino-frigorifero-pysharp](notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-pysharp.ipynb)
- [D1-fotovoltaico-csharp](notebooks/TimeSeries/Scenarios/D-fotovoltaico/D1-fotovoltaico-csharp.ipynb)
- [D1-fotovoltaico-pysharp](notebooks/TimeSeries/Scenarios/D-fotovoltaico/D1-fotovoltaico-pysharp.ipynb)

### MathemaSharp.NDSharp — Usage

Namespace: `MathemaSharp.NDSharp`. Notebook: `notebooks/NDSharp/`.


Il ponte usa dtype/storage runtime e contiene ancora conversioni via double: non è un kernel interamente Generic Math. Il notebook STL in TimeSeries mostra l'integrazione.
