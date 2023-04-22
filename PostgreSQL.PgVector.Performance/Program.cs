// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Npgsql;
using Pgvector;
using Pgvector.Npgsql;
using PostgreSQL.PgVector.Performance;
using System;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;

public class Program
{
    
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        //GlobalManager.ConnectionString = string.Format(GlobalManager.ConnectionString, args[0]);
        //Thread.Sleep(5 * 1000);

        //new Program().ProcessAsync().Wait();
        _ = BenchmarkRunner.Run<TestContext>();
        Console.ReadLine();
    }
    

}