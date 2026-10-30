# Previsioni di riferimento: mean, naive, seasonal naive

*(Reference image not included in the repository: third-party material.)*

> **Infografica** `time-series-forecasting-0-01` · banda «Reference forecasts» in testa all'infografica · **sempre valide** · stato ✅ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**Che cosa sono.** Tre modi di prevedere che non richiedono nessun modello:

| Metodo | Regola | $\hat y_{T+h}$ | Ottimo per |
|---|---|---|---|
| **Mean** | la media storica | $\bar y$ | rumore bianco attorno a una media |
| **Naive** | l'ultimo valore | $y_T$ | passeggiata aleatoria |
| **Seasonal naive** | l'ultimo valore della stessa stagione | $y_{T+h-m(k+1)}$, $k=\lfloor(h-1)/m\rfloor$ | stagionalità stabile senza trend |

L'infografica li chiama *«still benchmarks today»*: sono i **benchmark di oggi**. Non sono previsioni ingenue, sono i modelli *ottimi* per processi molto semplici. Per questo ogni modello più complesso deve batterli per giustificare la propria esistenza.

**Come si confronta.** Con la **MASE** (Hyndman & Koehler, 2006): l'errore fuori campione diviso per l'errore in campione del naive stagionale,

$$\text{MASE}=\frac{\text{MAE}_{\text{fuori campione}}}{\frac{1}{n-m}\sum_{t=m+1}^{n}|y_t-y_{t-m}|}.$$

MASE $<1$ significa «meglio del naive stagionale». È indipendente dalla scala e definita anche con gli zeri (la MAPE no).

**Limiti.** Il naive non sa fare trend né stagionalità; il seasonal naive eredita il rumore dell'ultimo ciclo (copia anche i suoi errori); la media ignora tutto ciò che cambia nel tempo.

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *Il consulente e il banale* · **Durata:** ~75 s · **Personaggi:** Giulia, Tommaso, Anguria · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Ufficio del magazzino frigorifero. Sul monitor, il consumo orario di energia: onde che si ripetono ogni giorno. Un consulente indica un grafico con tante frecce e scatole. | *Didascalia:* «Una proposta: un modello sofisticato per prevedere l'energia della prossima settimana.» | c'è chi vende complessità |
| 2 | Tommaso, braccia incrociate, timbro «VERIFICATO» in mano. | **Tommaso:** «Rispetto a cosa è meglio?» | la domanda giusta è sempre *rispetto a cosa* |
| 3 | Anguria sale su uno sgabello con tre cartelli: MEDIA, ULTIMO VALORE, COME IERI ALLA STESSA ORA. | **Anguria:** «Tre previsioni che non costano niente. Si possono battere?» | i tre benchmark |
| 4 | Il grafico: la MEDIA è una riga piatta; l'ULTIMO VALORE è una riga che parte dall'ultimo punto; COME LA SETTIMANA SCORSA ricopia le onde. | **Giulia:** «L'ultima copia l'onda. Ha già la forma giusta.» | seasonal naive = copia il ciclo |
| 5 | Una bilancia: da un lato il consulente con il suo modello, dall'altro un cartello «MASE 0.918». Sotto il cartello, in piccolo: «seasonal naive, 168 ore». | **Tommaso:** «Il tuo modello?» — **Consulente:** «Holt-Winters: 0.934.» | **il banale vince**: 0.918 contro 0.934 (numeri veri dello scenario A) |
| 6 | Il consulente guarda il cartello. Anguria gli porge il pennarello. | **Anguria:** «Non è una sconfitta. Adesso sai *quanto* deve migliorare.» | il benchmark misura il valore del modello |
| 7 | Tommaso timbra il foglio. | **Tommaso:** «Si parte da qui.» | |

**Morale:** *il primo avversario di ogni modello è il banale.*

## Nel repository

| | |
|---|---|
| **Stato** | ✅ implementata |
| **Capitolo** | [3 · Previsioni di riferimento e metriche](../../00-formule-e-libreria/03-riferimenti-e-metriche.md) |
| **Libreria** | [`ReferenceForecasts`](../../../../src/libs/MathemaSharp.TimeSeries/ReferenceForecasts.cs): `Mean`, `Naive`, `SeasonalNaive`, `Drift`; `Accuracy.Mase` |
| **Notebook C#** | [06](../../../../notebooks/TimeSeries/06-ts-reference-autoregression.ipynb), [10 · giro della libreria](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-csharp.ipynb), [D1](../../../../notebooks/TimeSeries/Scenarios/D-fotovoltaico/D1-fotovoltaico-csharp.ipynb) |
| **Notebook PySharp** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-pysharp.ipynb), [D1](../../../../notebooks/TimeSeries/Scenarios/D-fotovoltaico/D1-fotovoltaico-pysharp.ipynb) |
| **In fabbrica** | scenario A, sezione 3: il seasonal naive settimanale (MASE 0.918) batte Holt-Winters (0.934) sul profilo orario |

## Riferimenti

- Hyndman, R. J. & Koehler, A. B. (2006). *Another look at measures of forecast accuracy.* International Journal of Forecasting.
- Hyndman, R. J. & Athanasopoulos, G. *Forecasting: Principles and Practice* (2ª ed.), cap. 3.
