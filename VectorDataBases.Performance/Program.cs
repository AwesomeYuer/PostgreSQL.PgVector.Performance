// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using VectorDataBases.Performance;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        //GlobalManager.ConnectionString = string.Format(GlobalManager.ConnectionString, args[0]);
        //new TestContext().WikipediaSelfHostRediSearch_25k_ProcessAsync().Wait();
        new TestContext().WikipediaAzureRediSearch_25k_ProcessAsync().Wait();
        //_ = BenchmarkRunner.Run<TestContext>();
        Console.ReadLine();
    }
    

}