# Decision trees on lag features

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-02` · fourth row of the infographic (2010s) · **2010s** · status ⬜ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** Turn forecasting into a **supervised learning** problem: each instant $t$ becomes a row of a table, with **features** built from the past and as **target** the value to predict.

| Feature group | Examples |
|---|---|
| **Lags** | $y_{t-1},\;y_{t-2},\;y_{t-24},\;y_{t-168}$ |
| **Rolling statistics** | mean and standard deviation of the last 24 hours, minimum/maximum |
| **Calendar** | hour of the day, day of the week, holiday |

On this table an ensemble of decision trees is trained (random forest, *gradient boosting*). The infographic warns: *"XGBoost paper: 2016; approach predates it"*: the approach predates the XGBoost paper, which made it popular.

**How to forecast several steps.** *Recursive* (the forecast is reused as a lag) or *direct* (one model per horizon): it is the theme of the infographic "one step or the whole horizon".

**Advantages.** Captures non-linearities and interactions, accepts **covariates** of any kind (prices, weather, calendar), no assumption of stationarity or parametric form.

**Decisive limit: trees do not extrapolate.** A tree's forecast is an average of values already seen: beyond the training range it returns the last "step". With a rising trend, the forecasts **flatten**. It is remedied by removing the trend first (differences, STL) or with linear models in the leaves.

**Beware of information leakage from the future (*leakage*).** Every feature must use **only data available at time $t$**: a centred moving average or a random split of the dataset *before* building the lags cheat. To validate, a chronological split (rolling origin) is needed.

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The table of lags* · **Duration:** ~85 s · **Characters:** Anguria, Giulia, Tommaso · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | A strip of paper with the hourly consumption. Anguria takes the scissors. | **Anguria:** "I cut the strip into many rows." | sequence → table |
| 2 | Each row has columns: "an hour ago", "yesterday at the same hour", "mean of the last 24 hours", "day of the week". At the end: "now". | **Giulia:** "Each row says: *knowing this, how much was I consuming now?*" | lag, rolling, calendar features |
| 3 | A tree of questions: "yesterday at the same hour > 40 kWh?" yes/no, "is it Monday?" yes/no. At the end, a leaf with a number. | **Anguria:** "Lots of yes/no questions. A hundred trees, each answers a bit differently. The forecast is the average." | ensemble of trees |
| 4 | A note at the bottom: "2016: the XGBoost paper". An arrow points to "before 2016: it was already done". | *Caption:* "The method is older than the paper that made it famous." | history |
| 5 | A temperature that rises every day: the trees' forecast rises and then stops, like a step. | **Tommaso:** "And if tomorrow is hotter than anything you have seen?" — **Anguria:** "The tree repeats the last step. It cannot climb beyond it." | trees do not extrapolate |
| 6 | A table row with a feature "mean of the next 3 days". Tommaso circles it with the red stamp. | **Tommaso:** "This one peeks into the future." — **Anguria:** "Right. Every column can use only what was known at that hour." | leakage |
| 7 | Giulia compares the result of her model with the forecast "like last week". | **Giulia:** "First we beat it, then we trust." | the benchmark ([card 01 of 0-01](../../time-series-forecasting-0-01/01-previsioni-di-riferimento/README.md)) |

**Moral:** *a series becomes a table: but every column must know only what it knew then.*

## In the repository

| | |
|---|---|
| **Status** | ⬜ **not implemented** (the trees) |
| **What is close** | the **same step of turning into a table** is seen in [scenario D](../../../../scenari/D-fotovoltaico/README.md), where a **linear regression** (`MatrixAlgebra.LeastSquares`) uses the forecast irradiance and temperature as features: MASE 0.511 against 1.362 for persistence. They are not trees, but it is the same scheme "features known at time $t$ → target" |
| **What is missing** | decision trees, forests, *gradient boosting* |
| **To understand the limits** | [ch. 10](../../00-formule-e-libreria/10-algebra-e-simulazione.md): least squares; card on "leakage" in the validation infographic |

## References

- Chen, T. & Guestrin, C. (2016). *XGBoost: a scalable tree boosting system.* KDD.
- Hyndman, R. J. & Athanasopoulos, G. *Forecasting: Principles and Practice* (3rd ed.), section on machine-learning methods.
