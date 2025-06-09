// Common/Serialization/TermDocumentMatrixJsonSerializer.cs
using Common.DS;
// If you created ITermDocumentMatrixSerializer
using System.Text.Encodings.Web;
using System.Text.Json;

// For Dictionary and List

namespace Common.Serialization;

// Implement the new interface if you created it:
// public class TermDocumentMatrixJsonSerializer : ITermDocumentMatrixSerializer 
public class TermDocumentMatrixJsonSerializer // Or without interface for now
{
    public async Task SerializeAsync(TermDocumentMatrix matrix, string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // Convert the matrix's internal ConcurrentDictionary structure to a serializable format
        // _matrix is ConcurrentDictionary<string, ConcurrentDictionary<string, byte>>
        // We want to serialize it as Dictionary<string, List<string>>
        var serializableMatrix = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var termEntry in matrix.GetAllTerms().OrderBy(t => t)) // Order for consistent output
        {
            var docIds = matrix.GetDocumentsForTerm(termEntry).OrderBy(d => d).ToList(); // Order for consistent output
            if (docIds.Any())
            {
                serializableMatrix[termEntry] = docIds;
            }
        }
        
        await using var createStream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(createStream, serializableMatrix, options);
    }

    public async Task<TermDocumentMatrix> DeserializeAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"Error: File not found for TDM deserialization: {filePath}");
            return new TermDocumentMatrix(); // or throw
        }

        var options = new JsonSerializerOptions
        {
            // No specific options needed for deserialization here
        };

        await using var openStream = File.OpenRead(filePath);
        var deserialized = await JsonSerializer.DeserializeAsync<Dictionary<string, List<string>>>(openStream, options);

        var matrix = new TermDocumentMatrix();
        if (deserialized != null)
        {
            foreach (var termEntry in deserialized)
            {
                foreach (var documentId in termEntry.Value)
                {
                    matrix.Add(termEntry.Key, documentId);
                }
            }
        }
        return matrix;
    }
}
