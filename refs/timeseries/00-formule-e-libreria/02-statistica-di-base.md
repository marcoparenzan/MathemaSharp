# 2 · Statistica di base: autocorrelazione, PACF, Ljung-Box

Codice: [SeriesStatistics.cs](../../../src/libs/MathemaSharp.TimeSeries/SeriesStatistics.cs), [SpecialFunctions.cs](../../../src/libs/MathemaSharp.TimeSeries/SpecialFunctions.cs).

## Autocovarianza e autocorrelazione

$$\hat\gamma(k)=\frac1n\sum_{t=1}^{n-k}(y_t-\bar y)(y_{t+k}-\bar y),\qquad \hat\rho(k)=\frac{\hat\gamma(k)}{\hat\gamma(0)}$$

**Perché il divisore è $n$ e non $n-k$.** Lo stimatore con $n$ è distorto, ma la matrice di Toeplitz $\Gamma_{ij}=\hat\gamma(|i-j|)$ che ne deriva è **semidefinita positiva**: ogni sequenza di autocovarianze è quella di un processo stazionario reale. Con $n-k$ questa garanzia cade, e con essa quella che segue sulla PACF.

```csharp
for (var k = 0; k <= maxLag; k++)
{
    var sum = T.Zero;
    for (var t = 0; t + k < y.Length; t++) sum += (y[t] - mean) * (y[t + k] - mean);
    result[k] = sum / N<T>(y.Length);
}
```

Verifica: `AcfStartsAtOneAndDifferenceShortens`. Serie costante → $\hat\gamma(0)=0$ e l'ACF non è definita (`ArgumentException`).

## PACF e ricorsione di Levinson-Durbin

La **PACF** al lag $k$ è l'ultimo coefficiente $\phi_{kk}$ del miglior AR($k$): la correlazione fra $y_t$ e $y_{t-k}$ **tolte** le influenze dei lag intermedi. Si ottiene risolvendo le equazioni di Yule-Walker

$$\gamma(j)=\sum_{i=1}^{p}\phi_i\,\gamma(j-i),\qquad j=1..p$$

cioè $\Gamma\phi=\gamma$. Invertire $\Gamma$ costa $O(p^3)$; la struttura di Toeplitz permette $O(p^2)$ con la ricorsione:

$$\kappa_k=\frac{\gamma(k)-\sum_{j=1}^{k-1}\phi_{k-1,j}\,\gamma(k-j)}{v_{k-1}},\qquad \phi_{k,k}=\kappa_k,$$
$$\phi_{k,j}=\phi_{k-1,j}-\kappa_k\,\phi_{k-1,k-j}\;(j<k),\qquad v_k=v_{k-1}\,(1-\kappa_k^2),\quad v_0=\gamma(0)$$

$\kappa_k$ è la **PACF al lag $k$**; $v_k$ è la varianza d'innovazione dell'AR($k$).

```csharp
var kappa = acc / variance;                      // κ_k
phi[k - 1] = kappa;
for (var j = 1; j < k; j++) phi[j - 1] = previous[j - 1] - kappa * previous[k - j - 1];
variance *= T.One - kappa * kappa;               // v_k
```

**Perché $|\kappa_k|\le1$.** Dalla semidefinitezza di $\Gamma$: $v_k\ge0$ per ogni $k$, quindi $1-\kappa_k^2\ge0$. Ne segue anche che **l'AR di Yule-Walker stimato è sempre stazionario** (nessuna radice del polinomio caratteristico sul o dentro il cerchio unitario).

**Firma di un AR($p$)**: ACF che decade, PACF che si **annulla** dopo il lag $p$. Su un AR(1) teorico con $\phi=0.6$, $\gamma(k)\propto0.6^k$: $\kappa_1=0.6$, $\kappa_{k>1}=0$, $v_1=1-0.36=0.64$.

Verifiche: `LevinsonDurbinOnTheoreticalAr1Acf`, `LevinsonDurbinMatchesDirectYuleWalkerSolve` (confronto con la soluzione diretta $O(p^3)$ via QR), `LevinsonDurbinInEveryFormat`.

## Differenze

$$\Delta_m y_t=y_t-y_{t-m}$$

Con $m=1$ toglie un trend lineare, con $m$ pari al periodo toglie una stagionalità. Restituisce $n-m$ valori.

> **Attenzione a NumPy.** `np.diff(y, n=2)` è la differenza del *secondo ordine* ($\Delta\Delta y$), non quella a distanza 2: la differenza stagionale in NumPy si scrive `y[m:] - y[:-m]`.

## Test di Ljung-Box

Se i residui di un modello sono rumore bianco, le loro autocorrelazioni sono vicine a zero. Il test le somma:

$$Q=n(n+2)\sum_{k=1}^{h}\frac{\hat\rho_k^{2}}{n-k}\;\sim\;\chi^2_{\,h-p-q}\quad\text{(approssimativamente)}$$

con $h$ lag e $p+q$ parametri stimati. **P-value piccolo ⇒ è rimasta struttura nei residui**: il modello non ha catturato tutta la dinamica. Il p-value è $P(\chi^2_{\nu}\ge Q)=Q(\nu/2,\,Q/2)$, con $Q(a,x)$ gamma incompleta superiore regolarizzata.

Verifica: `LjungBoxSeparatesWhiteNoiseFromAr`: rumore bianco → $p>0.05$; AR(1) con $\phi=0.7$ → $p<10^{-10}$.

## Funzioni speciali (per il p-value)

**$\ln\Gamma$ con Lanczos** ($g=7$, 9 coefficienti):

$$\ln\Gamma(x+1)\approx\tfrac12\ln(2\pi)+\left(x+\tfrac12\right)\ln t-t+\ln A_g(x),\quad t=x+g+\tfrac12$$

con $A_g(x)=c_0+\sum_{i=1}^{8}\frac{c_i}{x+i}$. Errore relativo ~$10^{-15}$. Per $x<\tfrac12$ si usa $\Gamma(x)=\Gamma(x+1)/x$.

**Gamma incompleta superiore** $Q(a,x)=\Gamma(a,x)/\Gamma(a)$:

- per $x<a+1$, **serie** $P(a,x)=\dfrac{e^{-x}x^a}{\Gamma(a)}\displaystyle\sum_{n\ge0}\frac{x^n}{a(a+1)\cdots(a+n)}$ e $Q=1-P$;
- per $x\ge a+1$, **frazione continua** (algoritmo di Lentz) $Q=\dfrac{e^{-x}x^a}{\Gamma(a)}\cdot\cfrac{1}{x+1-a-\cfrac{1(1-a)}{x+3-a-\cfrac{2(2-a)}{x+5-a-\cdots}}}$.

Ciascuna converge in fretta nella propria regione. Quando il termine scende sotto $\max(\varepsilon_{\text{mach}},10^{-16})\cdot|\text{somma}|$ si ferma: oltre non ha senso, perché le costanti di Lanczos valgono ~$10^{-16}$.

Verifiche con valori noti, in ogni formato (`ChiSquareSurvivalInEveryFormat`): $\chi^2_2$: $P(X\ge x)=e^{-x/2}$ esatto; $\chi^2_1$ al 95%: $x=3.841458820694124$.

← [1 · Generic Math](01-generic-math.md) · [Indice](README.md) · [3 · Riferimenti e metriche →](03-riferimenti-e-metriche.md)
