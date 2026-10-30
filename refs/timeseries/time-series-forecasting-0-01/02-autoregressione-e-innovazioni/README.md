# Autoregressione e innovazioni

*(Reference image not included in the repository: third-party material.)*

> **Infografica** `time-series-forecasting-0-01` · prima riga dell'infografica (1927–1938) · **1927–1938** · stato ✅ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea (Yule, 1927).** Predire un valore dai valori precedenti, più uno shock casuale:

$$y_t-\mu=\sum_{i=1}^{p}\phi_i\,(y_{t-i}-\mu)+\varepsilon_t,\qquad \varepsilon_t\sim\text{rumore bianco}(0,\sigma^2).$$

Yule studiò i numeri delle macchie solari di Wolfer, che oscillano con un periodo di circa 11 anni. Il metodo dell'epoca cercava una sinusoide nascosta più rumore. L'idea nuova: **un ciclo può nascere dalla sola dinamica**, senza nessuna sinusoide, se ogni valore dipende dai due precedenti e viene spinto da shock casuali.

**Il ciclo nelle radici (AR(2)).** Con $\phi_1^2+4\phi_2<0$ le radici del polinomio $1-\phi_1z-\phi_2z^2$ sono complesse: l'autocorrelazione è una sinusoide smorzata di periodo $2\pi/\theta$, con $\cos\theta=\phi_1/(2\sqrt{-\phi_2})$. Con $\phi=(1.6,-0.9)$ il periodo è ≈ 11.07 passi.

**Stazionarietà.** Le radici devono stare **fuori** dal cerchio unitario. La stima di Yule-Walker (con Levinson-Durbin) la garantisce.

**Le innovazioni.** $\varepsilon_t$ è la parte di $y_t$ **non prevedibile** dal passato: la sorpresa.

**Wold (1938): un teorema, non un modello.** Ogni processo stazionario a media nulla si scrive come somma di shock passati, $y_t=\sum_{j\ge0}\psi_j\varepsilon_{t-j}$ più una parte deterministica. L'AR finito ne è un'approssimazione parsimoniosa. L'infografica lo dice: *«AR illustration; Wold is a theorem»*.

**Limiti.** Lineare; assume stazionarietà (serve differenziare se c'è trend); l'ordine $p$ va scelto.

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *Il pendolo e i piselli* · **Durata:** ~90 s · **Personaggi:** Anguria, Giulia, voce narrante · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Sepia. Un registro di numeri: le macchie solari, anno per anno, che salgono e scendono. | *Didascalia:* «1927. Gli astronomi contano le macchie del Sole: ogni 11 anni circa, tornano.» | un ciclo osservato |
| 2 | Una lavagna con una sinusoide e, sopra, puntini di rumore. | *Didascalia:* «Il metodo di allora: cercare un'onda perfetta nascosta nel disturbo.» | l'idea precedente: segnale periodico + rumore |
| 3 | Anguria disegna un pendolo. Intorno, ragazzini con una cerbottana sparano piselli a caso. | **Anguria:** «E se nessuna onda fosse nascosta? Se fosse solo il pendolo, spinto a caso?» *Didascalia:* «L'immagine, secondo la tradizione, è dello stesso Yule.» | **un ciclo senza sinusoide**: nasce dalla dinamica |
| 4 | Il pendolo oscilla con un ritmo suo, anche se i piselli arrivano a caso. | **Anguria:** «Il ritmo è del pendolo. Gli spintoni sono casuali.» | memoria + spinta casuale |
| 5 | Magazzino frigorifero. Giulia guarda la temperatura della cella: sale, scende, sale. Una porta si apre, un compressore parte. | **Giulia:** «Anche la mia cella è un pendolo. La porta e il termostato la spingono.» | stesso schema su un impianto |
| 6 | Sulla lavagna compare la formula, con due caselle «memoria» e un fulmine «spinta». | **Anguria:** «$y(t)=\phi_1\,y(t-1)+\phi_2\,y(t-2)+\varepsilon(t)$. Due memorie e una spinta.» | la formula dell'AR(2) |
| 7 | Un orologio gira a 1938. Un cassetto con scritto «tutto ciò che è stazionario». | *Didascalia:* «1938. Wold dimostra: qualunque processo stazionario è una somma di spinte passate.» | Wold è un **teorema**, non un modello |
| 8 | Anguria con il pennarello su un grafico di una stima: «0.5995 e −0.2008». | **Anguria:** «Stimati da 580 punti. I veri erano 0.6 e −0.2.» | la stima ritrova i parametri (numeri veri del notebook 10) |

**Morale:** *un ritmo può nascere dalla sola memoria, spinta da shock casuali.*

## Nel repository

| | |
|---|---|
| **Stato** | ✅ implementata (Yule-Walker); Wold è un teorema, non una funzione |
| **Capitoli** | [2 · Statistica di base](../../00-formule-e-libreria/02-statistica-di-base.md) (ACF, PACF, Levinson-Durbin), [4 · AR e ARIMA](../../00-formule-e-libreria/04-ar-arima.md) |
| **Libreria** | [`ArModel.FitYuleWalker`](../../../../src/libs/MathemaSharp.TimeSeries/Arima.cs), `SeriesStatistics.Acf/Pacf/LevinsonDurbin` |
| **Notebook C#** | [06](../../../../notebooks/TimeSeries/06-ts-reference-autoregression.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb) |
| **Notebook PySharp** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb) |

## Riferimenti

- Yule, G. U. (1927). *On a method of investigating periodicities in disturbed series, with special reference to Wolfer's sunspot numbers.* Philosophical Transactions of the Royal Society A.
- Wold, H. (1938). *A study in the analysis of stationary time series.* Uppsala.
