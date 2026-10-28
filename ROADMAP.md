# MàthemaSharp — roadmap

MathemaSharp è il codice e il nome dei pacchetti; **MàthemaSharp** è il marchio. Obiettivo: una libreria matematica modulare basata su Generic Math, con .NET 11 e supporto ai nuovi Decimal32/64/128 dove compatibili con gli algoritmi.

Questa roadmap sostituisce le precedenti voci aggregate che davano per completate migrazione e verifica dei notebook. Una build riuscita non dimostra che ogni API supporti ogni formato.

## Roadmap corrente — stato aggiornato

Questa sezione prevale su tutti i quadri e conteggi storici riportati più avanti. Non attribuisce alle ultime modifiche una baseline completa che non è stata ancora rieseguita. Ottimizzazioni di performance rimandate: prima correttezza, contratti, notebook, interoperabilità e distribuzione.

### Ultima baseline completa — NDSharp filled sampler/bands

Gate --no-images D:\Dev\BuildArtifacts\ecosystem-validation\20261028-215324: 31/31 exit0 dopo le ultime estrazioni. Solution build/test/audit, kernel package e source isolati, notebook linee/bande, browser/UI/cancellazione e selftest numerico completo rieseguiti. Suite shared227 casi, ND19 e fixture/query compatibilità riconfermate nel gate; test sospesi e warning restano dichiarati.
Zero nuovi file review/screenshot, notebook output PNG incorporati come previsti e encoding QA in memoria. Cartelle no-images solo JSON, report recenti/politica verificati e archiviati. Prevale sulle note di full gate non rieseguito. D03 resta parziale (palette/alpha/Skia/altre categorie/consumer/distribuzione), accessibilità completa e native JupyterSharp non certificati. Nessuna release.

### Verifica mirata precedente — selezione bande contourf

ContourBands delegato da DrawFilled: indici/bordi inclusivi e duplicati conservati, esterni/non-finiti senza banda, singleton senza bande. Snapshot livelli finiti non decrescenti, normalizzazione midpoint con ordine originale e overflow non riassociato. Palette/alpha/bitmap/Skia restano ND.
Build/suite shared227/227, ND19/19 verdi; kernel source ricostruito, metadata ContourBands verificato e notebook linee/bande320×240 ricontrollato. Zero file immagine di revisione; warning kernel noti visibili. Non full gate/publicazione, D03 parziale e baseline completa31 precedente da riconfermare.

### Verifica precedente mirata — sampler dei contour riempiti NDSharp

Condivisa solo Sample fisica in RectilinearFieldSampler: binary search e quattro termini invariati, 81 query prima della modifica congelate e tutte uguali. Snapshot assi/dati, assi crescenti/finite min2×2, shape coerente/query finite; dati non finiti propagano. Mutazione durante draw e input invalidi non garantiti. Line-contour discendenti separati non modificati; bande/palette/alpha/Skia locali.
Build e suite shared211/211, NDSharp.Plot19/19 passate. Kernel source-plots ricostruito, metadata sampler nuovo verificato e notebook reale linee/bande produce due PNG320×240 incorporati; niente review file/screenshot. Warning kernel preesistenti restano. Non nuovo full gate o release; baseline31 precedente da riconfermare.

### Baseline completa precedente — contour NDSharp nel kernel reale

Gate --no-images D:\Dev\BuildArtifacts\ecosystem-validation\20261028-203820: 31/31 passi exit0 dopo nuovo notebook e profilo source. Build kernel predefinito a pacchetti e variante source separata, metadata nuove DLL verificati, notebook realmente eseguito con JupyterNet/PySharp su .NET11. Linee e bande: due output PNG320×240 incorporati, non file di review/screenshot. Report recente archiviato insieme agli altri JSON/log.
Prima il binding usava ancora pacchetto NDSharp.Plot precedente: introdotto ScienceUseSourcePlots opt-in, default compatibile e kernel-source-plots distinto da kernel. Notebook sotto NDSharp/pysharp perché Ecosystem è riservato ai piloti JupyterSharp; test non indebolito. Due nuovi test, Core100/100 verificato separatamente e nel gate; warning PySharp preesistenti non soppressi.
Questa baseline prevale sulle note non-full-gate precedenti. D03 resta parziale (contourf/stili/Skia/altri consumer/distribuzione), native JupyterSharp e pixel-oracle CPython non dimostrati. Nessuna pubblicazione.

### Verifica mirata precedente — migrazione contour-line NDSharp

Geometria di ContourArtist.DrawLines effettivamente delegata a PlotSharp.RectilinearContours e kernel comune. Convenzioni distinte preservate: ND usa coordinate fisiche/nonuniformi, edge top/left forward, segmenti nulli e skip somma corner non finita; core a indici mantiene propria convenzione/default/mask/bounds. Livelli, Skia/paint/Axes e contourf ND non modificati.
Nuovo riferimento NDSharp.Plot→PlotSharp; build e18/18 test ND passati, shared204/204 passati con geometria confrontata esattamente con aritmetica pre-migrazione. Nessuna immagine. Report hashes/deps aggiornato con geometry delegated/family incomplete. Non nuovo full gate, notebook end-to-end o pubblicazione; D03 ora parziale, baseline completa precedente distinta.

### Verifica precedente mirata — inventario NDSharp.Plot

Sorgente attiva NDSharp/src/NDSharp.Plot ispezionata, non snapshot: net11.0, NDSharp + Skia/native3.119.0, Figure/Axes/Artists e font DejaVu. Report hashes/dipendenze in ops/inventory/ndsharp-plot-contract.json. Candidata la sola geometria contour lineare, che usa Xs/Ys fisici/nonuniformi, skip celle non finite e suppression dei livelli estremi; non conversione globale di indici PlotSharp.
Quattro baseline test aggiunti senza renderer changes, PNG solo in memoria. Build e suite NDSharp.Plot18/18 passate, inventario tool compilato/eseguito. Contourf/AutoLevels/Ticks/stili/Skia/raster devono restare separati fino a verifica. Nessuna migrazione/dipendenza PlotSharp/immagine/pubblicazione, non nuovo full gate.

### Baseline completa precedente — dopo consolidamento FieldPlot

ops/ValidateEcosystem.cs -- --no-images, D:\Dev\BuildArtifacts\ecosystem-validation\20261028-185732: 29/29 passi exit 0. Tutte le verifiche di soluzione, audit, notebook/piloti, CLI/Web, UI calcolatori/mobile, cancellazione quadro/fischio, selftest numerico originale completo, raster in memoria e browser rieseguite dopo le estrazioni. Nessun skip dei controlli, test sospesi sempre dichiarati nelle suite/TRX.
Renderer FieldPlot ora source-shared con facade Navier; fixture strutturali/pixel e suite PlotSharp198 casi riconfermate nel gate. Output notebook aggiornati; nessuna nuova immagine di revisione/screenshot. Tre cartelle *no-images contengono solo JSON; sette report recenti/politica immagini verificati e archiviati con log.
Questa baseline prevale sulle note precedenti di gate non rieseguito. Non chiude tutte le famiglie, consumer notebook Navier/originali, accessibilità completa, licenze o distribuzione. Nessun pacchetto pubblicato/sovrascritto.

### Verifica mirata precedente — assemblaggio FieldPlot condiviso

Charts.NodeFieldPlot compone header/stili e componenti consolidati; Navier mantiene FieldPlotOptions/firma tramite facade e rimuove helper privati inutilizzati. Numerica/scenario/UI non spostati; Heatmap a celle distinta. Quattro output di produzione congelati prima: markup strutturale e hash pixel RGB coincidono (solo GUID e PNG nel confronto normalizzati).
Nuova validation overlay same-shape, contour0–32, ostacoli finiti/raggio≥0, incompatibilità input fuori contratto dichiarate. Build/suite PlotSharp198/198 passate; Web Release/quadro/smoke --no-images ricontrollati. Zero immagini, non nuovo full gate o release. D02 richiede ancora verifica complessiva/consumer notebook/licenze; baseline full29 precedente da riconfermare.

### Verifica precedente mirata — frame/proiezione nodi

NodeFieldLayout condiviso per proporzioni/margini, colorbar position e contour X/Y. Math.Round altezza to-even, origine bottom e indici frazionari preservati, non cell-center; extrapolazioni finite non clippate. Create valida shape min2×2/max100k, larghezza/altezza1–10000 e collassi prima del raster.
Build/suite PlotSharp 192/192 passate, fixture precedenti e nuovi corner/rounding/proporzioni; Web Release/quadro/smoke --no-images ricontrollati, zero immagini. Assemblaggio/header/stili FieldPlot restano locali. Non nuovo full gate, benchmark o pubblicazione; baseline completa precedente da riconfermare.

