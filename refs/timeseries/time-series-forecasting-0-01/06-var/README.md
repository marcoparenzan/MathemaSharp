# VAR: modello vettoriale autoregressivo

*(Reference image not included in the repository: third-party material.)*

> **Infografica** `time-series-forecasting-0-01` · quinta riga dell'infografica (1980) · **1980** · stato ✅ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** Più serie insieme, ciascuna spiegata dai **propri ritardi e da quelli delle altre**:

$$Y_t=c+A_1Y_{t-1}+\dots+A_pY_{t-p}+\varepsilon_t,\qquad Y_t\in\mathbb R^k.$$

L'elemento $A_i[r,s]$ misura l'effetto della serie $s$ al lag $i$ sulla serie $r$. Per due serie e un ritardo:

$$x_t=c_x+a_{11}x_{t-1}+a_{12}y_{t-1}+\varepsilon^x_t,\qquad y_t=c_y+a_{21}x_{t-1}+a_{22}y_{t-1}+\varepsilon^y_t.$$

**Chi l'ha inventato?** L'infografica lo dice chiaramente: *«Sims: a landmark application, not VAR's invention»*. Sims (1980, *Macroeconomics and Reality*) lo usò in macroeconomia come applicazione di riferimento; il modello esiste prima di lui.

**Stima.** OLS equazione per equazione: con gli stessi regressori in ogni equazione coincide con lo stimatore SUR/GLS, quindi non si perde efficienza.

**Stabilità.** Stazionario se gli autovalori di $A_1$ stanno nel cerchio unitario; per $2\times2$, $\lambda=(\operatorname{tr}\pm\sqrt{\operatorname{tr}^2-4\det})/2$. La media stazionaria è $\mu=(I-A_1)^{-1}c$.

**La risposta a un impulso** $A_1^h e_j$ mostra come una perturbazione su una serie si propaga all'altra.

**Limiti.** Molti parametri ($k^2p$); solo VAR(1) bivariato per il controllo di stabilità; niente scelta dell'ordine, né Granger, né impulsi ortogonalizzati.

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *Il termostato e il compressore si parlano* · **Durata:** ~80 s · **Personaggi:** Giulia, Anguria, Tommaso · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Due curve sul monitor: la temperatura della cella e la corrente del compressore. Si muovono insieme, ma con uno scarto. | **Giulia:** «Quando sale una, poco dopo si muove l'altra.» | due serie legate |
| 2 | Due personaggi-fumetto: «TEMPERATURA» (un termometro) e «CORRENTE» (una presa). Parlano con due balloon. | **Temperatura:** «Sono calda!» — **Corrente:** «Allora parto!» | la dipendenza incrociata |
| 3 | La corrente sale: una freccia verso il basso sulla temperatura. | **Corrente:** «Adesso ti raffreddo.» | effetto negativo della corrente sulla temperatura |
| 4 | Anguria sulla lavagna: due equazioni accoppiate, con quattro frecce che si incrociano. | **Anguria:** «Ognuna guarda se stessa **e** l'altra, al passo precedente.» | forma del VAR(1) |
| 5 | Si accendono i numeri sulle frecce: «+9.06 A per grado» e «−0.033 gradi per ampere». | **Tommaso:** «Come lo sai?» — **Anguria:** «Non l'ho scritto io. Lo ha ritrovato dai dati.» | il VAR ritrova la logica del termostato (numeri veri, scenario A) |
| 6 | In un angolo, uno studioso con un libro: «Macroeconomics and Reality». | *Didascalia:* «1980. Sims lo usa per l'economia. Ma il modello c'era già: lui lo ha reso famoso.» | applicazione, non invenzione |
| 7 | Un colpo sulla temperatura: un'onda che si propaga fino alla corrente e si spegne. | **Giulia:** «Se la porta resta aperta, quanto ci mette il compressore a rispondere?» | risposta a un impulso |

**Morale:** *le serie non vivono sole: la loro conversazione è un'informazione.*

## Nel repository

| | |
|---|---|
| **Stato** | ✅ implementata (VAR($p$) per OLS; stabilità solo per VAR(1) bivariato) |
| **Capitolo** | [7 · VAR](../../00-formule-e-libreria/07-var.md) |
| **Libreria** | [`VarModel.Fit`](../../../../src/libs/MathemaSharp.TimeSeries/VectorAutoregression.cs), `Forecast`, `IsStableOrder1Bivariate`; `Interop.Var` per PySharp (matrici appiattite) |
| **Notebook C#** | [08](../../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-csharp.ipynb) |
| **Notebook PySharp** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-pysharp.ipynb) |
| **In fabbrica** | scenario A, sezione 5: corrente ← temperatura **+9.06 A/°C**; temperatura ← corrente **−0.033**: la logica del termostato, ritrovata dai dati |

## Riferimenti

- Sims, C. A. (1980). *Macroeconomics and reality.* Econometrica.
- Lütkepohl, H. (2005). *New Introduction to Multiple Time Series Analysis.* Springer.
