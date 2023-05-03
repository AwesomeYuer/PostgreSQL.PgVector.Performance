using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Loggers;
using Google.Protobuf.Collections;
using Grpc.Net.Client;
//using BenchmarkDotNet.Validators;
using Microshaoft.RediSearch;
using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Microsoft.SemanticKernel.Memory;
using Npgsql;
using NRedisStack;
using NRedisStack.Search;
using Pgvector;
using Pgvector.Npgsql;
using Qdrant;
using System.Data;
using System.Data.Common;
using System.Globalization;
using PgVector = Pgvector.Vector;



namespace VectorDataBases.Performance;

public class TestContext
{
    [Benchmark]
    public async Task PgVector_IvfflatVectorCosine_index_Cosine_11w_ProcessAsync()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(GlobalManager.PostgreSQLConnectionString);
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

            var pgVector = new PgVector(floats);
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
    public async Task PgVector_IvfflatVectorCosine_index_Cosine_25k_ProcessAsync()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(GlobalManager.PostgreSQLConnectionString);
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

            var pgVector = new PgVector(floats);
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
    public async Task AzureRediSearch_25k_ProcessAsync()
    {
        await RediSearch_FLAT_index_Cosine_25k_ProcessAsync(GlobalManager.AzureRedisConnectionString);
    }

    [Benchmark]
    public async Task RediSearch_FLAT_index_Cosine_25k_ProcessAsync()
    {
        await RediSearch_FLAT_index_Cosine_25k_ProcessAsync(GlobalManager.SelfHostRedisConnectionString);
    }

    private async Task RediSearch_FLAT_index_Cosine_25k_ProcessAsync(string connectionString)
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
    public async Task RediSearch_HNSW_index_Cosine_225k_ProcessAsync()
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
                            .Limit(0, 20)
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
    public async Task Qdrant_Grpc_HNSW_Index_Cosine_225k_ProcessAsync()
    {
        using var channel = GrpcChannel.ForAddress(GlobalManager.SelfHostQdrantGrpcConnectionString);
        
        var client = new Points.PointsClient(channel);

        var searchPoints = new SearchPoints()
        {
              CollectionName = "Articles"
            , Offset = 0
            , Limit = 20
            , WithPayload = new WithPayloadSelector()
            {
                //Exclude = new PayloadExcludeSelector()
                Include = new PayloadIncludeSelector()
            }
            , WithVectors = new WithVectorsSelector()
            {
                Enable = false
            }
            , VectorName = "title"
        };

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
                                //.ToArray()
                                ;

        foreach (var f in floats)
        {
            searchPoints.Vector.Add(f);
        }


        //var excludeFields = searchPoints
        //                            .WithPayload
        //                            .Exclude
        //                            .Fields;
        //excludeFields.Add("content_vector");
        //excludeFields.Add("title_vector");

        var includeFields = searchPoints
                                    .WithPayload
                                    .Include
                                    .Fields;

        includeFields.Add("title");
        includeFields.Add("id");
        includeFields.Add("url");

        var result =
                    (
                        await client
                                    .SearchAsync
                                        (
                                            searchPoints
                                        )
                    ).Result;

        var i = 0;
        foreach (var scoredPoint in result)
        {
            //Console.WriteLine(i++);
            MapField<string, Value> mapField = scoredPoint.Payload;
            // Iterate through the key-value pairs in the map field
            foreach (var keyValuePair in mapField)
            {
                _ = keyValuePair.Key;
                _ = keyValuePair.Value;
                // Console.WriteLine($"Key: {key}, Value: {@value}");
            }
        }
    }


    [Benchmark]
    public async Task qdrant_SK_HNSW_index_Cosine_6_ProcessAsync()
    {
        QdrantMemoryStore memoryStore =
                new QdrantMemoryStore
                            (
                                GlobalManager.SelfHostQdrantHttpConnectionString
                                , 6333
                                , vectorSize: 4
                                //, ConsoleLogger.Log
                            );
        IKernel kernel = Kernel.Builder
            //.WithLogger(ConsoleLogger.Log)
            .Configure
            (
                c =>
                {
                    c.AddOpenAITextCompletionService("text-davinci-003", "123");
                    c.AddOpenAITextEmbeddingGenerationService("text-embedding-ada-002", "123");
                }
            )
            .WithMemoryStorage(memoryStore)
            .Build();

        //Console.WriteLine("== Printing Collections in DB ==");
        //var collections = memoryStore.GetCollectionsAsync();
        //await foreach (var collection in collections)
        //{
        //    Console.WriteLine(collection);
        //}

        var MemoryCollectionName = "small";

        //Console.WriteLine("== Adding Memories ==");

        //var key1 = await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "cat1", text: "british short hair");
        //var key2 = await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "cat2", text: "orange tabby");
        //var key3 = await kernel.Memory.SaveInformationAsync(MemoryCollectionName, id: "cat3", text: "norwegian forest cat");

        //Console.WriteLine("== Printing Collections in DB ==");
        //collections = memoryStore.GetCollectionsAsync();
        //await foreach (var collection in collections)
        //{
        //    Console.WriteLine(collection);
        //}

        //Console.WriteLine("== Retrieving Memories Through the Kernel ==");
        //MemoryQueryResult? lookup = await kernel.Memory.GetAsync(MemoryCollectionName, "cat1");
        //Console.WriteLine(lookup != null ? lookup.Metadata.Text : "ERROR: memory not found");

        //Console.WriteLine("== Retrieving Memories Directly From the Store ==");
        //var memory1 = await memoryStore.GetWithPointIdAsync(MemoryCollectionName, key1);
        //var memory2 = await memoryStore.GetWithPointIdAsync(MemoryCollectionName, key2);
        //var memory3 = await memoryStore.GetWithPointIdAsync(MemoryCollectionName, key3);
        //Console.WriteLine(memory1 != null ? memory1.Metadata.Text : "ERROR: memory not found");
        //Console.WriteLine(memory2 != null ? memory2.Metadata.Text : "ERROR: memory not found");
        //Console.WriteLine(memory3 != null ? memory3.Metadata.Text : "ERROR: memory not found");

        Console.WriteLine("== Similarity Searching Memories: My favorite color is orange ==");
        var searchResults = kernel
                                .Memory
                                .SearchAsync
                                        (
                                            MemoryCollectionName
                                            , "My favorite color is orange"
                                            , limit: 20
                                            , minRelevanceScore: 0.8
                                            , withEmbeddings: true
                                        );

        await foreach (var item in searchResults)
        {
            //Console.WriteLine(item.Metadata.Text + " : " + item.Relevance);
            _ = item.Relevance;
        }

        //Console.WriteLine("== Removing Collection {0} ==", MemoryCollectionName);
        //await memoryStore.DeleteCollectionAsync(MemoryCollectionName);

        //Console.WriteLine("== Printing Collections in DB ==");
        //await foreach (var collection in collections)
        //{
        //    Console.WriteLine(collection);
        //}




    }
}
