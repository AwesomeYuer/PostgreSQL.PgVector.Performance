namespace VectorDataBase.Performance;

public static class GlobalManager
{
    // https://www.npgsql.org/doc/connection-string-parameters.html#pooling
    // https://stackoverflow.com/questions/44272459/postgres-npgsql-connection-pooling
    public static string postgreSQLConnectionString =
        "Host=localhost;Database=pgvectors;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=10;User Id=sa;Password=2022db@Qwer";
    public static string redisConnectionString = "localhost, password=p@$$w0rdw!th0ut";


}
