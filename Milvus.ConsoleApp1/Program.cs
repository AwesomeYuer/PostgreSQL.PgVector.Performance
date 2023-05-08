using IO.Milvus.Client;
using IO.Milvus.Grpc;
using IO.Milvus.Param;

using IO.Milvus.Param.Dml;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

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
}
;

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
    , MetricType = MetricType.L2
    , Params = @"{ ""M"": 8, ""ef"": 64 }"
    , TopK = topK
    , VectorFieldName = "title_vector"
    , Vectors = vectors
    , OutFields = new List<string>() { "id", "title", "content", "url" }
};

var searchResults = await milvusServiceClient.SearchAsync(searchParam);

var fieldsData = searchResults.Data.Results.FieldsData;

foreach (var f in fieldsData)
{
    if (f.FieldCase == FieldData.FieldOneofCase.Scalars)
    {
        var scalars = f.Scalars;
        Console.WriteLine($"{f.FieldName}: {f.Scalars}");
    }
}
