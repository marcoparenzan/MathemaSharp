# N-BEATS

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-02` · eighth row of the infographic (2019) · **2019** · status ⬜ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** A **chain of fully connected blocks** that pass the residual to each other. The infographic: *"Stack fully connected blocks with backcast and forecast outputs"* and *"No recurrence or attention required"*.

**The block.** It receives a segment of history $x$ and produces **two** outputs:

- a **backcast** $\hat x$: the part of the history that the block manages to explain;
- a **forecast** $\hat y$: the block's contribution to the future.

**The chain (*doubly residual stacking*).** The next block receives what the previous one **did not explain**: $x_{\ell+1}=x_\ell-\hat x_\ell$. The final forecast is the **sum** of the forecasts of all the blocks:

$$\hat y=\sum_{\ell}\hat y_\ell.$$

**Why it is interesting.**

- it is a *deep but simple* network: no recurrence, no attention;
- in the **interpretable** version the blocks' outputs are constrained to **polynomial bases** (trend) and **Fourier bases** (seasonality): the forecast decomposes into trend + seasonality, as STL does, but **learned**;
- the authors reported results superior to the winners of the M4 competition.

**Limits.** Horizon and input window fixed at training time; univariate series; requires training on a lot of data.

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The chain of correctors* · **Duration:** ~80 s · **Characters:** Anguria, Giulia, Tommaso · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | An assembly line with five inspectors in a row. On the belt, a sheet with the history of the last days. | **Anguria:** "Each inspector looks at the sheet and says what they understand." | blocks in series |
| 2 | The first inspector has two trays: "what I explained of the past" and "what I deduce for the future". | **Anguria:** "Explanation and forecast. Two outputs." | backcast and forecast |
| 3 | The first inspector subtracts what they explained from the sheet and passes the reduced sheet to the second. | **Giulia:** "The second sees only what the first did not understand." | the residual passes to the next block |
| 4 | The sheet becomes emptier and emptier along the chain. | **Anguria:** "Less and less to explain." | shrinking residual |
| 5 | At the end of the chain, all the "future" trays are poured into a single funnel. | **Anguria:** "The forecast is the sum of everyone's contributions." | sum of the forecasts |
| 6 | The interpretable version: the first inspector has a single pencil that draws polynomials (slow curves), the second one that draws waves. | **Tommaso:** "It reminds me of the mixer." — **Anguria:** "Yes: trend and seasonality. But here the pieces are learned by the network." | analogy with STL ([0-01, card 09](../../time-series-forecasting-0-01/09-stl/README.md)) |
| 7 | A sign: "no memory, no attention". The inspectors are all the same. | *Caption:* "2019. A simple network that in the authors' tests beats the M4 winners." | simplicity and results (according to the authors) |

**Moral:** *if each corrector explains a piece and passes on the rest, the future is the sum of the pieces.*

## In the repository

| | |
|---|---|
| **Status** | ⬜ **not implemented** |
| **What is close** | the **decomposition into components** (trend + seasonality + remainder) exists with `Stl.Decompose` ([ch. 9](../../00-formule-e-libreria/09-stl-loess.md)): STL decomposes with fixed filters, interpretable N-BEATS with learned bases. [Notebook 09](../../../../notebooks/TimeSeries/09-ts-stl-ndsharp.ipynb) forecasts by components and compares with Holt-Winters |
| **What is missing** | deep network, training, learned polynomial and Fourier bases |

## References

- Oreshkin, B. N., Carpov, D., Chapados, N. & Bengio, Y. (2020). *N-BEATS: neural basis expansion analysis for interpretable time series forecasting.* ICLR (arXiv preview of 2019).
