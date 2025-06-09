using Common.DS;

namespace Common.Serialization.Abstract;

public interface ITermDocumentMatrixSerializer
{
    Task SerializeAsync(TermDocumentMatrix matrix, string filePath);
    Task<TermDocumentMatrix> DeserializeAsync(string filePath);
}
