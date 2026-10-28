# Scenario A · Magazzino frigorifero (catena del freddo)

> Un magazzino a +2 °C custodisce merce di un cliente. Il PLC manda allo storico temperatura, porta, sbrinamento, corrente, pressione e vibrazione ogni minuto; un contatore manda i kWh ogni ora. Chi lavora sui dati deve **certificare la catena del freddo**, **prevedere l'energia** e **accorgersi in tempo che il compressore cede**. La domanda del cliente: **«la mia merce è stata sempre sotto +4 °C?»**

| | |
|---|---|
| Notebook C# | [`A1-magazzino-frigorifero-csharp.ipynb`](A1-magazzino-frigorifero-csharp.ipynb): API generica |
| Notebook PySharp | [`A1-magazzino-frigorifero-pysharp.ipynb`](A1-magazzino-frigorifero-pysharp.ipynb): facciata `Interop` e `matplotlib` |
| Concordanza | stesse analisi, **stessi 38 numeri**: lo verifica `ScenarioANotebooksAgreeAcrossLanguages` |
| Dati | [`data/`](data/): `cella_1min.csv` (72 h al minuto), `energia_oraria.csv` (6 settimane), `verita.json` |
| Generatore | [`generate-data.cs`](generate-data.cs): `dotnet run scenari/A-magazzino-frigorifero/generate-data.cs` |
| Esecuzione | `pwsh scripts/run-notebooks.ps1 -Filter 'A1-*'` |

**I dati sono sintetici.** Il generatore conosce la verità (`verita.json`: i minuti veri sopra soglia, quando cambia la vibrazione): è l'unico modo di sapere se la pipeline ha ragione. Con dati reali questa verifica non esiste.

## I segnali

| Segnale | Periodo | Note del generatore |
|---|---|---|
| `temp_cella_c` | 1 min | termostato con isteresi ±0.5 °C attorno a +2 °C, rumore del sensore σ = 0.08 °C, **6 buchi di comunicazione** (70 minuti, 1.6 %) |
| `porta_aperta`, `sbrinamento` | 1 min | porta aperta in orario di carico; sbrinamento alle 3, 11 e 19 per 25 minuti; **martedì 14:00 una porta lasciata aperta 22 minuti** |
| `corrente_compressore_a` | 1 min | ~17.5 A acceso, ~1.8 A spento |
| `pressione_aspirazione_bar` | 1 min | bassa a compressore acceso |
| `vibrazione_mm_s` | 1 min | livello per stato + shock GARCH(1,1); **dopo 48 h ω triplica** (cuscinetto che cede) |
| `kwh`, `temp_esterna_c` | 1 h | ciclo giornaliero e settimanale, effetto del meteo, sbrinamenti |

## Cosa si trova (risultati veri, dai notebook eseguiti)

### 1 · Acquisire: qualità dei dati
6 buchi, 70 minuti mancanti, il più lungo 20 minuti. **Un buco è «sospetto»** se la temperatura ai suoi bordi differisce di più di 1 °C: ce n'è uno, di 12 minuti.

### 2 · Kalman e il KPI HACCP
La varianza del processo $q$ si sceglie sulla log-verosimiglianza: $q=0.01$ (2689 contro −646 per $q=0.001$).

| Misura | Minuti sopra +4 °C |
|---|---|
| Osservati nei dati | **7** |
| Kalman, solo livello | 8 |
| Kalman, livello + pendenza | 9 |
| **Intervallo onesto** (osservati … osservati + buchi sospetti) | **7 – 19** |
| **Verità** (nota solo al generatore) | **11** ✓ dentro l'intervallo |

> **La salita avviene tutta dentro un buco di comunicazione** (la temperatura passa da 2.4 °C a 4.5 °C fra due campioni): nessun filtro può vederla. Il filtro non "indovina": mantiene l'ultimo livello (o prolunga la pendenza) e poi salta. Per questo la risposta al cliente non è «sempre sotto +4 °C» ma **«da 7 a 19 minuti sopra soglia in un'unica escursione»**, con la raccomandazione di verificare porta e gateway.

### 3 · Previsione dell'energia (una settimana di prova)

| Modello | MASE (m = 168) | Totale settimana | Errore sul totale |
|---|---|---|---|
| seasonal naive 24 h | 1.871 | 6479 kWh | −12.4 % |
| **seasonal naive 168 h** | **0.918** | 7120 kWh | −3.7 % |
| Holt-Winters 24 h | 1.904 | 6361 kWh | −14.0 % |
| Holt-Winters 168 h | 0.934 | **7415 kWh** | **+0.3 %** |
| *reale* | | *7396 kWh* | |

**Due criteri, due risposte**: il profilo orario lo fa meglio il seasonal naive settimanale, il totale (il budget) Holt-Winters. E con **una sola settimana** scelta e valutazione coincidono: è ottimista. La validazione seria (*rolling origin*) è una teoria del libro **non ancora implementata**.

### 4 · GARCH sulla vibrazione
Stima sulle prime 48 h: $(\omega,\alpha,\beta)=(0.0019,\,0.083,\,0.809)$ contro i veri $(0.002,\,0.10,\,0.80)$. Minuti in allarme (σ condizionale > 1.5 σ normale): **0.5 %** il primo giorno, 0.2 % il secondo, **22.7 %** il terzo. L'allarme sostenuto (≥ 8 minuti nell'ultima ora) scatta **71 minuti dopo** il cambiamento vero, senza falsi allarmi prima. Il GARCH non dice *cosa* cede, dice che il comportamento è cambiato.

### 5 · VAR: il termostato, ritrovato dai dati
Su medie a 5 minuti, nell'equazione della temperatura la corrente pesa **−0.033** (compressore acceso ⇒ si raffredda); in quella della corrente la temperatura pesa **+9.06 A/°C** (cella calda ⇒ il termostato accende il compressore).

### 6 · Comunicare
Riga di tile KPI (con simbolo e parola per lo stato, mai solo il colore), tre grafici e due testi pronti con i numeri: la risposta al cliente e la nota per la manutenzione.

## Cosa manca (e dove sta nel libro)

| Servirebbe | Perché | Stato |
|---|---|---|
| **Change point** (PELT, BOCPD) | datare il cambio di vibrazione o di setpoint senza conoscerlo | descritto nel libro, non implementato |
| **Matrix profile** (motif, discord) | riconoscere il ciclo di sbrinamento e le anomalie fra i cicli | idem |
| **Rolling origin** | valutare i modelli di energia su molte origini, non una settimana | idem |
| **Intervalli di previsione** | dare un range ai kWh, non un solo numero | idem (previsto con lo scenario D) |
