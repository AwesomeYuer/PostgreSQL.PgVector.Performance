namespace Microshaoft.Chromadb.Models;

public class QueryResponse
{
    public string[][]? ids { get; set; }
    public float[][]? embeddings { get; set; }
    public string[][]? documents { get; set; }
    public Metadata[][]? metadatas { get; set; }
    public float[][]? distances { get; set; }
}

public class Metadata
{
    public string? title { get; set; }
    public string? content { get; set; }
}
