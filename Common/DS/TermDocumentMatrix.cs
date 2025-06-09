using System.Collections.Concurrent;
using Common.DS.Abstract;

namespace Common.DS;

public class TermDocumentMatrix : IIndexAccessor // Implement the interface
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _matrix 
        = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, byte> _allDocumentIds 
        = new(StringComparer.OrdinalIgnoreCase); 

    public void Add(string term, string documentId)
    {
        if (string.IsNullOrWhiteSpace(term))
            throw new ArgumentNullException(nameof(term));
        if (string.IsNullOrWhiteSpace(documentId))
            throw new ArgumentNullException(nameof(documentId));

        _allDocumentIds.TryAdd(documentId, 0); 

        var documentSet = _matrix.GetOrAdd(term, _ => new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase));
        documentSet.TryAdd(documentId, 0); 
    }

    // Method from IIndexAccessor
    public IReadOnlySet<string> GetDocumentsForTerm(string term)
    {
        if (_matrix.TryGetValue(term, out var documentSet))
        {
            return new HashSet<string>(documentSet.Keys, StringComparer.OrdinalIgnoreCase);
        }
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    // Method from IIndexAccessor
    public IReadOnlySet<string> GetAllDocumentIds()
    {
        return new HashSet<string>(_allDocumentIds.Keys, StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<string> GetAllTerms()
    {
        return _matrix.Keys; 
    }

    public int GetVocabularySize() => _matrix.Count; 

    public int GetDocumentCount() => _allDocumentIds.Count; 
    
    public long GetNonZeroEntriesCount()
    {
        return _matrix.Values.Sum(set => (long)set.Count);
    }
}
