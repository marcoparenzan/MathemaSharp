# Time-series forecasting: from ideas to the factory

> **Reference images not included.** The infographics these chapters come from, and their crops (`riga.png`, `sintesi.png`), are third-party material of unknown provenance: they were removed from the repository. The descriptions and the text cards remain.

> A history of the ideas of forecasting, from baseline forecasts to *foundation models*, with the formulas of the classic techniques, their realisation in .NET 11 with **Generic Math** (`float`, `double`, `Half`, `Decimal32/64/128`), and **industrial IoT** scenarios: people who acquire data and signals from automation and must produce KPIs, dashboards and answers to customers.

Written in English (translated from the Italian original on 2026-10-30; the Italian text is in the git history). The formulas use KaTeX (`$…$`), which VS Code and GitHub render. The mdBook-format summary is in [SUMMARY.md](SUMMARY.md). The file and folder names are still the Italian ones, so that the links keep working.

## How it is organised

| Part | Contents | Where |
|---|---|---|
| **I · The formulas and their realisation** | for each technique: formula, code, test, limits. 10 chapters | [`00-formule-e-libreria/`](00-formule-e-libreria/README.md) |
| **II · The library** | which library, use function by function, **overlap with NumPy in PySharp**, examples in C# and PySharp | [`01-libreria/`](01-libreria/README.md) |
| **III · The scenarios** | theory in the field: A cold-storage warehouse, D photovoltaic park (B, C, E later) | [`scenari/`](../../scenari/README.md) |
| **IV · The atlas of theories** | the **91 theories** of the 18 infographics, with the implementation status | [ATLANTE.md](ATLANTE.md) |

## For the reader

- **Whoever wants to understand the mathematics**: Part I, in order. The chapters can also be read separately.
- **Whoever wants to use the library**: Part II, then the [library tour](01-libreria/04-esempi.md) in C# and in PySharp.
- **Whoever works on a plant**: the [scenarios](../../scenari/README.md). Each starts from PLC signals and ends with KPIs, dashboard and the answer to a customer's question.
- **Whoever wants to know what exists and what does not**: the [atlas](ATLANTE.md). It says honestly that **most of the theories of the infographics are not implemented**.

## The infographics

The original images are in this folder; the one already worked on was moved into its own subfolder. They describe ideas and chronology, *not a ranking of models*.

| Infographic | Theme |
|---|---|
| `time-series-forecasting-0-01` … `0-05` | chronology: statistical foundations, automation and neural models, transformers, language models, efficient models |
| `time-series-forecasting-1-…` (7 images) | architectures, representations of the history, recursive/direct, probabilistic forecasts, conditioning on an uncertain future |
| `time-series-diagnostic-01`, `02` | change points, matrix profile |
| `time-series-forecast-validation-01` … `04` | validation and comparison of forecasts, evaluation of pre-trained models |

## What is not yet done

- **The split into subfolders, the cropping of the rows and the theory cards (theoretical description + comic-strip story) are done for 2 infographics out of 18**: [`0-01`](time-series-forecasting-0-01/README.md) and [`0-02`](time-series-forecasting-0-02/README.md), with 18 theories. The other 16 (73 theories) are in the [atlas](ATLANTE.md) as an index only; the work proceeds one infographic at a time. The cast of the stories is in [PERSONAGGI.md](PERSONAGGI.md).
- Scenarios **B, C and E** are only described in the [scenario index](../../scenari/README.md).
- **68 of the 91 theories** of the atlas (⬜) have no code at all; 11 have part of it (◐); 12 are complete (✅).

## Automatic verifications

- `dotnet test src/MathemaSharp.slnx -c Release`: the tests on the mathematical identities in every format, and those that the C# and PySharp notebooks **print the same numbers**.
- `pwsh scripts/run-notebooks.ps1`: rebuilds and runs all the notebooks.
