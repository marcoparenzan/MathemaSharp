# 8 · ARCH e GARCH: modellare la varianza, non la media

Codice: [Garch.cs](../../../src/libs/MathemaSharp.TimeSeries/Garch.cs).

## Il problema

I modelli dei capitoli precedenti descrivono la **media** condizionale. Molte serie (rendimenti finanziari, ma anche le vibrazioni di una macchina) hanno una media quasi costante e una **volatilità che cambia**: periodi calmi e periodi agitati, a grappoli (*volatility clustering*). Gli shock $\varepsilon_t$ sono quasi incorrelati, ma i loro **quadrati** no: si vede dall'ACF di $\varepsilon_t^2$.

## ARCH (Engle, 1982) e GARCH (Bollerslev, 1986)

$$\varepsilon_t=\sigma_t\,z_t,\;\;z_t\sim\mathcal N(0,1)\qquad\sigma_t^2=\omega+\alpha\,\varepsilon_{t-1}^2+\beta\,\sigma_{t-1}^2$$

- $\omega>0$: livello di base della varianza;
- $\alpha$: reazione allo shock di ieri (*ARCH*);
- $\beta$: memoria della varianza di ieri (*GARCH*);
- **ARCH(1) è il caso $\beta=0$.**

**Stazionarietà in covarianza**: $\alpha+\beta<1$ (la **persistenza**). Allora la varianza ritorna a

$$\bar\sigma^2=\frac{\omega}{1-\alpha-\beta}$$

Non si modella la media: gli $\varepsilon$ sono assunti a media nulla (residui di un altro modello, o dati centrati).

```csharp
variances[0] = MeanSquare(shocks);      // σ²₀ = media di ε² (scelta convenzionale)
variances[t] = Omega + Alpha * shocks[t - 1] * shocks[t - 1] + Beta * variances[t - 1];
```

## Previsione della varianza

Poiché $E[\varepsilon_t^2\mid\mathcal F_{t-1}]=\sigma_t^2$, la previsione a $h$ passi si sposta verso $\bar\sigma^2$ con **decadimento geometrico di ragione $\alpha+\beta$**:

$$\hat\sigma^2_{T+1}=\omega+\alpha\,\varepsilon_T^2+\beta\,\sigma_T^2,\qquad \hat\sigma^2_{T+h}=\bar\sigma^2+(\alpha+\beta)^{h-1}\big(\hat\sigma^2_{T+1}-\bar\sigma^2\big)$$

La **persistenza** misura quanto dura uno shock: con $\alpha+\beta=0.95$ l'effetto si dimezza in $\ln0.5/\ln0.95\approx13.5$ passi.

Verifica `VarianceForecastConvergesToUnconditional`: con $(\omega,\alpha,\beta)=(0.1,0.1,0.8)$ risulta $\bar\sigma^2=1$, e $(\hat\sigma^2_{T+3}-1)/(\hat\sigma^2_{T+2}-1)=0.9$ a $10^{-12}$. Con persistenza 0.95 ($0.95^h$) servono ~900 passi per arrivare a $10^{-9}$: lo mostra `GarchAndVarInDecimal128`.

## Stima: massima verosimiglianza con *variance targeting*

La log-verosimiglianza gaussiana è

$$\ln L(\omega,\alpha,\beta)=-\tfrac12\sum_{t}\Big(\ln 2\pi+\ln\sigma_t^2+\frac{\varepsilon_t^2}{\sigma_t^2}\Big)$$

Tre parametri sono molti per una ricerca ingenua. Il **variance targeting** fissa la varianza incondizionata a quella campionaria $s^2$:

$$\omega=s^2\,(1-\alpha-\beta)$$

e il problema si riduce a due parametri $(\alpha,\beta)$ nel triangolo $\alpha>0,\,\beta\ge0,\,\alpha+\beta<1$. La libreria cerca il massimo su una **griglia** (passo $1/40$), poi la **raffina** attorno al migliore (passo ÷10, 3 volte). È robusta e semplice; **non è efficiente** (un Newton/BFGS sarebbe più veloce) e non fornisce errori standard.

Verifica `FitRecoversSimulatedParameters`: su 10 000 punti simulati con $(0.05,0.10,0.85)$ la stima ha $\hat\alpha\in[0.07,0.13]$ e persistenza in $[0.92,0.98]$.

## Cosa non fa

Solo GARCH(1,1) con innovazioni gaussiane: niente GARCH($p,q$), distribuzioni a code spesse (Student-$t$), effetti di leva (GJR, EGARCH), componente nella media (ARMA-GARCH). Vedi [ROADMAP](../../../ROADMAP.md).

← [7 · VAR](07-var.md) · [Indice](README.md) · [9 · LOESS e STL →](09-stl-loess.md)
