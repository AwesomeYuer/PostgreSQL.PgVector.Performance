namespace Microshaoft.Chromadb.Models;

public class QueryRequest
{
    public float[][]? query_embeddings { get; set; }
    public int n_results { get; set; }
    public Where where { get; set; } = new Where();
    public Where_Document where_document { get; set; } = new Where_Document();
    public string[]? include { get; set; }
}

public class Where
{
}

public class Where_Document
{
}
