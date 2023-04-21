// See https://aka.ms/new-console-template for more information
using Npgsql;
using Pgvector;
using Pgvector.Npgsql;
using System.Data;

Console.WriteLine("Hello, World!");

var mod = 101;
var i = 0;
var groups =
        ReadDataAsIEnumerable
                    (args[1])
                            .Select
                                (
                                    (x) =>
                                    {
                                        i ++;
                                        return
                                            x;
                                    }
                                )
                            .GroupBy
                                (
                                    (x) =>
                                    {
                                        return
                                            (i - 1)/mod;
                                    }
                                )
                    ;

var connectionString = $"Host=localhost;Database=pgvectors;User Id=sa;Password={args[0]}";

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.UseVector();

using var npgsqlDataSource = dataSourceBuilder.Build();
using var connection = npgsqlDataSource.OpenConnection();

var sql = @$"
INSERT INTO embeddings (content, vector_id, embedding)
VALUES
";
foreach (var group in groups)
{
    Console.WriteLine($"{nameof(group.Key)}: {group.Key}");
    await using (var sqlCommand = new NpgsqlCommand())
    {
        var j = 0;
        var values = string.Empty;
        foreach (var item in group)
        {
            //Console.WriteLine($"{nameof(item.GroupId)}: {item.GroupId}");
            var content = Guid.NewGuid().ToString();
            sqlCommand.Parameters.AddWithValue(content);

            // 2
            sqlCommand.Parameters.AddWithValue(item.GroupId);

            // 3
            var pgVector = new Vector(item.VectorArray);
            sqlCommand.Parameters.AddWithValue(pgVector);

            if (!string.IsNullOrEmpty(values))
            {
                values = $"{values}\r\n, ";
            }
            values = $"{values}(${++ j}, ${++ j}, ${++ j})";
        }
        if (!string.IsNullOrEmpty(values))
        {
            sqlCommand.Connection = connection;
            sqlCommand.CommandText = $"{sql}\r\n{values}";
            await sqlCommand.ExecuteNonQueryAsync();
        }
    }
}

IEnumerable<(int GroupId, float[] VectorArray)>
        ReadDataAsIEnumerable(string filePath, int arrayLength = 1536)
{
    using var stream = File.OpenRead(filePath);
    using var streamReader = new StreamReader(stream);
    var line = string.Empty;
    var groupId = string.Empty;
    var lastGroupId = string.Empty;
    var index = -1;
    var floatsArray = new float[arrayLength];

    while
        (
            !string
                .IsNullOrEmpty
                        (
                            line = streamReader.ReadLine()
                        )
        )
    {
        line = ReplaceToOneWhiteSpace(line!);
        var a = line!.Split(' ');
        groupId = a[0].Trim();
        if (index == -1)
        {
            lastGroupId = groupId;
        }
        if
            (
                lastGroupId != groupId
            )
        {
            yield
                return
                    (
                          int.Parse(lastGroupId)
                        , floatsArray
                    );
            floatsArray = new float[floatsArray.Length];
            lastGroupId = groupId;
        }
        index = int.Parse(a[1].Trim());
        floatsArray[index] = float.Parse(a[2].Trim());
    }
    if (index <= arrayLength)
    {
        yield
            return
                (
                      int.Parse(groupId)
                    , floatsArray
                );
    }
}


string ReplaceToOneWhiteSpace(string s)
{
    var length = s.Length;
    var lastLength = -1;
    do
    {
        lastLength = length;
        s = s.Replace("  ", " ");
        length = s.Length;
    }
    while
        (lastLength != length);
    return s;
}