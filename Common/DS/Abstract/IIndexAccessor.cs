namespace Common.DS.Abstract;

public interface IIndexAccessor
{
    /// <summary>
    /// Retrieves a set of document IDs that contain the specified term.
    /// </summary>
    /// <param name="term">The term to search for (should be pre-normalized).</param>
    /// <returns>A read-only set of document IDs. Returns an empty set if the term is not found.</returns>
    IReadOnlySet<string> GetDocumentsForTerm(string term);

    /// <summary>
    /// Retrieves a set of all unique document IDs present in the index.
    /// Crucial for NOT operations.
    /// </summary>
    /// <returns>A read-only set of all document IDs.</returns>
    IReadOnlySet<string> GetAllDocumentIds();
}
