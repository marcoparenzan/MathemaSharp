# PySharp skipped-test evidence

Generated from a real TRX test execution, not source discovery. No skips are waived.

Source: `D:\Dev\BuildArtifacts\ecosystem-validation\20261028-230541\test-results\pysharp.trx`
Generated UTC: 2026-10-28T23:17:01.9416256Z
Results: 1520; passed: 1507; skipped: 13; failed: 0

- **PySharp.Tests.M22_Orm.OrmPostgresSmokeTests.A_mapped_class_round_trips_through_create_all_insert_and_select_against_real_postgres**: No Postgres server reachable (PGHOST not set or unreachable)
- **PySharp.Tests.M6_Stdlib.Psycopg2Tests.A_real_str_subclass_and_int_subclass_bind_correctly_as_their_real_values**: No Postgres server reachable (PGHOST not set or unreachable)
- **PySharp.Tests.M6_Stdlib.Psycopg2Tests.A_unique_violation_raises_real_integrity_error_and_rollback_recovers**: No Postgres server reachable (PGHOST not set or unreachable)
- **PySharp.Tests.M6_Stdlib.Psycopg2Tests.Ddl_participates_in_the_same_real_transaction_as_dml_unlike_sqlite3**: No Postgres server reachable (PGHOST not set or unreachable)
- **PySharp.Tests.M6_Stdlib.Psycopg2Tests.Fetchone_advances_and_cursor_is_directly_iterable**: No Postgres server reachable (PGHOST not set or unreachable)
- **PySharp.Tests.M6_Stdlib.Psycopg2Tests.Lastrowid_is_always_zero_rowcount_is_real_matching_real_psycopg2**: No Postgres server reachable (PGHOST not set or unreachable)
- **PySharp.Tests.M6_Stdlib.Psycopg2Tests.Named_percent_paren_s_placeholders_bind_from_a_real_dict**: No Postgres server reachable (PGHOST not set or unreachable)
- **PySharp.Tests.M6_Stdlib.Psycopg2Tests.Percent_s_placeholders_insert_and_read_back_real_rows**: No Postgres server reachable (PGHOST not set or unreachable)
- **PySharp.Tests.M6_Stdlib.Psycopg2Tests.Reassigning_autocommit_mid_transaction_raises_a_real_programming_error**: No Postgres server reachable (PGHOST not set or unreachable)
- **PySharp.Tests.M6_Stdlib.Psycopg2Tests.Returning_clause_plus_fetchone_is_the_real_way_to_get_an_inserted_id**: No Postgres server reachable (PGHOST not set or unreachable)
- **PySharp.Tests.M6_Stdlib.Psycopg2Tests.Select_rowcount_is_the_real_row_count_not_negative_one**: No Postgres server reachable (PGHOST not set or unreachable)
- **PySharp.Tests.M6_Stdlib.Psycopg2Tests.With_conn_commits_on_clean_exit_and_does_not_close_the_connection**: No Postgres server reachable (PGHOST not set or unreachable)
- **PySharp.Tests.Oracle.OracleTests.Torchvision_snippet_matches_real_torchvision**: downloads pretrained weights (hundreds of MB): set PYSHARP_ORACLE_PRETRAINED=1 to run

## Re-run prerequisites

PostgreSQL: configure PGHOST/PGPORT/PGUSER/PGPASSWORD/PGDATABASE for a reachable test database (TLS required by PostgresLiveFixture). Do not commit credentials. Pretrained torchvision: set PYSHARP_ORACLE_PRETRAINED=1; large model downloads are opt-in. LocalDB tests may skip on other hosts. An unmet prerequisite is not a passing integration test.

Command: `dotnet test src/PySharp.Tests/PySharp.Tests.csproj -c Release --logger "trx;LogFileName=pysharp.trx" --results-directory D:/Dev/BuildArtifacts/ecosystem-skips` (from PySharp). Then run this script from MathemaSharp. The ecosystem gate generates a fresh TRX in its timestamped report directory.
