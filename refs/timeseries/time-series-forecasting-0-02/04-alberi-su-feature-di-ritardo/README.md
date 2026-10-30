# Alberi di decisione su feature di ritardo

*(Reference image not included in the repository: third-party material.)*

> **Infografica** `time-series-forecasting-0-02` · quarta riga dell'infografica (anni 2010) · **anni 2010** · stato ⬜ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** Trasformare la previsione in un problema di **apprendimento supervisionato**: ogni istante $t$ diventa una riga di una tabella, con **feature** costruite dal passato e **bersaglio** il valore da prevedere.

| Gruppo di feature | Esempi |
|---|---|
| **Ritardi** (*lags*) | $y_{t-1},\;y_{t-2},\;y_{t-24},\;y_{t-168}$ |
| **Statistiche mobili** (*rolling*) | media e deviazione standard delle ultime 24 ore, minimo/massimo |
| **Calendario** | ora del giorno, giorno della settimana, festivo |

Su questa tabella si addestra un insieme di alberi di decisione (foresta casuale, *gradient boosting*). L'infografica avverte: *«XGBoost paper: 2016; approach predates it»*: l'approccio è precedente all'articolo di XGBoost, che lo ha reso popolare.

**Come si prevede a più passi.** *Ricorsivo* (si riusa la previsione come ritardo) o *diretto* (un modello per orizzonte): è il tema dell'infografica «un passo o l'intero orizzonte».

**Vantaggi.** Cattura non linearità e interazioni, accetta **covariate** di ogni tipo (prezzi, meteo, calendario), nessuna ipotesi di stazionarietà o forma parametrica.

**Limite decisivo: gli alberi non estrapolano.** La previsione di un albero è una media di valori già visti: oltre l'intervallo del training restituisce l'ultimo «gradino». Con un trend che sale, le previsioni si **appiattiscono**. Si rimedia togliendo il trend prima (differenze, STL) o con modelli lineari nelle foglie.

**Attenzione alla perdita di informazione dal futuro (*leakage*).** Ogni feature deve usare **solo dati disponibili al tempo $t$**: una media mobile centrata o una divisione casuale del dataset *prima* di costruire i ritardi barano. Per validare serve una divisione cronologica (rolling origin).

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *La tabella dei ritardi* · **Durata:** ~85 s · **Personaggi:** Anguria, Giulia, Tommaso · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Una striscia di carta con i consumi orari. Anguria prende le forbici. | **Anguria:** «Taglio la striscia in tante righe.» | sequenza → tabella |
| 2 | Ogni riga ha colonne: «un'ora fa», «ieri alla stessa ora», «media delle ultime 24 ore», «giorno della settimana». In fondo: «adesso». | **Giulia:** «Ogni riga dice: *sapendo questo, quanto consumavo adesso?*» | feature di ritardo, rolling, calendario |
| 3 | Un albero di domande: «ieri alla stessa ora > 40 kWh?» sì/no, «è lunedì?» sì/no. Alla fine, una foglia con un numero. | **Anguria:** «Tante domande sì/no. Cento alberi, ognuno risponde un po' diverso. La previsione è la media.» | insieme di alberi |
| 4 | Una nota in basso: «2016: l'articolo su XGBoost». Una freccia indica «prima del 2016: già si faceva». | *Didascalia:* «Il metodo è più vecchio dell'articolo che l'ha reso famoso.» | storia |
| 5 | Una temperatura che sale ogni giorno: la previsione degli alberi sale e poi si ferma, come uno scalino. | **Tommaso:** «E se domani è più caldo di tutto ciò che hai visto?» — **Anguria:** «L'albero ripete l'ultimo gradino. Non sa salire oltre.» | gli alberi non estrapolano |
| 6 | Una riga della tabella con una feature «media dei prossimi 3 giorni». Tommaso la cerchia con il timbro rosso. | **Tommaso:** «Questa sbircia nel futuro.» — **Anguria:** «Giusto. Ogni colonna può usare solo ciò che si sapeva a quell'ora.» | leakage |
| 7 | Giulia confronta il risultato del suo modello con la previsione «come la settimana scorsa». | **Giulia:** «Prima lo battiamo, poi ci fidiamo.» | il benchmark ([scheda 01 della 0-01](../../time-series-forecasting-0-01/01-previsioni-di-riferimento/README.md)) |

**Morale:** *una serie diventa una tabella: ma ogni colonna deve conoscere solo ciò che sapeva allora.*

## Nel repository

| | |
|---|---|
| **Stato** | ⬜ **non implementata** (gli alberi) |
| **Cosa c'è di vicino** | lo **stesso passo di trasformazione in tabella** si vede nello [scenario D](../../../../scenari/D-fotovoltaico/README.md), dove una **regressione lineare** (`MatrixAlgebra.LeastSquares`) usa come feature l'irraggiamento previsto e la temperatura: MASE 0.511 contro 1.362 della persistenza. Non sono alberi, ma è lo stesso schema «feature note al tempo $t$ → bersaglio» |
| **Cosa manca** | alberi di decisione, foreste, *gradient boosting* |
| **Per capire i limiti** | [cap. 10](../../00-formule-e-libreria/10-algebra-e-simulazione.md): minimi quadrati; scheda sul «leakage» nell'infografica sulla validazione |

## Riferimenti

- Chen, T. & Guestrin, C. (2016). *XGBoost: a scalable tree boosting system.* KDD.
- Hyndman, R. J. & Athanasopoulos, G. *Forecasting: Principles and Practice* (3ª ed.), sezione sui metodi di apprendimento automatico.
