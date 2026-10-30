# MathemaSharp — Open points

Suite-wide points are in [../ScienceSharp/OPENPOINTS.md](../ScienceSharp/OPENPOINTS.md). The status log is [ROADMAP.md](ROADMAP.md);
the history is [CHANGELOG.md](CHANGELOG.md).

| # | Point | Status |
| --- | --- | --- |
| MA-1 | **Perimeter** (R10): classify notebooks, legacy helpers and assets still left; migrate the relevant ones or justify keeping them. | open |
| MA-2 | **Precision and formats** (R06): a successful build does not prove that every API supports every format (Decimal32/64/128). The supported platform matrix is not decided. | open |
| MA-3 | **Performance** work is deferred until correctness, contracts and notebooks are settled. | deferred |
| MA-4 | CLI `mathema` exists (stats, acf, diff, forecast on one CSV column). No ARIMA, STL, Kalman, FFT or matrix commands yet. | done, extend later (suite G9) |
| MA-5 | The ecosystem gate lives here (`ops/ValidateEcosystem.cs`, 32 steps plus `skip-report`, all exit 0). It takes over 10 minutes and rewrites notebooks in place. | known |
| MA-6 | Notebook kernelspec metadata is now `jupytersharp` for notebooks saved by JupyterSharp and `jupyternet` for older ones; the structure test accepts both. Normalise. | open |
| MA-7 | `scripts/run-notebooks.ps1` and the `ops/*.proj` files still carry Italian descriptions and the "jupyternet" wording in places; `.config/dotnet-tools.json` no longer pins `jupyternet.cli` (emptied on 2026-10-30). | open |
| MA-8 | 1507 PySharp tests pass in the gate with 13 skipped (12 PostgreSQL, 1 model download). | accepted |
| MA-9 | `src/imports/*` snapshots: origin not audited (suite L3). | open |
| MA-10 | Documents are mostly in Italian and the status sections stack on top of older ones ("prevale la prima sezione"). Rewrite as one current state. | open (suite G7) |
| MA-11 | The accent in the brand "MàthemaSharp" versus the package/namespace "MathemaSharp" is a convention that every document must keep. | known |
| MA-12 | **Gate output is reproducible.** The 37 notebooks are committed with their outputs and a gate run leaves the working tree clean: PlotSharp `preview.2` names SVG clip paths and gradients by a hash of the chart instead of a random GUID. `ops/validation` (gate evidence, 14 files) is ignored, not tracked; each run archives a copy under `BuildArtifacts\ecosystem-validation`. Two Ecosystem notebooks load the packaged PlotSharp by path (`consumer-cache/plotsharp/<version>/...`): when the pinned version in `ops/VerifyNet11Packages.cs` moves, change that path in `01-signal-spectrum` and `03-indexed-charts` too, or they keep running the stale DLL. | done |
| MA-13 | The logo that read "MàthemaNet" (the old name) was removed from the tree on 2026-10-30 and replaced by the ScienceSharp logo set; the old image stays in the history (the author's own picture). The README logo block is generated, see `ScienceSharp/branding`. | done |
