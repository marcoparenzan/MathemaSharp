# Decomposizione STL

*(Reference image not included in the repository: third-party material.)*

> **Infografica** `time-series-forecasting-0-01` · ottava riga dell'infografica (1990) · **1990** · stato ✅ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** *Separa tendenza-ciclo, stagionalità e resto con LOESS.* Poi, dice l'infografica, **decomponi prima, prevedi dopo**: *«Decompose first; forecast components separately»*.

$$y_t=T_t+S_t+R_t.$$

**LOESS.** In ogni punto si adatta una **retta pesata** ai $q$ punti vicini, con pesi tricubici $w_i=(1-(|x_i-x|/h)^3)^3$. Una retta è riprodotta esattamente, anche estrapolando.

**Il ciclo interno** (6 passi): togliere il trend; **sottoserie cicliche** (tutti i «gennaio», tutti i «febbraio»…, lisciate con LOESS e estese di un ciclo); **passa-basso** di quella serie (tre medie mobili + LOESS); stagionalità $S=C-L$; destagionalizzare; LOESS del trend.

**Robustezza.** Dopo ogni ciclo interno si calcolano pesi bisquare dai residui: un **outlier** ha peso ≈ 0 e finisce nel resto, senza trascinare trend e stagionalità.

**Esattezza.** Su una retta più una stagionalità periodica a media nulla, STL separa le componenti **esattamente**: i LOESS riproducono le rette e la media mobile di un periodo annulla la stagionalità.

**Limiti.** Fedele all'*algoritmo*, non al Fortran originale (niente *jump*, solo LOESS di grado 1); una sola stagionalità; additiva (per la moltiplicativa si passa dal logaritmo).

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *Il mixer a tre canali* · **Durata:** ~85 s · **Personaggi:** Anguria, Giulia, Tommaso, voce narrante · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Uno studio di registrazione. Un mixer a tre canali: «BASSO», «RITMO», «FRUSCIO». Una canzone entra da un cavo. | **Anguria:** «La canzone è tutta insieme. Ma dentro ci sono tre cose.» | y = trend + stagionalità + resto |
| 2 | Il canale BASSO: una linea lenta che sale. | **Anguria:** «Il basso è la tendenza: lento, cambia piano.» | trend |
| 3 | Il canale RITMO: un battito che si ripete ogni 12 battute. | **Anguria:** «Il ritmo torna uguale: la stagionalità.» | stagionalità |
| 4 | Il canale FRUSCIO: puntini irregolari. | **Giulia:** «E il fruscio è ciò che resta: il resto.» | resto |
| 5 | Una mano cancella la stagione: tutti i gennaio uno dopo l'altro formano una piccola serie a sé. | *Didascalia:* «1990, Bell Labs. STL guarda i gennaio insieme, i febbraio insieme: ogni mese è una serie.» | sottoserie cicliche |
| 6 | Sul monitor di Giulia, i consumi del magazzino con due picchi anomali (due interruzioni). I picchi finiscono diritti nel canale FRUSCIO. | **Tommaso:** «E se un guasto sporca la canzone?» — **Anguria:** «Con la versione robusta, il guasto finisce nel fruscio. Ritmo e basso restano puliti.» | robustezza: pesi $\approx0$ sugli outlier |
| 7 | Una tabella: errore sul ritmo 0.69 (normale) contro 0.39 (robusta). | **Tommaso:** «Ne sei sicura?» — **Anguria:** «Numeri veri: l'errore sulla stagionalità cala del 43 %.» | notebook 09 |
| 8 | Il mixer: i tre canali previsti separatamente e poi rimessi insieme. | **Anguria:** «Prima si separa, poi si prevede ogni canale, poi si ricompone.» | decomponi prima, prevedi dopo |
| 9 | Giulia scrolla le spalle: in una prova la previsione per componenti ha MASE 0.39, Holt-Winters 0.34. | **Giulia:** «Qui Holt-Winters ha vinto lo stesso.» | onestà: non vince sempre |

**Morale:** *capire da cosa è fatto un segnale è già metà della previsione.*

## Nel repository

| | |
|---|---|
| **Stato** | ✅ implementata (STL con robustezza, LOESS lineare) |
| **Capitolo** | [9 · LOESS e STL](../../00-formule-e-libreria/09-stl-loess.md) |
| **Libreria** | [`Stl.Decompose`](../../../../src/libs/MathemaSharp.TimeSeries/Stl.cs), `Stl.Loess` |
| **Notebook C#** | [09](../../../../notebooks/TimeSeries/09-ts-stl-ndsharp.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb) |
| **Notebook PySharp** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb) |
| **Risultato** | notebook 09: con due outlier, errore sulla stagionalità 0.687 (non robusta) contro 0.394 (robusta, 6 iterazioni); peso robusto degli outlier 0. Previsione per componenti: MASE 0.390 contro 0.337 di Holt-Winters damped e 0.372 del seasonal naive |

## Riferimenti

- Cleveland, R. B., Cleveland, W. S., McRae, J. E. & Terpenning, I. (1990). *STL: a seasonal-trend decomposition procedure based on loess.* Journal of Official Statistics.
