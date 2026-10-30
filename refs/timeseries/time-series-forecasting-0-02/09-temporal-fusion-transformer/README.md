# Temporal Fusion Transformer

*(Reference image not included in the repository: third-party material.)*

> **Infografica** `time-series-forecasting-0-02` · nona riga dell'infografica (2019) · **2019** · stato ⬜ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** Combinare **informazioni di natura diversa** in un unico modello di previsione a più orizzonti. L'infografica: *«Combine static, historical and future-known inputs»*.

| Tipo di ingresso | Esempio in uno stabilimento |
|---|---|
| **Statico** | quale compressore, modello, sito: non cambia nel tempo |
| **Storico** (osservato solo nel passato) | temperatura, corrente, vibrazione fino ad adesso |
| **Futuro noto** | calendario di produzione, ora del giorno, festività, il **piano** dei turni |

**Componenti** (*«Selection + LSTM + attention»*):

1. **Reti di selezione delle variabili**: imparano *quali* ingressi contano, per ciascun istante;
2. un **LSTM** (encoder-decoder, [scheda 01](../01-lstm/README.md)) per i pattern locali;
3. un'**attenzione multi-testa interpretabile** per le dipendenze di lungo periodo;
4. **porte** che permettono di saltare componenti inutili.

**Uscita:** previsioni a **più orizzonti**, come **quantili** (*«Multi-horizon quantiles»*), addestrate con la **perdita pinball**.

**Interpretabilità.** Si leggono l'importanza delle variabili e i pesi di attenzione nel tempo.

**Cosa non è.** L'infografica lo dice: *«Hybrid architecture, not a plain transformer»*: usa un LSTM *e* l'attenzione.

**Limiti.** Molti parametri e dati; i quantili non hanno garanzia di copertura; l'attenzione mostra *dove guarda* il modello, non *perché*.

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *Il tavolo con tre tipi di informazione* · **Durata:** ~90 s · **Personaggi:** Giulia, Anguria, Tommaso · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Giulia ha tre cartelline sul tavolo: «CHI È» (la scheda del compressore), «COSA È SUCCESSO» (le letture fino a oggi), «COSA SAPPIAMO DEL FUTURO» (il piano dei turni). | **Giulia:** «Per pianificare domani uso tutte e tre.» | ingressi statici, storici, futuro noto |
| 2 | Anguria sul tavolo: un cartello «non tutto serve sempre». | **Anguria:** «Il modello decide quali righe leggere, ora per ora.» | selezione delle variabili |
| 3 | Un piccolo quaderno con le porte ([racconto dell'LSTM](../01-lstm/README.md)) che scorre le letture del passato. | **Anguria:** «Prima si ricorda ciò che è successo da vicino.» | l'LSTM per il locale |
| 4 | Una sala di partecipanti ([racconto del Transformer](../07-transformer/README.md)): ognuno guarda i più utili. | **Anguria:** «Poi guarda lontano: la settimana scorsa, lo stesso giorno.» | attenzione per il lungo periodo |
| 5 | Il piano dei turni entra dall'alto con la scritta «noto in anticipo». | **Giulia:** «Se domani c'è un turno in più, il modello lo sa.» | il futuro noto come ingresso |
| 6 | Per ogni ora del giorno dopo, tre curve: bassa, media, alta. | **Tommaso:** «Non voglio un numero. Voglio una fascia.» — **Anguria:** «Le dà per tutte le ore insieme.» | quantili a più orizzonti |
| 7 | Una scheda con due frecce: «si addestra con la perdita pinball». Giulia la riconosce. | **Giulia:** «È la stessa che uso nello scenario del fotovoltaico.» | legame con `PinballLoss` |
| 8 | Tommaso con il timbro: «verifica la copertura». | **Tommaso:** «Quante volte il reale cade nella fascia?» | la copertura va controllata |

**Morale:** *un buon piano usa ciò che è, ciò che è stato e ciò che si sa già del futuro.*

## Nel repository

| | |
|---|---|
| **Stato** | ⬜ **non implementata** |
| **Cosa c'è di vicino** | tre idee, in forma elementare, nello [scenario D](../../../../scenari/D-fotovoltaico/README.md): (1) un **ingresso noto nel futuro** (l'irraggiamento previsto) in una regressione; (2) **quantili a più orizzonti** (P10/P50/P90 ora per ora); (3) la **perdita pinball** (`Accuracy.PinballLoss`, la stessa con cui il TFT si addestra) e la copertura (`Accuracy.IntervalCoverage`) |
| **Cosa manca** | reti di selezione, LSTM, attenzione multi-testa, addestramento |

## Riferimenti

- Lim, B., Arık, S. Ö., Loeff, N. & Pfister, T. (2021). *Temporal fusion transformers for interpretable multi-horizon time series forecasting.* International Journal of Forecasting (anteprima arXiv del 2019).
