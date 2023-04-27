using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using StackExchange.Redis;
using System.Runtime.InteropServices;

namespace Microshaoft.RediSearch;
public static class NRedisStackExtensions
{
    public static Query AddArrayParameter<T>
                (
                    this Query @this
                    , string parameterName
                    , T[] parameter
                )
        where T : struct
    {
        byte[] bytes = MemoryMarshal.Cast<T, byte>(parameter).ToArray();
        @this.AddParam(parameterName, bytes);
        return @this;
    }

    public static async Task<SearchResult> FTSearchAsync
                (
                      this Query @this
                    , string connectionString
                    , string indexName
                )
    {
        using var redis =
                ConnectionMultiplexer
                        .Connect(connectionString);
        IDatabase database = redis.GetDatabase();
        SearchCommands searcher = database.FT();
        var searchResult =
                await
                    searcher
                        .SearchAsync
                            (
                                indexName
                                , @this
                            );
        await redis.CloseAsync();
        return searchResult;
    }
}
