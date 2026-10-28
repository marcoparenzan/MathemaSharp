# 5 · Smoothing esponenziale: SES, Holt, damped trend, Holt-Winters

Codice: [ExponentialSmoothing.cs](../../../src/libs/MathemaSharp.TimeSeries/ExponentialSmoothing.cs).

L'idea (anni '50): **aggiorna il livello, poi aggiungi trend e stagionalità.** Tre ingredienti, un'ipotesi: i dati recenti contano più di quelli remoti, con pesi che decadono geometricamente.

## SES: solo il livello

$$\ell_t=\alpha\,y_t+(1-\alpha)\,\ell_{t-1},\qquad \hat y_{T+h}=\ell_T\quad(\ell_0=y_0)$$

Srotolando la ricorsione, il livello è una **media pesata con pesi geometrici**:

$$\ell_T=\sum_{j=0}^{T-1}\alpha(1-\alpha)^j\,y_{T-j}+(1-\alpha)^T\ell_0$$

- $\alpha\to0$: previsione = media (memoria lunga);
- $\alpha\to1$: previsione = naive (memoria zero).

Verifica `SesIsGeometricWeightedAverage`: $\ell_2=\alpha y_2+\alpha(1-\alpha)y_1+(1-\alpha)^2y_0$ esatto, e $\alpha=0\Rightarrow$ media, $\alpha=1\Rightarrow$ ultimo valore.

### Stimare $\alpha$: sezione aurea

Si minimizza l'SSE degli errori a un passo $\sum(y_t-\hat y_{t|t-1})^2$ su $[0,1]$. La **sezione aurea** riduce l'intervallo del fattore $1/\varphi\approx0.618$ a ogni passo **riusando un punto già valutato** (una sola valutazione per iterazione):

$$c=b-\tfrac1\varphi(b-a),\quad d=a+\tfrac1\varphi(b-a)$$

Presuppone un SSE **unimodale** in $\alpha$: tipico, non garantito. Tolleranza di default $\sqrt{\varepsilon_{\text{mach}}}$. Verifica `FitSesAlphaFindsLowAlphaForNoiseAndHighForRandomWalk`: rumore bianco → $\alpha<0.1$; passeggiata aleatoria → $\alpha>0.9$.

## Holt (1957) e il damped trend (Gardner & McKenzie, 1985)

Si aggiunge una pendenza $b_t$. Con smorzamento $\phi\in[0,1]$:

$$\hat y_{t|t-1}=\ell_{t-1}+\phi\,b_{t-1}$$
$$\ell_t=\alpha\,y_t+(1-\alpha)(\ell_{t-1}+\phi\,b_{t-1})$$
$$b_t=\beta\,(\ell_t-\ell_{t-1})+(1-\beta)\,\phi\,b_{t-1}$$

**Previsione a $h$ passi:**

$$\hat y_{T+h}=\ell_T+\Big(\sum_{i=1}^{h}\phi^i\Big)b_T=\ell_T+\frac{\phi(1-\phi^h)}{1-\phi}\,b_T\;\xrightarrow{h\to\infty}\;\ell_T+\frac{\phi}{1-\phi}\,b_T$$

- $\phi=1$: **Holt lineare**, il trend prosegue all'infinito.
- $\phi<1$: il trend **si spegne**: a orizzonti lunghi la previsione tende a una costante.

Empiricamente (competizioni M) è uno dei metodi più robusti: raramente il migliore, quasi mai disastroso.

```csharp
power *= phi;
cumulative += power;                            // φ + φ² + … + φʰ
result[h - 1] = level + cumulative * trend + season(h);
```

Verifiche (`HoltReproducesLinesAndDampingConverges`): una retta è riprodotta con SSE nullo; con $\phi=0.9$ la previsione a 500 passi coincide col limite $\ell_T+\phi b_T/(1-\phi)$ a $10^{-9}$.

## Holt-Winters additivo (1960)

Aggiunge $m$ valori stagionali $s_j$ (periodo $m$):

$$\ell_t=\alpha\,(y_t-s_{t-m})+(1-\alpha)(\ell_{t-1}+\phi b_{t-1})$$
$$s_t=\gamma\,(y_t-\ell_{t-1}-\phi b_{t-1})+(1-\gamma)\,s_{t-m}$$
$$\hat y_{T+h}=\ell_T+\Big(\sum_{i=1}^{h}\phi^i\Big)b_T+s_{T+h-m(k+1)},\quad k=\lfloor(h-1)/m\rfloor$$

### L'inizializzazione conta (e qui è esatta)

Con i primi due cicli:

$$b_0=\frac{\bar y_{\text{2° ciclo}}-\bar y_{\text{1° ciclo}}}{m},\qquad \ell_0=\bar y_{\text{1° ciclo}}+b_0\,\frac{m-1}{2},\qquad s_j=y_j-\Big(\bar y_{\text{1° ciclo}}+b_0\big(j-\tfrac{m-1}{2}\big)\Big)$$

**Perché lo spostamento di $(m-1)/2$.** La media del primo ciclo stima il livello **al centro** del ciclo (tempo $(m-1)/2$), non alla fine. Spostarla lungo il trend fino a $t=m-1$ e togliere dalla stagionalità iniziale sia il livello sia il trend fa sì che una retta più una stagionalità a media nulla venga **riprodotta esattamente**.

Fu un errore trovato proprio dal test: la prima versione usava la media del ciclo senza lo spostamento, e il test di esattezza (tolleranza 0.05) falliva. Ora `HoltWintersExactInEveryFormat` verifica un errore inferiore a $10^{-9}$ in `double` (tolleranza $2\cdot10^{-3}$ in `float`) su 8 passi di previsione.

## Parentele con altre famiglie

| Metodo | Equivale a |
|---|---|
| SES | ARIMA(0,1,1), $\theta=\alpha-1$ (cap. 4) — e al Kalman stazionario del local level (cap. 6) |
| Holt lineare | ARIMA(0,2,2) e Kalman local linear trend |
| Holt-Winters | modello ETS(A,A,A) del quadro in spazio degli stati (Hyndman et al., 2002) |

ETS e ARIMA sono **famiglie complementari**, non una l'aggiornamento dell'altra: si sovrappongono solo sui modelli lineari.

## Cosa non fa

Inizializzazioni **deterministiche, non stimate** (ETS le stimerebbe come parametri); niente stagionalità moltiplicativa; niente intervalli di previsione. Vedi [ROADMAP](../../../ROADMAP.md).

← [4 · AR e ARIMA](04-ar-arima.md) · [Indice](README.md) · [6 · Filtro di Kalman →](06-kalman.md)
