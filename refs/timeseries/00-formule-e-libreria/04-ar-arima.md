# 4 · AR e ARIMA: da Yule a Box-Jenkins

Codice: [Arima.cs](../../../src/libs/MathemaSharp.TimeSeries/Arima.cs) (`ArModel`, `ArimaModel`).

## AR($p$): l'idea di Yule (1927)

$$y_t-\mu=\sum_{i=1}^{p}\phi_i\,(y_{t-i}-\mu)+\varepsilon_t,\qquad \varepsilon_t\sim\text{rumore bianco}(0,\sigma^2)$$

Yule modellò le macchie solari come un AR(2) guidato da shock casuali: un **ciclo senza sinusoidi**, solo dinamica.

**Stazionarietà.** Il processo è stazionario se le radici di $1-\phi_1z-\dots-\phi_pz^p$ sono **fuori** dal cerchio unitario. La stima di Yule-Walker (cap. 2) lo garantisce.

**Il ciclo nelle radici (AR(2)).** Con $\phi_1^2+4\phi_2<0$ le radici sono complesse coniugate: l'ACF è una sinusoide smorzata di periodo

$$P=\frac{2\pi}{\theta},\qquad \cos\theta=\frac{\phi_1}{2\sqrt{-\phi_2}}$$

Con $\phi=(1.6,-0.9)$: $\cos\theta=0.843$, $\theta=0.567$, $P\approx11.07$ passi.

**Previsione.** Ricorsiva: i valori futuri ignoti sono sostituiti dalle loro previsioni.

$$\hat y_{T+h}=\mu+\sum_{i=1}^{p}\phi_i\,(\hat y_{T+h-i}-\mu)\;\xrightarrow{h\to\infty}\;\mu$$

```csharp
var value = Mean;
for (var i = 0; i < Phi.Length; i++) value += Phi[i] * (buffer[^(i + 1)] - Mean);
buffer.Add(value);
```

Verifica: `YuleWalkerRecoversAr2` (20 000 punti: $\hat\phi=(0.50,-0.30)\pm0.02$), `ArForecastDecaysToMean`.

## Wold (1938): un teorema, non un modello

Ogni processo stazionario a media nulla si scrive

$$y_t=\sum_{j=0}^{\infty}\psi_j\,\varepsilon_{t-j}+\text{parte deterministica},\qquad \psi_0=1$$

L'AR finito ne è un'approssimazione parsimoniosa. I pesi $\psi_j$ (la risposta a un impulso unitario) si calcolano da $\phi$ e $\theta$:

$$\psi_j=\theta_j+\sum_{i=1}^{\min(j,p)}\phi_i\,\psi_{j-i}\qquad(\theta_j=0\text{ per }j>q)$$

Per ARMA(1,1): $\psi_1=\phi+\theta$, poi decadimento geometrico di ragione $\phi$. Il notebook [08](../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb) li disegna.

## ARIMA($p,d,q$): Box-Jenkins (1970)

Si differenzia $d$ volte per togliere la non stazionarietà, $w=\Delta^d y$, poi si adatta un ARMA:

$$w_t=c+\sum_{i=1}^{p}\phi_i\,w_{t-i}+e_t+\sum_{j=1}^{q}\theta_j\,e_{t-j}$$

> **MA = innovazioni ritardate, non una media mobile.** Il termine $\theta_j e_{t-j}$ è lo *shock* di $j$ passi fa, una quantità non osservata, non una media dei dati.

**Residui (CSS).** Con $e_t=0$ per $t<p$, poi $e_t=w_t-\hat w_{t|t-1}$. `Residuals` restituisce $n-d$ valori.

**Previsione.** Si estende $w$ con la ricorsione ARMA (innovazioni future $=0$) e si **integra** $d$ volte a partire dall'ultimo valore di ogni livello:

```csharp
for (var k = D - 1; k >= 0; k--)
{
    var last = levels[k][^1];
    for (var h = 0; h < horizon; h++) forecast[h] = last += forecast[h];
}
```

Casi limite verificati: ARIMA(0,1,0) senza costante = **naive**; ARIMA(0,2,0) = prosecuzione dell'ultima pendenza (`Arima010IsNaiveAndArima020ExtrapolatesLine`).

## Stima: Hannan-Rissanen (1982)

Il termine MA contiene innovazioni *non osservate*: la regressione diretta non è possibile. Due passi lineari:

1. **AR lungo** (ordine $m=\max(p+q,\lceil10\log_{10}n\rceil)$) stimato per OLS: i suoi residui sono le innovazioni stimate $\hat e_t$.
2. **OLS di $w_t$** su $[1,\,w_{t-1..p},\,\hat e_{t-1..q}]$.

Ogni regressione usa la **QR di Householder** (cap. 10), non le equazioni normali. La varianza d'innovazione è la somma dei quadrati dei residui divisa per i gradi di libertà.

**Cosa NON è.** Non è la massima verosimiglianza esatta: è un **ottimo stimatore iniziale**, consistente ma meno efficiente. Stazionarietà e invertibilità **non sono imposte**. Su 20 000 punti ritrova ARMA(1,1) $\phi=0.6,\theta=0.4$ entro $\pm0.04$ (`HannanRissanenRecoversArma11`).

**Scelta dell'ordine.** AIC approssimato $n\ln\hat\sigma^2+2k$ più Ljung-Box sui residui (cap. 2). Il notebook [08](../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb) confronta cinque ordini.

## L'identità che lega ARIMA a SES

Il SES $\ell_t=\alpha y_t+(1-\alpha)\ell_{t-1}$ si riscrive, con l'errore a un passo $e_t=y_t-\ell_{t-1}$, come

$$\ell_t=\ell_{t-1}+\alpha e_t\quad\Longrightarrow\quad\hat y_{t+1}=y_t-(1-\alpha)\,e_t$$

che è un ARIMA(0,1,1) $\;y_{t+1}=y_t+e_{t+1}+\theta e_t\;$ con **$\theta=\alpha-1$**. Le previsioni coincidono (stessi valori a $10^{-9}$ in `double`, in ogni formato con tolleranza adeguata): `SesEqualsArima011InEveryFormat`.

← [3 · Riferimenti e metriche](03-riferimenti-e-metriche.md) · [Indice](README.md) · [5 · Smoothing esponenziale →](05-smoothing.md)
