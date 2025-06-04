// Common/Serialization/JsonIndexSerializer.cs
using Common.DS;
using Common.Serialization.Abstract;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web; // Потрібно для JavaScriptEncoder
using System.Text.Json;
using System.Threading.Tasks;

namespace Common.Serialization;

public class JsonIndexSerializer : IIndexSerializer
{
    public async Task SerializeAsync(InvertedIndex index, string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            // Дозволяє записувати символи поза ASCII (наприклад, кирилицю, грецьку) як є,
            // а не як \uXXXX послідовності.
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // Для серіалізації ConcurrentDictionary може знадобитись конвертація
        // в звичайний Dictionary або спеціальний конвертер.
        // Простий варіант - серіалізувати результат GetIndex()
        var serializableIndex = index.GetIndex()
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(d => d.Key, d => d.Value));

        await using var createStream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(createStream, serializableIndex, options);
        // Немає потреби в StreamWriter з явною кодуванням UTF-8,
        // JsonSerializer.SerializeAsync(Stream, ...) за замовчуванням використовує UTF-8.
    }

    public async Task<InvertedIndex> DeserializeAsync(string filePath)
    {
        // При десеріалізації System.Text.Json коректно обробляє як \uXXXX, так і прямі UTF-8 символи.
        // Тому тут додаткові налаштування зазвичай не потрібні.
        var options = new JsonSerializerOptions
        {
            // Encoder тут не потрібен для читання
        };

        await using var openStream = File.OpenRead(filePath);
        var deserialized = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, int>>>(openStream, options);

        var index = new InvertedIndex();
        if (deserialized != null)
        {
            foreach (var termEntry in deserialized)
            {
                foreach (var docEntry in termEntry.Value)
                {
                    // Припускаємо, що InvertedIndex.Add може обробляти додавання одного слова
                    // або у вас є метод для встановлення частоти
                    for (var i = 0; i < docEntry.Value; i++)
                    {
                        index.Add(termEntry.Key, docEntry.Key);
                    }
                }
            }
        }
        //Console.WriteLine($"[JsonIndexSerializer] Індекс завантажено: {filePath}");
        return index;
    }
}
