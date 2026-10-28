# Transformer

![settima riga dell'infografica (2017)](riga.png)

> **Infografica** [`time-series-forecasting-0-02`](../time-series-forecasting-0-02.jpg) · settima riga dell'infografica (2017) · **2017** · stato ⬜ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**L'idea.** Usare l'**attenzione** per imparare le relazioni all'interno di una sequenza, **senza ricorrenza**. L'infografica: *«Originally machine translation; forecasting adaptations follow»*: nato per la traduzione automatica (Vaswani et al., 2017), adattato alle serie temporali solo in seguito.

**Auto-attenzione.** Ogni elemento della sequenza (un *token*) è proiettato in tre vettori: **query** $q$, **chiave** $k$ e **valore** $v$. L'uscita di un elemento è una **media pesata dei valori di tutti gli altri**, con pesi dati da quanto la sua query somiglia alle loro chiavi:

$$\text{Attention}(Q,K,V)=\operatorname{softmax}\!\Big(\frac{QK^\top}{\sqrt d}\Big)V.$$

I pesi sono quindi **dipendenti dal contenuto** e cambiano per ogni elemento.

**Cosa cambia rispetto a una rete ricorrente.**

- Ogni elemento «vede» direttamente tutti gli altri in un solo passo: nessun messaggio che sbiadisce lungo una catena.
- Tutti i passi si calcolano **in parallelo**, quindi l'addestramento è veloce.
- **L'ordine non è intrinseco**: va aggiunto con una *codifica posizionale*.
- Il costo è **quadratico** nella lunghezza della sequenza.

**Per le serie temporali.** Che cosa sia un *token* è una scelta: un singolo istante, una *patch* di istanti, l'intera storia di una variabile. È il tema dell'infografica successiva.

**Limiti.** Molti dati e molta potenza di calcolo; un trasformatore *non* ha un'ipotesi naturale su trend e stagionalità; l'attenzione non è una spiegazione causale.

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *La riunione in cui tutti si guardano* · **Durata:** ~80 s · **Personaggi:** Anguria, Giulia, voce narrante · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Una sala riunioni con otto sedie in fila, una per ogni ora di un turno. Su ogni sedia, un segnale del compressore. | **Anguria:** «Ogni ora è un partecipante. Hanno tutti un'informazione.» | token = un istante |
| 2 | Il partecipante dell'ora corrente ha in mano un cartellino con una domanda: «Chi mi può aiutare a capire cosa succede?» (la *query*). Ogni altro partecipante ha un cartellino con ciò che sa fare (la *chiave*). | **Anguria:** «La domanda cerca la risposta più somigliante.» | query e chiave |
| 3 | Dal partecipante corrente partono otto fili di spessore diverso verso gli altri: il più spesso verso «ieri alla stessa ora» e verso «una settimana fa». | **Giulia:** «Sa guardare indietro dove serve, non solo all'ora prima.» | pesi di attenzione, dipendenti dai dati |
| 4 | I fili portano in cambio un valore ciascuno; il partecipante li mescola in una media pesata e la scrive sulla lavagna. | **Anguria:** «La sua risposta è la media di ciò che gli altri dicono, pesata dall'attenzione.» | la formula softmax(QKᵀ/√d)V |
| 5 | Le sedie sono mescolate, ma una targhetta numerica è appesa a ognuna. | **Anguria:** «La sala non sa chi è arrivato prima. Gli diamo un numero.» | codifica posizionale |
| 6 | La sala diventa enorme: un filo per ogni coppia, una ragnatela fittissima. | **Giulia:** «Con una settimana di dati, sono migliaia di fili.» — **Anguria:** «Il costo cresce col quadrato.» | costo quadratico |
| 7 | Un cartello «2017: traduzione automatica». Poi un altro: «poi: serie temporali». | *Didascalia:* «Nato per tradurre frasi. Adattato ai sensori dopo.» | origine e adattamento |

**Morale:** *invece di passare il messaggio di mano in mano, ognuno ascolta direttamente chi serve.*

## Nel repository

| | |
|---|---|
| **Stato** | ⬜ **non implementata** |
| **Cosa c'è di vicino** | niente di diretto. In piccolo, una **media pesata dei valori passati con pesi che dipendono dalla distanza** è ciò che fanno LOESS con pesi tricubici ([cap. 9](../../00-formule-e-libreria/09-stl-loess.md)) e lo smoothing esponenziale, ma con pesi *fissati*, non appresi dai dati |
| **Per approfondire** | l'infografica successiva ([0-03](../../ATLANTE.md)) confronta le varianti per le serie temporali |

## Riferimenti

- Vaswani, A. et al. (2017). *Attention is all you need.* NeurIPS.
