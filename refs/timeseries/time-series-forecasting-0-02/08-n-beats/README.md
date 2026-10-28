# N-BEATS

![ottava riga dell'infografica (2019)](riga.png)

> **Infografica** [`time-series-forecasting-0-02`](../time-series-forecasting-0-02.jpg) · ottava riga dell'infografica (2019) · **2019** · stato ⬜ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** Una **catena di blocchi completamente connessi** che si passano il residuo. L'infografica: *«Stack fully connected blocks with backcast and forecast outputs»* e *«No recurrence or attention required»*.

**Il blocco.** Riceve un segmento di storia $x$ e produce **due** uscite:

- una **retrocessione** (*backcast*) $\hat x$: la parte della storia che il blocco riesce a spiegare;
- una **previsione** (*forecast*) $\hat y$: il contributo del blocco al futuro.

**La catena (*doubly residual stacking*).** Il blocco successivo riceve ciò che il precedente **non ha spiegato**: $x_{\ell+1}=x_\ell-\hat x_\ell$. La previsione finale è la **somma** delle previsioni di tutti i blocchi:

$$\hat y=\sum_{\ell}\hat y_\ell.$$

**Perché interessa.**

- è una rete *profonda ma semplice*: nessuna ricorrenza, nessuna attenzione;
- nella versione **interpretabile** le uscite dei blocchi sono vincolate a **basi di polinomi** (trend) e **di Fourier** (stagionalità): la previsione si scompone in trend + stagionalità, come fa STL, ma **appresa**;
- gli autori hanno riportato risultati superiori ai vincitori della competizione M4.

**Limiti.** Orizzonte e finestra di ingresso fissati al momento dell'addestramento; serie univariata; richiede addestramento su molti dati.

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *La catena dei correttori* · **Durata:** ~80 s · **Personaggi:** Anguria, Giulia, Tommaso · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Una catena di montaggio con cinque ispettori in fila. Sul nastro, un foglio con la storia degli ultimi giorni. | **Anguria:** «Ogni ispettore guarda il foglio e dice cosa capisce.» | blocchi in serie |
| 2 | Il primo ispettore ha due vaschette: «cosa ho spiegato del passato» e «cosa ne deduco per il futuro». | **Anguria:** «Spiegazione e previsione. Due uscite.» | backcast e forecast |
| 3 | Il primo ispettore sottrae ciò che ha spiegato dal foglio e passa il foglio ridotto al secondo. | **Giulia:** «Il secondo vede solo ciò che il primo non ha capito.» | il residuo passa al blocco successivo |
| 4 | Il foglio si fa sempre più vuoto lungo la catena. | **Anguria:** «Sempre meno da spiegare.» | residuo che si riduce |
| 5 | In fondo alla catena, tutte le vaschette «futuro» vengono versate in un unico imbuto. | **Anguria:** «La previsione è la somma dei contributi di tutti.» | somma delle previsioni |
| 6 | La versione interpretabile: il primo ispettore ha una sola matita che disegna polinomi (curve lente), il secondo una che disegna onde. | **Tommaso:** «Mi ricorda il mixer.» — **Anguria:** «Sì: trend e stagionalità. Ma qui i pezzi li impara la rete.» | analogia con STL ([0-01, scheda 09](../../time-series-forecasting-0-01/09-stl/README.md)) |
| 7 | Un cartello: «niente memoria, niente attenzione». Gli ispettori sono tutti uguali. | *Didascalia:* «2019. Una rete semplice che nelle prove degli autori batte i vincitori di M4.» | semplicità e risultati (secondo gli autori) |

**Morale:** *se ogni correttore spiega un pezzo e passa il resto, il futuro è la somma dei pezzi.*

## Nel repository

| | |
|---|---|
| **Stato** | ⬜ **non implementata** |
| **Cosa c'è di vicino** | la **scomposizione in componenti** (trend + stagionalità + resto) esiste con `Stl.Decompose` ([cap. 9](../../00-formule-e-libreria/09-stl-loess.md)): STL scompone con filtri fissi, N-BEATS interpretabile con basi apprese. Il [notebook 09](../../../../notebooks/TimeSeries/09-ts-stl-ndsharp.ipynb) prevede per componenti e confronta con Holt-Winters |
| **Cosa manca** | rete profonda, addestramento, basi polinomiali e di Fourier apprese |

## Riferimenti

- Oreshkin, B. N., Carpov, D., Chapados, N. & Bengio, Y. (2020). *N-BEATS: neural basis expansion analysis for interpretable time series forecasting.* ICLR (anteprima arXiv del 2019).
