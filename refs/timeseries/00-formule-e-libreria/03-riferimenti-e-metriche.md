# 3 · Previsioni di riferimento e metriche di errore

Codice: [ReferenceForecasts.cs](../../../src/libs/MathemaSharp.TimeSeries/ReferenceForecasts.cs) (contiene anche `Accuracy`).

## Le quattro previsioni di riferimento

Sono ancora i **benchmark di oggi**: un modello che non le batte non giustifica la propria complessità. Non sono previsioni "stupide", sono i modelli ottimi per processi specifici.

| Metodo | Formula $\hat y_{T+h}$ | Ottimo per |
|---|---|---|
| Mean | $\bar y$ | rumore bianco attorno a una media |
| Naive | $y_T$ | passeggiata aleatoria |
| Seasonal naive | $y_{T+h-m(k+1)},\;k=\lfloor(h-1)/m\rfloor$ | stagionalità stabile, senza trend |
| Drift | $y_T+h\,\dfrac{y_T-y_1}{T-1}$ | passeggiata aleatoria con deriva |

```csharp
for (var h = 1; h <= horizon; h++) result[h - 1] = y[y.Length - period + (h - 1) % period];   // seasonal naive
```

Verifica: `Benchmarks` (valori calcolati a mano su $y=[1,2,3,10,20,30]$).

## Misure di accuratezza fuori campione

Con $e_t=y_t-\hat y_t$ su $H$ osservazioni fuori campione:

$$\text{MAE}=\frac1H\sum|e_t|,\qquad \text{RMSE}=\sqrt{\frac1H\sum e_t^2},\qquad \text{MAPE}=\frac{100}{H}\sum\left|\frac{e_t}{y_t}\right|$$

- **RMSE** punisce di più gli errori grandi; è la misura coerente con la media come previsione puntuale.
- **MAE** è coerente con la mediana.
- **MAPE** è in percentuale e leggibile da chiunque, ma **non è definita con osservazioni nulle** (la libreria lo dichiara con `ArgumentException`) ed esplode vicino a zero. Una temperatura in °C che passa per 0 non si valuta con la MAPE.

### MASE (Hyndman & Koehler, 2006)

$$\text{MASE}=\frac{\text{MAE}_{\text{fuori campione}}}{\dfrac{1}{n-m}\displaystyle\sum_{t=m+1}^{n}|y_t-y_{t-m}|}$$

Il denominatore è il MAE *in campione* del seasonal naive di periodo $m$ ($m=1$: naive). Quindi:

- **MASE $<1$**: il modello è meglio del naive stagionale **in campione**;
- è **indipendente dalla scala** (si confrontano serie in unità diverse) e **definita anche con zeri**.

```csharp
for (var t = period; t < training.Length; t++) scale += T.Abs(training[t] - training[t - period]);
scale /= N<T>(training.Length - period);
if (scale == T.Zero) throw new ArgumentException("Training costante sul periodo: scala nulla.");
return Mae(actual, forecast) / scale;
```

Verifica: `MaseOfNaiveOnRandomWalkIsNearOne`: su $[1,2,4,7]$ la scala vale $(1+2+3)/3=2$; con errore $|9-7|=2$ la MASE è esattamente 1.

> **Quale confronto fa onestamente una MASE.** Il denominatore è calcolato sul training: confronta il modello con un naive *addestrato sugli stessi dati*, ma l'errore al numeratore è fuori campione. È una scala, non un test di significatività: per dire se un modello batte davvero un altro, si vedano i metodi di validazione (Diebold-Mariano, Clark-West, Model Confidence Set…) negli infografici sulla *forecast validation*.

← [2 · Statistica di base](02-statistica-di-base.md) · [Indice](README.md) · [4 · AR e ARIMA →](04-ar-arima.md)
