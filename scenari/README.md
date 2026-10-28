# Scenari: la teoria sul campo

> Ogni scenario mette le tecniche del libro in una **fabbrica o un impianto reale**: persone che acquisiscono dati e segnali da automazioni (PLC, sensori, contatori) e che alla fine devono produrre **KPI**, **dashboard** e **risposte ai clienti**. Ogni scenario ha dati, un notebook C# e uno PySharp con gli stessi numeri.

## Come leggere un scenario

Ogni scenario segue la stessa traccia, che è anche la domanda di chi lavora sull'impianto:

| Passo | Domanda | Strumenti del libro |
|---|---|---|
| 1 · **Acquisire** | I dati sono completi e affidabili? Dove sono i buchi? | qualità dei dati, Kalman con `NaN` |
| 2 · **Capire** | Che comportamento ha il segnale? Cicli, trend, volatilità? | ACF/PACF, STL, GARCH |
| 3 · **Prevedere** | Che cosa succederà nelle prossime ore/giorni? | riferimenti, smoothing, ARIMA, VAR |
| 4 · **Misurare** | Il modello è meglio del riferimento? | MASE, Ljung-Box |
| 5 · **Comunicare** | Qual è il KPI? Come lo mostro? Cosa rispondo al cliente? | tile KPI, grafici, testo con i numeri |

## Gli scenari possibili

| | Scenario | Segnali dalle automazioni | KPI e dashboard | Domanda del cliente | Stato |
|---|---|---|---|---|---|
| **A** | **Magazzino frigorifero** (catena del freddo) | temperatura cella, setpoint, corrente compressore, vibrazione, pressione evaporatore, porta, sbrinamento, kWh | tempo fuori soglia HACCP, numero di escursioni, kWh/giorno, allarme vibrazione | «La mia merce è stata sempre sotto +4 °C?» | ✅ **fatto** · [A](A-magazzino-frigorifero/README.md) |
| **B** | Sala compressori aria | pressione di rete, portata, kW, temperatura olio, cicli carico/vuoto | kWh/Nm³, % perdite, disponibilità | «Quanto risparmio se abbasso la pressione di 0,5 bar?» | più avanti |
| **C** | Stazione di pompaggio acquedotto | livello serbatoio, portata, pressione di rete, kW pompe | kWh/m³, ore di marcia, pressione minima garantita | «Garantite la pressione domani mattina?» | più avanti |
| **D** | **Parco fotovoltaico + accumulo** | irraggiamento, temperatura modulo, potenza AC, SoC batteria, temperatura inverter | performance ratio, disponibilità, errore di previsione | «Quanta energia offrite sul mercato domani?» (con intervalli P10/P50/P90) | ✅ **fatto** · [D](D-fotovoltaico/README.md) |
| **E** | Centro di lavoro CNC | vibrazione mandrino, corrente assi, temperatura, contapezzi | OEE, usura utensile, MTBF/MTTR, pezzi/ora | «Quando consegnate il lotto?» | più avanti |

I dati degli scenari sono **sintetici**, generati da uno script deterministico (con il seme): lo scopo è poter verificare i risultati contro la verità nota. Nessun dato è una misura reale di un impianto reale.

## Quali teorie del libro mette in gioco ciascuno scenario

✅ = implementata nella libreria · ◐ = realizzabile con le funzioni esistenti più poco codice nel notebook · ⬜ = descritta nel libro, non ancora implementata.

| Tecnica | A · frigo | B · compressori | C · pompaggio | D · fotovoltaico | E · CNC |
|---|---|---|---|---|---|
| Previsioni di riferimento e **MASE** ✅ | ✅ consumi | ✅ | ✅ domanda | ✅ | ✅ |
| Seasonal naive, **Holt-Winters** ✅ | ✅ ciclo 24 h / 168 h | ✅ | ✅ | ✅ ciclo giornaliero | — |
| **STL** ✅ | ✅ consumi | ✅ | ✅ | ✅ giorno/anno | ✅ turni |
| **Kalman** con dati mancanti ✅ | ✅ **temperatura con buchi di comunicazione** | ✅ pressione | ✅ livello | ✅ | ✅ |
| AR/**ARIMA**, Ljung-Box ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **VAR** (segnali accoppiati) ✅ | ✅ temperatura–corrente–pressione | ✅ pressione–portata–kW | ✅ livello–portata | ◐ irraggiamento–potenza | — |
| **GARCH** (volatilità) ✅ | ✅ **vibrazione del compressore** | — | — | ◐ variabilità da nuvolosità | ✅ vibrazione mandrino |
| Quantili e **intervalli di previsione** ✅ (quantili, pinball, copertura) | ◐ | — | ✅ | ✅ **P10/P50/P90, traiettorie, offerta ottima** | ◐ |
| **Change point** (PELT, BOCPD) ⬜ | cambio setpoint, sensore | nuovo setpoint, perdita | — | — | cambio utensile |
| **Matrix profile** (motif, discord) ⬜ | **cicli di sbrinamento**, anomalie | cicli carico/vuoto | — | — | **ciclo di lavorazione** |
| **Validazione** (rolling origin, Clark-West…) ⬜ | ◐ | ◐ | ◐ | ✅ | ◐ |
| Condizionare a un futuro noto/incerto ⬜ | — | — | meteo | ✅ **irraggiamento previsto** | — |
| **Interventi** (osservare ≠ fare) ⬜ | — | ✅ **cambiare il setpoint** | — | — | — |

Le righe ⬜ non hanno ancora codice nella libreria: nell'atlante delle teorie sono descritte, e gli scenari le richiamano come **passo successivo naturale**, non le simulano.

## Struttura di una cartella scenario

```text
scenari/
  README.md                       questo indice
  A-magazzino-frigorifero/
    README.md                     contesto, segnali, KPI, domande del cliente, risultati
    generate-data.cs              generatore deterministico dei dati (dotnet run generate-data.cs)
    data/*.csv                    i dati, come li esporterebbe uno storico
    A1-...-csharp.ipynb           notebook C# (API generica)
    A1-...-pysharp.ipynb          notebook PySharp (facciata Interop), stessi numeri
  D-fotovoltaico/                 stessa struttura (D1-...)
```

I notebook si eseguono con `pwsh scripts/run-notebooks.ps1 -Filter 'A1-*'`; i due linguaggi sono verificati da un test che confronta i numeri stampati.

## Ordine di lavoro

1. **A · magazzino frigorifero**: fatto
2. **D · parco fotovoltaico**: fatto
3. B, C, E: **più avanti**, a seconda di cosa serve.

I notebook 01–10 di [`notebooks/`](../notebooks/) restano tutti: sono i *samples* generici su cui gli scenari si appoggiano.
