namespace Common.DS.Abstract;

public interface IInvertedIndexBuilder
{
    Task<InvertedIndex> BuildAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default, bool fakeSingleThreaded = false);
}
