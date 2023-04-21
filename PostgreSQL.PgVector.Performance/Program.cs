// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Npgsql;
using Pgvector.Npgsql;
using System.Data;
using System.Data.Common;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        _ = BenchmarkRunner.Run<Program>();
        Console.ReadLine();
    }

    [Benchmark]
    public async Task ProcessAsync()
    {
        var connectionString = $"Host=localhost;Database=pgvectors;User Id=sa;Password=!@#123QWE";

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseVector();

        using var npgsqlDataSource = dataSourceBuilder.Build();
        using var connection = await npgsqlDataSource.OpenConnectionAsync();

        var sql = @$"
SELECT
    *
FROM
    embeddings AS a
LIMIT 10;
";
        using var npgsqlCommand = new NpgsqlCommand();
        npgsqlCommand.Connection = connection;
        npgsqlCommand.CommandText = sql;

        using
            (
                DbDataReader dataReader =
                                await npgsqlCommand.ExecuteReaderAsync()
            )
        {
            while (await dataReader.ReadAsync())
            {
                IDataRecord dataRecord = dataReader;
                var fieldsCount = dataRecord.FieldCount;
                for (var i = 0; i < fieldsCount; i++)
                {
                    _ = dataRecord[dataReader.GetName(i)];
                }
            }
        }
    }

    [Benchmark]
    public async Task ProcessAsync2()
    {
        
        //await using
        //    (
        //        DbDataReader dataReader =
        //                        await _npgsqlCommand.ExecuteReaderAsync()
        //    )
        //{
        //    while (await dataReader.ReadAsync())
        //    {
        //        IDataRecord dataRecord = dataReader;
        //        var fieldsCount = dataRecord.FieldCount;
        //        for (var i = 0; i < fieldsCount; i++)
        //        {
        //            _ = dataRecord[dataReader.GetName(i)];
        //        }
        //    }
        //}
    }
}