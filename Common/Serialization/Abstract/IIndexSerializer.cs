using Common.DS;

namespace Common.Serialization.Abstract;

public interface IIndexSerializer
{
    Task SerializeAsync(InvertedIndex index, string filePath);
    Task<InvertedIndex> DeserializeAsync(string filePath);
}
