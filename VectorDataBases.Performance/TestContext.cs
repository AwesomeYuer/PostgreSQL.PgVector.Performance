using BenchmarkDotNet.Attributes;
using Npgsql;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using Pgvector.Npgsql;
using StackExchange.Redis;
using System.Data;
using System.Data.Common;
using Pgvector;
using Microshaoft.RediSearch;

namespace VectorDataBases.Performance;

public class TestContext
{
    [Benchmark]
    public async Task PostgreSQL_11w_ProcessAsync()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(GlobalManager.postgreSQLConnectionString);
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
        , embedding <=> $1::vector  as ""CosineDistance""
    FROM
        embeddings AS a
    ORDER BY
        ""CosineDistance""
    LIMIT $2
)
SELECT
    *
    , (1 - a.""CosineDistance"")    as ""CosineSimilarity""
FROM
    T AS a
ORDER BY
    ""CosineSimilarity""
                    DESC

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
    public async Task WikipediaPostgreSQL_ivfflat_vector_cosine_index_25k_ProcessAsync()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(GlobalManager.postgreSQLConnectionString);
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
                                                new Random()
                                                        .NextSingle();
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
        , title_vector <=> $1::vector  as ""CosineDistance""
    FROM
        wikipedia AS a
    ORDER BY
        ""CosineDistance""
    LIMIT $2
)
SELECT
    *
    , (1 - a.""CosineDistance"")    as ""CosineSimilarity""
FROM
    T AS a
ORDER BY
    ""CosineSimilarity""
                DESC
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

    //[Benchmark]
    public async Task WikipediaAzureRediSearch_25k_ProcessAsync()
    {
        await WikipediaRediSearch_FLAT_index_Cosine_25k_ProcessAsync(GlobalManager.AzureRedisConnectionString);
    }

    [Benchmark]
    public async Task WikipediaSelfHostRediSearch_FLAT_index_Cosine_25k_ProcessAsync()
    {
        await WikipediaRediSearch_FLAT_index_Cosine_25k_ProcessAsync(GlobalManager.SelfHostRedisConnectionString);
    }

    private async Task WikipediaRediSearch_FLAT_index_Cosine_25k_ProcessAsync(string connectionString)
    {
        // https://redis.io/docs/stack/search/reference/vectors/
        //await Task.CompletedTask;

        var vector = new float[1536]
                                .Select
                                    (
                                        (x) =>
                                        {
                                            return
                                                new Random()
                                                        .NextSingle();
                                        }
                                    )
                                .ToArray()
                                ;

        int k = 20;
        var indexName = "embeddings-index";
        var queryString = $"*=>[KNN {k} @title_vector ${nameof(vector)} AS score]";
        var searchResult =
                await new Query
                        (
                            queryString
                        )
                            .AddArrayParameter
                                (
                                      nameof(vector)
                                    , vector
                                )
                            .SetSortBy("score")
                            .Limit(0, k)
                            .Dialect(2)
                            .FTSearchAsync
                                (
                                    GlobalManager
                                        .SelfHostRedisConnectionString
                                    , indexName
                                );
        var documents = searchResult.Documents;
        foreach (var document in documents)
        {
            var keyValuePairs = document.GetProperties();
            foreach (var keyValuePair in keyValuePairs)
            {
                if (keyValuePair.Key == "score")
                {
                    // Console.WriteLine($"id: {document.Id}, score: {keyValuePair.Value}");
                }
            }
        }
        
    }

    [Benchmark]
    public async Task VecSimRediSearch_HNSW_index_Cosine_225k_ProcessAsync()
    {
        // https://redis.io/docs/stack/search/reference/vectors/
        //await Task.CompletedTask;

        var vector = 
                    new 
                        //double
                        float
                            [1536]
                                .Select
                                    (
                                        (x) =>
                                        {
                                            return
                                                new Random()
                                                        .NextSingle();
                                                        //.NextDouble();
                                        }
                                    )
                                .ToArray()
                        ;

        int k = 20;
        var indexName = "hnsw-cosine-index-001";
        var queryString = $"*=>[KNN {k} @vector ${nameof(vector)} AS score]";
        var searchResult =
                await
                    new Query
                        (
                            queryString
                        )
                            .AddArrayParameter
                                (
                                      nameof(vector)
                                    , vector
                                )
                            .SetSortBy("score")
                            .Limit(0, k)
                            .Dialect(2)
                            .FTSearchAsync
                                (
                                    GlobalManager
                                        .SelfHostRedisConnectionString
                                    , indexName 
                                );
        
        var documents = searchResult.Documents;
        foreach (var document in documents)
        {
            var keyValuePairs = document.GetProperties();
            foreach (var keyValuePair in keyValuePairs)
            {
                if (keyValuePair.Key == "score")
                {
                    Console.WriteLine($"id: {document.Id}, score: {keyValuePair.Value}");
                }
            }
        }
    }
}
