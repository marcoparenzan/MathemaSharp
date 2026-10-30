# 3 · Baseline forecasts and error metrics

Code: [ReferenceForecasts.cs](../../../src/libs/MathemaSharp.TimeSeries/ReferenceForecasts.cs) (also contains `Accuracy`).

## The four baseline forecasts

They are still today's **benchmarks**: a model that does not beat them does not justify its own complexity. They are not "stupid" forecasts, they are the optimal models for specific processes.

| Method | Formula $\hat y_{T+h}$ | Optimal for |
|---|---|---|
| Mean | $\bar y$ | white noise around a mean |
| Naive | $y_T$ | random walk |
| Seasonal naive | $y_{T+h-m(k+1)},\;k=\lfloor(h-1)/m\rfloor$ | stable seasonality, without trend |
| Drift | $y_T+h\,\dfrac{y_T-y_1}{T-1}$ | random walk with drift |

```csharp
for (var h = 1; h <= horizon; h++) result[h - 1] = y[y.Length - period + (h - 1) % period];   // seasonal naive
```

Verification: `Benchmarks` (values computed by hand on $y=[1,2,3,10,20,30]$).

## Out-of-sample accuracy measures

With $e_t=y_t-\hat y_t$ on $H$ out-of-sample observations:

$$\text{MAE}=\frac1H\sum|e_t|,\qquad \text{RMSE}=\sqrt{\frac1H\sum e_t^2},\qquad \text{MAPE}=\frac{100}{H}\sum\left|\frac{e_t}{y_t}\right|$$

- **RMSE** punishes large errors more; it is the measure consistent with the mean as a point forecast.
- **MAE** is consistent with the median.
- **MAPE** is a percentage and readable by anyone, but it is **not defined with zero observations** (the library declares it with `ArgumentException`) and explodes near zero. A temperature in °C that passes through 0 cannot be evaluated with the MAPE.

### MASE (Hyndman & Koehler, 2006)

$$\text{MASE}=\frac{\text{MAE}_{\text{out of sample}}}{\dfrac{1}{n-m}\displaystyle\sum_{t=m+1}^{n}|y_t-y_{t-m}|}$$

The denominator is the *in-sample* MAE of the seasonal naive of period $m$ ($m=1$: naive). Therefore:

- **MASE $<1$**: the model is better than the seasonal naive **in sample**;
- it is **scale-independent** (series in different units are compared) and **defined even with zeros**.

```csharp
for (var t = period; t < training.Length; t++) scale += T.Abs(training[t] - training[t - period]);
scale /= N<T>(training.Length - period);
if (scale == T.Zero) throw new ArgumentException("Constant training over the period: zero scale.");
return Mae(actual, forecast) / scale;
```

Verification: `MaseOfNaiveOnRandomWalkIsNearOne`: on $[1,2,4,7]$ the scale is $(1+2+3)/3=2$; with error $|9-7|=2$ the MASE is exactly 1.

> **What comparison a MASE honestly makes.** The denominator is computed on the training set: it compares the model with a naive *trained on the same data*, but the error in the numerator is out of sample. It is a scale, not a significance test: to say whether one model really beats another, see the validation methods (Diebold-Mariano, Clark-West, Model Confidence Set…) in the infographics on *forecast validation*.

← [2 · Basic statistics](02-statistica-di-base.md) · [Index](README.md) · [4 · AR and ARIMA →](04-ar-arima.md)
