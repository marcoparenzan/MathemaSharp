# Registro degli obiettivi

## 1
> Con .NET 11 vengono introdotti i nuovi formati Decimal. E' ora di fare un recap della matematica in .NET con GenericMath, Complex numbers, Decimal ecc..
> Test performance esempi notebooks
> tutta una serie di esempi
> Target .NET non .NET 10

Intento: realizzare un laboratorio riproducibile sulla matematica in .NET, con Generic Math, Complex, decimal, test, benchmark e notebook, verificando lo stato delle novità .NET 11 e gli SDK disponibili. Il vincolo di sviluppo di questa sessione impone net10.0 per i progetti compilati: documentare esplicitamente questa discrepanza, senza spacciare API future per disponibili.

## 2
> in refs\timeseries ho aggiunto una immagine sulla timeserie forecasting. Che dici se approfondisci i temi e fai qualche implementazione con la matematica?
> Creerei anche dei notebook con JupiterNet. Che dici?
> Puoi riorganizzare il repo in modo da ottimizzare e fare una libreria di utilizzo. Poi in PySharp c'è una implementazione di NumPy....forse la libreria matematica potrebbe essere generalizzata...che dici?

Intento: implementare la cronologia dell'infografica (benchmark, AR, smoothing, Kalman, ARIMA, VAR, GARCH, damped trend, STL) con test sulle identità fra modelli; riorganizzare il repository in librerie pacchettizzabili; collegare NDSharp con un ponte separato invece di fondere le librerie; migrare tutti i notebook a JupyterNet ed eseguirli davvero, compreso il 05 su runtime .NET 11.

## 3
> (dalla stessa sessione) Tutta la libreria deve essere in .NET 11 e i formati di IGenericMath.
> L'organizzazione dei contenuti deve diventare sostanzialmente un libro.
> Crea un file di indice degli scenari possibili. Prima una spiegazione delle formule in teoria e realizzazione della libreria. Poi scenari: prima A poi D. Tieni tutti i samples. Vedremo in seguito.
> Ho caricato tutte le immagini con l'elenco delle teorie: splitta ogni immagine in una sottocartella, una cartella per ogni riga con i dettagli della teoria, un file in testa con l'elenco completo delle teorie e l'implementazione (testo, notebook C# e PySharp, riferimenti alla libreria). Poi la libreria: indicazione, uso funzione per funzione, overlap con NumPy in PySharp, esempi in C# e PySharp.
> Contestualizza ogni esempio in uno scenario di IoT industriale (persone che acquisiscono dati da automazioni, KPI, dashboard, risposte ai clienti). Le bottiglie no.

Intento: portare tutte le librerie a net11.0 e renderle generiche su `IFloatingPointIeee754<T>` (float, double, Half, Decimal32/64/128, `Complex<T>`); scrivere il libro (Parte I formule e realizzazione, Parte II libreria con overlap NumPy, atlante delle teorie); realizzare gli scenari A e D con dati sintetici, notebook C# e PySharp con gli stessi numeri. **Non ancora fatta**: la divisione delle 18 infografiche in sottocartelle con una scheda per ciascuna teoria (resta l'atlante come indice).

## 1. "puoi proseguire con l'analisi delle immagini quindi andare avanti con l'implementazione delle infogr…"

puoi proseguire con l'analisi delle immagini quindi andare avanti con l'implementazione delle infografiche

_2026-10-24 20:08_

## 2. "Puoi creare un sito web Blazor Server, tailwindcss javascript con classi per navigare nella document…"

Puoi creare un sito web Blazor Server, tailwindcss javascript con classi per navigare nella documentazione?

_2026-10-24 21:50_

## 3. "fai partire il sito"

_2026-10-24 22:02_

## 4. "fai partire il sito"

_2026-10-24 22:05_

## 5. "si può fare qualcosa sulla rappresentazione html? Qualcosa con i css?"

_2026-10-24 22:06_

## 6. "fai partire il sito"

_2026-10-24 22:08_

## 7. "Puoi formattare Tabelle con righe alternate, header con grassetti e padding"

