# 6 · Il filtro di Kalman

Codice: [KalmanFilter.cs](../../../src/libs/MathemaSharp.TimeSeries/KalmanFilter.cs).

## Il modello

Uno stato nascosto $x_t$ evolve linearmente e si osserva solo attraverso una misura rumorosa:

$$x_t=F\,x_{t-1}+w_t,\;\;w_t\sim\mathcal N(0,Q)\qquad y_t=H\,x_t+v_t,\;\;v_t\sim\mathcal N(0,R)$$

La libreria tratta il caso di **osservazione scalare**: $H$ è un vettore riga, $y_t$ e $R$ numeri. Allora l'innovazione ha varianza scalare e **non serve invertire nessuna matrice**.

## Predict e Update

**Predict** (propaga stato e incertezza):

$$\hat x^-=F\hat x,\qquad P^-=F\,P\,F^\top+Q,\qquad S=H P^-H^\top+R$$

`Predict()` restituisce la previsione di $y$, $H\hat x^-$, e la sua varianza $S$.

**Update** (corregge con l'osservazione $y$):

$$v=y-H\hat x^-,\qquad K=\frac{P^-H^\top}{S},\qquad \hat x=\hat x^-+K\,v$$

### La covarianza in forma di Joseph

$$P=(I-KH)\,P^-\,(I-KH)^\top+K\,R\,K^\top$$

È algebricamente uguale alla più nota $P=(I-KH)P^-$, ma **resta simmetrica e semidefinita positiva** anche con gli arrotondamenti; la forma corta può perdere la simmetria e produrre varianze negative. Verifica `JosephCovarianceStaysSymmetric` (uguaglianza *esatta* $P_{01}=P_{10}$).

```csharp
var joseph = Multiply(Multiply(a, Covariance), Transpose(a));     // (I−KH) P⁻ (I−KH)ᵀ
for (...) joseph[i, j] += Gain[i] * _r * Gain[j];                 // + K R Kᵀ
```

### Dati mancanti

Un `NaN` è un dato mancante: **si salta l'update**. Lo stato resta la sola previsione e l'incertezza cresce di $Q$ a ogni passo vuoto. È il vantaggio del modello di stato sullo smoothing "a ricetta" (cap. 5): nessun artificio per i buchi. Verifica `MissingValuesSkipUpdateAndGrowUncertainty`: durante i buchi il guadagno è 0 e $S_t$ cresce.

### Log-verosimiglianza

Per la decomposizione dell'errore di previsione:

$$\ln L=-\tfrac12\sum_t\Big(\ln(2\pi S_t)+\frac{v_t^2}{S_t}\Big)$$

È ciò che si **massimizza per stimare $Q$ e $R$**. `Filter` la restituisce (i passi con dato mancante non contribuiscono).

## Local level e local linear trend

**Local level** (una passeggiata aleatoria osservata con rumore): $F=1$, $H=1$, $Q=q$, $R=r$.

$$\ell_t=\ell_{t-1}+\eta_t,\qquad y_t=\ell_t+\varepsilon_t$$

**Local linear trend**: stato $(\ell,b)$, $F=\begin{pmatrix}1&1\\0&1\end{pmatrix}$, $H=(1\;0)$, $Q=\mathrm{diag}(q_\ell,q_b)$. È la controparte di stato del metodo di Holt.

## Il guadagno a regime e l'equivalenza con SES

Nel local level, la varianza a priori $P^-$ dopo molti passi si stabilizza. Con $P^+=P^-r/(P^-+r)$ (varianza dopo l'update) e $P^-=P^++q$:

$$P^-=\frac{P^-r}{P^-+r}+q\;\Longrightarrow\;(P^-)^2-q\,P^--q\,r=0$$

(equazione di **Riccati** scalare). La radice positiva e il guadagno:

$$P^*=\frac{q+\sqrt{q^2+4qr}}{2},\qquad K^*=\frac{P^*}{P^*+r}$$

$K^*$ dipende **solo dal rapporto segnale/rumore $q/r$**. Ora l'update del livello è

$$\hat\ell=\hat\ell^-+K\,(y-\hat\ell^-)=K\,y+(1-K)\hat\ell^-$$

cioè **esattamente una riga di SES con $\alpha=K^*$**. Perciò:

> **SES = filtro di Kalman stazionario del local level**, e stimare $\alpha$ minimizzando l'SSE equivale a stimare $q/r$.

Con $q=0.5,\;r=2$: $q/r=0.25$, $K^*=0.3904$.

Verifiche:

- `LocalLevelGainConvergesToRiccatiSolution`: il guadagno del filtro converge a $K^*$ a $10^{-12}$; il primo passo, con prior diffuso ($P_0=10^6$), vale $K\approx1$;
- `SteadyStateKalmanIsSes`: dopo il transitorio le previsioni a un passo del Kalman e di SES($\alpha=K^*$) coincidono a $10^{-9}$;
- `KalmanSteadyStateGainInEveryFormat`.

Nel notebook [07](../../../notebooks/TimeSeries/07-ts-smoothing-kalman.ipynb) $\hat\alpha$ stimato con la sezione aurea (0.4301, da dati con $q/r=0.25$) è vicino a $K^*=0.3904$: stesso ordine, differenza dovuta al campione finito.

## Cosa non fa

Osservazione solo scalare; nessuno *smoother* (Rauch-Tung-Striebel); $Q$ e $R$ **non sono stimati** (si passano); nessun filtro non lineare (EKF, UKF, particelle). Vedi [ROADMAP](../../../ROADMAP.md).

← [5 · Smoothing esponenziale](05-smoothing.md) · [Indice](README.md) · [7 · VAR →](07-var.md)
