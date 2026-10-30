# Previsione di serie temporali: dalle idee alla fabbrica

> **Immagini di riferimento non incluse.** Le infografiche da cui nascono questi capitoli, e i loro ritagli (`riga.png`, `sintesi.png`), sono materiale di terzi di cui non è nota la provenienza: sono state rimosse dal repository. Restano le descrizioni e le schede in testo.

> Una storia delle idee del forecasting, dalle previsioni di riferimento ai *foundation model*, con le formule delle tecniche classiche, la loro realizzazione in .NET 11 con **Generic Math** (`float`, `double`, `Half`, `Decimal32/64/128`), e scenari di **IoT industriale**: persone che acquisiscono dati e segnali da automazioni e devono produrre KPI, dashboard e risposte ai clienti.

Scritto in italiano. Le formule usano KaTeX (`$…$`), che VS Code e GitHub visualizzano. Il sommario in formato mdBook è in [SUMMARY.md](SUMMARY.md).

## Come è organizzato

| Parte | Contenuto | Dove |
|---|---|---|
| **I · Le formule e la loro realizzazione** | per ogni tecnica: formula, codice, test, limiti. 10 capitoli | [`00-formule-e-libreria/`](00-formule-e-libreria/README.md) |
| **II · La libreria** | quale libreria, uso funzione per funzione, **overlap con NumPy in PySharp**, esempi in C# e PySharp | [`01-libreria/`](01-libreria/README.md) |
| **III · Gli scenari** | la teoria sul campo: A magazzino frigorifero, D parco fotovoltaico (B, C, E più avanti) | [`scenari/`](../../scenari/README.md) |
| **IV · L'atlante delle teorie** | le **91 teorie** delle 18 infografiche, con lo stato di implementazione | [ATLANTE.md](ATLANTE.md) |

## Per chi legge

- **Chi vuole capire la matematica**: Parte I, in ordine. I capitoli si possono leggere anche separatamente.
- **Chi vuole usare la libreria**: Parte II, poi il [giro della libreria](01-libreria/04-esempi.md) in C# e in PySharp.
- **Chi lavora su un impianto**: gli [scenari](../../scenari/README.md). Ognuno parte da segnali di PLC e finisce con KPI, dashboard e la risposta a una domanda del cliente.
- **Chi vuole sapere cosa c'è e cosa no**: l'[atlante](ATLANTE.md). Dice con onestà che **la maggior parte delle teorie delle infografiche non è implementata**.

## Le infografiche

Le immagini originali stanno in questa cartella; quella già lavorata è stata spostata nella propria sottocartella. Descrivono idee e cronologia, *non una classifica di modelli*.

| Infografica | Tema |
|---|---|
| `time-series-forecasting-0-01` … `0-05` | cronologia: fondamenti statistici, automazione e modelli neurali, transformer, modelli linguistici, modelli efficienti |
| `time-series-forecasting-1-…` (7 immagini) | architetture, rappresentazioni della storia, ricorsivo/diretto, previsioni probabilistiche, condizionamento a un futuro incerto |
| `time-series-diagnostic-01`, `02` | punti di cambiamento, matrix profile |
| `time-series-forecast-validation-01` … `04` | validazione e confronto fra previsioni, valutazione dei modelli pre-addestrati |

## Cosa non è ancora fatto

- **La divisione in sottocartelle, il ritaglio delle righe e le schede di teoria (descrizione teorica + racconto a fumetti) sono fatti per 2 infografiche su 18**: [`0-01`](time-series-forecasting-0-01/README.md) e [`0-02`](time-series-forecasting-0-02/README.md), con 18 teorie. Le altre 16 (73 teorie) sono nell'[atlante](ATLANTE.md) come solo indice; si procede una infografica alla volta. Il cast dei racconti è in [PERSONAGGI.md](PERSONAGGI.md).
- Gli scenari **B, C ed E** sono solo descritti nell'[indice degli scenari](../../scenari/README.md).
- **68 delle 91 teorie** dell'atlante (⬜) non hanno nessun codice; 11 ne hanno una parte (◐); 12 sono complete (✅).

## Verifiche automatiche

- `dotnet test src/MathemaSharp.slnx -c Release`: i test sulle identità matematiche in ogni formato, e quelli che i notebook C# e PySharp **stampino gli stessi numeri**.
- `pwsh scripts/run-notebooks.ps1`: ricostruisce ed esegue tutti i notebook.
