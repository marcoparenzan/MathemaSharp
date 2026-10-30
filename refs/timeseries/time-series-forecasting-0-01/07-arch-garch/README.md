# ARCH e GARCH

*(Reference image not included in the repository: third-party material.)*

> **Infografica** `time-series-forecasting-0-01` · sesta riga dell'infografica (1982 / 1986) · **1982 / 1986** · stato ◐ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** I modelli precedenti descrivono la **media**. Molte serie hanno una media quasi costante e una **varianza che cambia**: periodi calmi, periodi agitati, a grappoli (*volatility clustering*). Gli shock sono quasi incorrelati, ma i loro **quadrati** no.

**ARCH (Engle, 1982)** e **GARCH (Bollerslev, 1986):**

$$\varepsilon_t=\sigma_t z_t,\;z_t\sim\mathcal N(0,1)\qquad \sigma_t^2=\omega+\alpha\,\varepsilon_{t-1}^2+\beta\,\sigma_{t-1}^2.$$

- $\alpha$: reazione allo shock di ieri (la parte ARCH); $\beta$: memoria della varianza di ieri (la parte GARCH); **ARCH(1) è il caso $\beta=0$**.
- Stazionario in covarianza se $\alpha+\beta<1$; la varianza ritorna a $\bar\sigma^2=\omega/(1-\alpha-\beta)$.

**Previsione della varianza.** Decade geometricamente con ragione $\alpha+\beta$:

$$\hat\sigma^2_{T+h}=\bar\sigma^2+(\alpha+\beta)^{h-1}\big(\hat\sigma^2_{T+1}-\bar\sigma^2\big).$$

**Stima.** Massima verosimiglianza gaussiana con *variance targeting* ($\omega=s^2(1-\alpha-\beta)$) su una griglia raffinata.

**Cosa NON fa.** *«Volatility, not the mean»*, come dice l'infografica: non prevede il valore, prevede **quanto sarà incerto**.

**Stato: ◐ parziale.** Solo GARCH(1,1) con innovazioni gaussiane: niente GARCH($p,q$), code spesse (Student-$t$), effetti di leva (GJR, EGARCH).

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *Il mare calmo e il mare mosso* · **Durata:** ~90 s · **Personaggi:** Giulia, Anguria, Tommaso, voce narrante · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Il porto di una città. Il mare è piatto, poi, a tratti, si increspa, poi torna piatto. | *Didascalia:* «Il valore medio dell'onda non cambia. Cambia quanto si agita.» | media costante, varianza che cambia |
| 2 | Sul monitor di Giulia: la vibrazione del compressore. Le oscillazioni piccole, poi un gruppo di oscillazioni grandi, poi di nuovo piccole. | **Giulia:** «Le scosse grandi vengono insieme. Dopo una scossa grande ne arriva un'altra.» | volatility clustering |
| 3 | Anguria disegna due code: «ieri è stata agitata» e «ieri era tranquilla». Il termometro dell'agitazione di oggi dipende dalle due. | **Anguria:** «L'agitazione di oggi = un po' di base + la scossa di ieri + la memoria dell'agitazione di ieri.» | $\sigma_t^2=\omega+\alpha\varepsilon^2+\beta\sigma^2$ |
| 4 | Una molla che si carica quando c'è uno shock e poi si distende lentamente. | *Didascalia:* «Uno shock ci mette tempo a spegnersi: dipende da $\alpha+\beta$.» | persistenza |
| 5 | Didascalia con due date: 1982 e 1986. Una mano disegna una piccola «A» e poi una «G». | *Didascalia:* «1982, Engle: l'agitazione di ieri. 1986, Bollerslev: anche la sua memoria.» | ARCH → GARCH |
| 6 | Tre giorni di vibrazione compressa in un fotogramma. Le prime 48 ore: il mare calmo. Poi qualcosa cambia. | **Anguria:** «Il cuscinetto sta cedendo. Ma nessuno lo sa ancora.» | cambio di regime (scenario A) |
| 7 | Un campanello. Un orologio mostra 71 minuti dopo l'inizio del cambiamento. | **Tommaso:** «Il cambiamento è iniziato a mezzanotte. L'allarme?» — **Giulia:** «71 minuti dopo. Nessun falso allarme prima.» | **il momento del cambiamento non è il momento dell'allarme** |
| 8 | Il grafico dell'allarme: lo 0.5 % dei minuti il primo giorno, il 23 % l'ultimo. | **Anguria:** «Non dice cosa si rompe. Dice che è cambiato.» | il GARCH come campanello |

**Morale:** *a volte il segnale non è il valore, ma quanto trema.*

## Nel repository

| | |
|---|---|
| **Stato** | ◐ parziale: solo GARCH(1,1) gaussiano (ARCH(1) è il caso β = 0) |
| **Capitolo** | [8 · ARCH e GARCH](../../00-formule-e-libreria/08-garch.md) |
| **Libreria** | [`Garch11`](../../../../src/libs/MathemaSharp.TimeSeries/Garch.cs): `ConditionalVariances`, `LogLikelihood`, `ForecastVariance`, `FitVarianceTargeting` |
| **Notebook C#** | [08](../../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-csharp.ipynb) |
| **Notebook PySharp** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-pysharp.ipynb) |
| **In fabbrica** | scenario A, sezione 4: stima $(0.0019,\,0.083,\,0.809)$ contro i veri $(0.002,\,0.10,\,0.80)$; allarme in 0.5 % dei minuti il primo giorno e 22.7 % l'ultimo; scatta 71 minuti dopo il cambio vero |

## Riferimenti

- Engle, R. F. (1982). *Autoregressive conditional heteroscedasticity with estimates of the variance of United Kingdom inflation.* Econometrica.
- Bollerslev, T. (1986). *Generalized autoregressive conditional heteroskedasticity.* Journal of Econometrics.
