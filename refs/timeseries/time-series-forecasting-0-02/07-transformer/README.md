# Transformer

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-02` · seventh row of the infographic (2017) · **2017** · status ⬜ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** Use **attention** to learn the relationships inside a sequence, **without recurrence**. The infographic: *"Originally machine translation; forecasting adaptations follow"*: born for machine translation (Vaswani et al., 2017), adapted to time series only later.

**Self-attention.** Each element of the sequence (a *token*) is projected into three vectors: **query** $q$, **key** $k$ and **value** $v$. The output of an element is a **weighted average of the values of all the others**, with weights given by how much its query resembles their keys:

$$\text{Attention}(Q,K,V)=\operatorname{softmax}\!\Big(\frac{QK^\top}{\sqrt d}\Big)V.$$

The weights are therefore **content-dependent** and change for every element.

**What changes compared with a recurrent network.**

- Each element "sees" all the others directly in a single step: no message that fades along a chain.
- All the steps are computed **in parallel**, so training is fast.
- **Order is not intrinsic**: it must be added with a *positional encoding*.
- The cost is **quadratic** in the length of the sequence.

**For time series.** What a *token* is, is a choice: a single instant, a *patch* of instants, the whole history of a variable. It is the theme of the next infographic.

**Limits.** A lot of data and a lot of computing power; a transformer has *no* natural assumption about trend and seasonality; attention is not a causal explanation.

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The meeting where everybody looks at everybody* · **Duration:** ~80 s · **Characters:** Anguria, Giulia, narrator · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | A meeting room with eight chairs in a row, one for each hour of a shift. On each chair, a compressor signal. | **Anguria:** "Each hour is a participant. They all have a piece of information." | token = an instant |
| 2 | The participant of the current hour holds a card with a question: "Who can help me understand what is happening?" (the *query*). Every other participant has a card with what they can do (the *key*). | **Anguria:** "The question looks for the most similar answer." | query and key |
| 3 | From the current participant eight threads of different thickness go towards the others: the thickest towards "yesterday at the same hour" and towards "a week ago". | **Giulia:** "It knows how to look back where needed, not only at the previous hour." | attention weights, data-dependent |
| 4 | The threads bring back a value each; the participant mixes them in a weighted average and writes it on the board. | **Anguria:** "Its answer is the average of what the others say, weighted by attention." | the formula softmax(QKᵀ/√d)V |
| 5 | The chairs are shuffled, but a numbered plate is hung on each. | **Anguria:** "The room does not know who arrived first. We give them a number." | positional encoding |
| 6 | The room becomes enormous: a thread for every pair, a very dense web. | **Giulia:** "With a week of data, there are thousands of threads." — **Anguria:** "The cost grows with the square." | quadratic cost |
| 7 | A sign "2017: machine translation". Then another: "later: time series". | *Caption:* "Born to translate sentences. Adapted to sensors later." | origin and adaptation |

**Moral:** *instead of passing the message from hand to hand, everyone listens directly to whoever is needed.*

## In the repository

| | |
|---|---|
| **Status** | ⬜ **not implemented** |
| **What is close** | nothing direct. In miniature, a **weighted average of the past values with weights that depend on the distance** is what LOESS with tricube weights ([ch. 9](../../00-formule-e-libreria/09-stl-loess.md)) and exponential smoothing do, but with *fixed* weights, not learned from the data |
| **To go further** | the next infographic ([0-03](../../ATLANTE.md)) compares the variants for time series |

## References

- Vaswani, A. et al. (2017). *Attention is all you need.* NeurIPS.
