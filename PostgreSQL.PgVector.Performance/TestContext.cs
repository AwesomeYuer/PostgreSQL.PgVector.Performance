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

namespace PostgreSQL.PgVector.Performance
{
    public class TestContext
    {
        [Benchmark()]
        public async Task ProcessAsync()
        {
            Thread.Sleep(10);
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(GlobalManager.ConnectionString);
            dataSourceBuilder.UseVector();

            using var npgsqlDataSource = dataSourceBuilder.Build();
            var connection = await npgsqlDataSource.OpenConnectionAsync();
            //Console.WriteLine("Opened");

            var floats = new float[1536]
                                .Select
                                    (
                                        (x) =>
                                        {
                                            return
                                               (float)new Random().NextDouble();
                                        }
                                    )
                                .ToArray();
            var pgVector = new Vector(floats);

            var sql = @$"
SELECT
    *
    , embedding <-> $1::vector                      as ""EuclideanDistance""
    , cosine_distance(embedding, $1::vector)        as ""CosineDistance""
FROM
    embeddings AS a
order by
    ""EuclideanDistance""
LIMIT $2;
";
            using var npgsqlCommand = new NpgsqlCommand();
            npgsqlCommand.Connection = connection;
            npgsqlCommand.CommandText = sql;
            npgsqlCommand.Parameters.AddWithValue(pgVector);
            npgsqlCommand.Parameters.AddWithValue(10);
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

        //[Benchmark]
        //public async Task ProcessAsync2()
        //{

        //    //await using
        //    //    (
        //    //        DbDataReader dataReader =
        //    //                        await _npgsqlCommand.ExecuteReaderAsync()
        //    //    )
        //    //{
        //    //    while (await dataReader.ReadAsync())
        //    //    {
        //    //        IDataRecord dataRecord = dataReader;
        //    //        var fieldsCount = dataRecord.FieldCount;
        //    //        for (var i = 0; i < fieldsCount; i++)
        //    //        {
        //    //            _ = dataRecord[dataReader.GetName(i)];
        //    //        }
        //    //    }
        //    //}
        //}
    }
}
