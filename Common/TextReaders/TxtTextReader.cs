using System.Text.RegularExpressions;
using Common.TextReaders.Abstract;
// using Common.TextProcessing; // Не потрібен, якщо нормалізатор тут не використовується
// using Common.TextProcessing.Abstract; // Не потрібен

namespace Common.TextReaders;

public class TxtTextReader : ITextReader
{
    // Конструктор за замовчуванням, без залежності від ITokenNormalizer
    public TxtTextReader()
    {
    }

    public bool CanRead(string filePath)
    {
        // Можна було б зробити константою або статичним полем
        string[] readsThisTypes = [".txt"];
        return readsThisTypes.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<string> ReadWords(Stream stream)
    {
        using var reader = new StreamReader(stream);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            // Регулярний вираз для вилучення слів (літери та діакритичні знаки)
            foreach (Match match in Regex.Matches(line, @"[\p{L}\p{M}]+"))
            {
                // Повертаємо сире слово, як воно є у тексті
                yield return match.Value;
            }
        }
    }
}
