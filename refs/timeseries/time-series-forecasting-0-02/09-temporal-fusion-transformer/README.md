# Temporal Fusion Transformer

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-02` · ninth row of the infographic (2019) · **2019** · status ⬜ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** Combine **information of different nature** in a single multi-horizon forecasting model. The infographic: *"Combine static, historical and future-known inputs"*.

| Type of input | Example in a plant |
|---|---|
| **Static** | which compressor, model, site: it does not change over time |
| **Historical** (observed only in the past) | temperature, current, vibration up to now |
| **Known future** | production calendar, hour of the day, holidays, the shift **plan** |

**Components** (*"Selection + LSTM + attention"*):

1. **Variable-selection networks**: they learn *which* inputs matter, for each instant;
2. an **LSTM** (encoder-decoder, [card 01](../01-lstm/README.md)) for the local patterns;
3. an **interpretable multi-head attention** for the long-range dependencies;
4. **gates** that allow useless components to be skipped.

**Output:** **multi-horizon** forecasts, as **quantiles** (*"Multi-horizon quantiles"*), trained with the **pinball loss**.

**Interpretability.** The importance of the variables and the attention weights over time can be read.

**What it is not.** The infographic says so: *"Hybrid architecture, not a plain transformer"*: it uses an LSTM *and* attention.

**Limits.** Many parameters and data; the quantiles have no coverage guarantee; attention shows *where the model looks*, not *why*.

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The table with three kinds of information* · **Duration:** ~90 s · **Characters:** Giulia, Anguria, Tommaso · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | Giulia has three folders on the table: "WHO IT IS" (the compressor's file), "WHAT HAPPENED" (the readings up to today), "WHAT WE KNOW ABOUT THE FUTURE" (the shift plan). | **Giulia:** "To plan tomorrow I use all three." | static, historical, known-future inputs |
| 2 | Anguria on the table: a sign "not everything is always needed". | **Anguria:** "The model decides which rows to read, hour by hour." | variable selection |
| 3 | A small notebook with gates ([the LSTM story](../01-lstm/README.md)) that scrolls through the past readings. | **Anguria:** "First it remembers what happened nearby." | the LSTM for the local |
| 4 | A room of participants ([the Transformer story](../07-transformer/README.md)): each looks at the most useful ones. | **Anguria:** "Then it looks far: last week, the same day." | attention for the long range |
| 5 | The shift plan enters from above with the sign "known in advance". | **Giulia:** "If tomorrow there is an extra shift, the model knows it." | the known future as input |
| 6 | For each hour of the next day, three curves: low, medium, high. | **Tommaso:** "I don't want a number. I want a band." — **Anguria:** "It gives them for all the hours together." | multi-horizon quantiles |
| 7 | A card with two arrows: "trained with the pinball loss". Giulia recognises it. | **Giulia:** "It is the same one I use in the photovoltaic scenario." | link with `PinballLoss` |
| 8 | Tommaso with the stamp: "verify the coverage". | **Tommaso:** "How often does the actual fall inside the band?" | coverage must be checked |

**Moral:** *a good plan uses what is, what has been and what is already known about the future.*

## In the repository

| | |
|---|---|
| **Status** | ⬜ **not implemented** |
| **What is close** | three ideas, in elementary form, in [scenario D](../../../../scenari/D-fotovoltaico/README.md): (1) a **future-known input** (the forecast irradiance) in a regression; (2) **multi-horizon quantiles** (P10/P50/P90 hour by hour); (3) the **pinball loss** (`Accuracy.PinballLoss`, the same one the TFT is trained with) and the coverage (`Accuracy.IntervalCoverage`) |
| **What is missing** | selection networks, LSTM, multi-head attention, training |

## References

- Lim, B., Arık, S. Ö., Loeff, N. & Pfister, T. (2021). *Temporal fusion transformers for interpretable multi-horizon time series forecasting.* International Journal of Forecasting (arXiv preview of 2019).
