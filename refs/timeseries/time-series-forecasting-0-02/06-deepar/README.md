# DeepAR

![sesta riga dell'infografica (2017)](riga.png)

> **Infografica** [`time-series-forecasting-0-02`](../time-series-forecasting-0-02.jpg) · sesta riga dell'infografica (2017) · **2017** · stato ⬜ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** Addestrare **un solo modello ricorrente su molte serie correlate**, invece di un modello per serie. L'infografica: *«Train one recurrent model on many related series»*, con *«Shared weights across series»*.

**Come funziona.**

- Una rete ricorrente (LSTM) elabora, passo dopo passo, il valore precedente $y_{t-1}$, le covariate $x_t$ e un'**identità** della serie.
- A ogni passo non emette un numero ma i **parametri di una distribuzione**: per esempio media e deviazione standard di una gaussiana, o i parametri di una binomiale negativa per i conteggi.
- Si addestra massimizzando la **verosimiglianza** di tutte le serie.
- Per prevedere si **campionano traiettorie**: si estrae $\hat y_{t}$ dalla distribuzione, lo si riusa come ingresso del passo successivo, e si ripete molte volte. Dai campioni si ottengono quantili e intervalli (*«Predictive distribution»*).

**Il vantaggio dell'apprendimento globale.** Le serie con poca storia (un nuovo impianto, un nuovo prodotto) beneficiano di ciò che il modello ha imparato dalle altre: si parla di *cold-start*. Le serie hanno scale diverse: il modello riscala per serie.

**Limiti.** Serve un parco di serie *davvero correlate* e abbastanza lungo; addestramento costoso; le previsioni non hanno garanzia di copertura.

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *Un solo allievo, cento maestri* · **Durata:** ~85 s · **Personaggi:** Giulia, Anguria, Tommaso · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Lo stabilimento ha 200 celle frigorifere. Ogni cella è un piccolo grafico. | **Giulia:** «Ho 200 serie. Una per cella.» | molte serie correlate |
| 2 | Un esperto per ogni cella, ognuno chiuso nel suo ufficio, a studiare solo la sua serie. | **Anguria:** «Duecento esperti, ognuno impara da solo. Si ripetono gli stessi errori.» | un modello per serie: non condivide nulla |
| 3 | Una sola aula con un solo allievo. Sul banco, le 200 serie una dopo l'altra. | **Anguria:** «Un allievo solo. Studia tutte le celle. I suoi pesi sono gli stessi per tutte.» | pesi condivisi |
| 4 | Arriva una cella nuova, installata da due giorni. Un grafico quasi vuoto. | **Giulia:** «Questa non ha storia. Cosa prevediamo?» | cold-start |
| 5 | L'allievo guarda la cella nuova e la riconosce: «somiglia a quelle del lato nord». | **Anguria:** «Non parte da zero: ha già visto celle così.» | l'apprendimento globale aiuta le serie corte |
| 6 | L'allievo non risponde con un numero ma con una campana: più larga dove è incerto. Anguria la fa girare e ne estrae un punto, poi un altro. | **Anguria:** «A ogni passo dà la forma dell'incertezza. Poi ne estraggo un esempio e vado avanti.» | uscita probabilistica e campionamento |
| 7 | Tanti futuri possibili come fili colorati che si aprono a ventaglio. | **Tommaso:** «E la fascia che mi mostri, dove l'hai verificata?» — **Anguria:** «Sulle sue prove. Si controlla sempre.» | traiettorie campionate, copertura da verificare |

**Morale:** *se le serie si somigliano, imparare insieme è più rapido che imparare da soli.*

## Nel repository

| | |
|---|---|
| **Stato** | ⬜ **non implementata** |
| **Cosa c'è di vicino** | l'**uscita probabilistica** e le **traiettorie campionate** si vedono, con un metodo molto più semplice, nello [scenario D](../../../../scenari/D-fotovoltaico/README.md): quantili dei residui per ora, traiettorie ricampionando blocchi di residui, `Accuracy.PinballLoss`, `Accuracy.IntervalCoverage`. Non c'è né una rete né un apprendimento fra molte serie |
| **Cosa manca** | rete ricorrente addestrata su più serie, uscite parametriche, campionamento autoregressivo |

## Riferimenti

- Salinas, D., Flunkert, V., Gasthaus, J. & Januschowski, T. (2020). *DeepAR: probabilistic forecasting with autoregressive recurrent networks.* International Journal of Forecasting (anteprima arXiv del 2017).
