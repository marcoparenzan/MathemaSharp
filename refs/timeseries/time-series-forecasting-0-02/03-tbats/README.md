# TBATS: stagionalità lunghe, multiple e non intere

![terza riga dell'infografica (2011)](riga.png)

> **Infografica** [`time-series-forecasting-0-02`](../time-series-forecasting-0-02.jpg) · terza riga dell'infografica (2011) · **2011** · stato ⬜ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**Il problema.** Holt-Winters e STL gestiscono **una** stagionalità, di periodo **intero** e non troppo lungo. Ma molte serie ne hanno più d'una (un ciclo giornaliero e uno settimanale) e a volte con periodo **non intero**: un anno ha 365.25 giorni, non 365.

**L'idea (De Livera, Hyndman, Snyder, 2011).** Il nome è l'elenco dei componenti: **T**rigonometric seasonality, **B**ox-Cox transform, **A**RMA errors, **T**rend, **S**easonal components. La stagionalità non è una tabella di $m$ valori da ricordare, ma una **somma di sinusoidi**,

$$s_t^{(i)}=\sum_{j=1}^{k_i}s_{j,t}^{(i)},\qquad s_{j,t}^{(i)}=s_{j,t-1}^{(i)}\cos\lambda_j+s^{*(i)}_{j,t-1}\sin\lambda_j+\gamma\,d_t,$$

(e l'analoga per la componente gemella $s^{*}$, sfasata di un quarto di giro), con frequenza $\lambda_j=2\pi j/m_i$ **che può essere un numero reale qualunque**, e ampiezza e fase che **evolvono nel tempo** (gli *stati stagionali* dell'infografica). La trasformazione di **Box-Cox** stabilizza la varianza; gli errori seguono un **ARMA**.

**Perché una sinusoide e non una tabella.** Con periodo 365.25 una tabella da 365 valori sarebbe sbagliata, e per periodi lunghi sarebbe una quantità enorme di stati: poche armoniche ($k_i$ piccolo) bastano.

**Limiti.** Stima lenta su serie lunghe; le stagionalità devono essere indicate dall'analista; fornisce intervalli di previsione gaussiani.

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *Il calendario con due orologi* · **Durata:** ~80 s · **Personaggi:** Anguria, Giulia, voce narrante · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | I consumi orari del magazzino: onde che si ripetono ogni giorno, ma l'altezza delle onde cambia da lunedì a domenica. | **Giulia:** «C'è il ritmo del giorno e c'è il ritmo della settimana.» | due stagionalità |
| 2 | Anguria tiene in mano un orologio con una sola lancetta. | **Anguria:** «Holt-Winters ha un solo orologio. Sceglie giorno o settimana.» | un solo periodo |
| 3 | Un secondo orologio, grande come una ruota, con sette spicchi. | **Anguria:** «Qui servono due: la lancetta delle ore e la ruota dei giorni.» | periodi multipli |
| 4 | Il calendario: un anno è un cerchio con un piccolo spazio che non si chiude. | *Didascalia:* «Un anno ha 365 giorni e un quarto. Nessuna tabella intera lo contiene.» | periodo non intero |
| 5 | Una sinusoide sul tavolo. Anguria la fa girare con un angolo che non è un numero tondo. | **Anguria:** «Una sinusoide gira a qualunque velocità. Anche a una che non torna a casa in un numero intero di passi.» | stagionalità trigonometrica |
| 6 | Due sinusoidi sommate: il risultato imita il ciclo giornaliero con le sue gobbe. | **Anguria:** «Poche sinusoidi bastano per fare una forma complicata. E ampiezza e fase possono cambiare piano piano.» | poche armoniche, stati che evolvono |
| 7 | Il nome TBATS scritto su cinque tessere: T, B, A, T, S. Anguria le gira una alla volta. | *Didascalia:* «2011. Sinusoidi, trasformazione di Box-Cox, errori ARMA, trend, componenti stagionali.» | l'acronimo |

**Morale:** *quando il calendario non si divide in parti uguali, le sinusoidi non hanno bisogno di farlo.*

## Nel repository

| | |
|---|---|
| **Stato** | ⬜ **non implementata** |
| **Cosa c'è di vicino** | una stagionalità per volta con `ExponentialSmoothing.HoltWinters` ([cap. 5](../../00-formule-e-libreria/05-smoothing.md)) e con `Stl.Decompose` ([cap. 9](../../00-formule-e-libreria/09-stl-loess.md)); nello [scenario A](../../../../scenari/A-magazzino-frigorifero/README.md) il ciclo giornaliero (24 h) e quello settimanale (168 h) vengono provati **separatamente**, e il periodo 168 batte il 24 per MASE (0.918 contro 1.871) |
| **Cosa manca** | stagionalità trigonometrica con periodi reali, stagionalità multiple nello stesso modello, trasformazione di Box-Cox, errori ARMA nello spazio degli stati |

## Riferimenti

- De Livera, A. M., Hyndman, R. J. & Snyder, R. D. (2011). *Forecasting time series with complex seasonal patterns using exponential smoothing.* Journal of the American Statistical Association.
