// ConsoleApplication/Workflow/IndexSerializationManager.cs
using System.Diagnostics;
using Common.DS;
using Common.Serialization; // For TermDocumentMatrixJsonSerializer, TermDocumentMatrixCustomTextSerializer
using Common.Serialization.Abstract; // For IIndexSerializer

namespace ConsoleApplication.Workflow;

public class IndexSerializationManager
{
    private readonly IIndexSerializer _jsonInvertedIndexSerializer;
    private readonly IIndexSerializer _customTextInvertedIndexSerializer;
    private readonly TermDocumentMatrixJsonSerializer _termDocumentMatrixJsonSerializer;
    private readonly TermDocumentMatrixCustomTextSerializer _termDocumentMatrixCustomTextSerializer; // Added
    private readonly string _outputDirectoryPath;

    public IndexSerializationManager(
        IIndexSerializer jsonInvertedIndexSerializer,
        IIndexSerializer customTextInvertedIndexSerializer,
        TermDocumentMatrixJsonSerializer termDocumentMatrixJsonSerializer,
        TermDocumentMatrixCustomTextSerializer termDocumentMatrixCustomTextSerializer, // Added
        string outputDirectoryPath)
    {
        _jsonInvertedIndexSerializer = jsonInvertedIndexSerializer ?? throw new ArgumentNullException(nameof(jsonInvertedIndexSerializer));
        _customTextInvertedIndexSerializer = customTextInvertedIndexSerializer ?? throw new ArgumentNullException(nameof(customTextInvertedIndexSerializer));
        _termDocumentMatrixJsonSerializer = termDocumentMatrixJsonSerializer ?? throw new ArgumentNullException(nameof(termDocumentMatrixJsonSerializer));
        _termDocumentMatrixCustomTextSerializer = termDocumentMatrixCustomTextSerializer ?? throw new ArgumentNullException(nameof(termDocumentMatrixCustomTextSerializer)); // Added
        _outputDirectoryPath = outputDirectoryPath;
    }

    public async Task SerializeInvertedIndexAsync(InvertedIndex invertedIndex)
    {
        if (invertedIndex == null)
        {
            Console.WriteLine("Інвертований індекс відсутній, серіалізацію пропущено.");
            return;
        }

        Console.WriteLine("\n--- Збереження інвертованого індексу ---");
        var stopwatch = new Stopwatch();

        // JSON Serialization
        var jsonFilePath = Path.Combine(_outputDirectoryPath, "inverted_index.json");
        Console.WriteLine($"Збереження у JSON форматі: {jsonFilePath}");
        stopwatch.Restart();
        try
        {
            await _jsonInvertedIndexSerializer.SerializeAsync(invertedIndex, jsonFilePath);
            stopwatch.Stop();
            Console.WriteLine($"  Збережено за: {stopwatch.ElapsedMilliseconds} мс");
            Console.WriteLine($"  Розмір файлу: {new FileInfo(jsonFilePath).Length / 1024.0:F2} KB");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"  ПОМИЛКА при збереженні інвертованого індексу у JSON: {ex.Message}");
        }

        // Custom Text Serialization
        var customTextFilePath = Path.Combine(_outputDirectoryPath, "inverted_index_custom.txt");
        Console.WriteLine($"Збереження у власному текстовому форматі: {customTextFilePath}");
        stopwatch.Restart();
        try
        {
            await _customTextInvertedIndexSerializer.SerializeAsync(invertedIndex, customTextFilePath);
            stopwatch.Stop();
            Console.WriteLine($"  Збережено за: {stopwatch.ElapsedMilliseconds} мс");
            Console.WriteLine($"  Розмір файлу: {new FileInfo(customTextFilePath).Length / 1024.0:F2} KB");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"  ПОМИЛКА при збереженні інвертованого індексу у Custom Text: {ex.Message}");
        }
    }

    public async Task SerializeTermDocumentMatrixAsync(TermDocumentMatrix termDocumentMatrix)
    {
        if (termDocumentMatrix == null)
        {
            Console.WriteLine("Матриця інцидентності відсутня, серіалізацію пропущено.");
            return;
        }

        Console.WriteLine("\n--- Збереження матриці інцидентності \"термін-документ\" ---");
        var stopwatch = new Stopwatch();

        // JSON Serialization for TermDocumentMatrix
        var tdmJsonFilePath = Path.Combine(_outputDirectoryPath, "term_document_matrix.json");
        Console.WriteLine($"Збереження у JSON форматі: {tdmJsonFilePath}");
        stopwatch.Restart();
        try
        {
            await _termDocumentMatrixJsonSerializer.SerializeAsync(termDocumentMatrix, tdmJsonFilePath);
            stopwatch.Stop();
            Console.WriteLine($"  Збережено за: {stopwatch.ElapsedMilliseconds} мс");
            Console.WriteLine($"  Розмір файлу: {new FileInfo(tdmJsonFilePath).Length / 1024.0:F2} KB");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"  ПОМИЛКА при збереженні матриці інцидентності у JSON: {ex.Message}");
        }

        // Custom Text Serialization for TermDocumentMatrix
        var tdmCustomTextFilePath = Path.Combine(_outputDirectoryPath, "term_document_matrix_custom.txt");
        Console.WriteLine($"Збереження у власному текстовому форматі: {tdmCustomTextFilePath}");
        stopwatch.Restart();
        try
        {
            await _termDocumentMatrixCustomTextSerializer.SerializeAsync(termDocumentMatrix, tdmCustomTextFilePath);
            stopwatch.Stop();
            Console.WriteLine($"  Збережено за: {stopwatch.ElapsedMilliseconds} мс");
            Console.WriteLine($"  Розмір файлу: {new FileInfo(tdmCustomTextFilePath).Length / 1024.0:F2} KB");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"  ПОМИЛКА при збереженні матриці інцидентності у Custom Text: {ex.Message}");
        }
    }
}
