# Box-Jenkins e ARIMA

![quarta riga dell'infografica (1970)](riga.png)

> **Infografica** [`time-series-forecasting-0-01`](../time-series-forecasting-0-01.jpg) · quarta riga dell'infografica (1970) · **1970** · stato ◐ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** Non un modello, ma un **metodo sistematico**: identificare, stimare, controllare i residui, e ripetere finché i residui sono rumore bianco.

**Il modello.** Si differenzia $d$ volte per togliere la non stazionarietà, $w=\Delta^d y$, e si adatta un ARMA:

$$w_t=c+\sum_{i=1}^{p}\phi_i\,w_{t-i}+e_t+\sum_{j=1}^{q}\theta_j\,e_{t-j}.$$

> **MA = innovazioni ritardate, non una media mobile.** Il termine $\theta_j e_{t-j}$ è lo *shock* di $j$ passi fa (una quantità non osservata), non una media dei dati. L'infografica lo ricorda: *«MA = lagged innovations, not a rolling average»*.

**I tre passi.**

1. **Identificazione**: ACF e PACF suggeriscono gli ordini (un AR($p$) ha PACF che si annulla dopo $p$; un MA($q$) ha ACF che si annulla dopo $q$).
2. **Stima**: qui con **Hannan-Rissanen** (due regressioni lineari: AR lungo per ricostruire le innovazioni, poi OLS su ritardi e innovazioni).
3. **Controllo**: i residui devono essere rumore bianco; il test di **Ljung-Box** lo verifica.

**Previsione.** Si estende l'ARMA con innovazioni future nulle e si **integra** $d$ volte.

**Stato: ◐ parziale.** La libreria stima con Hannan-Rissanen, che è un ottimo stimatore iniziale ma **non la massima verosimiglianza esatta**; stazionarietà e invertibilità **non sono imposte**; nessuna selezione automatica dell'ordine.

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *I tre atti del metodo* · **Durata:** ~85 s · **Personaggi:** Tommaso, Giulia, Anguria, voce narrante · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Un sipario. Sul palco, un cartello: «Atto 1 · Identificare». | *Didascalia:* «1970. Box e Jenkins scrivono un libro: previsione come mestiere, non come intuizione.» | un metodo, non un trucco |
| 2 | Giulia mostra una serie che sale senza fermarsi, come una rampa. | **Anguria:** «Non sta ferma: prima la rendiamo piatta. Differenza: oggi meno ieri.» | differenziare per rendere stazionaria |
| 3 | Due istogrammi, ACF e PACF, come impronte digitali. Anguria li legge. | **Anguria:** «Questa si ferma dopo due barre: ricorda due passi.» | l'identificazione |
| 4 | «Atto 2 · Stimare». Una bilancia mette insieme i coefficienti. | *Didascalia:* «Due regressioni: la prima ricostruisce gli shock passati, la seconda li usa.» | Hannan-Rissanen |
| 5 | Un fantasma sul palco con la scritta «eco dell'errore di ieri». | **Anguria:** «Il "MA" non è una media mobile. È l'eco degli errori di ieri.» | MA = innovazioni ritardate |
| 6 | «Atto 3 · Controllare». Tommaso con il timbro in mano guarda i residui: devono essere monotoni come il rumore di un frigorifero. | **Tommaso:** «Se nei residui c'è ancora una melodia, il modello non ha capito tutto.» | residui bianchi, Ljung-Box |
| 7 | Una tabella di cinque modelli. Il (1,1,1) ha un p-value di 0.034, il (2,1,2) 0.015. | **Giulia:** «Anche il migliore non è perfetto.» | scelta dell'ordine con AIC e Ljung-Box (notebook 08) |
| 8 | Il sipario si richiude, poi si riapre sull'«Atto 1». | **Anguria:** «È un ciclo: si ricomincia finché i residui tacciono.» | il metodo è iterativo |

**Morale:** *un modello non si fida: si interroga, e si ascoltano i residui.*

## Nel repository

| | |
|---|---|
| **Stato** | ◐ parziale: Hannan-Rissanen, non la massima verosimiglianza esatta; nessuna selezione automatica |
| **Capitolo** | [4 · AR e ARIMA](../../00-formule-e-libreria/04-ar-arima.md) |
| **Libreria** | [`ArimaModel.FitHannanRissanen`](../../../../src/libs/MathemaSharp.TimeSeries/Arima.cs), `Residuals`, `Forecast`; `SeriesStatistics.LjungBox` |
| **Notebook C#** | [08](../../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb) |
| **Notebook PySharp** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb) |
| **Risultato** | notebook 08: ARIMA(1,1,1) su 400 punti, RMSE fuori campione 3.81 contro 10.63 del naive e 5.36 del drift. Con 400 punti i parametri singoli sono rumorosi ($\hat\phi=0.40$, $\hat\theta=0.54$ contro 0.5 e 0.4 veri) |

## Riferimenti

- Box, G. E. P. & Jenkins, G. M. (1970). *Time Series Analysis: Forecasting and Control.* Holden-Day.
- Hannan, E. J. & Rissanen, J. (1982). *Recursive estimation of mixed autoregressive-moving average order.* Biometrika.