### Verifica precedente mirata — colorbar a gradiente

Frammento colorbar Navier condiviso in GradientColorbar: campioni k/10, 3 tick, rect/gradiente/anchor/escaping preservati, inclusa metà altezza intera legacy. Tre markup di produzione frozen prima della migrazione coincidono, normalizzati solo GUID. Default Heatmap a strisce invariato; input validati (limiti/proiezione finiti, colori #RRGGBB).
Build/suite PlotSharp 178/178 passate, Web Release/quadro/smoke --no-images ricontrollati. Zero immagini e nessun full gate/pubblicazione. FieldPlot posizionamento/frame e proiezione contour ancora locali, D02 parziale. Baseline full29 precedente alle ultime estrazioni da riconfermare.

### Verifica precedente mirata — geometria frecce Navier

Geometria vettoriale NodeVectorOverlay condivisa con normalizzazione globale, stride/soglia/proiezione nodi e teste aperte legacy. Quiver Heatmap precedente invariato; stile e frame locali. Quattro path FieldPlot frozen prima della migrazione coincidono. Nuova validation finita/max100k/shapes/stride/proiezione esplicita, input prima non validati non garantiti.
Build e suite PlotSharp 168/168 passate: quattro direzioni, massimo non campionato, zero/soglia, input immutati e report pressione reale. Web Release/quadro e smoke --no-images ricontrollati, zero immagini. Non nuovo full gate o benchmark, nessuna pubblicazione. Proiezione contour/rendering colorbar e D02 completa restano aperti; baseline full29 precedente da riconfermare.

### Verifica precedente mirata — limiti colore dei campi

Min/max, floor-index percentili, simmetria e fallback legacy min+1 condivisi in FieldColorLimits.Resolve; ColorScale default costante invariato. Otto fixture dai label FieldPlot originale congelate prima della migrazione. NaN omessi; tutto NaN con limiti espliciti supportato. Resolve ora rifiuta input fuori contratto (vuoti/max100k, infinito/±1e12, clip [0,0.5], limiti invertiti); comportamento input prima non validati non garantito. Normalizzazione diretta legacy conserva aritmetica dei chiamanti.
Build e suite PlotSharp 154/154 passate, inclusi zero/simmetria, clipping, quasi-costanti, NaN e bordo espanso. Web Release/quadro/smoke adozione --no-images ricontrollati, zero immagini. Non nuovo full gate/pubblicazione; layout/proiezione/render colorbar/frecce locali. Baseline completa a 29 passi precedente alle ultime estrazioni, da riconfermare.

### Verifica precedente mirata — campionamento bilineare RGB

Sampler di nodi condiviso in PlotSharp, facade Navier FieldToPng conserva normalizzazione/ColorMap e maschera grigia. Centri pixel, orientamento top-down e ordine bilineare originali preservati; campi min 2×2 espliciti, callback gestisce valori non finiti, maschere in coordinate campione con colore richiesto.
Build e suite PlotSharp 138/138 passate, inclusi fixture PNG frozen (scanline/dimensioni sempre, hash compressi sulla piattaforma registrata) e casi del sampler. Web Release e quadro smoke ricontrollati, nessuna immagine o benchmark completo. Normalizzazione/proiezione/colorbar/frecce restano locali, D02 parziale.
Non nuovo full gate o release; baseline completa a 29 passi è precedente alle ultime estrazioni e va riconfermata.

### Verifica precedente mirata — encoder RGB PNG

Solo encoding RGB8/filter-0 condiviso in PlotSharp: Navier conserva campionamento/masking e firma FieldToPng, delega chunk/CRC/ZLib e usa dimensionamento buffer anti-overflow. Tre fixture pre-encoder congelate come raw hex/hash, non immagini. Test confrontano sempre scanline/dimensioni/CRC, hash compressi sul runtime/OS registrato; input invalidi e concorrenza verificati. Non nuovo backend raster completo.
Build e suite PlotSharp 127/127 passate; Web Release costruita, quadro con due cicli e smoke adozione --no-images exit 0. Nessuna immagine di revisione o benchmark completo. La baseline a 29 passi è precedente a contour/palette/encoder e non riconfermata su tutte queste ultime modifiche. Proiezione, campionamento, normalizzazione/colorbar/frecce restano locali; nessuna pubblicazione.

### Verifica precedente mirata — palette RGB condivise

ColorPalette immutabile centralizza RGB con preset 4/8/9 stop; default PlotSharp 4 e Navier 8/9 restano distinti. Spacing/troncamento storici conservati, facade ColorMap mantiene NaN/infinito legacy, API condivisa strict sui non finiti. PNG/normalizzazione/colorbar/proiezione/frecce non migrate.
Prima della modifica: 323 input e hash dei due sorgenti congelati in ops/compatibility/palette-baseline.json, inclusi adiacenze agli stop e campioni colorbar. Estensione consentita solo prima della modifica con hash corrispondenti; fixture copiata nei test, non rigenerata. Tutte e tre le palette coincidono sui byte attesi.
Build e suite PlotSharp 117/117 passate; Web Release costruita e quadro smoke exit 0 con due cicli. Nessuna immagine, nuovo full gate o pubblicazione. Baseline completa a 29 passi precedente a contour/palette, da riconfermare prima di attribuirla all'intera working tree.

### Verifica precedente mirata — geometria contour NavierStokes

Consolidata solo MarchingSquares: facade tuple Navier → core PlotSharp con modalità di compatibilità per segmenti nulli ai vertici. Coordinate, ordine delle righe e pairing fisso a sella conservati; default core precedente invariato. Limiti espliciti del core (campo non vuoto, max100000 campioni, finiti/livello ±1e12) applicati, senza promessa di compatibilità per input unbounded. Segmenti bufferizzati per livello.
Build PlotSharp e suite 109/109 riuscite: 16 configurazioni binarie confrontate con geometria attesa, uguaglianza, coordinate rettangolari, limiti e path SVG esatto. Web Release costruita, --quadro ricontrollato con due cicli, exit 0. Nessuna immagine e nessun full gate rieseguito dopo questa modifica al kernel grafico.
PNG, FieldPlot/proiezione, colorbar e frecce restano locali; D02 parziale e nessuna pubblicazione. La baseline completa a 29 passi sotto è precedente e va riconfermata prima di attribuirla a queste ultime modifiche.

### Verifica completa precedente — consolidamento senza nuove immagini

`ops/ValidateEcosystem.cs -- --no-images`, report D:\Dev\BuildArtifacts\ecosystem-validation\20261028-150530: 29/29 passi exit 0 dopo tutte le modifiche. Build/test, audit, notebook, NuGet pilot, CLI/Web, UI calcolatori/mobile, cancellazione quadro/fischio e selftest numerico completo rieseguiti. Nessuna verifica saltata o fallimento ignorato; test sospesi ancora registrati nelle suite/TRX.
La modalità impedisce salvataggio screenshot e SVG/PNG nei tre tool visuali, ma mantiene rasterizzazione/encoding PNG in memoria, XML/DOM/check browser reali. Solo JSON nelle cartelle *no-images; notebook output aggiornati, vecchie immagini preservate e non dichiarate rigenerate. Eventuali immagini temporanee necessarie ai test non sono soppresse.
Sei test di politica immagini e dieci di report temporali aggiunti: suite operational 66/66 verificata separatamente. Il gate rifiuta report mancanti, obsoleti/futuri, malformati o con SaveImages incoerente e archivia sette JSON recenti insieme ai log. Default legacy dei singoli tool preservato; uso consigliato con --no-images.
Questa baseline completa prevale sulle note mirate precedenti. Non chiude distribuzione, renderer di campo, altre famiglie o accessibilità completa; nessun pacchetto pubblicato/sovrascritto e nessuna nuova revisione visiva manuale.

### Verifica precedente mirata — contrasto testi di supporto/accento

Misure CSS reali in Edge per campioni HTML diretti su sfondi solidi: accenti prima 4.05–4.30:1, testi attenuati sotto 4.5. Correzione di soli --accent e --ink-3 Web; serie SVG e algoritmi non modificati. Dopo build Release Web, --mobile exit 0 con 17 campioni home/tubo/vento, min circa 4.97:1. Layout, controllo tastiera e calcoli continuano a passare; desktop ricontrollato, zero immagini.
Sette nuovi casi del calcolo sRGB di contrasto: build riuscita e suite operational 50/50. Report/log in ops/validation/navier-mobile-smoke. Testi selezionati soltanto, non audit completo WCAG o SVG/stati/telefoni; sfondi con immagini/gradienti e opacità di gruppo sono fuori supporto e generano segnalazione. Gate futuro 29 passi non rieseguito completamente, baseline completa precedente distinta.

### Verifica precedente mirata — dimensione controlli e tastiera

Misura iniziale min 16 px negli input/select tubo/vento. Correzione CSS della Web NavierStokes sotto 600 px: altezza minima 44 px e focus visibile. Build Release riuscita; --mobile exit 0 a 360×800: min altezza 44 px, no overflow documento, testi SVG min 11 px; freccia destra nativa ha aggiornato vento 180→181 km/h, forza 1926→1948 N, indicatore di focus presente. Desktop ricontrollato, exit 0. Zero immagini e nessuna simulazione.
Otto nuovi casi per altezza minima: build riuscita, suite operational 43/43. Report/log in ops/validation/navier-mobile-smoke. Non certifica touch/hit-testing reale, contrasto, completa accessibilità o simulazioni mobile. Gate futuro 29 passi non rieseguito completamente, baseline completa precedente distinta.

### Verifica precedente mirata — dimensione testi SVG e scorrimento

Misura iniziale dei grafici tubo/vento a 360×800: min 5.85 px effettivi. Correzione locale nella Web NavierStokes: sotto 600 px SVG nativi/min 560 px in contenitore scrollabile, grid con min-width sicuro, Chart focusabile con nome e focus visibile. Core PlotSharp e dati non modificati.
Build Release Web riuscita; controllo mobile exit 0 con min 11 px, nessun overflow documento, tastiera freccia destra e bordo finale dei tre grafici raggiunti. Desktop tubo/vento ricontrollato, exit 0. Zero immagini. Otto nuovi casi del criterio di dimensione testo: build riuscita e suite operational 35/35 passata. Report/log in ops/validation/navier-mobile-smoke.
Soglia test di 10 px non equivale a leggibilità/accessibilità completa, contrasto, screen reader o touch; simulazioni mobile escluse. Gate futuro 29 passi invariato e full gate non rieseguito; baseline completa precedente resta distinta.

### Verifica precedente mirata — viewport stretto, senza immagini

ops/VerifyNavierWeb.cs -- --mobile exit 0, Kestrel+Edge a 360×800: home/tubo/vento, documento 360 px senza overflow e controlli/pannelli/grafici entro i limiti orizzontali. Cinque controlli tubo e tre vento, aggiornamenti numerici verificati. Nessuna modifica CSS necessaria e zero immagini. Desktop tubo/vento ricontrollato, exit 0.
Validatore dei limiti con otto nuovi casi xUnit (inclusa tolleranza 1 px e overflow intero): build riuscita, suite operational 27/27 passata. Report/log in ops/validation/navier-mobile-smoke. Gate futuro 29 passi compilato, non nuovo full gate eseguito; baseline completa precedente a 25 resta distinta. Non certifica telefoni reali, touch, testi dei grafici, simulazioni mobile o deployment.

### Verifica precedente mirata — cancellazione fischio, senza immagini

ops/VerifyNavierWeb.cs -- --fischio exit 0: Kestrel/Edge, griglia LBM minima 140×50, due cicli con progresso osservato, stop e comandi ripristinati. Modalità --quadro ricontrollata dopo la condivisione della logica, exit 0. Zero immagini generate, report/log separati in ops/validation/navier-fischio-smoke.
Due test unit di cancellazione/ripresa CylinderLbm aggiunti: build riuscita e PlotSharp 98/98 test passati. Nessuna modifica agli algoritmi o ai renderer. Il gate prevede ora 28 passi, compilazione verificata ma full gate non rieseguito (baseline completa precedente a 25 resta distinta). Periodicità/Strouhal, convergenza/accuratezza, mobile, popup nativi e deployment non certificati.

### Verifica precedente mirata — cancellazione quadro

ops/VerifyNavierWeb.cs -- --quadro exit 0: Kestrel/Edge reali, griglia 31, due cicli con progresso osservato, parametri disabilitati durante l'esecuzione, stop e comandi ripristinati. Nessuna immagine generata; log/report in ops/validation/navier-quadro-smoke. Parser/calcolatori precedente tubo/vento rieseguito, exit 0. Due test di cancellazione core aggiunti: PlotSharp 96/96 con build riuscita.
Tool integrato nel gate, ora 27 passi previsti; full gate non rieseguito in questa tranche (ultima baseline completa resta a 25). Nessuna prova di convergenza/accuratezza della simulazione, fischio, mobile o deploy. Il server temporaneo è chiuso dal tool.

### Verifica precedente mirata — Web NavierStokes, senza immagini

Build Release Web riuscita, suite Ecosystem.Validation.Tests 19/19 (13 nuovi casi parser UI) passata; ops/VerifyNavierWeb.cs exit 0. Server Kestrel reale su loopback ed Edge: navigazione home/tubo/vento, range input e aggiornamento Blazor, SVG condivisi e assenza di errori. Osservati tramite DOM: pressione 0.42 → 0.84 bar; forza 963 → 3853 → 1926 N per velocità raddoppiata e area dimezzata, entro le tolleranze dei display arrotondati.
Nessuna immagine generata. Report/log in ops/validation/navier-web-smoke; server/browser chiusi dal tool. Un eventuale --screenshot produce una sola immagine. Tool aggiunto al gate: 26 passi previsti, **non rieseguito completamente in questa tranche**; baseline completa precedente a 25 passi resta distinta. Questa prova non copre quadro/fischio, mobile, popup nativi, contenuti editoriali o deployment.

### Ultima verifica completa precedente — nuovo consumatore NavierStokes, 2026-10-28

Gate completo dopo le modifiche: `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-104324`, 25/25 passi exit 0. PlotSharp 94/94 test passati separatamente (sette nuovi casi NavierStokes); CLI/Web NavierStokes costruite e selftest numerico originale completo passato. Tre report lineari su fixture ridotte verificati in Edge e screenshot osservati; fixture smoke non sono una prova di accuratezza di regime.

NavierStokes.Core → PlotSharp per line/scatter tramite facade con API preservata. Campo/PNG/contour e utility locali restano invariati e non migrati. Nuova validazione rigorosa, clip/layout differenze dichiarate e report Ghia più largo; UI Web e notebook dell'articolo ancora da verificare E2E. D02 parziale, non completata.

Inventario app MathemaSharp: 2 file Lab e 11 Docs .cs/.razor senza renderer SVG locale; non sono state aggiunte dipendenze fittizie. Lab console numerica e Docs viewer Markdown/asset. Scan lessicale, non certificazione di ogni asset. D01 resta parziale per inventario/API/asset, altre famiglie e distribuzione restano aperte. Nessun pacchetto pubblicato o sovrascritto.

### Verifica precedente — scenari e dashboard, 2026-10-28

Gate dopo tutte le modifiche: `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-101452`, 21/21 passi exit 0. Core MathemaSharp: 98/98 verificati separatamente, poi riconfermati nel gate; concordanza numerica degli scenari A/D C#/PySharp protetta dai test esistenti.

Migrati A1 magazzino e D1 fotovoltaico: 3/2 SVG e un gruppo KPI/due risposte ciascuno. Otto notebook, 43 SVG eseguiti e XML validati. Il manifest dichiara gap richiesto per magazzino, conteggi KPI/quote; runner e test li controllano. Presentation.csx aggiunge classi semantiche senza cambiare stili/testi o introdurre dipendenze host in PlotSharp.

Edge: 43 grafici individuali e due dashboard di output HTML assemblati a larghezza desktop 1280 px, cinque tile/due risposte non vuote ciascuna e nessun overflow orizzontale. Due screenshot completi osservati. Non include markdown, celle, UI VS Code, layout mobile o popup tooltip OS; target sovrapposti mantengono il limite dichiarato. D01 resta parziale per app/altri percorsi, altre famiglie ancora aperte. Nessuna preview pubblicata o sovrascritta.

### Verifica precedente — STL/tour e manifest, 2026-10-28

Gate dopo tutte le modifiche: `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-075021`, 19/19 passi exit 0. Core MathemaSharp verificato separatamente: 96/96. PlotSharp non modificato in questa tranche, suite confermata nel gate. Test di concordanza del tour C#/PySharp attivi e verdi.

Migrati TimeSeries/09 (STL/NDSharp, 6 SVG e tabelle preservate) e 10 (tour, 5 SVG); sei notebook applicativi, 38 SVG eseguiti realmente in JupyterNet e validati XML. Manifest condiviso esteso con requireTable; il gate genera automaticamente i passi migrati e runner/test verificano SVG e tabelle richieste. Browser e rasterizer ricavano lo stesso perimetro dal manifest. Edge ha verificato 38 grafici; osservati gli 11 nuovi screenshot, pannelli/outlier/legende leggibili.

D01 rimane parziale: scenari, app e altri consumatori non tutti migrati. Nessuna equivalenza completa del notebook intero/VS Code o garanzia di hit target individuali nei grafici densi; popup OS e altre famiglie aperti. Source-build locale, nessun pacchetto preview pubblicato o sovrascritto.

### Verifica precedente — browser e revisione, 2026-10-28

Gate dopo tutte le modifiche: `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-065717`, 17/17 passi exit 0. PlotSharp 87/87 test, MathemaSharp Core 94/94, verificati separatamente. Nuovo test sul report browser salvato, distinto dall'esecuzione reale.

Il rasterizer segue tutto il manifest (27 PNG). Osservati i 19 nuovi raster 06/08; Edge Chromium ha rilevato tre collisioni titolo/legenda su 27 SVG. Corrette tramite SeparateLegendRow opt-in, attivo in IndexedCharts; aggiunti tooltip alle aste. Rerun Edge: 27/27 con testi nel viewport SVG, nessuna collisione titolo/testi, almeno un title hit target raggiungibile per grafico. Tre screenshot browser dei casi corretti osservati. Screenshot/report in ops/visual/browser, browser nel gate, richiede Edge installato.

Raggiungibilità individuale di tutti i target sovrapposti, popup nativi OS, renderer VS Code e altri browser non verificati. Le verifiche DOM non sostituiscono una review visiva esaustiva dell'intera UI. Distribuzione, altre famiglie e completamento D01 rimangono aperti; nessuna preview pubblicata o sovrascritta.

### Tranche precedente — marker e manifest, 2026-10-28

Questo riepilogo prevale sui conteggi storici più avanti. Gate completo: `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-061520`, 16/16 passi exit 0, dopo tutte le modifiche. PlotSharp verificato separatamente: 83/83 test; MathemaSharp Core: 93/93 test. Test sospesi PySharp ancora registrati nel TRX, non trasformati in passati.

Quattro notebook applicativi source-build migrati: Core/03 e TimeSeries/06,07,08. Rispettivamente 2/6/6/13 SVG eseguiti nel vero host JupyterNet e validati XML (27 totali); tabelle preservate. Runner e test leggono `ops/migrated-notebooks.json`; smoothing mantiene la verifica del gap. Piloti JupyterSharp su pacchetti restano separati.

Clipping dei marker campione corretto tramite PreserveBoundaryMarkers opt-in: overlay solo per centri finiti nel dominio chiuso, clip geometrico/scale invariati; default core legacy preservato. IndexedCharts abilita la correzione. Due spettri rigenerati e osservati mostrano marker interi a zero e ai massimi. Highlight non modificati. Review 06/08 e browser ancora aperti; niente promesse di interazione tooltip o backend PNG.

D01 MathemaSharp resta parziale: quattro notebook non equivalgono a una famiglia completamente migrata. Nuove API solo nei sorgenti locali, nessun package preview sovrascritto o pubblicato. I progetti operativi Ecosystem.Validation restano net10.0; librerie scientifiche esistenti net11.0.

### Completato e verificato

- [x] Target net11.0 per i progetti attivi dei sei repository; SDK .NET 11 RC3, non GA. Snapshot importati esclusi.
- [x] Kernel MathemaSharp consolidati e validati su casi multiformato e oracoli indipendenti: Core, LinearAlgebra, TimeSeries e Quantum.
- [x] NDArray<T>: storage typed, viste, slicing, broadcasting Add, Sum, Dot e MatMul; ponte LU/QR/diagnostica senza double intermedio.
- [x] PySharp: costruttori Decimal32/64/128, operatori CLR, binding esplicito dei metodi generici, recupero dei notebook gemelli e compatibilità FastAPI/AnyIO nei test.
- [x] PySharp: chiusura esplicita dei tipi CLR generici (esempio host-injected List<Decimal128>), vincoli e arità verificati. Non equivale a inferenza, import di tutte le famiglie generiche o NumPy typed.
- [x] MatSharp: scalari e matrici reali typed 2D column-major, indicizzazione, copie, trasposta, concatenazione e prodotti.
- [x] MatSharp: A \\ b typed tramite adattatore opt-in: LU su quadrate, QR su sovradeterminate, più colonne RHS; test sui sei formati e conservazione di 1e-25 in Decimal128. Sistemi sottodeterminati esplicitamente rifiutati.
- [x] Pilota JupyterSharp reale → MathemaSharp FFT → PlotSharp SVG, eseguito tramite pacchetti NuGet: notebooks/Ecosystem/01-signal-spectrum.ipynb.
- [x] Pacchetti preview generati nel feed comune senza sovrascrittura; consumer isolato verificato anche sulle ultime API PySharp e sul solver MatSharp.
- [x] Difetto Matplotlib: center_baseline dei tick Y corretto; doppio arrotondamento eliminato nello snippet, golden CPython invariato e test non arrotondato aggiunto.
- [x] Audit NuGet delle sei soluzioni e gate ripetibile con exit code non zero su finding/fallimenti. Nessuna soppressione dei problemi.

### Verifiche: distinguere baseline e ultime modifiche

Baseline storica completa verde: ops/ValidateEcosystem.cs, report in D:\Dev\BuildArtifacts\ecosystem-validation\20261027-223603. 2624 test passati, zero falliti, 13 sospesi PySharp; 24 notebook della baseline, percorsi PySharp/MatSharp e pilota NuGet verificati.

Aggiornamento 2026-10-28: gate completo rieseguito dopo le modifiche, exit 0 per tutti i nove passi. Report: D:\Dev\BuildArtifacts\ecosystem-validation\20261028-005251. Include build/test delle sei soluzioni, audit, baseline, kernel/notebook PySharp, notebook MatSharp e tre piloti. Core.Tests verificato anche separatamente: 86/86 passati. Non riutilizzare il precedente totale 2624 come conteggio della nuova esecuzione; i conteggi dettagliati sono nei log delle suite.

Il secondo notebook Ecosystem è ora eseguito e verificato nel gate, con marker di conservazione della precisione Python/MATLAB. Il terzo pilota `03-indexed-charts.ipynb` carica una facade opt-in condivisa: Lines/Panels/Stems delegano a PlotSharp, quattro SVG validati XML. Output precedenti cancellati dal runner prima dell'esecuzione. Test xUnit aggiunto sugli output salvati, distinto dall'esecuzione reale. Renderer legacy conservato: nessuna equivalenza dichiarata per gap non finiti, layout/tick/tooltip o verifica visiva. Migrazione completa dei consumatori ancora aperta.

Ultima verifica completa dopo la tranche successiva: `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-010433`, 12/12 passi exit 0. Prima migrazione applicativa: Core/03 usa il bootstrap PlotSharp, due SVG eseguiti in JupyterNet e validati XML, output precedenti cancellati. Un test dedicato protegge bootstrap/output. Soluzione MathemaSharp verificata anche separatamente: 320 test passati (87 Core e 6 nuovi test del parser TRX inclusi), zero falliti/sospesi. PySharp: 1507 passati, 13 sospesi; report rigenerabile e conservato nel gate. Dei 13 sospesi, 12 richiedono PostgreSQL raggiungibile e una theory richiede download pretrained opt-in. Non dichiarare completata la migrazione delle famiglie né la verifica visiva.

### P0 — chiudere la tranche corrente

- [x] Eseguire notebooks/Ecosystem/02-typed-language-interop.ipynb e verificare riferimenti/output.
- [x] Secondo e terzo pilota nel gate; test sugli output salvati distinti dall'esecuzione reale.
- [ ] Completare inventario API; Usage contiene già tipi generici PySharp e solver typed MatSharp.
- [x] Rieseguire il gate completo dopo queste modifiche: tutti i dodici passi exit 0.
- [x] Registro delle motivazioni dei test sospesi, rigenerato da TRX fresco in ogni gate; nessun test sospeso eliminato o contato come passato.
- [ ] Verificare coerenza delle versioni degli assembly/kernel rispetto ai pacchetti, senza riutilizzare output obsoleti.

### P1 — completare interoperabilità e Generic Math

- [ ] PySharp: import/selezione delle famiglie di tipi generici, inferenza controllata, array typed e integrazione NDArray<T>.
- [ ] PySharp: strategia esplicita per Span/byref, conversioni numeriche e NumPy typed; niente promesse di marshalling universale.
- [ ] MatSharp: complessi typed, array N-D, literal interi grandi esatti; normalizzare il nome GenericScalar<T>, ora usato anche per matrici.
- [ ] MatSharp: sistemi sottodeterminati/minimum-norm e ulteriori contratti del solver; mantenere distinta la divisione matriciale da quella elemento per elemento.
- [ ] NDSharp: coabitazione runtime/typed, conversioni checked, complessi, prodotti batched e adattatori agli algoritmi condivisi.
- [ ] FFT/eig, wavelet e distribuzioni importate: migrazione per famiglie e test, evitando duplicazione dei kernel in NDSharp.
- [ ] Python AST: completare nodi, visitor, validazione e semantica di compile/exec/eval nei limiti dichiarati.

### P2 — visualizzazione ed esecuzione condivise

- [ ] Migrare i renderer duplicati a PlotSharp per categorie, conservando compatibilità e test prima di rimuovere il codice precedente.
- [ ] Adattatori Decimal: normalizzazione nel tipo prima della proiezione grafica; il primo pilota usa double intenzionalmente.
- [ ] Verificare plugin PySharp e MatSharp nativi in JupyterSharp. I notebook C# di embedding non costituiscono questa prova.
- [ ] Aggiornare loader e notebook applicativi alla distribuzione tramite pacchetti, senza dipendenze grafiche nei kernel matematici.

### P3 — copertura numerica e documentazione

- [ ] Inventario API-per-API di constraint, tipi verificati, precisione e conversioni residue.
- [ ] Contratti: dominio, overflow, tolleranze, mutabilità/copie/viste, rappresentabilità e costo della diagnostica.
- [ ] Estendere oracoli a Half/Decimal32 e ulteriori matrici/segnali/code; non confondere residuo e accuratezza della soluzione.
- [ ] Completare Usage/Reference per libreria e rieseguire i notebook non inclusi nella selezione corrente.
- [ ] Portale: formule, CSS compilato, logo MàthemaSharp, accessibilità, responsive e stampa verificati nel browser.

### P4 — rilascio e operatività

- [ ] CI riproducibile, politica RC/GA, metadati/licenze e compatibilità API dei pacchetti.
- [ ] Rendere manifest-driven i runner e il versionamento: gli script correnti hanno ancora versioni e percorsi fissati.
- [ ] Audit di progetti fuori soluzione, tool e vecchi pacchetti; sicurezza applicativa/runtime distinta dall’audit NuGet.
- [ ] Documentare aggiornamento e ritiro delle vecchie preview, senza cancellazioni o sovrascritture indiscriminate.

### P5 — performance, esplicitamente dopo

- [ ] Riduzione delle allocazioni Bluestein, workspace, caching, SIMD e parallelismo solo dopo la stabilizzazione funzionale.
- [ ] Benchmark out-of-process attendibili, confronti per formato, memoria e costo diagnostica/Solve.
- [ ] BLAS/GPU soltanto come obiettivi separati, senza garanzie universali di velocità.

## Quadro precedente — storico, non stato corrente

Le tabelle e checkbox seguenti conservano gli stati delle tranche precedenti e possono contenere voci oggi superate. Fare riferimento esclusivamente alla roadmap corrente sopra per priorità e completamento.

### Stato delle librerie

| Libreria/progetto | Stato verificato | Limiti e lavori aperti |
|---|---|---|
| MathemaSharp.Core | Generic Math, precisione, DFT e FFT/IFFT radix-2/Bluestein; notebook e oracoli FFT | FFT solo nei sorgenti, non nei pacchetti 0.2.0; ampliare oracoli e contratti |
| MathemaSharp.LinearAlgebra | Matrici, norme, QR/minimi quadrati, LU, inversa e diagnostica; test multiformato e Hilbert | Nessuna SVD/Cholesky generale o autovalori generali; stima condizionamento non certificata, nessun refinement |
| MathemaSharp.TimeSeries | Kernel generici consolidati, forecasting e statistiche; notebook C# selezionati eseguiti | Lanczos non garantisce 34 cifre; gaussiani default da double; scenari e confronti PySharp non chiusi |
| MathemaSharp.Quantum | Complex<T>, norma, normalizzazione, autostati, hermitiano 2x2, MatrixExp; oracoli indipendenti | Taylor/scaling-squaring non garantito su ogni matrice; estendere casi estremi e dimensionalità |
| MathemaSharp.NDSharp | Ponte runtime migliorato e typed verso LU/QR/diagnostica; 16 test anche via pacchetto 0.2.0 | Percorso runtime verso NDArray ancora float64; tipi runtime e generici separati; dipendenza locale al repository fratello nei sorgenti |
| NDSharp | NDArray<T>: storage, viste, slicing, broadcasting Add, Sum, Dot, MatMul; suite completa 563 test all'ultima verifica | Non è una conversione completa: FFT/eig/wavelet/distribuzioni e API runtime non migrate; niente MatMul batched; target net10.0 |
| PlotSharp | Ultima verifica: build riuscita, 70 test; libreria, Gallery, Inventory presenti | Rivalutare stato della sessione dedicata; integrazione MathemaSharp/JupyterSharp e migrazione dei grafici non verificata qui |
| MatSharp | Matrici double e typed reali 2D column-major, NDSharp 2.8.0 e ponte LinearAlgebra; GenericScalar<T> ora include storage matriciale, concatenazione, broadcast, prodotti, trasposta e assegnazione; build senza warning e 130 test passati; notebook Decimal128 eseguito | Solver typed, complessi, N-D, literal esatti grandi e packaging aperti; Plot resta NDSharp.Plot; notebook C# di embedding, non kernel MATLAB. Nome GenericScalar<T> da normalizzare perché ora contiene anche matrici |
| JupyterSharp | Ultima verifica: build riuscita, 44 test, 60 warning | Classificare warning; il runner MathemaSharp usa ancora il tool JupyterNet: non prova integrazione con JupyterSharp |
| Portale Docs | Blazor Server interattivo, Markdown, navigazione e CSS; test esistenti | Tailwind CDN, formule, accessibilità e verifica visuale da completare |

Stato aggiornato dopo la migrazione: tutti i progetti attivi dei sei repository target net11.0 (SDK RC3). MathemaSharp: 310 test passati, nessun sospeso; baseline 24 notebook, più percorsi PySharp e embedding MatSharp rieseguiti. Pacchetti MathemaSharp 0.2.1-net11-preview.1 includono le FFT; NDSharp 2.8.1-net11-preview.1. JupyterSharp 0.3.1-net11-preview.2; PySharp/Pyodbc 2.5.1-net11-preview.2 correggono dipendenze crittografiche. PySharp: 1503 passati, 1 fallito (layout Matplotlib), 13 sospesi. Audit NuGet delle sei soluzioni senza vulnerabilità rilevate dalle fonti correnti. Questo riepilogo e NET11-MIGRATION.md prevalgono sui limiti/versioni storici riportati nella tabella.

### Open points — priorità prima delle performance

- [ ] Aggiornare inventario API/constraint e copertura nativa per tipo dopo le nuove API typed e FFT; evitare dichiarazioni globali di supporto.
- [ ] Contratti API-per-API: precisione, overflow, dominio, mutabilità/copie/viste, tolleranze e costi di memoria.
- [x] Pacchetti preview .NET 11 generati nel feed comune senza sovrascrittura; FFT/IFFT e ponte verificati in consumer NuGet isolato.
- [x] Audit NuGet ripetibile delle sei soluzioni, con exit code non zero su finding/errori: `dotnet run --file ops/AuditSciencePackages.cs`.
- [ ] Correggere il layout Matplotlib P5_layout, senza alterare il golden confermato da CPython.
- [ ] Ampliare audit a progetti fuori soluzione e consumer dei pacchetti tool; le preview precedenti nel feed restano storiche.
- [ ] Metadati e licenze dei pacchetti MathemaSharp; eliminare descrizioni obsolete Numbers, stabilire versionamento e compatibilità API.
- [ ] Rendere ripetibile il rilascio: gli script operativi attuali contengono versioni e percorsi fissati; evitare repack silenzioso della stessa versione.
- [ ] CI e SDK/runtime fissati, politica RC/stabile, target residui net10.0 esplicitati.
- [ ] Rieseguire mode consumer NuGet quando cambiano i pacchetti; mantenerla separata dai test sui sorgenti.
- [ ] Oracoli su ulteriori famiglie di segnali/matrici e grandi lunghezze; Half/Decimal32 non inclusi negli oracoli mpmath FFT generali.
- [ ] Estendere analisi di condizionamento, stabilità, overflow intermedi e funzioni speciali/code estreme; non confondere residuo con errore forward.
- [ ] Reference e Usage aggiornate in ogni libreria; altri notebook e scenari non selezionati restano non verificati.
- [x] PySharp ricollegato agli assembly MathemaSharp: kernel compilato esternamente, tour e scenari A/D eseguiti in entrambe le lingue, tre test di concordanza riattivati e passati. Ultimo BuildAndTest: 304 passati, zero sospesi.
- [x] Primo percorso precisione PySharp: Decimal128Interop in Core, notebook eseguito su .NET 11 con scalar 1+1e-25, array CLR/lista e Complex<Decimal128> hermitiano. Oggetti CLR preservati dal marshaller esistente, facciata esplicita non generica; 307 test passati, zero sospesi.
- [x] Primo dispatch operatori CLR nel runtime PySharp: aritmetica, confronti e unary; notebook eseguito sui tre Decimal con incremento Decimal128 1e-25 e int misti. Quattro test CLR mirati passati, MathemaSharp 308 test verdi.
- [x] Modulo builtin PySharp `mathema`: decimal32/64/128 importabili, stringhe/interi/valori dello stesso tipo, float impliciti rifiutati. Notebook senza helper locali eseguito; 8 test mirati PySharp verdi con roll-forward .NET 11. Richiede runtime .NET 11 per costruire i Decimal IEEE.
- [x] Binding esplicito dei metodi CLR `method[ClrType](args)`: chiusura dei generic e constraint; notebook MathemaSharp Identity/Determinant2x2/Transpose in Decimal128 eseguito senza facciate. 12 test PySharp mirati verdi; MathemaSharp 309 test verdi.
- [ ] Inferenza generica, costruzione dei tipi generici, parametri Span/byref, numpy typed e nuovi pacchetti: aperti. Il binder corrente esclude Span/byref dalla reflection; non dichiarare supporto a tutte le API Generic Math. Float e tipi CLR numerici diversi non vengono convertiti implicitamente. Il marshalling generale di decimal/float preesistente non è stato modificato; non dichiarare preservazione universale per qualunque oggetto numerico CLR.
- [ ] Suite completa PySharp e regressioni più ampie del dispatch: in questa tranche eseguiti solo i quattro test nuovi e il notebook Decimal.
- [ ] NDSharp: decidere integrazione/coabitazione runtime e typed, API per operazioni ancora mancanti, complessi, conversioni checked esplicite; no duplicazione dei kernel MathemaSharp.
- [ ] NDSharp: FFT/eig, wavelet, distribuzioni e prodotti batched sono lavori funzionali separati, non completati.
- [x] Notebook pilota end-to-end tramite NuGet: `notebooks/Ecosystem/01-signal-spectrum.ipynb`, CLI JupyterSharp reale, FFT MathemaSharp e due SVG PlotSharp; eseguito e output salvati. Comando: `dotnet run --file ops/RunEcosystemPilot.cs`.
- [x] Gate ecosistema: `dotnet run --file ops/ValidateEcosystem.cs`, report esterno e nessuna deroga sui fallimenti. Ultima esecuzione: audit, baseline, kernel, notebook PySharp/MatSharp e pilota verdi; step build/test rosso per il layout legacy PySharp.
- [ ] Adattatori grafici Decimal: il pilota usa intenzionalmente double per dati e coordinate; non certifica la visualizzazione di differenze Decimal128.
- [ ] Migrare grafici duplicati a PlotSharp solo dopo verifica funzionale, mantenendo i kernel matematici indipendenti dal renderer.
- [ ] Ricontrollare script e configurazioni di tutti i repository per vecchi percorsi e output locali; build output centralizzato non copre automaticamente tool esterni, publish, report custom e cache.
- [ ] Portale: logo MàthemaSharp, CSS compilato, formule, responsive, accessibilità e stampa verificati nel browser.
- [ ] Fonti delle infografiche e confine tra API implementate e modelli solo concettuali.

### Performance — esplicitamente dopo

- [ ] Ridurre buffer/allocazioni Bluestein e valutare riuso di workspace, caching, SIMD o parallelismo solo dopo contratti e test.
- [ ] Benchmark out-of-process su .NET 11 riconosciuto dalla toolchain; le misure attuali sono ShortRun in-process su double, non definitive.
- [ ] Confronti per tipo e kernel, costo diagnostica vs Solve, scalabilità, memoria e scenari reali.
- [ ] Valutare BLAS/GPU solo come obiettivi separati; nessuna promessa universale di velocità.

## Stato storico delle tranche iniziali (vedere quadro corrente sopra)

- [x] Rinominati soluzione, progetti, namespace e riferimenti in `MathemaSharp.*`.
- [x] Librerie separate: Core, LinearAlgebra, TimeSeries, Quantum e ponte NDSharp.
- [x] Notebook organizzati nella root `notebooks/`, per libreria.
- [x] Portale documentazione Blazor con navigazione Markdown, immagini e stili per tabelle.
- [x] Algebra lineare estratta da TimeSeries; LU generica, solver, determinante e inversa generale aggiunti con test.
- [x] Patch storiche e valutazione rolling-origin implementate e testate.
- [x] Ultima verifica dopo la rinomina: build senza warning/errori; 124 test passati, 3 sospesi.
- [x] Raccolta iniziale PlotSharp: 148 copie di riferimento e manifest di provenienza. Non è ancora una libreria attiva verificata.
- [ ] Tutti i progetti sul medesimo target .NET 11: alcuni, incluso il portale, restano net10.0.
- [ ] Conversione completa del materiale NDSharp importato.
- [ ] Riesecuzione dei notebook dopo spostamenti e rinomina; ripristino dei 3 test sospesi.
- [ ] Logo grafico aggiornato a MàthemaSharp: il file rinominato contiene ancora il vecchio nome.

## P0 — inventario e baseline riproducibile

- [ ] Censire API pubbliche, constraint, target, conversioni a double e copertura per tipo di ogni libreria.
- [ ] Classificare ciascuna API: generica verificata, generica non verificata, wrapper runtime, da migrare o fuori scope.
- [ ] Verificare dipendenze e licenze degli snapshot importati, mantenendo attribuzioni e provenienza.
- [ ] Fissare SDK/runtime .NET 11 disponibile e politica degli aggiornamenti preview/stabili. Migrare i target residui solo quando i vincoli dell'ambiente lo consentono.
- [ ] Rendere build e test riproducibili in CI, separando test veloci, integrazione e notebook.

**Criterio di completamento:** inventario API-per-API e pipeline verde; nessuna percentuale di conversione senza un denominatore esplicito.

## P1 — completare il nucleo Generic Math

### LinearAlgebra
- [x] Prima tranche QR/minimi quadrati: sistema normalizzato, norme Hypot, riflettori unitari, soglia min(4096 epsilon, sqrt(epsilon)), controllo dati finiti e intermedi non rappresentabili. Dieci test aggiunti: sei formati, scale 1e-200/1e200, coefficiente Decimal128 1e-25 e input invalidi. Build senza warning/errori, 175 test passati e 3 sospesi.
- [ ] Ampliare ulteriormente i test di prodotti, norme, QR e LU: matrici mal condizionate, errori relativi/residui e oracoli indipendenti; la tranche QR non costituisce una certificazione generale.
- [ ] Convertire autovalori importati: bilanciamento, Hessenberg e QR complesso; documentare convergenza e casi limite.
- [ ] Valutare Cholesky e SVD come tranche separate, senza dichiararle disponibili prima di implementazione e test.
- [ ] Spostare i test algebrici ancora ospitati nel progetto TimeSeries in un progetto dedicato.

### Segnali e trasformate
- [ ] Decidere confini e nome del modulo segnali prima di estrarre i kernel.
- [x] FFT/IFFT radix-2 in Core: confronto DFT e round-trip sui sei formati, casi analitici impulso/costante, Decimal128 1e-25, input e overflow. Percorso radix-2 per potenze di due, affiancato ora da Bluestein; nessuna certificazione generale.
- [x] Bluestein per lunghezze non potenze di due: convoluzione radix-2, fasi ridotte con aritmetica intera, cinque nuovi test su lunghezze prime/composite e sei formati. Notebook FFT aggiornato ed eseguito.
- [ ] Oracoli FFT indipendenti ad alta precisione su segnali generali, grandi lunghezze e benchmark. I casi analitici piccoli non costituiscono certificazione generale.
- [x] Ponte NuGet 0.2.0: 16 test eseguiti in modalità UsePackagedBridge=true con dipendenze dal feed locale; mapping MathemaSharp.* corretto.
- [ ] Wavelet e filtri: conversione di costanti, normalizzazione, condizioni al bordo e ricostruzione.

### Statistica e probabilità
- [ ] Inventariare distribuzioni, generatori e funzioni speciali importate.
- [ ] Estrarre kernel generici senza conversioni nascoste a double; distinguere calcolo numerico e sorgente casuale.
- [ ] Test di momenti, quantili, dominio e riproducibilità; test statistici non fragili.

### NDSharp — conversione ripresa su richiesta

- [x] Storage `NDArray<T>` con `INumber<T>`, shape, stride, viste, slicing e broadcasting.
- [x] Sum, Add, Dot e MatMul generici senza intermedi double.
- [x] Ponte verso LU, QR e diagnostica esistenti; verifiche sui sei formati e Decimal128 1e-25.
- [x] Notebook Usage/Reference eseguito e incluso nel manifest della baseline.
- [ ] Conversione completa delle API runtime, FFT, wavelet e distribuzioni: non completata.
- [ ] Prodotti batched e ottimizzazioni: non implementati.

NDSharp gestisce array e viste; MathemaSharp i kernel matematici. Il nuovo contenitore affianca quello runtime. Gli snapshot importati non sono automaticamente convertiti.

### Interoperabilità non NDSharp
- [ ] Documentare i wrapper non generici necessari a PySharp, separandoli dalle API matematiche.

**Criterio di completamento:** ogni kernel migrato ha test su tutti i tipi dichiarati, errori documentati e confronto con fixture/oracoli. Generic Math non significa imporre lo stesso constraint a ogni operazione.

### Quantum — consolidamento
- [x] Norm con Hypot sulle componenti, Normalize scalata sulla massima componente finita. Uno stato finito può essere normalizzato anche quando la sua norma originale supera il range del tipo.
- [x] Sette test NativeStateNormTests: sei formati, ampiezze massime/subnormali, preservazione degli input, zero/NaN/infinito rifiutati e componente Decimal128 1e-25. Build senza warning/errori, 182 test passati e 3 sospesi.
- [x] Diagonalizzazione hermitiana 2×2 normalizzata, Hypot per il raggio, soglia fissa 1e-12 rimossa, autovettori senza sottrazioni instabili e complemento ortonormale anche in degenerazione. Otto test nativi: sei formati, scale massime/subnormali, residui, zero/identità, coupling Decimal128 1e-25 e input/overflow. Baseline: 190 test passati, 3 sospesi, build senza warning/errori.
- [x] CheckEigenstate: stato/operatore scalati separatamente, quoziente di Rayleigh e residuo relativo alla norma Frobenius dell'operatore, validazione tolleranza/dimensioni/componenti. Sette test sui sei formati: ampiezze massime/subnormali, operatori piccoli, autovalore complesso e perturbazione Decimal128 1e-25. Baseline: 197 test passati e 3 sospesi.
- [x] MatrixExp: Frobenius con Hypot, copia esplicita e scaling per dimezzamenti senza 2^s, termini Taylor positivi richiesti e intermedi finiti verificati. TimeEvolution valida t finito e hbar positivo finito. Otto test sui sei formati: zero, diagonale, nilpotente grande, evoluzione unitaria, termine Decimal128 1e-25 e overflow/input. Build verde, 205 test passati e 3 sospesi.
- [ ] Validare MatrixExp con oracoli e matrici non normali/scale estreme: 24 termini non danno una garanzia generale, underflow e perdita di piccoli termini durante lo scaling restano possibili. Ampliare test di diagonalizzazione a matrici quasi singolari e autovalori molto sbilanciati. La normalizzazione globale può perdere componenti sotto il range di T; il recupero dell'autovalore piccolo dal determinante è stato aggiunto nella tranche oracoli, ma non elimina ogni problema di condizionamento/underflow.

## P2 — notebook, Usage e Reference

- [x] Quattro notebook `notebooks/<Core|LinearAlgebra|TimeSeries|Quantum>/11-consolidation.ipynb` con Usage/Reference focalizzate, verifiche in T e output reali sui sei formati, eseguiti con JupyterNet/.NET 11. Bootstrap numerico senza Charts/NDSharp e runner `dotnet msbuild ops/consolidation-notebooks.proj -t:Run`.
- [x] Corretto il percorso soluzione nel runner e la compatibilità della risoluzione percorsi con Windows PowerShell. Quattro test aggiunti per evidenza degli output salvati (non riesecuzione).
- [x] Quattro notebook `12-api-reference.ipynb` eseguiti: catalogo da reflection di tipi esportati e membri pubblici dichiarati, firme e constraint. Core 3 tipi, LinearAlgebra 4 (inclusa diagnostica), Quantum 4, TimeSeries 36 (inclusi wrapper Interop). Runner: `dotnet msbuild ops/reference-notebooks.proj -t:Run`. Quattro test per gli output del catalogo; non certificano comportamento delle API.
- [x] Rieseguiti Core 01–05 su .NET 11 RC3 con output reali: Generic Math, precisione, Complex/DFT, ambiente SIMD e nuovi Decimal/parsing parziale. Aggiunti contratti ai notebook 01–03, cinque test degli output salvati e runner selettivo `dotnet msbuild ops/core-notebooks.proj -t:Run`. Il notebook performance non esegue benchmark statistici.
- [x] Riscritti e rieseguiti LinearAlgebra/01-lu e Quantum/01-operators: contratti, esempi con verifiche, collegamenti Reference, output .NET 11 reali. LU/residuo/inversa/riuso, QR Decimal128; normalizzazione estrema, autostati, densità e TimeEvolution Decimal128 con errore ≤1e-28 nel caso specifico. Due test sugli output. Runner: `dotnet msbuild ops/algebra-quantum-notebooks.proj -t:Run`.
- [x] TimeSeries C# 06–08 e tour 10: aggiunti contratti/limiti e link alla Reference, rieseguiti con output reali .NET 11. Runner `dotnet msbuild ops/timeseries-notebooks.proj -t:Run`, selezione top-level e wildcard PowerShell. Quattro test degli output; il tour ha almeno 24 etichette numeriche salvate.
- [ ] Completare contratti e verifiche numeriche end-to-end restanti; scenari e PySharp non rieseguiti, tre confronti ancora sospesi. Il 09 (STL + ponte NDSharp) resta escluso finché NDSharp è parcheggiato.

- [ ] Inventariare gli esempi console e trasferire quelli didattici nei notebook; valutare se mantenere la console solo come harness.
- [ ] Per ogni libreria creare notebook Usage e Reference con indice delle API, constraint, esempi, limiti e collegamenti alla documentazione.
- [ ] Rieseguire notebook su kernel/runtime documentati e verificare percorsi relativi dalla loro posizione effettiva.
- [ ] Ripristinare i confronti C#/PySharp sospesi; distinguere notebook eseguiti, strutturalmente validi e non verificati.
- [ ] Preferire pacchetti versionati ai riferimenti ad artifacts per gli esempi destinati agli utenti.
- [ ] Allineare README, USAGE e ARCHITECTURE al reale stato dei moduli.

**Criterio di completamento:** ogni notebook pubblicato come eseguito è riproducibile; ogni API pubblica è rintracciabile nella Reference.

## P3 — PlotSharp, dipendenza esterna in preparazione

Il progetto autonomo è in `D:\Dev\2027\repos\PlotSharp`; gli obiettivi di estrazione sono nel suo `OBJECTIVES.md`. La raccolta iniziale non garantisce tutte le dipendenze e non prova compatibilità o compilazione.

- [ ] Attendere inventario, verifica licenze e prima API stabile con renderer/test in PlotSharp.
- [ ] Censire le implementazioni Charts locali e i loro utilizzatori prima di rimuovere codice.
- [ ] Integrare PlotSharp in un notebook pilota, poi migrare progressivamente gli altri esempi.
- [ ] Mantenere i notebook matematici in MathemaSharp; la galleria dei grafici appartiene a PlotSharp.
- [ ] Limitare la dipendenza grafica ai sample e agli adattatori: i kernel matematici non devono dipendere dal renderer.
- [ ] Rimuovere duplicazioni Charts solo dopo equivalenza funzionale e verifica visiva.

**Criterio di completamento:** esempi migrati verificati e nessuna dipendenza grafica introdotta nei kernel. NDSharp, JupyterSharp e NavierStokes si aggiornano in tranche autorizzate nei rispettivi repository.

## Consolidamento avviato — Core

- [x] Corretto NearlyEqual: tolleranza default min(4096 epsilon, sqrt(epsilon)), per evitare un default circa 4 su Half. È un cambiamento comportamentale per i formati a bassa precisione; usare l'overload esplicito quando serve una policy applicativa.
- [x] Aggiunti sette test NativePrecisionTests: confronti/valori speciali e piccoli esempi Sum, Mean, Kahan, Horner, VectorDot, DFT e HermitianDot su Half, float, double, Decimal32/64/128, con fixture e confronti in T.
- [x] Test Decimal128 che conserva una differenza 1e-25 in somma/prodotto scalare e la rileva con tolleranza esplicita, senza passaggi da double.
- [x] Prima tranche TimeSeries: LOESS con ascisse centrate, margine 999/1000 calcolato in T, soglie legate a epsilon e validazione osservazioni/pesi/ascissa.
- [x] Nove test NativeLoessTests: rette su sei formati, trend Decimal128 1e-25, indici grandi e input invalidi. Baseline: build verde, 140 test passati e 3 sospesi.
- [x] GARCH variance targeting: griglia a indici interi, margine di stazionarietà calcolato in T e limitato dal predecessore di 1, input invalidi rifiutati e likelihood finita richiesta. Sette test nativi: ricorrenza/forecast/fit riproducibile sui sei formati e input errati. Baseline: 147 test passati, 3 sospesi, build senza warning/errori.
- [x] Simulation: overload ARMA, random walk, GARCH e VAR con sorgente di innovazioni Func<Random,T>; i default conservano Box-Muller double. Nove test: ricorrenze e conteggio chiamate su sei formati, differenze Decimal128 1e-25, default confrontato con sorgente legacy esplicita e input invalidi. Baseline: build verde, 156 test passati, 3 sospesi. Non è stato implementato un RNG gaussiano ad alta precisione.
- [x] Prima tranche SpecialFunctions: coefficienti Lanczos e soglia da stringhe invariant in T con cache, dominio/NaN/infinito espliciti, identità per LogGamma(1/2,1,2) e Q(1,x), segnalazione non convergenza/overflow. Nove test: identità e ricorrenze sui sei formati, differenza Decimal128 1e-25 nel caso esponenziale, limite iterazioni e overflow. Baseline: 165 test passati, 3 sospesi, build senza warning/errori.
- [ ] Validazione più ampia delle funzioni speciali (code estreme, oracoli indipendenti, qualità dell'approssimazione e clamp [0,1]), parametri delle simulazioni/overflow delle ricorrenze e copertura LinearAlgebra/Quantum; verificare anche il costo della validazione LOESS ripetuta dentro STL.
- [ ] Notebook Usage/Reference e riesecuzione: non eseguiti in questa tranche.

## Validazione indipendente — prima tranche

- [x] Fixture mpmath 1.3.0: 200 cifre di lavoro, 160 salvate, generatore indipendente da MathemaSharp e dipendenza Python isolata. Fixture versionabile in ops/oracles/numerical-reference.json; nessun Python necessario per dotnet test.
- [x] 12 casi gamma Q e 7 LogGamma confrontati su float/double/Decimal64/Decimal128; chi-quadro verificato per i parametri equivalenti con gradi interi. Tolleranza relativa 0.002 per float e 1e-10 per gli altri; underflow float esplicitamente distinto. Half/Decimal32 non certificati su queste code.
- [x] Hilbert 3/5/8: LU e QR confrontate con soluzione indipendente su double/Decimal128, residui e determinanti. Errore di soluzione ammesso dipendente dal condizionamento, non una tolleranza universale.
- [x] Limite Hilbert12 double documentato: LU accetta il sistema con residuo relativo circa 1.31e-16 ma errore massimo della soluzione circa 0.00595; QR rifiuta il rango numerico. Non alzata arbitrariamente la soglia LU.
- [x] Notebook 13-independent-oracles eseguito, con errori relativi massimi sulle fixture gamma circa 1.46e-12 double e 2.46e-13 Decimal128; nessuna promessa di 34 cifre Lanczos.
- [x] Oracoli Quantum: 5 MatrixExp da mpmath.expm (Jordan 2/3, caso più rigido e complesso) su float/double/Decimal64/128; 3 spettri hermitiani da eighe su double/Decimal128, inclusi autovalori 1e-40. Sei test numerici e un test notebook aggiunti.
- [x] Corretto un difetto trovato dagli oracoli: diagonalizzazione restituiva zero su diag(1,1e-40). Diagonali trattate direttamente, autovalore soggetto a cancellazione ricavato dal determinante con FMA e dall'altro autovalore. Notebook Quantum/13 eseguito: errore MatrixExp scalato massimo circa 1.98e-14 double e 3.38e-32 Decimal128 sulle 5 fixture.
- [x] Diagnostica opt-in LinearSystemDiagnostics.Solve<T>: soluzione LU, residuo relativo infinito, backward error normwise, condition/rcond e avviso rcond ≤ sqrt(epsilon). n solve aggiuntivi per norma dell'inversa, senza memorizzarla; non è un condition estimator iterativo economico o certificazione forward. Test nativi sui sei formati e condition contro mpmath; notebook LinearAlgebra/13 eseguito e Reference rigenerate.
- [ ] Estendere ancora: matrici non normali più difficili, determinanti quasi nulli, input arrotondati, casi gamma vicini alla cancellazione/underflow, regressioni sovradeterminate rumorose; diagnostica componente per componente, refinement e stima economica del condizionamento.

## P4 — validazione numerica e performance

- [ ] Test mirati Decimal32/64/128: precisione, conversioni, NaN/infinito, rounding, parsing e funzioni trascendenti.
- [ ] Ampliare test Complex<T>, errori relativi/assoluti e casi estremi per algoritmo.
- [ ] Benchmark diretti dei formati, FFT, LU e altri kernel, senza parsing/reflection nei loop.
- [ ] Eseguire job BenchmarkDotNet attendibili, documentando hardware, SDK/runtime, allocazioni e configurazione.
- [ ] Separare smoke test Dry dalle misure utilizzabili per confronti; nessuna promessa universale di velocità.

## P5 — approfondimenti e scenari

- [ ] Applicare il rolling-origin già implementato agli scenari frigorifero e fotovoltaico con finestre comparabili e senza leakage.
- [ ] Realizzare gli scenari B (compressori), C (acquedotto), E (CNC).
- [ ] Valutare Diebold-Mariano/Clark-West, previsione conforme, matrix profile, PELT/BOCPD.
- [ ] Estendere ARIMA con likelihood esatta, Kalman con stima q/r e smoother RTS, ETS state-space e intervalli ARIMA/VAR.
- [ ] Completare schede delle teorie dell'atlante e verificare le fonti primarie; distinguere concetti, baseline e modelli completi.
- [ ] Aggiornare logo e marchio nel portale; verificare resa responsive, stampa, accessibilità e formule.
- [ ] Sostituire Tailwind CDN con CSS compilato prima di un uso produttivo.

## Baseline automatizzata

- [x] Unico comando `dotnet msbuild ops/validate.proj -t:Baseline`: build Release, tool restore, 22 notebook espliciti, test Release e report JSON/log con SDK/runtime e conteggi. Eseguito con successo: 272 test passati, 3 sospesi; 7 notebook esclusi con motivazione.
- [x] Modalità BuildAndTest registra notebook non eseguiti e FullBaseline=false. Manifest validato da due nuovi test, nessuna selezione automatica di NDSharp/PySharp/scenari.
- [ ] CI con SDK/runtime fissati e feed riproducibili; benchmark statistici separati dalla baseline funzionale.

## P6 — rilascio

- [ ] Definire versionamento, compatibilità API e changelog.
- [ ] Verificare packaging, licenze, metadati e consumo dei pacchetti da un progetto esterno pulito.
- [ ] Pubblicare/installare solo su richiesta, dopo build, test e verifica degli esempi.

## Fuori scope per queste tranche

Algebra simbolica completa, finanza normativa, implementazioni alternative dei tipi Decimal, un custom INumber completo, GPU/BLAS ottimizzati e Transformer/foundation model completi. SVD, autovalori e FFT sono obiettivi da valutare/implementare, non capacità già garantite.

## Prossima tranche consigliata

1. Aggiornare inventario, contratti e documentazione al codice corrente.
2. Rilasciare una nuova versione locale contenente FFT/IFFT e verificarla via NuGet.
3. Verificare un notebook pilota con JupyterSharp e PlotSharp.

Performance rimandate. NDSharp è stato ripreso; conversione runtime completa, autovalori generali e wavelet restano aperti. Le FFT in Core sono implementate e verificate, ma non ancora nei pacchetti 0.2.0.
