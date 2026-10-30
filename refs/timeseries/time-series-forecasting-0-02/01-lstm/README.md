# LSTM: memory with gates

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-02` · first row of the infographic (1997) · **1997** · status ⬜ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The problem.** A simple recurrent network updates a hidden state at every step, $h_t=f(h_{t-1},x_t)$. To learn long-range dependencies the gradients must cross many steps, but they **shrink (or explode) at every passage**: after a few dozen steps the information of a distant event has vanished. This is the *vanishing gradient* problem.

**The idea (Hochreiter & Schmidhuber, 1997).** Add to the state a **memory cell** $c_t$ that is modified **additively** and controlled by three gates, values in $[0,1]$ computed from the data:

$$f_t=\sigma(W_f[h_{t-1},x_t]),\quad i_t=\sigma(W_i[h_{t-1},x_t]),\quad o_t=\sigma(W_o[h_{t-1},x_t]),$$
$$c_t=f_t\odot c_{t-1}+i_t\odot\tilde c_t,\qquad h_t=o_t\odot\tanh(c_t).$$

- **forget** gate $f_t$: how much of the old memory to keep;
- **input** gate $i_t$: how much of the new candidate content $\tilde c_t$ to write;
- **output** gate $o_t$: how much of the memory to show to the outside.

Since $c_t$ is updated with a sum, the gradient can cross many steps without vanishing.

**What it is not.** The infographic specifies: *"General sequence architecture, not forecasting-specific"*. It was born for sequences in general (text, speech); in time-series forecasting it is a container, not a statistical model.

**Limits.** Many parameters, training on a lot of data is needed; it processes one step at a time (hardly parallelisable); no prediction interval unless one is added (see DeepAR).

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The notebook with gates* · **Duration:** ~80 s · **Characters:** Anguria, Giulia, narrator · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | Giulia, night shift, in front of a register. Every hour she writes a line about the state of the compressor. | **Giulia:** "At six I have to say whether there was anything strange tonight." | a sequence to remember |
| 2 | A game: the telephone game. Ten children in a row, the message changes at every passage. | **Anguria:** "A simple network remembers like this. After many passages, the message is gone." | the vanishing gradient |
| 3 | At three in the morning, a strange vibration. Giulia underlines it in red. | **Giulia:** "This is important. I still want it at six." | the useful information can be far away |
| 4 | The register has three gates on its edge, with three knobs: FORGET, WRITE, READ. | *Caption:* "1997. Hochreiter and Schmidhuber give the notebook three gates." | the three gates |
| 5 | The FORGET knob at zero on the trivial lines: they disappear. At one on the red one: it stays intact. | **Anguria:** "Every hour the gate decides what to keep. What you keep does not fade: you carry it forward as it is." | memory that is added, not overwritten |
| 6 | The READ knob: at six, Giulia opens the gate and reads only the red line. | **Anguria:** "And only what is needed comes out." | the output gate |
| 7 | A plate on the notebook: "general use: texts, voice, sensors". Anguria points at it. | **Anguria:** "It was not born for forecasts. It is a notebook for any sequence." | general architecture, not specific |

**Moral:** *remembering well is not remembering everything: it is choosing what not to forget.*

## In the repository

| | |
|---|---|
| **Status** | ⬜ **not implemented** |
| **What is close** | the recursion of a state through time exists in the Kalman filter ([ch. 6](../../00-formule-e-libreria/06-kalman.md)), but there the dynamics is linear and **chosen**, not learned; a network requires a training that the library does not offer |
| **Notebook** | none |
| **To go further** | the memory cell with gates is the same scheme that the Temporal Fusion Transformer uses in its *encoder* ([card 09](../09-temporal-fusion-transformer/README.md)) |

## References

- Hochreiter, S. & Schmidhuber, J. (1997). *Long short-term memory.* Neural Computation.
