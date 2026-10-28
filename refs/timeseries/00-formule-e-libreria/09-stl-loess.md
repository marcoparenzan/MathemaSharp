# 9 · LOESS e STL: decomporre prima, prevedere dopo

Codice: [Stl.cs](../../../src/libs/MathemaSharp.TimeSeries/Stl.cs).

## LOESS: una retta locale

Per ogni punto $x$ si adatta una **retta pesata** ai $q$ punti più vicini; il valore stimato è il valore della retta in $x$. I pesi sono **tricubici**:

$$w_i=\Big(1-\Big(\frac{|x_i-x|}{h}\Big)^{3}\Big)^{3}\;\text{ per }|x_i-x|<h,\qquad 0\text{ altrimenti}$$

dove $h$ è la distanza dal $q$-esimo vicino. Con i pesi $w_i$ (moltiplicati per i pesi di robustezza) la retta dei minimi quadrati pesati ha

$$\bar x=\frac{\sum w_ix_i}{\sum w_i},\quad\bar y=\frac{\sum w_iy_i}{\sum w_i},\quad \text{slope}=\frac{\sum w_ix_iy_i/\sum w_i-\bar x\bar y}{\sum w_ix_i^2/\sum w_i-\bar x^2},\quad \hat y(x)=\bar y+\text{slope}\,(x-\bar x)$$

```csharp
var slope = variance > C<T>(1e-12) * T.Max(T.One, meanX * meanX)
    ? (swxy / sw - meanX * meanY) / variance : T.Zero;     // grado 0 se i punti pesati sono degeneri
return meanY + slope * (x - meanX);
```

**Proprietà usata dai test**: una retta è riprodotta *esattamente*, anche estrapolando fuori dall'intervallo (`LoessReproducesLinesEvenWhenExtrapolating`). Se $q>n$, $h$ cresce di $(q-n)/2$; il margine $h/0.999$ (come nel Fortran originale) dà ai punti sul bordo un peso quasi nullo, non esattamente nullo.

## STL (Cleveland, Cleveland, McRae, Terpenning, 1990)

Decompone additivamente in **trend** $T_t$, **stagionalità** $S_t$ e **resto** $R_t$:

$$y_t=T_t+S_t+R_t$$

(il resto è per definizione $y-T-S$, quindi la somma è esatta). Un **ciclo interno**, ripetuto $n_i$ volte, con periodo $n_p$:

| Passo | Operazione |
|---|---|
| 1 | togliere il trend: $y_t-T_t$ |
| 2 | **sottoserie cicliche**: per ogni fase $j$ (tutti i "gennaio", tutti i "febbraio"…) un LOESS di ampiezza $n_s$, esteso di **un ciclo prima e uno dopo** → serie $C$ di lunghezza $n+2n_p$ |
| 3 | **passa-basso** di $C$: medie mobili MA($n_p$), MA($n_p$), MA(3), poi LOESS($n_l$) → $L$ di lunghezza $n$ |
| 4 | stagionalità $S=C-L$ (si toglie a $C$ il livello lento che il passa-basso ha catturato) |
| 5 | destagionalizzare: $y_t-S_t$ |
| 6 | **trend** = LOESS($n_t$) della serie destagionalizzata |

Parametri: $n_s$ **dispari** (≥ 7 consigliato): più grande ⇒ stagionalità più stabile nel tempo; $n_t$ è il minimo dispari $\ge\dfrac{1.5\,n_p}{1-1.5/n_s}$; $n_l$ è il minimo dispari $\ge n_p$.

**Perché le tre medie mobili.** Una media mobile lunga un periodo annulla qualsiasi stagionalità di periodo $n_p$ a media nulla; applicata due volte e poi con MA(3) rende il passa-basso molto liscio e accorcia $C$ da $n+2n_p$ a $n$ ($-(n_p-1)-(n_p-1)-2=-2n_p$).

### Esattezza

Su $y_t=a+bt+s_{t\bmod n_p}$ con $\sum s=0$: i LOESS riproducono le rette e la MA di un periodo annulla $s$, quindi STL separa **esattamente** trend e stagionalità (a $10^{-9}$ in `double`, $5\cdot10^{-3}$ in `float`; `StlExactInEveryFormat`).

## Robustezza (ciclo esterno)

Dopo il ciclo interno si calcolano i **pesi bisquare**

$$\rho_t=B\!\left(\frac{|R_t|}{6\,\operatorname{mediana}|R|}\right),\qquad B(u)=\begin{cases}(1-u^2)^2&u<1\\0&u\ge1\end{cases}$$

che entrano come pesi nei LOESS dei passi 2 e 6 delle iterazioni successive. Un **outlier** (grande $|R_t|$) ha $\rho_t\approx0$ e non trascina più trend e stagionalità: finisce nel resto. Verifica `RobustIterationsIsolateOutlier`: con un picco di +30 la stagionalità robusta sbaglia 3 volte meno della non robusta, il resto in $t=60$ resta $>25$ e il suo peso $<0.01$.

Con la robustezza, $n_i=1$ basta.

## Decomporre prima, prevedere dopo

STL non prevede. Lo schema classico: trend destagionalizzato → Holt con damping; stagionalità → seasonal naive sulla componente $S$; previsione = somma. Il notebook [09](../../../notebooks/TimeSeries/09-ts-stl-ndsharp.ipynb) lo confronta con Holt-Winters su dati sintetici con due outlier; il risultato è onesto: nel caso mostrato **non** batte Holt-Winters.

## Cosa non fa

Fedele all'*algoritmo*, non al Fortran originale: niente *jump* di valutazione (il LOESS è valutato in ogni punto), niente LOESS di grado 0, niente stagionalità moltiplicativa (si usa il logaritmo a monte). Nessuna stagionalità multipla (MSTL).

← [8 · ARCH e GARCH](08-garch.md) · [Indice](README.md) · [10 · Algebra e simulazione →](10-algebra-e-simulazione.md)
