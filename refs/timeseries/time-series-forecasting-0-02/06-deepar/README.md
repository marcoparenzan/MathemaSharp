# DeepAR

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-02` · sixth row of the infographic (2017) · **2017** · status ⬜ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** Train **a single recurrent model on many related series**, instead of one model per series. The infographic: *"Train one recurrent model on many related series"*, with *"Shared weights across series"*.

**How it works.**

- A recurrent network (LSTM) processes, step by step, the previous value $y_{t-1}$, the covariates $x_t$ and an **identity** of the series.
- At each step it emits not a number but the **parameters of a distribution**: for example mean and standard deviation of a Gaussian, or the parameters of a negative binomial for counts.
- It is trained by maximising the **likelihood** of all the series.
- To forecast one **samples trajectories**: $\hat y_{t}$ is drawn from the distribution, reused as input of the next step, and repeated many times. From the samples quantiles and intervals are obtained (*"Predictive distribution"*).

**The advantage of global learning.** Series with little history (a new plant, a new product) benefit from what the model has learned from the others: this is called *cold-start*. The series have different scales: the model rescales per series.

**Limits.** A pool of *really related* and long enough series is needed; costly training; the forecasts have no coverage guarantee.

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *One student, a hundred teachers* · **Duration:** ~85 s · **Characters:** Giulia, Anguria, Tommaso · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | The plant has 200 cold chambers. Each chamber is a small chart. | **Giulia:** "I have 200 series. One per chamber." | many related series |
| 2 | An expert for each chamber, each closed in their own office, studying only their series. | **Anguria:** "Two hundred experts, each learns alone. The same mistakes are repeated." | one model per series: shares nothing |
| 3 | A single classroom with a single student. On the desk, the 200 series one after the other. | **Anguria:** "One student only. They study all the chambers. Their weights are the same for all." | shared weights |
| 4 | A new chamber arrives, installed two days ago. An almost empty chart. | **Giulia:** "This one has no history. What do we forecast?" | cold-start |
| 5 | The student looks at the new chamber and recognises it: "it looks like those on the north side". | **Anguria:** "It does not start from zero: it has already seen chambers like this." | global learning helps short series |
| 6 | The student does not answer with a number but with a bell: wider where it is uncertain. Anguria spins it and draws a point from it, then another. | **Anguria:** "At each step it gives the shape of the uncertainty. Then I draw an example and go on." | probabilistic output and sampling |
| 7 | Many possible futures as coloured threads that fan out. | **Tommaso:** "And the band you show me, where did you verify it?" — **Anguria:** "On its tests. It is always checked." | sampled trajectories, coverage to be verified |

**Moral:** *if the series resemble each other, learning together is faster than learning alone.*

## In the repository

| | |
|---|---|
| **Status** | ⬜ **not implemented** |
| **What is close** | the **probabilistic output** and the **sampled trajectories** are seen, with a much simpler method, in [scenario D](../../../../scenari/D-fotovoltaico/README.md): quantiles of the residuals per hour, trajectories by resampling blocks of residuals, `Accuracy.PinballLoss`, `Accuracy.IntervalCoverage`. There is neither a network nor learning across many series |
| **What is missing** | recurrent network trained on several series, parametric outputs, autoregressive sampling |

## References

- Salinas, D., Flunkert, V., Gasthaus, J. & Januschowski, T. (2020). *DeepAR: probabilistic forecasting with autoregressive recurrent networks.* International Journal of Forecasting (arXiv preview of 2017).
