using System.Text.RegularExpressions;
using Common.DS;
using Common.Serialization.Abstract;
using System.IO; // Для StreamWriter та File
using System.Threading.Tasks; // Для Task
using System.Collections.Generic; // Для IAsyncEnumerable (потрібен для File.ReadLinesAsync)

namespace Common.Serialization;

public class CustomTextIndexSerializer : IIndexSerializer
{
    // Regex to parse term lines like "[term]"
    private static readonly Regex TermRegex = new(@"^\[(.*)\]$", RegexOptions.Compiled);
    // Regex to parse doc-frequency lines like "document.txt=5"
    private static readonly Regex DocFreqRegex = new(@"^(.*)=(\d+)$", RegexOptions.Compiled);

    public async Task SerializeAsync(InvertedIndex index, string filePath)
    {
        var indexData = index.GetIndex();
        // Використовуємо асинхронний StreamWriter
        // За замовчуванням StreamWriter використовує UTF-8 без BOM, що зазвичай добре
        await using var writer = new StreamWriter(filePath); // `await using` для IAsyncDisposable (якщо є) або IDisposable

        // Сортування для консистентності вихідного файлу (опціонально, але корисно для тестів/порівнянь)
        foreach (var termEntry in indexData.OrderBy(kv => kv.Key))
        {
            await writer.WriteLineAsync($"[{termEntry.Key}]").ConfigureAwait(false);
            foreach (var docEntry in termEntry.Value.OrderBy(kv => kv.Key))
            {
                await writer.WriteLineAsync($"{docEntry.Key}={docEntry.Value}").ConfigureAwait(false);
            }
        }
        // FlushAsync може бути корисним, якщо потрібно гарантувати запис перед закриттям
        // await writer.FlushAsync().ConfigureAwait(false);
        // StreamWriter буде автоматично змито та закрито при виході з блоку using
    }

    public async Task<InvertedIndex> DeserializeAsync(string filePath)
    {
        var index = new InvertedIndex();
        string? currentTerm = null;

        // Перевіряємо, чи файл існує, щоб уникнути FileNotFoundException від ReadLinesAsync
        if (!File.Exists(filePath))
        {
            // Можна повернути порожній індекс, або кинути більш специфічний виняток
            Console.Error.WriteLine($"Error: File not found for deserialization: {filePath}");
            return index; // або throw new FileNotFoundException("Index file not found", filePath);
        }

        // Асинхронне читання рядків
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
                // Логуємо помилку, але продовжуємо, намагаючись обробити решту файлу
                // Або можна кинути виняток, якщо файл має бути строго валідним
                Console.Error.WriteLine($"Warning: Found document entry before term in file: {filePath} - Line: {line}");
                continue;
            }

            var docFreqMatch = DocFreqRegex.Match(line);
            if (docFreqMatch.Success)
            {
                var document = docFreqMatch.Groups[1].Value;
                if (int.TryParse(docFreqMatch.Groups[2].Value, out var frequency))
                {
                    for (var i = 0; i < frequency; i++)
                    {
                        index.Add(currentTerm, document); // Ця операція залишається синхронною
                    }
                }
                else
                {
                    Console.Error.WriteLine($"Warning: Could not parse frequency for term '{currentTerm}', doc '{document}' in file: {filePath} - Line: {line}");
                }
            }
            else
            {
                Console.Error.WriteLine($"Warning: Malformed line for term '{currentTerm}' in file: {filePath} - Line: {line}");
            }
        }
        return index;
    }
}
