# LSTM: memoria con porte

*(Reference image not included in the repository: third-party material.)*

> **Infografica** `time-series-forecasting-0-02` · prima riga dell'infografica (1997) · **1997** · stato ⬜ (legenda nell'[atlante](../../ATLANTE.md))

## Descrizione teorica

**Il problema.** Una rete ricorrente semplice aggiorna uno stato nascosto a ogni passo, $h_t=f(h_{t-1},x_t)$. Per imparare dipendenze lontane i gradienti devono attraversare molti passi, ma si **riducono (o esplodono) a ogni passaggio**: dopo qualche decina di passi l'informazione di un evento lontano è svanita. È il problema del *gradiente che svanisce*.

**L'idea (Hochreiter & Schmidhuber, 1997).** Affiancare allo stato una **cella di memoria** $c_t$ che viene modificata **additivamente** e controllata da tre porte, valori in $[0,1]$ calcolati dai dati:

$$f_t=\sigma(W_f[h_{t-1},x_t]),\quad i_t=\sigma(W_i[h_{t-1},x_t]),\quad o_t=\sigma(W_o[h_{t-1},x_t]),$$
$$c_t=f_t\odot c_{t-1}+i_t\odot\tilde c_t,\qquad h_t=o_t\odot\tanh(c_t).$$

- porta di **dimenticanza** $f_t$: quanto della memoria vecchia tenere;
- porta di **ingresso** $i_t$: quanto del nuovo contenuto candidato $\tilde c_t$ scrivere;
- porta di **uscita** $o_t$: quanto della memoria mostrare all'esterno.

Poiché $c_t$ si aggiorna con una somma, il gradiente può attraversare molti passi senza svanire.

**Cosa non è.** L'infografica lo precisa: *«General sequence architecture, not forecasting-specific»*. Nasce per sequenze in generale (testo, parlato); in previsione di serie temporali è un contenitore, non un modello statistico.

**Limiti.** Molti parametri, serve addestramento su molti dati; si elabora un passo alla volta (poco parallelizzabile); nessun intervallo di previsione se non lo si aggiunge (vedi DeepAR).

## Il racconto

*Storyboard per un breve video a fumetti. Le battute sono del [cast ricorrente](../../PERSONAGGI.md); i personaggi storici compaiono solo nelle didascalie.*

**Titolo:** *Il quaderno con le porte* · **Durata:** ~80 s · **Personaggi:** Anguria, Giulia, voce narrante · [cast](../../PERSONAGGI.md)

| # | Inquadratura | Testo | Cosa fa capire |
|---|---|---|---|
| 1 | Giulia, turno di notte, davanti a un registro. Ogni ora scrive una riga sullo stato del compressore. | **Giulia:** «Alle sei devo dire se stasera c'era qualcosa di strano.» | una sequenza da ricordare |
| 2 | Un gioco: il telefono senza fili. Dieci bambini in fila, il messaggio cambia a ogni passaggio. | **Anguria:** «Una rete semplice ricorda così. Dopo tanti passaggi, il messaggio è sparito.» | il gradiente che svanisce |
| 3 | Alle tre di notte, una vibrazione strana. Giulia la sottolinea in rosso. | **Giulia:** «Questo è importante. Lo voglio ancora alle sei.» | l'informazione utile può essere lontana |
| 4 | Il registro ha tre porte sul bordo, con tre manopole: DIMENTICA, SCRIVI, LEGGI. | *Didascalia:* «1997. Hochreiter e Schmidhuber danno al quaderno tre porte.» | le tre porte |
| 5 | La manopola DIMENTICA a zero sulle righe banali: spariscono. A uno su quella in rosso: resta intatta. | **Anguria:** «Ogni ora la porta decide cosa tenere. Ciò che tieni non sbiadisce: lo porti avanti così com'è.» | memoria che si somma, non si sovrascrive |
| 6 | La manopola LEGGI: alle sei, Giulia apre la porta e legge solo la riga in rosso. | **Anguria:** «E solo ciò che serve esce.» | la porta di uscita |
| 7 | Una targhetta sul quaderno: «uso generale: testi, voce, sensori». Anguria la indica. | **Anguria:** «Non è nato per le previsioni. È un quaderno per qualunque sequenza.» | architettura generale, non specifica |

**Morale:** *ricordare bene non è ricordare tutto: è scegliere che cosa non dimenticare.*

## Nel repository

| | |
|---|---|
| **Stato** | ⬜ **non implementata** |
| **Cosa c'è di vicino** | la ricorsione di uno stato nel tempo esiste nel filtro di Kalman ([cap. 6](../../00-formule-e-libreria/06-kalman.md)), ma lì la dinamica è lineare e **scelta**, non appresa; una rete richiede un addestramento che la libreria non offre |
| **Notebook** | nessuno |
| **Per approfondire** | la cella di memoria con porte è lo stesso schema che il Temporal Fusion Transformer usa nel suo *encoder* ([scheda 09](../09-temporal-fusion-transformer/README.md)) |

## Riferimenti

- Hochreiter, S. & Schmidhuber, J. (1997). *Long short-term memory.* Neural Computation.
