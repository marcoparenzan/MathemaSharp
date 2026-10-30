# time-series forecasting · 02 · automation and neural models

*(Reference image not included in the repository: third-party material.)*

*1997–2019: new architectures and shared learning.* The dates indicate reference publications or first previews; the diagrams are illustrative. Sources of the infographic: FPP2, Hyndman & Khandakar, DeepAR, N-BEATS, TFT.

| # | Theory (card with theoretical description and story) | Years | Status |
|---|---|---|---|
| 1 | [LSTM: memory with gates](01-lstm/README.md) | 1997 | ⬜ |
| 2 | [ETS framework and Auto ARIMA](02-ets-e-auto-arima/README.md) | 2002 / 2008 | ⬜ |
| 3 | [TBATS: long, multiple and non-integer seasonalities](03-tbats/README.md) | 2011 | ⬜ |
| 4 | [Decision trees on lag features](04-alberi-su-feature-di-ritardo/README.md) | 2010s | ⬜ |
| 5 | [Prophet](05-prophet/README.md) | 2017 | ⬜ |
| 6 | [DeepAR](06-deepar/README.md) | 2017 | ⬜ |
| 7 | [Transformer](07-transformer/README.md) | 2017 | ⬜ |
| 8 | [N-BEATS](08-n-beats/README.md) | 2019 | ⬜ |
| 9 | [Temporal Fusion Transformer](09-temporal-fusion-transformer/README.md) | 2019 | ⬜ |

Legend: ✅ implemented · ◐ in part · ⬜ not implemented ([atlas](../ATLANTE.md)). **Cast of the stories:** [PERSONAGGI.md](../PERSONAGGI.md).

## "Two different changes"

*(Reference image not included in the repository: third-party material.)*

The infographic closes by distinguishing two things that are often confused:

| | **Automation** | **Global learning** |
|---|---|---|
| In one sentence | adapt and choose the models **with less manual work** | **share the parameters** among many series |
| Theories of this infographic | ETS framework and Auto ARIMA, TBATS, trees on lag features | DeepAR, N-BEATS (trained on many series), TFT |
| Model per series | one for each, chosen automatically | **a single one**, for all |
| When it is needed | many series, little desire to choose by hand | related series, many with little history |

**In the plant.** Automation: "two hundred chambers, choose the model for each without me". Global learning: "two hundred chambers, learn from the others when a new one has no history".

**What is in the repository for this infographic.** None of the nine theories is implemented: they are all **⬜**. For each, the card indicates what there is that is *close* (for example Holt-Winters ≈ ETS(A,A,A), the regression with covariate in [scenario D](../../../scenari/D-fotovoltaico/README.md), the pinball loss) and what is missing.
