namespace PostgreSQL.PgVector.Performance;

public static class GlobalManager
{
    // https://www.npgsql.org/doc/connection-string-parameters.html#pooling
    public static string ConnectionString =
        "Host=localhost;Database=pgvectors;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=20;User Id=sa;Password=2022db@Qwer";
}
