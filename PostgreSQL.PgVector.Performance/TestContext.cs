using BenchmarkDotNet.Attributes;
using Npgsql;
using Pgvector;
using Pgvector.Npgsql;
using System.Data;
using System.Data.Common;

namespace PostgreSQL.PgVector.Performance;

public class TestContext
{
    [Benchmark]
    public async Task ProcessAsync()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(GlobalManager.ConnectionString);
        dataSourceBuilder.UseVector();

        using var npgsqlDataSource = dataSourceBuilder.Build();
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
                                    (float)
                                        new Random()
                                                .NextDouble();
                            }
                        )
                    .ToArray();

            var pgVector = new Vector(floats);
            var limit = 20;
            var sql = @$"
WITH
T
AS
(
    SELECT
        *
        , embedding <-> $1::vector  as ""EuclideanL2Distance""
        , embedding <=> $1::vector  as ""CosineDistance""
    FROM
        embeddings AS a
)
SELECT
    *
    , (1 - a.""CosineDistance"")    as ""CosineSimilarity""
FROM
    T AS a
ORDER BY
    --""EuclideanL2Distance""
    ""CosineDistance"" 
    --""CosineSimilarity""
                --DESC
LIMIT $2;
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
            await connection.CloseAsync();
        }
    }
}
