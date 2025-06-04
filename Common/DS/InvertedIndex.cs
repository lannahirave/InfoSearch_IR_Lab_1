using System.Collections.Concurrent;

namespace Common.DS;

public class InvertedIndex
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _index 
        = new(StringComparer.OrdinalIgnoreCase);

    public void Add(string term, string document)
    {
        var docDict = _index.GetOrAdd(term, _ => new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase));
        docDict.AddOrUpdate(document, 1, (_, count) => count + 1);
    }

    public IReadOnlyDictionary<string, ConcurrentDictionary<string, int>> GetIndex() => _index;

    /// <summary>
    /// Gets the size of the vocabulary (number of unique terms).
    /// </summary>
    public int GetVocabularySize() => _index.Count;

    /// <summary>
    /// Gets the total number of words (tokens) indexed in the collection.
    /// This is the sum of frequencies of all terms in all documents.
    /// </summary>
    public long GetTotalWordCount()
    {
        // Summing up all frequency counts.
        // Using long to prevent potential overflow if there are many words.
        return _index.Values.SelectMany(docFrequencies => docFrequencies.Values).Sum(count => (long)count);
    }
}