_2026-10-24 22:18_

## 8. "fai partire il sito"

_2026-10-24 22:19_

## 9. "Che ne dici di implementare nel progetto 1793027138639.jpg"

_2026-10-26 23:19_

## 10. "mmmhh...sbaglio o sotto timeseries c'è Linear Algebra che starebbe meglio in LinearAlgebra?"

_2026-10-26 23:38_

## 11. "Se Numbers è una libreria generica di matematica, se guardi dentro D:\Dev\2026\repos\NDArray trovi m…"

Se Numbers è una libreria generica di matematica, se guardi dentro D:\Dev\2026\repos\NDArray trovi materiale che vale più come Numbers? Intanto importi?

_2026-10-26 23:44_

## 12. "scusa D:\Dev\2026\repos\NDSharp"

_2026-10-26 23:46_

## 13. "Puoi rendere tutta la libreria Generic Math?"

_2026-10-26 23:50_

## 14. "come siamo con la conversione?"

_2026-10-26 23:55_

## 15. "Allora. Ho trovato un nome. MàthemaNet."

Allora. Ho trovato un nome. MàthemaNet.
La radice della parola Matematica.
Una libreria in Generic Math, aggiornato a .NET 11 quindi anche con i nuovi Decimal32/64 dove dentro c'è tutto.
Tutti i sample sono notebook in radice (notebooks/*) divisi per come sono divise le librerie.
Quindi serve:
- riorganizzare
- rinominare MathemaNet.<lib>
- notebooks
- <lib>: <usage>, <reference>
Il logo da generare deve invece riportare l'accento altrimenti succede che finisce sulla seconda.

_2026-10-27 00:06_

## 16. "Per favore rinomina MathemaNet in MathemaSharp"

_2026-10-27 00:12_

## 17. "domanda (senza agire): qui ci sono Charts, in NDSharp ci sono charts, in D:\dev\2026\repos\JupyterSh…"

domanda (senza agire): qui ci sono Charts, in NDSharp ci sono charts, in D:\dev\2026\repos\JupyterSharp ci sono charts. 
IN D:\Dev\2026\articoli\2026.10 NavierStokes ci sono charts. Che dici ha senso fare una libreria di Charts?
Se si, hanno senso come notebook? Li lasciamo qui in MathemaSharp?

_2026-10-27 00:19_

## 18. "Che nome possiamo darle?"

_2026-10-27 00:19_

## 19. "Ok, crea allora la libreria D:\dev\2027\repos\PlotSharp"

Ok, crea allora la libreria D:\dev\2027\repos\PlotSharp
Intanto raccogli tutto e butta dentro. Poi aggiorniamo i vari progetti.

_2026-10-27 00:20_

## 20. "quindi?"

_2026-10-27 00:21_

## 21. "Non puoi farlo tu?"

_2026-10-27 00:21_

## 22. "ok puoi scrivere gli obiettivi in un file che poi apro un'altra sessione?"

_2026-10-27 00:22_

## 23. "ok intanto puoi buttare tutto quello che è inutile e temporaneo?"

_2026-10-27 00:27_

## 24. "Allora PlotSharp è in preparazione."

Allora PlotSharp è in preparazione.
Possiamo tracciare una roadmap di quello che deve essere fatto?

_2026-10-27 00:54_

## 25. "Inventario API: cosa è davvero Generic Math, quali tipi sono verificati e dove rimangono conversioni…"

Inventario API: cosa è davvero Generic Math, quali tipi sono verificati e dove rimangono conversioni a double.

_2026-10-27 00:55_

## 26. "parcheggiamo NDSharp...vediamo dopo...segna..."

_2026-10-27 01:01_

## 27. "prima di toccare altre librerie?"

_2026-10-27 01:02_

## 28. "procedi"

_2026-10-27 01:03_

## 29. "procedi"

_2026-10-27 01:04_

## 30. "procedi"

_2026-10-27 01:06_

## 31. "procedi"

_2026-10-27 01:08_

## 32. "procedi"

_2026-10-27 01:17_

## 33. "procedi"

_2026-10-27 01:20_

## 34. "procedi"

_2026-10-27 01:23_

## 35. "procedi"

_2026-10-27 01:25_

## 36. "procedi"

_2026-10-27 01:26_

## 37. "procedi"

_2026-10-27 01:29_

## 38. "procedi"

_2026-10-27 01:37_

## 39. "procedi"

_2026-10-27 01:43_

## 40. "procedi"

_2026-10-27 01:46_

## 41. "procedi"

_2026-10-27 01:49_

## 42. "procedi"

_2026-10-27 01:53_

## 43. "quindi?"

_2026-10-27 01:57_

## 44. "Validazione più ampia con oracoli indipendenti, matrici mal condizionate e code estreme."

_2026-10-27 01:57_

## 45. "procedi"

_2026-10-27 02:05_

## 46. "quindi?"

_2026-10-27 07:38_

## 47. "quindi?"

_2026-10-27 07:46_

## 48. "ok procedi"

_2026-10-27 07:47_

## 49. "ok procedi"

_2026-10-27 07:53_

## 50. "quindi?"

_2026-10-27 08:02_

## 51. "procedi"

_2026-10-27 08:03_

## 52. "Allora ho fatto un cambiamento. Ho spostato tutti i progetti coinvolti in una unica cartella. Se con…"

Allora ho fatto un cambiamento. Ho spostato tutti i progetti coinvolti in una unica cartella. Se controlli D:\dev\science trovi tutti i progetti coinvolti. Setta questa come percorso. Io gestirò tutto da qui

_2026-10-27 09:38_

## 53. "quindi:"

quindi:
quale è il punto di PlotSharp, MathemaSharp e JupyterSharp

_2026-10-27 09:39_

## 54. "procedi"

_2026-10-27 09:40_

## 55. "quindi?"

_2026-10-27 09:47_

## 56. "Una cosa. Tutti i progetti che fanno build devono mettere i packages in D:\Dev\NuGetLocalFeed. Tutti…"

Una cosa. Tutti i progetti che fanno build devono mettere i packages in D:\Dev\NuGetLocalFeed. Tutti gli artifact locali ai singoli progetti vanni cancellati

_2026-10-27 09:49_

## 57. "quindi?"

_2026-10-27 10:09_

## 58. "procedi"

_2026-10-27 10:09_

## 59. "quindi?"

_2026-10-27 11:15_

## 60. "procedi"

_2026-10-27 12:09_

## 61. "Allora. Aggiorna anche NDSharp allora"

_2026-10-27 12:12_

## 62. "procedi all'aggiornamento a GenericMath"

_2026-10-27 12:19_

## 63. "procedi all'aggiornamento a GenericMath"

_2026-10-27 12:31_

## 64. "quindi?"

_2026-10-27 13:06_

## 65. "procedi"

_2026-10-27 13:07_

## 66. "procedi"

_2026-10-27 13:13_

## 67. "procedi"

_2026-10-27 13:18_

## 68. "procedi"

_2026-10-27 13:32_

## 69. "quindi?"

_2026-10-27 13:34_

## 70. "procedi"

_2026-10-27 13:37_

## 71. "procedi"

_2026-10-27 13:43_

## 72. "procedi"

_2026-10-27 13:51_

## 73. "procedi"

_2026-10-27 14:03_

## 74. "procedi"

_2026-10-27 14:21_

## 75. "procedi"

_2026-10-27 14:25_

## 76. "quindi?"

_2026-10-27 14:30_

## 77. "allora. io direi. Terrei le ottimizzazioni di performance dopo. Scrivi tutti gli openpoints."

allora. io direi. Terrei le ottimizzazioni di performance dopo. Scrivi tutti gli openpoints.
come siamo con lo stato delle librerie?

_2026-10-27 14:31_

## 78. "in realtà manca ricollegare pysharp e matsharp"

_2026-10-27 14:42_

## 79. "procedi"

_2026-10-27 14:43_

## 80. "quindi?"

_2026-10-27 14:54_

## 81. "procedi"

_2026-10-27 14:57_

## 82. "Limiti: non è ancora binding generico diretto. Le operazioni passano dalla facciata; Decimal32/64, o…"

Limiti: non è ancora binding generico diretto. Le operazioni passano dalla facciata; Decimal32/64, operatori Python, NumPy typed e pacchetti aggiornati restano aperti. Per mantenere le cifre,
  · Response received; extracting answer and usage · 182,8s
  · Turn saved; usage history → D:\Dev\Science\MathemaSharp\.ralfai\cost-history.jsonl · 182,8s
i Decimal vanno creati da stringhe, non da literal Python float.
Brutto

_2026-10-27 15:54_

## 83. "quindi?"

_2026-10-27 15:57_

## 84. "procedi"

_2026-10-27 15:59_

## 85. "procedi"

_2026-10-27 16:06_

## 86. "vai"

_2026-10-27 16:26_

## 87. "matsharp? Allineato?"

_2026-10-27 16:35_

## 88. "No, c'è anche D:\dev\Science\MatSharp"

_2026-10-27 16:36_

## 89. "ha la stessa struttura di PySharp, solo che è una implementazione di MatLab"

_2026-10-27 16:37_

## 90. "procedi"

_2026-10-27 16:38_

## 91. "procedi"

_2026-10-27 16:43_

## 92. "procedi"

_2026-10-27 16:50_

## 93. "Converti tutti i progetti in .NET 11"

_2026-10-27 16:55_

## 94. "procedi"

_2026-10-27 17:06_

## 95. "vai"

_2026-10-27 17:25_

## 96. "ottimo procedi"

_2026-10-27 17:33_

## 97. "ottimo procedi"

_2026-10-27 17:38_

## 98. "ottimo procedi"

_2026-10-27 17:47_

## 99. "ottimo procedi"

_2026-10-27 17:54_

## 100. "ottimo procedi"

_2026-10-27 18:23_

## 101. "ottimo procedi"

_2026-10-27 18:35_

## 102. "ottimo procedi"

_2026-10-27 18:39_

## 103. "ottimo procedi"

_2026-10-27 19:12_

## 104. "quindi?"

_2026-10-27 19:21_

## 105. "procedi"

_2026-10-27 19:21_

## 106. "procedi"

_2026-10-27 20:16_

## 107. "procedi"

_2026-10-27 20:27_

## 108. "fai il punto?"

_2026-10-27 22:31_

## 109. "Senti, puoi provare ad andare avanti fino alla fine? Mi fido, vai tranquillo...perchè non riesco ora…"

Senti, puoi provare ad andare avanti fino alla fine? Mi fido, vai tranquillo...perchè non riesco ora a darti conferma. Vai

_2026-10-27 22:32_

## 110. "Senti, puoi provare ad andare avanti fino alla fine? Mi fido, vai tranquillo...perchè non riesco ora…"

Senti, puoi provare ad andare avanti fino alla fine? Mi fido, vai tranquillo...perchè non riesco ora a darti conferma. Vai

_2026-10-27 23:22_

## 111. "Senti, puoi provare ad andare avanti fino alla fine? Mi fido, vai tranquillo...perchè non riesco ora…"

Senti, puoi provare ad andare avanti fino alla fine? Mi fido, vai tranquillo...perchè non riesco ora a darti conferma. Vai

_2026-10-28 00:27_

## 112. "quindi?"

_2026-10-28 00:47_

## 113. "quale è la roadmap aggiornata?"

_2026-10-28 00:48_

## 114. "C'è un documento che traccia tutto quello che è stato fatto?"

_2026-10-28 00:50_

## 115. "fallo per piacere"

_2026-10-28 00:50_

## 116. "Senti, puoi provare ad andare avanti fino alla fine? Mi fido, vai tranquillo...perchè non riesco ora…"

Senti, puoi provare ad andare avanti fino alla fine? Mi fido, vai tranquillo...perchè non riesco ora a darti conferma. Vai

_2026-10-28 01:04_
