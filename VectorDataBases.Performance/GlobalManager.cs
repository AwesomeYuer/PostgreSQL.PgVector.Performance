namespace VectorDataBases.Performance;

public static class GlobalManager
{
    // https://www.npgsql.org/doc/connection-string-parameters.html#pooling
    // https://stackoverflow.com/questions/44272459/postgres-npgsql-connection-pooling
    public static string postgreSQLConnectionString =
        //"Host=localhost;Database=pgvectors;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100;User Id=sa;Password=2022db@Qwer"
        "Host=localhost;Database=pgvectors;User Id=sa;Password=2022db@Qwer";
    //;
    public static string SelfHostRedisConnectionString = "localhost, password=2022db@Qwer";
    public static string AzureRedisConnectionString = "awesomeyuer.eastus.redisenterprise.cache.azure.net:10000, password=ethDvIX6Ccc4qL8kq7Wsp7X2aNObRiXN28MnDxaKm0c=";


}
