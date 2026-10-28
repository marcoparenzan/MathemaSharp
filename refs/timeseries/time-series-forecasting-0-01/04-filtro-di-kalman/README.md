# Filtro di Kalman

![terza riga dell'infografica (1960)](riga.png)

> **Infografica** [`time-series-forecasting-0-01`](../time-series-forecasting-0-01.jpg) · terza riga dell'infografica (1960) · **1960** · stato ✅ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** *Predici uno stato nascosto; correggilo con le nuove osservazioni.* Lo stato $x_t$ evolve linearmente e si vede solo attraverso una misura rumorosa:

$$x_t=F\,x_{t-1}+w_t,\;w_t\sim\mathcal N(0,Q)\qquad y_t=H\,x_t+v_t,\;v_t\sim\mathcal N(0,R).$$

**Predict** (propaga stato e incertezza): $\hat x^-=F\hat x$, $\;P^-=FPF^\top+Q$.

**Update** (corregge con l'osservazione): $v=y-H\hat x^-$, $\;K=P^-H^\top/S$, $\;\hat x=\hat x^-+Kv$, con $S=HP^-H^\top+R$.

Il **guadagno** $K$ è la fiducia data all'osservazione: alto se la misura è affidabile ($R$ piccolo) o se lo stato è incerto ($P^-$ grande).

**Dati mancanti.** Se $y_t$ manca, si salta l'update: lo stato resta la sola previsione e l'incertezza **cresce** di $Q$ a ogni passo. È il vantaggio sul smoothing «a ricetta».

**Equivalenza con SES.** Nel *local level* il guadagno converge a $K^*=P^*/(P^*+r)$, con $P^*=(q+\sqrt{q^2+4qr})/2$ (equazione di Riccati). L'update diventa una riga di SES con $\alpha=K^*$.

**Limiti.** Lineare e gaussiano; qui con osservazione scalare e $Q$, $R$ **non stimati** (si passano). *Non indovina ciò che non vede*: durante un buco predice, non scopre.

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *Il detective e l'indizio* · **Durata:** ~90 s · **Personaggi:** Anguria, Giulia, Tommaso, voce narrante · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Anguria in impermeabile e cappello da detective. Davanti a lei, una cella frigorifera sullo sfondo, nel buio. | **Anguria:** «Sono certa che la cella sia a 2 gradi. Ma il sensore mente un po'.» | stato nascosto + misura rumorosa |
| 2 | Giulia porge un foglio: la lettura del sensore, 2.16, poi 2.10, poi 2.09: a scatti. | **Giulia:** «Rumore. Il sensore sbaglia di 0.08 gradi.» | il rumore di misura $R$ |
| 3 | Anguria disegna una freccia: «dove dovrebbe essere ora, secondo me». | *Didascalia:* «**Prevedi.** Dal punto di prima, e da come si muove la cella.» | predict |
| 4 | Arriva un indizio, la nuova lettura. Il detective confronta freccia e indizio. | *Didascalia:* «**Correggi.** Quanto ti fidi dell'indizio dipende da quanto sei sicura di te.» | update e guadagno $K$ |
| 5 | Una manopola «fiducia» con due estremi: «mi fido solo di me» / «mi fido solo del sensore». | **Anguria:** «Il guadagno gira da solo. Se il rumore del processo è un quarto di quello di misura, a regime si ferma a 0.39.» | guadagno stazionario |
| 6 | Il gateway perde il segnale: il quadro dei dati diventa vuoto per 12 minuti. Anguria continua a disegnare frecce, ma le frecce diventano sempre più sfocate. | **Anguria:** «Senza indizi posso solo dedurre. E più passa il tempo, meno sono sicura.» | dati mancanti: l'incertezza cresce |
| 7 | Il segnale torna: la temperatura è saltata a 4.5 gradi. Anguria guarda la freccia, ferma a 2.6. | **Tommaso:** «Quando è salita sopra i 4?» — **Anguria:** «Non lo so. Era dentro il buio. Dico: tra 7 e 19 minuti.» | **non si indovina ciò che non si vede**; il KPI come intervallo (scenario A: verità 11) |
| 8 | Didascalia con una piccola capsula spaziale sullo sfondo. | *Didascalia:* «1960, Kalman. Lo stesso metodo ha poi guidato le missioni Apollo.» | importanza storica |

**Morale:** *prevedi, poi correggi: e quando non vedi, dillo.*

## Nel repository

| | |
|---|---|
| **Stato** | ✅ implementata (osservazione scalare, local level, local linear trend, forma di Joseph, dati mancanti) |
| **Capitolo** | [6 · Il filtro di Kalman](../../00-formule-e-libreria/06-kalman.md) |
| **Libreria** | [`KalmanFilter`](../../../../src/libs/MathemaSharp.TimeSeries/KalmanFilter.cs): `LocalLevel`, `LocalLinearTrend`, `LocalLevelSteadyStateGain`, `Filter` |
| **Notebook C#** | [07](../../../../notebooks/TimeSeries/07-ts-smoothing-kalman.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-csharp.ipynb) |
| **Notebook PySharp** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-pysharp.ipynb) |
| **In fabbrica** | scenario A: temperatura con 6 buchi di comunicazione; osservati 7 minuti sopra +4 °C, Kalman 8, intervallo onesto 7–19, verità 11 |

## Riferimenti

- Kalman, R. E. (1960). *A new approach to linear filtering and prediction problems.* Journal of Basic Engineering.
- Durbin, J. & Koopman, S. J. *Time Series Analysis by State Space Methods.* Oxford University Press.
