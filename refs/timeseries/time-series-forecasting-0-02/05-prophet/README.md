# Prophet

![quinta riga dell'infografica (2017)](riga.png)

> **Infografica** [`time-series-forecasting-0-02`](../time-series-forecasting-0-02.jpg) · quinta riga dell'infografica (2017) · **2017** · stato ⬜ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** Un modello **decomponibile** e *configurabile da un analista*: la serie è la somma di tre componenti più rumore,

$$y(t)=g(t)+s(t)+h(t)+\varepsilon_t.$$

L'infografica lo precisa: *«A decomposable model, not a neural network»*.

| Componente | Che cos'è |
|---|---|
| **Trend** $g(t)$ | lineare a tratti (o logistico, con una capacità) con **punti di cambio** (*changepoints*): la pendenza può cambiare in certi istanti, e il numero e la flessibilità dei cambi si regolano |
| **Stagionalità** $s(t)$ | somma di **serie di Fourier**: $s(t)=\sum_{n=1}^{N}\big(a_n\cos\frac{2\pi nt}{P}+b_n\sin\frac{2\pi nt}{P}\big)$, con $P$ = 7 giorni, 365.25 giorni… |
| **Festività ed eventi** $h(t)$ | un effetto per ogni evento (Natale, la chiusura di agosto, una promozione), con una finestra attorno alla data |

Si stima in modo bayesiano (stima MAP o campionamento con Stan) e si **ottengono intervalli** simulando.

**Perché è popolare.** Pensato per analisti non specialisti con serie di *business*: tollera dati mancanti e outlier, si **regola con parametri leggibili** («quanto cambia il trend», «quanto è forte la stagionalità») e si possono **inserire a mano** conoscenze di dominio (le date dei fermi).

**Limiti.** Non modella la dipendenza a breve termine (nessun termine autoregressivo): i residui possono essere autocorrelati; la configurabilità non è una garanzia di accuratezza: va sempre confrontato con un benchmark ([0-01, scheda 01](../../time-series-forecasting-0-01/01-previsioni-di-riferimento/README.md)).

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *Il lego del trend, della stagione e delle feste* · **Durata:** ~80 s · **Personaggi:** Anguria, Giulia, Tommaso · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Lo stabilimento: un calendario con Ferragosto cerchiato e un cartello «chiuso per ferie». Il grafico della produzione ha una buca. | **Giulia:** «Ogni agosto la linea si ferma. E ogni agosto i modelli sbagliano.» | gli eventi di calendario rompono la previsione |
| 2 | Anguria apre una scatola di mattoncini: tre colori, TREND, STAGIONE, FESTE. | **Anguria:** «Costruisco la serie con tre mattoncini.» | decomposizione additiva |
| 3 | Il mattoncino TREND: una linea spezzata. Su un punto, Giulia mette una bandierina: «qui è cambiata la pendenza». | **Giulia:** «Qui abbiamo aggiunto un secondo turno.» | punti di cambio del trend |
| 4 | Il mattoncino STAGIONE: una curva liscia fatta sommando poche onde. | **Anguria:** «Settimana e anno: poche onde sommate.» | serie di Fourier |
| 5 | Il mattoncino FESTE: picchi verticali, uno per ogni data cerchiata. Giulia li aggiunge a mano. | **Giulia:** «Questo lo so io, non lo può imparare il modello.» | conoscenza di dominio inserita a mano |
| 6 | I tre mattoncini si incastrano e la somma imita la produzione. Anguria alza il cartello «non è una rete neurale». | **Anguria:** «È un lego. Ogni pezzo si guarda e si regola da solo.» | trasparenza |
| 7 | Tommaso con un metro in mano: misura lo scarto rispetto a «come la settimana scorsa». | **Tommaso:** «Bello da vedere. Ma batte il banale?» — **Anguria:** «Questa domanda si fa a ogni modello.» | confronto con il benchmark |

**Morale:** *un modello che puoi smontare in tre pezzi è un modello di cui ti puoi fidare, o dubitare, con cognizione.*

## Nel repository

| | |
|---|---|
| **Stato** | ⬜ **non implementata** |
| **Cosa c'è di vicino** | la parte «decomposizione» è `Stl.Decompose` ([cap. 9](../../00-formule-e-libreria/09-stl-loess.md)); trend e stagionalità di Fourier si potrebbero stimare con `MatrixAlgebra.LeastSquares` aggiungendo colonne seno/coseno e variabili indicatrici per le festività, come nello [scenario D](../../../../scenari/D-fotovoltaico/README.md) si aggiungono covariate |
| **Cosa manca** | trend lineare a tratti con punti di cambio, stima bayesiana, intervalli per simulazione |

## Riferimenti

- Taylor, S. J. & Letham, B. (2018). *Forecasting at scale.* The American Statistician (anteprima del 2017).
