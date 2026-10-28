# Smoothing esponenziale: SES, Holt, Holt-Winters

![seconda riga dell'infografica (anni '50 – 1960)](riga.png)

> **Infografica** [`time-series-forecasting-0-01`](../time-series-forecasting-0-01.jpg) · seconda riga dell'infografica (anni '50 – 1960) · **anni '50 – 1960** · stato ✅ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** *Aggiorna il livello, poi aggiungi trend e stagionalità.* I dati recenti contano più di quelli remoti, con pesi che decadono in modo geometrico.

**SES (anni '50): solo il livello.**

$$\ell_t=\alpha\,y_t+(1-\alpha)\,\ell_{t-1},\qquad \hat y_{T+h}=\ell_T.$$

Srotolando: $\ell_T=\sum_{j\ge0}\alpha(1-\alpha)^j\,y_{T-j}+\dots$ cioè una **media pesata con pesi geometrici**. Con $\alpha\to0$ la previsione tende alla media (memoria lunga); con $\alpha\to1$, al naive (memoria zero).

**Holt (1957): + trend.** Si aggiunge una pendenza $b_t$ con la propria costante $\beta$; la previsione è una retta $\hat y_{T+h}=\ell_T+h\,b_T$.

**Holt-Winters (1960): + stagionalità** di periodo $m$, con la propria costante $\gamma$:

$$s_t=\gamma\,(y_t-\ell_{t-1}-b_{t-1})+(1-\gamma)\,s_{t-m}.$$

**Parentele.** SES equivale ad ARIMA(0,1,1) con $\theta=\alpha-1$, e al filtro di Kalman stazionario del *local level*. Holt lineare equivale ad ARIMA(0,2,2). ETS e ARIMA sono famiglie complementari.

**Limiti.** Inizializzazione non stimata; niente stagionalità moltiplicativa; niente intervalli di previsione.

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *La memoria che sbiadisce* · **Durata:** ~80 s · **Personaggi:** Anguria, Giulia, voce narrante · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Giulia davanti a una fila di ricambi. Sulla scrivania, i consumi delle ultime settimane come una pila di fogli. | **Giulia:** «Quanti cuscinetti ordino per la prossima settimana?» | un problema di scorte, dove il metodo si usa ancora |
| 2 | Anguria sfoglia la pila: l'ultimo foglio è nitido, quelli sotto sempre più sbiaditi. | **Anguria:** «L'ultima settimana conta tanto, quella di un mese fa quasi niente.» | peso che decade nel tempo |
| 3 | Una tazza di caffè che si raffredda: il calore dei sorsi vecchi si disperde. | *Didascalia:* «Ogni nuovo dato pesa $\alpha$; tutto il passato pesa $1-\alpha$.» | la formula di SES in una immagine |
| 4 | Un cursore $\alpha$: a sinistra «memoria lunga, previsione piatta come la media», a destra «memoria zero, previsione come l'ultimo valore». | **Anguria:** «Il cursore decide quanto fidarsi dell'ultimo dato.» | il parametro $\alpha$ |
| 5 | Tre matrioske. La più piccola: «livello». | *Didascalia:* «Anni '50: il livello.» | SES |
| 6 | Si apre la seconda: una freccia inclinata. | *Didascalia:* «1957. Holt aggiunge la pendenza: se i consumi salgono, la previsione sale.» | trend |
| 7 | Si apre la terza: un'onda che si ripete. | *Didascalia:* «1960. Winters aggiunge la stagione: il lunedì somiglia ai lunedì.» | stagionalità |
| 8 | Giulia ripete il calcolo sui consumi del magazzino: il modello a 168 ore. | **Giulia:** «Sul totale della settimana sbaglia solo dello 0.3 %.» | caso reale (scenario A: Holt-Winters 168 h, +0.3 %) |

**Morale:** *ricorda tutto, ma dimentica con eleganza.*

## Nel repository

| | |
|---|---|
| **Stato** | ✅ implementata (SES, Holt, Holt-Winters additivo, con damping) |
| **Capitolo** | [5 · Smoothing esponenziale](../../00-formule-e-libreria/05-smoothing.md) |
| **Libreria** | [`ExponentialSmoothing`](../../../../src/libs/MathemaSharp.TimeSeries/ExponentialSmoothing.cs): `Ses`, `Holt`, `HoltWinters`, `FitSesAlpha` |
| **Notebook C#** | [07](../../../../notebooks/TimeSeries/07-ts-smoothing-kalman.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-csharp.ipynb) |
| **Notebook PySharp** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-pysharp.ipynb) |
| **In fabbrica** | scenario A, sezione 3: il totale settimanale previsto con Holt-Winters 168 h sbaglia dello +0.3 % |

## Riferimenti

- Holt, C. C. (1957). *Forecasting seasonals and trends by exponentially weighted moving averages.* Office of Naval Research (pubblicato in International Journal of Forecasting, 2004).
- Winters, P. R. (1960). *Forecasting sales by exponentially weighted moving averages.* Management Science.
- Hyndman, R. J. et al. (2002). *A state space framework for automatic forecasting using exponential smoothing methods.* International Journal of Forecasting.
