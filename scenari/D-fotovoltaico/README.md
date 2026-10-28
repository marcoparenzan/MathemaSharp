# Scenario D · Parco fotovoltaico

> Un parco da 1 MWp (inverter da 600 kW AC) vende energia sul mercato del giorno prima. Il SCADA registra ogni ora irraggiamento, temperature, potenza e stato dell'inverter; un servizio meteo manda, il giorno prima, la **previsione di irraggiamento** con il suo errore. La domanda del cliente (il trader): **«quanta energia offrite sul mercato domani?»**, e la risposta va data **con un'incertezza**, perché produrre meno di quanto offerto costa più che produrre di più.

| | |
|---|---|
| Notebook C# | [`D1-fotovoltaico-csharp.ipynb`](D1-fotovoltaico-csharp.ipynb): API generica |
| Notebook PySharp | [`D1-fotovoltaico-pysharp.ipynb`](D1-fotovoltaico-pysharp.ipynb): facciata `Interop` e `matplotlib` |
| Concordanza | stesse analisi, **stessi numeri**: lo verifica `ScenarioDNotebooksAgreeAcrossLanguages` |
| Dati | [`data/`](data/): `impianto_orario.csv` (56 giorni orari), `verita.json` |
| Generatore | [`generate-data.cs`](generate-data.cs): `dotnet run scenari/D-fotovoltaico/generate-data.cs` |
| Esecuzione | `pwsh scripts/run-notebooks.ps1 -Filter 'D1-*'` |

**I dati sono sintetici.** Il generatore usa la posizione del sole a 45°N, tre tipi di giornata (chiara 45 %, variabile 35 %, coperta 20 %), un errore del servizio meteo (σ = 0.12 sul grado di serenità di giornata), lo sporcamento dei moduli (0.09 % al giorno) azzerato da una pioggia al giorno 30, un guasto dell'inverter il giorno 38 (5 ore) e il limite di 600 kW. 49 giorni servono per addestrare, 7 (14–20 settembre) per la prova.

## Cosa si trova (risultati veri, dai notebook eseguiti)

### 1 · KPI dell'impianto
| KPI | Valore |
|---|---|
| Performance ratio prima della pulizia (giorni 24–29) | **0.780** |
| Performance ratio dopo la pulizia (31–36) | **0.809**: lo sporco costava il ~3.6 % dell'energia |
| Performance ratio nel giorno del guasto | 0.368 |
| Disponibilità (ore di sole con inverter in servizio) | 99.3 % |
| Ore alla potenza massima dell'inverter (clipping) | 17 |

### 2 · Previsione della potenza, sapendo il meteo previsto
Regressione $P\approx\beta_0+\beta_1G_{\text{prev}}+\beta_2G_{\text{prev}}T_{\text{aria}}$ con $\beta=(10.99,\;0.807,\;-0.0027)$, limitata a 0…600 kW, addestrata a QR escludendo le ore di guasto.

| | MASE (scala: seasonal naive a 24 h) |
|---|---|
| **Modello con covariata meteo** | **0.511** |
| Persistenza (ieri alla stessa ora) | 1.362 |

La **MAPE non esiste**: di notte la potenza è zero e la libreria lo dichiara (`mape_definita: 0`).

### 3 · Intervalli: quantili dei residui, ora per ora
Perdita pinball: P10 = 4.86, P50 = 11.59, P90 = 4.67 (kW). **Copertura P10–P90 sulle ore di sole: 84.6 %** (atteso 80 %). Su *tutte* le ore sembra 91.7 %, ma è un'illusione: di notte l'intervallo è [0, 0] e contiene sempre il valore.

### 4 · L'energia dell'intera giornata: traiettorie, non somme di quantili
| Giorno | Reale kWh | P10 | P50 | P90 | Dentro P10–P90 |
|---|---|---|---|---|---|
| lun 14 | 1415 | 914 | 1749 | 2443 | sì |
| mar 15 | 4037 | 2433 | 3269 | 3964 | no (sopra) |
| mer 16 | 3909 | 2803 | 3640 | 4334 | sì |
| gio 17 | 3150 | 1481 | 2315 | 3010 | no (sopra) |
| ven 18 | 3722 | 2835 | 3670 | 4365 | sì |
| sab 19 | 1192 | 1196 | 2028 | 2722 | no (di 4 kWh sotto) |
| dom 20 | 3001 | 2309 | 3139 | 3834 | sì |

**Sommare i P10 orari non dà il P10 del giorno.** La banda dalle traiettorie è larga in media **1529 kWh** contro **1884** ottenuta sommando i quantili orari (che contiene tutti e 7 i giorni, perché supporre che tutte le ore sbaglino insieme nel peggiore dei modi è prudente all'eccesso). Le traiettorie danno **4 giorni su 7** dentro la banda contro l'80 % atteso: con soli 7 giorni è compatibile con la sorte (vedere 4 o meno su 7 con probabilità 0.8 capita circa 15 volte su 100). **Non è una calibrazione.**

### 5 · Quanto offrire: il quantile che minimizza il costo
Se il difetto costa $a=3$ e l'eccesso $b=1$ per kWh, l'offerta ottima è il quantile di livello $b/(a+b)=0.25$. Costo sulla settimana di prova:

| Offerta | Costo |
|---|---|
| P10 | 6470 |
| **P25** | **5094** |
| P50 (mediana) | 5845 |
| P90 | 13593 |

Il livello 0.25 vince, in accordo con la teoria; con sette giorni è una conferma, non una dimostrazione.

### 6 · Comunicare
Cinque tile KPI, due grafici e due testi pronti: la risposta al trader («per il 20 settembre offriamo 2 mila kWh circa…», con mediana e intervallo) e la nota per O&M (il PR salito dopo la pulizia, il guasto del giorno 38).

## Cosa manca (e dove sta nel libro)

| Servirebbe | Perché | Stato |
|---|---|---|
| **Previsione conforme** (*model prediction set*, intervalli con copertura garantita) | gli intervalli per quantili dei residui non garantiscono la copertura su dati nuovi | descritto nel libro, non implementato |
| **Rolling origin** | una settimana di prova non basta; la valutazione andrebbe ripetuta su molte origini | idem |
| **Condizionare a un futuro incerto** (propagare l'errore meteo) | oggi l'errore del servizio meteo entra nei residui, ma non è modellato esplicitamente | idem |
| **Confronto fra previsioni** (Diebold-Mariano, Clark-West…) | dire se il modello batte *davvero* la persistenza | idem |
