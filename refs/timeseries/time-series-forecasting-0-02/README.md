# time-series forecasting · 02 · automazione e modelli neurali

*(Reference image not included in the repository: third-party material.)*

*1997–2019: nuove architetture e apprendimento condiviso.* Le date indicano pubblicazioni di riferimento o prime anteprime; gli schemi sono illustrativi. Fonti dell'infografica: FPP2, Hyndman & Khandakar, DeepAR, N-BEATS, TFT.

| # | Teoria (scheda con descrizione teorica e racconto) | Anni | Stato |
|---|---|---|---|
| 1 | [LSTM: memoria con porte](01-lstm/README.md) | 1997 | ⬜ |
| 2 | [Quadro ETS e Auto ARIMA](02-ets-e-auto-arima/README.md) | 2002 / 2008 | ⬜ |
| 3 | [TBATS: stagionalità lunghe, multiple e non intere](03-tbats/README.md) | 2011 | ⬜ |
| 4 | [Alberi di decisione su feature di ritardo](04-alberi-su-feature-di-ritardo/README.md) | anni 2010 | ⬜ |
| 5 | [Prophet](05-prophet/README.md) | 2017 | ⬜ |
| 6 | [DeepAR](06-deepar/README.md) | 2017 | ⬜ |
| 7 | [Transformer](07-transformer/README.md) | 2017 | ⬜ |
| 8 | [N-BEATS](08-n-beats/README.md) | 2019 | ⬜ |
| 9 | [Temporal Fusion Transformer](09-temporal-fusion-transformer/README.md) | 2019 | ⬜ |

Legenda: ✅ implementata · ◐ in parte · ⬜ non implementata ([atlante](../ATLANTE.md)). **Cast dei racconti:** [PERSONAGGI.md](../PERSONAGGI.md).

## «Due cambiamenti diversi»

*(Reference image not included in the repository: third-party material.)*

L'infografica chiude distinguendo due cose che spesso si confondono:

| | **Automazione** | **Apprendimento globale** |
|---|---|---|
| In una frase | adattare e scegliere i modelli **con meno lavoro manuale** | **condividere i parametri** fra molte serie |
| Teorie di questa infografica | quadro ETS e Auto ARIMA, TBATS, alberi su feature di ritardo | DeepAR, N-BEATS (addestrato su molte serie), TFT |
| Modello per serie | uno per ciascuna, scelto automaticamente | **uno solo**, per tutte |
| Quando serve | molte serie, poca voglia di scegliere a mano | serie correlate, molte con poca storia |

**Nello stabilimento.** Automazione: «duecento celle, scegli il modello per ognuna senza di me». Apprendimento globale: «duecento celle, impara dalle altre quando una nuova non ha storia».

**Cosa c'è nel repository per questa infografica.** Nessuna delle nove teorie è implementata: sono tutte **⬜**. Per ciascuna, la scheda indica che cosa c'è di *vicino* (per esempio Holt-Winters ≈ ETS(A,A,A), la regressione con covariata nello [scenario D](../../../scenari/D-fotovoltaico/README.md), la perdita pinball) e che cosa manca.
