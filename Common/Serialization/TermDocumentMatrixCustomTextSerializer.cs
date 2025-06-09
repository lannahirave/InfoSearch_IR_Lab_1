// Common/Serialization/TermDocumentMatrixCustomTextSerializer.cs
using Common.DS;
// If you created ITermDocumentMatrixSerializer and want to implement it:
// using Common.Serialization.Abstract; 
using System.Text.RegularExpressions;

// For IAsyncEnumerable if File.ReadLinesAsync is used in Deserialize

namespace Common.Serialization;

// public class TermDocumentMatrixCustomTextSerializer : ITermDocumentMatrixSerializer // If using interface
public class TermDocumentMatrixCustomTextSerializer
{
    // Regex to parse term lines like "[term]"
    private static readonly Regex TermRegex = new(@"^\[(.*)\]$", RegexOptions.Compiled);

    public async Task SerializeAsync(TermDocumentMatrix matrix, string filePath)
    {
        await using var writer = new StreamWriter(filePath); // Default UTF-8

        // Order terms for consistent output
        foreach (var term in matrix.GetAllTerms().OrderBy(t => t))
        {
            await writer.WriteLineAsync($"[{term}]").ConfigureAwait(false);
            // Order document IDs for consistent output
            foreach (var docId in matrix.GetDocumentsForTerm(term).OrderBy(d => d))
            {
                await writer.WriteLineAsync(docId).ConfigureAwait(false);
            }
        }
    }

    public async Task<TermDocumentMatrix> DeserializeAsync(string filePath)
    {
        var matrix = new TermDocumentMatrix();
        string? currentTerm = null;

        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"Error: File not found for TDM Custom Text deserialization: {filePath}");
            return matrix; // or throw
        }

        await foreach (var line in File.ReadLinesAsync(filePath).ConfigureAwait(false))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var termMatch = TermRegex.Match(line);
            if (termMatch.Success)
            {
                currentTerm = termMatch.Groups[1].Value;
                continue;
            }

            if (currentTerm == null)
            {
                Console.Error.WriteLine($"Warning: Found document entry before term in TDM Custom Text file: {filePath} - Line: {line}");
                continue;
            }

            // If it's not a term line and we have a currentTerm, it must be a document ID
            var documentId = line.Trim(); // Trim to be safe, though not strictly necessary if format is clean
            if (!string.IsNullOrEmpty(documentId))
            {
                matrix.Add(currentTerm, documentId);
            }
            else
            {
                Console.Error.WriteLine($"Warning: Empty document ID line for term '{currentTerm}' in TDM Custom Text file: {filePath} - Line: {line}");
            }
        }
        return matrix;
    }
}
