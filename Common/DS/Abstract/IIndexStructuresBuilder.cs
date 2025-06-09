namespace Common.DS.Abstract;

public interface IIndexStructuresBuilder
{
    Task<(InvertedIndex InvertedIndex, TermDocumentMatrix TermDocumentMatrix)> BuildAsync(
        IEnumerable<string> filePaths, 
        CancellationToken cancellationToken = default, 
        bool fakeSingleThreaded = false);
}
