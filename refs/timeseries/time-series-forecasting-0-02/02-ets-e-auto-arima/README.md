# Quadro ETS e Auto ARIMA

![seconda riga dell'infografica (2002 / 2008)](riga.png)

> **Infografica** [`time-series-forecasting-0-02`](../time-series-forecasting-0-02.jpg) · seconda riga dell'infografica (2002 / 2008) · **2002 / 2008** · stato ⬜ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** Rendere i modelli statistici **facili da stimare e da scegliere su larga scala**. L'infografica la riassume con due scatole: *ETS (2002) → verosimiglianza + intervalli* e *Auto ARIMA (2008) → seleziona gli ordini + stima*.

**Il quadro ETS (Hyndman, Koehler, Snyder, Grose, 2002).** Ogni metodo di smoothing esponenziale (cap. 5) diventa un **modello in spazio degli stati con errore innovativo**, con una verosimiglianza. Il nome è la tripla (**E**rrore, **T**rend, **S**tagione):

| Componente | Opzioni |
|---|---|
| Errore | additivo (A), moltiplicativo (M) |
| Trend | nessuno (N), additivo (A), additivo smorzato (A$_d$), moltiplicativo (M), moltiplicativo smorzato (M$_d$) |
| Stagione | nessuna (N), additiva (A), moltiplicativa (M) |

cioè $2\times5\times3=30$ combinazioni. Per ciascuna si stimano parametri **e stati iniziali** per massima verosimiglianza, e si scelgono con un criterio di informazione (AICc). Il guadagno rispetto allo smoothing «a ricetta»: **intervalli di previsione** e un modo oggettivo di scegliere.

*Esempio:* ETS(A,A$_d$,A) è Holt-Winters additivo con trend smorzato.

**Auto ARIMA (Hyndman & Khandakar, 2008).** Automatizza il metodo di Box-Jenkins (cap. 4): sceglie $d$ (e $D$) con test di radice unitaria (KPSS), poi cerca $(p,q,P,Q)$ con una **ricerca a gradini** che minimizza l'AICc, e stima per massima verosimiglianza.

**Attenzione.** L'AICc confronta modelli **sulla stessa serie differenziata**: non si può confrontare ARIMA con $d$ diversi, né ETS con ARIMA, con l'AICc. L'automazione non sostituisce il controllo dei residui.

**Limiti.** Previsioni *locali*: un modello per serie, nessuna condivisione fra serie (il tema dell'altra metà dell'infografica, vedi DeepAR).

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *Il catalogo e il sommelier* · **Durata:** ~90 s · **Personaggi:** Giulia, Tommaso, Anguria, voce narrante · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Giulia davanti a un muro di 200 grafici, uno per ogni compressore e cella dello stabilimento. | **Giulia:** «Duecento serie. Non posso scegliere il modello a mano per ognuna.» | il problema della scala |
| 2 | Anguria porta un catalogo con tre colonne a tendina: ERRORE, TREND, STAGIONE. | *Didascalia:* «2002. Hyndman e colleghi: trenta combinazioni, ognuna con la sua verosimiglianza.» | il quadro ETS: 2 × 5 × 3 |
| 3 | Anguria sfoglia: «piatto, senza stagione» fino a «trend smorzato, stagione additiva». | **Anguria:** «Sono i metodi dello smoothing, ma adesso ognuno è un modello vero.» | smoothing → modello in spazio degli stati |
| 4 | Un sommelier con il calice: assaggia un modello, dà un punteggio che premia l'aderenza e punisce la complicazione. | **Tommaso:** «Rispetto a cosa?» — **Anguria:** «Rispetto al punteggio AICc: più è basso, meglio è.» | selezione con criterio di informazione |
| 5 | Un secondo catalogo: ordini ARIMA. Il sommelier prova una combinazione, poi la vicina, e si sposta dove il punteggio scende. | *Didascalia:* «2008. Auto ARIMA: cerca a gradini fra gli ordini e sceglie il migliore.» | ricerca a gradini |
| 6 | Il sommelier mostra due calici su tavoli diversi, uno con un cartello «una differenza», l'altro «due differenze». Un cartello rosso: «non confrontabili». | **Tommaso:** «Non puoi mettere a confronto modelli con differenze diverse.» — **Anguria:** «Giusto. Le differenze si scelgono prima, con un test.» | l'AICc non confronta tutto |
| 7 | Sul tavolo di Giulia, 200 previsioni con la fascia di incertezza. Tommaso ne guarda una: i residui. | **Tommaso:** «Le ho guardate lo stesso.» | l'automazione non toglie il controllo |

**Morale:** *automatizzare la scelta non vuol dire smettere di guardare i residui.*

## Nel repository

| | |
|---|---|
| **Stato** | ⬜ **non implementata**, ma con una parentela stretta |
| **Cosa c'è di vicino** | `ExponentialSmoothing.HoltWinters` (additivo, con $\phi$) dà le **stesse previsioni puntuali di ETS(A,A,A)** e di ETS(A,A$_d$,A), ma con parametri **passati a mano**, inizializzazione deterministica e **senza intervalli**. Nel [notebook 08](../../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb) il confronto fra ordini ARIMA si fa **a mano** con un AIC approssimato |
| **Cosa manca** | verosimiglianza esatta in spazio degli stati, stima degli stati iniziali, selezione automatica (AICc), test KPSS, ricerca a gradini |
| **Capitoli** | [5 · Smoothing](../../00-formule-e-libreria/05-smoothing.md), [4 · AR e ARIMA](../../00-formule-e-libreria/04-ar-arima.md) |

## Riferimenti

- Hyndman, R. J., Koehler, A. B., Snyder, R. D. & Grose, S. (2002). *A state space framework for automatic forecasting using exponential smoothing methods.* International Journal of Forecasting.
- Hyndman, R. J. & Khandakar, Y. (2008). *Automatic time series forecasting: the forecast package for R.* Journal of Statistical Software.
- Hyndman, R. J. & Athanasopoulos, G. *Forecasting: Principles and Practice* (2ª ed.), cap. 7 e 8.
