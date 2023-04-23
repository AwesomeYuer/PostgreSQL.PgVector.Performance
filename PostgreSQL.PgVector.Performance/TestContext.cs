using BenchmarkDotNet.Attributes;
using Npgsql;
using Pgvector;
using Pgvector.Npgsql;
using System.Data;
using System.Data.Common;
using StackExchange.Redis;

using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;
using NRedisStack.Search.Aggregation;



namespace PostgreSQL.PgVector.Performance;

public class TestContext
{
    [Benchmark]
    public async Task PostgreSQLProcessAsync()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(GlobalManager.ConnectionString);
        dataSourceBuilder.UseVector();

        using var npgsqlDataSource = dataSourceBuilder.Build();
        // don't using for finally close only for return to connection pool
        var connection = await npgsqlDataSource.OpenConnectionAsync();

        try
        {
            var floats =
                    new float[1536]
                                .Select
                                    (
                                        (x) =>
                                        {
                                            return
                                                (float)
                                                    new Random()
                                                            .NextDouble();
                                        }
                                    )
                                .ToArray();

            var pgVector = new Vector(floats);
            var limit = 20;
            var sql = @$"
WITH
T
AS
(
    SELECT
        *
        , embedding <-> $1::vector  as ""EuclideanL2Distance""
        , embedding <=> $1::vector  as ""CosineDistance""
    FROM
        embeddings AS a
)
SELECT
    *
    , (1 - a.""CosineDistance"")    as ""CosineSimilarity""
FROM
    T AS a
ORDER BY
    --""EuclideanL2Distance""
    ""CosineDistance"" 
    --""CosineSimilarity""
                --DESC
LIMIT $2;
";
            using var npgsqlCommand = new NpgsqlCommand();
            npgsqlCommand.Connection = connection;
            npgsqlCommand.CommandText = sql;
            npgsqlCommand.Parameters.AddWithValue(pgVector);
            npgsqlCommand.Parameters.AddWithValue(limit);

            using
                (
                    DbDataReader dataReader =
                                    await npgsqlCommand.ExecuteReaderAsync()
                )
            {
                var j = 0;
                while (await dataReader.ReadAsync())
                {
                    IDataRecord dataRecord = dataReader;
                    var fieldsCount = dataRecord.FieldCount;
                    for (var i = 0; i < fieldsCount; i++)
                    {
                        _ = dataRecord[dataReader.GetName(i)];
                    }
                    j++;
                }
                //Console.WriteLine(j);
            }
        }
        finally
        {
            // don't dispose, just close only
            // return to connection pool
            await connection.CloseAsync();
        }
    }


    [Benchmark]
    public async Task WikipediaPostgreSQLProcessAsync()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(GlobalManager.ConnectionString);
        dataSourceBuilder.UseVector();

        using var npgsqlDataSource = dataSourceBuilder.Build();
        // don't using for finally close only for return to connection pool
        var connection = await npgsqlDataSource.OpenConnectionAsync();

        try
        {
            var floats =
                    new float[1536]
                                .Select
                                    (
                                        (x) =>
                                        {
                                            return
                                                (float)
                                                    new Random()
                                                            .NextDouble();
                                        }
                                    )
                                .ToArray();

            var pgVector = new Vector(floats);
            var limit = 20;
            var sql = @$"
WITH
T
AS
(
    SELECT
        *
        , title_vector <-> $1::vector  as ""EuclideanL2Distance""
        , title_vector <=> $1::vector  as ""CosineDistance""
    FROM
        wikipedia AS a
)
SELECT
    *
    , (1 - a.""CosineDistance"")    as ""CosineSimilarity""
FROM
    T AS a
ORDER BY
    --""EuclideanL2Distance""
    ""CosineDistance"" 
    --""CosineSimilarity""
                --DESC
LIMIT $2;
";
            using var npgsqlCommand = new NpgsqlCommand();
            npgsqlCommand.Connection = connection;
            npgsqlCommand.CommandText = sql;
            npgsqlCommand.Parameters.AddWithValue(pgVector);
            npgsqlCommand.Parameters.AddWithValue(limit);

            using
                (
                    DbDataReader dataReader =
                                    await npgsqlCommand.ExecuteReaderAsync()
                )
            {
                var j = 0;
                while (await dataReader.ReadAsync())
                {
                    IDataRecord dataRecord = dataReader;
                    var fieldsCount = dataRecord.FieldCount;
                    for (var i = 0; i < fieldsCount; i++)
                    {
                        _ = dataRecord[dataReader.GetName(i)];
                    }
                    j++;
                }
                //Console.WriteLine(j);
            }
        }
        finally
        {
            // don't dispose, just close only
            // return to connection pool
            await connection.CloseAsync();
        }
    }



    [Benchmark]
    public async Task WikipediaRediSearchProcessAsync()
    {
        // https://redis.io/docs/stack/search/reference/vectors/
        var floats = new float[1536]
                                .Select
                                    (
                                        (x) =>
                                        {
                                            return
                                                new Random()
                                                        .NextSingle();
                                        }
                                    )
                                //.ToArray()
                                    ;
        var vectors = floats
                            .SelectMany
                                (
                                    (x) =>
                                    {
                                        return
                                            BitConverter
                                                .GetBytes(x);
                                    }
                                )
                            .ToArray();

        //var vectorsHexString =
        //            vectors
        //                .Select
        //                    (
        //                        (x) =>
        //                        {
        //                            return
        //                                $@"\x{x:X2}";
        //                        }
        //                    )
        //                .Aggregate
        //                    (
        //                        (x, y) =>
        //                        {
        //                            return
        //                                $"{x}{y}";
        //                        }
        //                    );
        using ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, password=p@$$w0rdw!th0ut");
        IDatabase db = redis.GetDatabase();
        int radius = 10;
        var indexName = "embeddings-index";
        var query = $"*=>[KNN {radius} @title_vector ${nameof(vectors)} AS vector_score]";
        SearchCommands ftSearcher = db.FT();
        //var ftSearch = $@"FT.SEARCH {indexName} ""{query}"" PARAMS 2 {nameof(vectors)} ""{vectorsHexString}"" return 1 title";
        var searchResult =
                await
                    ftSearcher
                            .SearchAsync
                                (
                                    indexName
                                    , new Query(query)
                                                .AddParam
                                                    (
                                                         nameof(vectors)
                                                        , vectors
                                                    )
                                                .SetSortBy("vector_score")
                                                .Limit(0, 20)
                                                //.Dialect(2)
                                );
        var documents = searchResult.Documents;
        foreach (var document in documents)
        {
            var keyValuePairs = document.GetProperties();
            foreach (var keyValuePair in keyValuePairs)
            {
                if (keyValuePair.Key == "vector_score")
                {
                    //Console.WriteLine($"id: {document.Id}, score: {keyValuePair.Value}");
                }
            }
        }
        await redis.CloseAsync();
    }
}
