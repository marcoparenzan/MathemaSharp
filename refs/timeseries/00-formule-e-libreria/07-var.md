# 7 · VAR: più serie, ritardi incrociati

Codice: [VectorAutoregression.cs](../../../src/libs/MathemaSharp.TimeSeries/VectorAutoregression.cs).

## Il modello (Sims, 1980)

Con $k$ serie raccolte in un vettore $Y_t\in\mathbb R^k$:

$$Y_t=c+A_1Y_{t-1}+\dots+A_pY_{t-p}+\varepsilon_t,\qquad \varepsilon_t\sim(0,\Sigma)$$

L'elemento $A_i[r,s]$ misura l'effetto della serie $s$ al lag $i$ sulla serie $r$: **ogni serie dipende dai propri ritardi e da quelli delle altre**. Sims lo usò in macroeconomia come applicazione di punta: il VAR non è una sua invenzione.

Per $k=2,\;p=1$: $\;x_t=c_x+a_{11}x_{t-1}+a_{12}y_{t-1}+\varepsilon^x_t$, $\;y_t=c_y+a_{21}x_{t-1}+a_{22}y_{t-1}+\varepsilon^y_t$.

## Stima

**OLS equazione per equazione.** Le $k$ equazioni hanno gli **stessi regressori**: in questo caso l'OLS singolo coincide con lo stimatore GLS/SUR di Zellner, quindi non si perde efficienza a stimarle separatamente. La matrice dei regressori ha $1+kp$ colonne:

$$X_t=\big(1,\;Y_{t-1}^\top,\;\dots,\;Y_{t-p}^\top\big),\qquad \text{righe }t=p+1..n$$

Ogni equazione si risolve con la QR di Householder (cap. 10). La covarianza dei residui usa il divisore $n-p-(1+kp)$ (gradi di libertà):

$$\hat\Sigma=\frac{1}{n-p-(1+kp)}\,\hat E^\top\hat E$$

Verifica `FitRecoversBivariateVar1`: 20 000 punti, $|\hat A-A|<0.02$ per ogni elemento, e $\hat\Sigma\approx I$ (covarianza fuori diagonale nulla entro 0.05).

## Stabilità e media stazionaria

Un VAR($1$) è **stazionario** se tutti gli autovalori di $A_1$ stanno dentro il cerchio unitario. Per $2\times2$, con $\operatorname{tr}$ e $\det$:

$$\lambda_{\pm}=\frac{\operatorname{tr}\pm\sqrt{\operatorname{tr}^2-4\det}}{2}$$

- discriminante $\ge0$: autovalori reali, si controlla $|\lambda_\pm|<1$;
- discriminante $<0$: coniugati complessi con $|\lambda|^2=\det$, quindi la condizione è $\det<1$ (nessuna radice complessa da calcolare).

La **media stazionaria** è $\mu=(I-A_1)^{-1}c$. La previsione ricorsiva vi converge:

$$\hat Y_{T+h}=c+\sum_{i=1}^{p}A_i\hat Y_{T+h-i}\;\xrightarrow{h\to\infty}\;\mu$$

Verifica `ForecastConvergesToStationaryMean`: con $A=\begin{pmatrix}0.5&0.2\\-0.1&0.3\end{pmatrix}$, $c=(1,1)$ si ha $\det(I-A)=0.37$ e $\mu=\big(\tfrac{0.9}{0.37},\tfrac{0.4}{0.37}\big)$, raggiunta a $10^{-9}$.

## La risposta a un impulso

Uno shock unitario sulla serie $j$ al tempo 0 produce al tempo $h$ lo scarto $A_1^h e_j$ dalla traiettoria di riferimento. Il notebook [08](../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb) lo ottiene sottraendo due previsioni (con e senza shock). Mostra **come una perturbazione su una serie si propaga all'altra**: per un impianto, «se la pressione sale, quanto e quando risale la temperatura?».

## Cosa non fa

Niente criteri di scelta dell'ordine $p$ (AIC/BIC), niente test di causalità di Granger, niente impulsi ortogonalizzati (fattore di Cholesky di $\Sigma$), niente cointegrazione (VECM). `IsStableOrder1Bivariate` copre solo il VAR(1) bivariato: per ordini maggiori servirebbero gli autovalori della matrice compagna, e **`np.linalg.eigvals` non esiste neppure nel NumPy di PySharp**.

← [6 · Filtro di Kalman](06-kalman.md) · [Indice](README.md) · [8 · ARCH e GARCH →](08-garch.md)
