using System.Collections.Concurrent;
using Common.DS.Abstract;

namespace Common.DS;

public class InvertedIndex : IIndexAccessor // Implement the interface
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _index 
        = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, byte> _allDocumentIds 
        = new(StringComparer.OrdinalIgnoreCase); 

    public void Add(string term, string document)
    {
        var docDict = _index.GetOrAdd(term, _ => new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase));
        docDict.AddOrUpdate(document, 1, (_, count) => count + 1);
        
        _allDocumentIds.TryAdd(document, 0); 
    }

    public IReadOnlyDictionary<string, ConcurrentDictionary<string, int>> GetIndex() => _index;

    public int GetVocabularySize() => _index.Count;

    public long GetTotalWordCount()
    {
        return _index.Values.SelectMany(docFrequencies => docFrequencies.Values).Sum(count => (long)count);
    }

    // Method from IIndexAccessor
    public IReadOnlySet<string> GetDocumentsForTerm(string term)
    {
        if (_index.TryGetValue(term, out var docFrequencies))
        {
            return new HashSet<string>(docFrequencies.Keys, StringComparer.OrdinalIgnoreCase);
        }
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase); 
    }

    // Method from IIndexAccessor
    public IReadOnlySet<string> GetAllDocumentIds()
    {
        return new HashSet<string>(_allDocumentIds.Keys, StringComparer.OrdinalIgnoreCase);
    }

    public int GetDocumentCount() => _allDocumentIds.Count;
}
