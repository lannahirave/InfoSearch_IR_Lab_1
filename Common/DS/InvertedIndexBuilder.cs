using Common.DS.Abstract;
using Common.TextProcessing.Abstract;
using Common.TextReaders.Interfaces;

namespace Common.DS;
public class InvertedIndexBuilder : IInvertedIndexBuilder
{
    private readonly ITextReaderFactory _readerFactory;
    private readonly ITokenNormalizer _tokenNormalizer;

    public InvertedIndexBuilder(ITextReaderFactory readerFactory, ITokenNormalizer tokenNormalizer)
    {
        _tokenNormalizer = tokenNormalizer ?? throw new ArgumentNullException(nameof(tokenNormalizer));
        _readerFactory = readerFactory ?? throw new ArgumentNullException(nameof(readerFactory));
    }

    public async Task<InvertedIndex> BuildAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default, bool fakeSingleThreaded = false)
    {
        var index = new InvertedIndex();
        
        var degreeOfParallelism = fakeSingleThreaded ? 1 : Environment.ProcessorCount; // Використовуємо всі ядра, якщо не вказано інакше
        await Task.Run(() =>
        {
            Parallel.ForEach(filePaths, new ParallelOptions { CancellationToken = cancellationToken, MaxDegreeOfParallelism = degreeOfParallelism }, filePath =>
            {
                var reader = _readerFactory.GetReader(filePath);
                if (reader == null) return;

                using var stream = File.OpenRead(filePath);
                foreach (var rawWord in reader.ReadWords(stream)) // reader.ReadWords тепер повертає сирі слова
                {
                    var normalizedWord = _tokenNormalizer.Normalize(rawWord); // Нормалізація тут
                    if (!string.IsNullOrWhiteSpace(normalizedWord)) // Нормалізатор може повернути порожній рядок
                    {
                        index.Add(normalizedWord, Path.GetFileName(filePath));
                    }
                }
            });
        }, cancellationToken);

        return index;
    }
}
