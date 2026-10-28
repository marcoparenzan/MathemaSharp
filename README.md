# MàthemaSharp

![Logo MàthemaSharp](refs/branding/mathemasharp-logo.png)

Una famiglia di librerie matematiche basate su Generic Math. Il marchio si scrive **MàthemaSharp**, con accento grave sulla prima a; pacchetti, assembly e namespace usano **MathemaSharp** senza accento.

## Documentazione e storico

- [CHANGELOG.md](CHANGELOG.md): registro ricostruito delle modifiche, verifiche, pacchetti e limiti.
- [ROADMAP.md](ROADMAP.md): stato corrente e attività ancora da completare.
- [NET11-MIGRATION.md](NET11-MIGRATION.md): dettaglio della migrazione e delle verifiche dell’ecosistema.
- [ARCHITECTURE.md](ARCHITECTURE.md): struttura, decisioni e inventario delle API.

Per lo stato aggiornato prevale la prima sezione della roadmap; i conteggi storici riportati più avanti descrivono verifiche precedenti.

## Verifica completa

```sh
dotnet msbuild ops/validate.proj -t:Baseline
```

Su Windows: build Release, 22 notebook selezionati e test, con report/log in `ops/artifacts/baseline/`. Ultima verifica: **272 test passati, 3 sospesi**. Esclusioni esplicite: notebook NDSharp, PySharp e scenari. Dettagli in [USAGE.md](USAGE.md#baseline-completa--un-comando). Il logo grafico conserva ancora il marchio precedente e resta da aggiornare.

## Librerie

| Libreria | Contenuto | Usage | Reference |
|---|---|---|---|
| MathemaSharp.Core | Generic Math, precisione, SIMD e DFT | [Usage](USAGE.md#mathemasharpcore--usage) | [Reference](ARCHITECTURE.md#mathemasharpcore--reference) |
| MathemaSharp.LinearAlgebra | Matrici, QR, LU, norme e sistemi lineari | [Usage](USAGE.md#mathemasharplinearalgebra--usage) | [Reference](ARCHITECTURE.md#mathemasharplinearalgebra--reference) |
| MathemaSharp.Quantum | Stati e operatori complessi generici | [Usage](USAGE.md#mathemasharpquantum--usage) | [Reference](ARCHITECTURE.md#mathemasharpquantum--reference) |
| MathemaSharp.TimeSeries | Statistica e previsione temporale | [Usage](USAGE.md#mathemasharptimeseries--usage) | [Reference](ARCHITECTURE.md#mathemasharptimeseries--reference) |
| MathemaSharp.NDSharp | Ponte ndarray, non ancora completamente generico | [Usage](USAGE.md#mathemasharpndsharp--usage) | [Reference](ARCHITECTURE.md#mathemasharpndsharp--reference) |

## Struttura

```text
src/MathemaSharp.slnx
src/libs/MathemaSharp.<lib>/
notebooks/Core/
notebooks/LinearAlgebra/
notebooks/Quantum/
notebooks/TimeSeries/Scenarios/
notebooks/NDSharp/
notebooks/shared/
refs/branding/
```

Usage e Reference sono organizzate per libreria nei documenti root, senza duplicare il catalogo. Lo snapshot `src/imports/NDSharp` resta invariato e fuori dalla soluzione.

## Avvio

```sh
dotnet build src/MathemaSharp.slnx -c Release
dotnet test src/MathemaSharp.slnx
pwsh scripts/run-notebooks.ps1
dotnet run --project src/apps/MathemaSharp.Docs/MathemaSharp.Docs/MathemaSharp.Docs.csproj
```

L'ambiente richiede l'SDK .NET 11 indicato da `global.json` e i feed di `nuget.config`. I kernel già .NET 11 supportano i nuovi Decimal32/64/128 secondo i rispettivi vincoli. Alcuni progetti restano net10.0: questa riorganizzazione non ne cambia il target. Non tutti i kernel importati sono convertiti a Generic Math.

La rinomina è una **breaking change**: aggiornare `using Numbers.*`, riferimenti ai progetti e nomi degli assembly a `MathemaSharp.*`. I notebook sono stati spostati e gli output precedenti azzerati: devono essere rieseguiti. Il logo è stato generato e controllato visivamente per l'accento; il sito non è stato verificato nel browser.
