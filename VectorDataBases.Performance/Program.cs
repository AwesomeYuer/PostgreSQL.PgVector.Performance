// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using VectorDataBases.Performance;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        //GlobalManager.ConnectionString = string.Format(GlobalManager.ConnectionString, args[0]);
        new TestContext().WikipediaPostgreSQL_ivfflat_vector_cosine_ops_index_25k_ProcessAsync().Wait();
        //new TestContext().WikipediaSelfHostRediSearch_FLAT_index_Cosine_25k_ProcessAsync().Wait();
        new TestContext().VecSimRediSearch_HNSW_index_Cosine_225k_ProcessAsync().Wait();
        //_ = BenchmarkRunner.Run<TestContext>();
        Console.ReadLine();
    }
    

}