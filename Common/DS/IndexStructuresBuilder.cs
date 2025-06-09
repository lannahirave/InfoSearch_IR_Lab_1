using Common.DS.Abstract;
using Common.TextProcessing.Abstract;
using Common.TextReaders.Abstract;

namespace Common.DS;
public class IndexStructuresBuilder : IIndexStructuresBuilder
{
    private readonly ITextReaderFactory _readerFactory;
    private readonly ITokenNormalizer _tokenNormalizer;

    public IndexStructuresBuilder(ITextReaderFactory readerFactory, ITokenNormalizer tokenNormalizer)
    {
        _tokenNormalizer = tokenNormalizer ?? throw new ArgumentNullException(nameof(tokenNormalizer));
        _readerFactory = readerFactory ?? throw new ArgumentNullException(nameof(readerFactory));
    }

   public async Task<(InvertedIndex InvertedIndex, TermDocumentMatrix TermDocumentMatrix)> BuildAsync(
        IEnumerable<string> filePaths, 
        CancellationToken cancellationToken = default, 
        bool fakeSingleThreaded = false)
    {
        var invertedIndex = new InvertedIndex();
        var termDocumentMatrix = new TermDocumentMatrix();
        
        // Collect all unique document IDs processed to pass to TermDocumentMatrix if needed later for 'GetAllDocumentIds'
        // Although TermDocumentMatrix now collects them itself, this could be an alternative.
        // For now, TermDocumentMatrix handles its own _allDocumentIds.

        var degreeOfParallelism = fakeSingleThreaded ? 1 : Environment.ProcessorCount;
        
        await Task.Run(() =>
        {
            Parallel.ForEach(filePaths, new ParallelOptions { CancellationToken = cancellationToken, MaxDegreeOfParallelism = degreeOfParallelism }, filePath =>
            {
                cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation

                var reader = _readerFactory.GetReader(filePath);
                if (reader == null)
                {
                    // Optionally log or handle unsupported file types
                    // Console.WriteLine($"Warning: No reader found for file: {filePath}");
                    return; 
                }

                try
                {
                    using var stream = File.OpenRead(filePath);
                    string documentId = Path.GetFileName(filePath); // Use filename as document ID

                    foreach (var rawWord in reader.ReadWords(stream))
                    {
                        cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation frequently

                        var normalizedWord = _tokenNormalizer.Normalize(rawWord);
                        if (!string.IsNullOrWhiteSpace(normalizedWord))
                        {
                            invertedIndex.Add(normalizedWord, documentId);
                            termDocumentMatrix.Add(normalizedWord, documentId);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Propagate cancellation
                    throw;
                }
                catch (Exception ex)
                {
                    // Log error for the specific file and continue with others
                    // This prevents one bad file from stopping the entire process.
                    Console.Error.WriteLine($"Error processing file {filePath}: {ex.Message}");
                    // Depending on requirements, you might want to re-throw or handle differently.
                }
            });
        }, cancellationToken);

        return (invertedIndex, termDocumentMatrix);
    }
}
