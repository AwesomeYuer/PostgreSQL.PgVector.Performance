using BenchmarkDotNet.Attributes;
using Google.Protobuf.Collections;
using Grpc.Net.Client;
using IO.Milvus.Client;
using IO.Milvus.Grpc;
using IO.Milvus.Param;
using IO.Milvus.Param.Dml;
using Microshaoft.RediSearch;
using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Npgsql;
using NRedisStack;
using NRedisStack.Search;
using Pgvector.Npgsql;
using Qdrant;
using System.Data;
using System.Data.Common;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ChromaQueryRequest = Microshaoft.Chromadb.Models.QueryRequest;
using ChromaQueryResponse = Microshaoft.Chromadb.Models.QueryResponse;
using PgVector = Pgvector.Vector;

namespace VectorDataBases.Performance;

public class TestContext
{
    //[Benchmark]
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


    //[Benchmark]
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

    //[Benchmark]
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

    //[Benchmark]
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

    //[Benchmark]
    public async Task Qdrant_Grpc_HNSW_Index_Cosine_100w_ProcessAsync()
    {
        using var channel = GrpcChannel.ForAddress(GlobalManager.SelfHostQdrantGrpcConnectionString);

        var client = new Points.PointsClient(channel);

        var searchPoints = new SearchPoints()
        {
            CollectionName = "embeddings"
            ,
            Offset = 0
            ,
            Limit = 20
            ,
            WithPayload = new WithPayloadSelector()
            {
                //Exclude = new PayloadExcludeSelector()
                Include = new PayloadIncludeSelector()
            }
            ,
            Params = new SearchParams()
            {
                Exact = false
                 ,
                HnswEf = 64
            }
        };
        var vectorDimension = 1536;
        var vector =
                Enumerable
                    .Range
                        (0, vectorDimension)
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

        foreach (var f in vector)
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

        includeFields.Add("id");
        includeFields.Add("text");
        includeFields.Add("title");
        includeFields.Add("url");
        includeFields.Add("vector_id");

        var result =
                    (
                        await client
                                    .SearchAsync
                                        (
                                            searchPoints
                                        )
                    ).Result;

        //var i = 0;
        foreach (var scoredPoint in result)
        {
            //Console.WriteLine(i++);
            MapField<string, Value> mapField = scoredPoint.Payload;
            // Iterate through the key-value pairs in the map field
            foreach (var keyValuePair in mapField)
            {
                _ = keyValuePair.Key;
                _ = keyValuePair.Value;
                //Console.WriteLine($"Key: {keyValuePair.Key}, Value: {keyValuePair.Value}");
            }
        }
    }

    //[Benchmark]
    public async Task Qdrant_SK_Http_HNSW_index_Cosine_100w_ProcessAsync()
    {
        var vectorDimension = 1536;

        var collectionName = "embeddings";

        var vector =
                Enumerable
                        .Range
                            (0, vectorDimension)
                        .Select
                            (
                                (x) =>
                                {
                                    return
                                        new Random()
                                                .NextSingle();
                                }
                            );

        using var httpClient = new HttpClient()
        {
            BaseAddress = new Uri(GlobalManager.SelfHostQdrantHttpConnectionString)
        };

        var qdrantVectorDbClient =
                        new QdrantVectorDbClient
                                (
                                    GlobalManager.SelfHostQdrantHttpConnectionString
                                    , vectorDimension
                                    , 6333
                                    , httpClient
                                );

        var searchResults =
                    qdrantVectorDbClient
                            .FindNearestInCollectionAsync
                                        (
                                            collectionName
                                            , vector
                                            , 0
                                            , top: 20

                                        );

        await foreach (var (qdrantVectorRecord, score) in searchResults)
        {
            var keyValuePairs = qdrantVectorRecord.Payload;
            foreach (var keyValuePair in keyValuePairs)
            {
                _ = keyValuePair.Key;
                _ = keyValuePair.Value;
                //Console.WriteLine($"{keyValuePair.Key}:{keyValuePair.Value}:({score})");
            }
        }
    }
    //[Benchmark]
    public async Task Milvus_Grpc_HNSW_index_L2_200w_ProcessAsync()
    {
        var vectors = new List<List<float>>()
        {
            Enumerable
                    .Range(0, 1536)
                    .Select
                        (
                            (x) =>
                            {
                                return
                                    new Random()
                                            .NextSingle();
                            }
                        )
                    .ToList()
        };

        var topK = 20;

        var milvusServiceClient =
                new MilvusServiceClient
                            (
                                ConnectParam
                                        .Create
                                            (
                                                "kc-misc-001-vm.koreacentral.cloudapp.azure.com"
                                                , 19530
                                            )
                            );

        var searchParam = new SearchParam<List<float>>()
        {
            CollectionName = "embeddings"
            ,
            MetricType = MetricType.L2
            ,
            Params = @"{ ""M"": 8, ""ef"": 8192 }"
            ,
            TopK = topK
            ,
            VectorFieldName = "title_vector"
            ,
            Vectors = vectors
            ,
            OutFields = new List<string>() { "id", "title", "content", "url" }
        };

        var searchResults = await milvusServiceClient.SearchAsync(searchParam);

        if (searchResults.Status != IO.Milvus.Param.Status.Success)
        {
            throw new Exception($"{searchResults.Status}", searchResults.Exception);
        }

        var fieldsData = searchResults.Data.Results.FieldsData;

        foreach (var fieldData in fieldsData)
        {
            if (fieldData.FieldCase == FieldData.FieldOneofCase.Scalars)
            {
                var scalars = fieldData.Scalars;
                _ = fieldData.FieldName;
                _ = fieldData.Scalars.ToString();
                //Console.WriteLine($"{fieldData.FieldName}: {fieldData.Scalars}");
            }
        }
    }

    [Benchmark]
    public async Task Chroma_Http_HNSW_index_Cosine_305k_ProcessAsync()
    {
        var vectorDimension = 1536;
        var n = 20;
        //var collectionName = "embeddings";
        var collectionId = "049e8e86-2652-49ce-8273-92d021f7c769";
        var vector =
                Enumerable
                        .Range
                            (0, vectorDimension)
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

        using var httpClient = new HttpClient()
        {
            BaseAddress = new Uri(GlobalManager.SelfHostChromaHttpConnectionString)
        };
        var queryRequest = new ChromaQueryRequest()
        { 
             query_embeddings = new float[][] { vector }
             , n_results = n
             , include = new string[] 
             {
                  "metadatas"
                , "documents"
                , "distances"
             }
        };
        var relativeUrl = $"api/v1/collections/{collectionId}/query";

        var json = 
            JsonSerializer.Serialize(queryRequest);

        var content = new StringContent(json);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        httpClient.DefaultRequestHeaders.TransferEncodingChunked = false;
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Length", content.Headers.ContentLength.ToString());

        var httpResponseMessage =
                        await httpClient
                                    .PostAsync
                                        (
                                            new Uri(relativeUrl, UriKind.Relative)
                                            //, queryRequest
                                            , content
                                        );

        httpResponseMessage.EnsureSuccessStatusCode();

        var queryResponse = await httpResponseMessage.Content.ReadFromJsonAsync<ChromaQueryResponse>();
        
        //Console.WriteLine(JsonSerializer.Serialize(queryResponse));
    }
}
