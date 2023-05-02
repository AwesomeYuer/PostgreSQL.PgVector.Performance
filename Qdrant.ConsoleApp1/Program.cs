// See https://aka.ms/new-console-template for more information
using Google.Protobuf.Collections;
using Grpc.Net.Client;
using Qdrant;

Console.WriteLine("Hello, World!");

var channel = GrpcChannel.ForAddress("http://kc-misc-001-vm.koreacentral.cloudapp.azure.com:6334");
//var client = new Greeter.GreeterClient(channel);

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

foreach (var f in result)
{
    MapField<string, Value> mapField = f.Payload;
    // Iterate through the key-value pairs in the map field
    foreach (var pair in mapField)
    {
        string key = pair.Key;
        Value @value = pair.Value;
        Console.WriteLine($"Key: {key}, Value: {@value}");
    }
}

//Console.WriteLine("asdasd");

