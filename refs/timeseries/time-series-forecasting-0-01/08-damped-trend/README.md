# Damped trend

*(Reference image not included in the repository: third-party material.)*

> **Infografica** `time-series-forecasting-0-01` · settima riga dell'infografica (1985) · **1985** · stato ✅ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** Il trend lineare di Holt prosegue **all'infinito**: un'ipotesi quasi mai vera. Il *damped trend* **smorza** il contributo della pendenza agli orizzonti lunghi, con un parametro $\phi\in[0,1]$.

$$\hat y_{t|t-1}=\ell_{t-1}+\phi\,b_{t-1},\quad \ell_t=\alpha y_t+(1-\alpha)(\ell_{t-1}+\phi b_{t-1}),\quad b_t=\beta(\ell_t-\ell_{t-1})+(1-\beta)\phi b_{t-1}.$$

**Previsione a $h$ passi:**

$$\hat y_{T+h}=\ell_T+\Big(\sum_{i=1}^{h}\phi^i\Big)b_T=\ell_T+\frac{\phi(1-\phi^h)}{1-\phi}\,b_T\;\xrightarrow{h\to\infty}\;\ell_T+\frac{\phi}{1-\phi}\,b_T.$$

- $\phi=1$: Holt lineare, la retta non si ferma;
- $\phi<1$: il trend **si spegne**, la previsione tende a una costante.

È ciò che l'infografica mostra: *«Linear trend»* tratteggiato che sale, *«Damped»* che si piega. Empiricamente (competizioni M) è uno dei metodi più robusti: raramente il migliore, quasi mai disastroso.

**Limiti.** $\phi$ va scelto o stimato (qui si passa); non è un modello probabilistico (nessun intervallo).

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *La crescita che si stanca* · **Durata:** ~70 s · **Personaggi:** Anguria, Giulia, Tommaso · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Una pianta in vaso, cresciuta tanto da toccare il soffitto di cartone. Sulla lavagna di Anguria, una linea di crescita che sale. | **Anguria:** «Questa pianta cresce di 5 centimetri al mese. Quanto sarà alta tra dieci anni?» | il trend lineare |
| 2 | Anguria disegna la retta: attraversa il soffitto, le nuvole, la luna. | **Tommaso:** «Alta sei metri? Non è un palazzo.» | estrapolare all'infinito è assurdo |
| 3 | Un impianto: una nuova linea di produzione, i pezzi al giorno crescono nelle prime settimane e poi la crescita rallenta. | **Giulia:** «Il nostro ramp-up fa così: all'inizio sale, poi si assesta.» | la crescita reale si stanca |
| 4 | Un cursore $\phi$ con una molla: a 1 la retta prosegue; a 0.85 si piega. | **Anguria:** «Ogni mese la crescita pesa un po' meno del mese prima: la moltiplico per $\phi$.» | lo smorzamento geometrico |
| 5 | Il grafico: la linea tratteggiata (lineare) schizza via, la linea piena (smorzata) si adagia su un piano. | *Didascalia:* «1985. Gardner e McKenzie smorzano il trend.» | $\ell+\phi b/(1-\phi)$ |
| 6 | Una bilancia con due numeri: «Holt lineare: errore 4.34» e «Damped: errore 1.53». | **Tommaso:** «Come lo sai che funziona?» — **Anguria:** «Su una crescita che rallenta, l'errore cala di due terzi.» | numeri veri del notebook 07 (RMSE a 20 passi) |
| 7 | Il limite: una linea orizzontale a 84.5 sul grafico. | **Anguria:** «Questa è dove si ferma la previsione. Lo so già oggi.» | il limite asintotico |

**Morale:** *niente cresce per sempre: la previsione non deve crederci.*

## Nel repository

| | |
|---|---|
| **Stato** | ✅ implementata (Holt e Holt-Winters con $\phi$) |
| **Capitolo** | [5 · Smoothing esponenziale](../../00-formule-e-libreria/05-smoothing.md) |
| **Libreria** | [`ExponentialSmoothing.Holt(y, α, β, horizon, phi)`](../../../../src/libs/MathemaSharp.TimeSeries/ExponentialSmoothing.cs), `HoltWinters(…, phi)` |
| **Notebook C#** | [07](../../../../notebooks/TimeSeries/07-ts-smoothing-kalman.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb) |
| **Notebook PySharp** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb) |
| **Risultato** | notebook 07, crescita che rallenta: RMSE a 20 passi **4.34** (Holt lineare) contro **1.53** (damped, $\phi=0.85$); limite asintotico 84.5 |

## Riferimenti

- Gardner, E. S. & McKenzie, E. (1985). *Forecasting trends in time series.* Management Science.
