using System.Text.RegularExpressions;
using Common.TextReaders.Abstract;

namespace Common.TextReaders;

public class CsvTextReader : ITextReader
{
    private readonly int _textColumnIndex; // 0-based index of the column with text


    public CsvTextReader(int textColumnIndex = 0)
    {
        
        _textColumnIndex = textColumnIndex;
        if (textColumnIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(textColumnIndex), "Column index cannot be negative.");
        }
    }

    public bool CanRead(string filePath)
    {
        return filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
    }

    public IEnumerable<string> ReadWords(Stream stream)
    {
        using var reader = new StreamReader(stream);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var columns = line.Split(','); // Basic CSV split, not robust for all CSVs
            if (columns.Length > _textColumnIndex)
            {
                var textContent = columns[_textColumnIndex];
                // Using a similar regex as TxtTextReader for word extraction
                foreach (Match match in Regex.Matches(textContent, @"[\p{L}\p{M}]+"))
                {
                    yield return match.Value;
                }
            }
        }
    }
}
