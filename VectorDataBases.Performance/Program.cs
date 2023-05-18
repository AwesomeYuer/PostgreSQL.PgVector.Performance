// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using VectorDataBases.Performance;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        //GlobalManager.ConnectionString = string.Format(GlobalManager.ConnectionString, args[0]);
        //new TestContext().WikipediaPostgreSQL_ivfflat_vector_cosine_index_25k_ProcessAsync().Wait();
        ////new TestContext().WikipediaSelfHostRediSearch_FLAT_index_Cosine_25k_ProcessAsync().Wait();
        // new TestContext().RediSearch_HNSW_index_Cosine_225k_ProcessAsync().Wait();

        //new TestContext().Qdrant_Grpc_HNSW_Index_Cosine_225k_ProcessAsync().Wait();
        //new TestContext().Qdrant_SK_Http_HNSW_index_Cosine_225k_ProcessAsync().Wait();
        //new TestContext().Milvus_Grpc_HNSW_index_L2_50w_ProcessAsync().Wait();

        //new TestContext().Chroma_Http_HNSW_index_Cosine_100w_ProcessAsync().Wait(); 

        _ = BenchmarkRunner.Run<TestContext>();
        Console.WriteLine($"{nameof(BenchmarkRunner)}.{nameof(BenchmarkRunner.Run)} done!");
        Console.ReadLine();
    }
    

}