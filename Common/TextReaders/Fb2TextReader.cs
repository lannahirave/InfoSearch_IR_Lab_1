using System.Text.RegularExpressions;
// Required for Task
using System.Xml;
using Common.TextReaders.Abstract;

namespace Common.TextReaders;


public class Fb2TextReader : ITextReader
{
    
    public bool CanRead(string filePath)
    {
        return filePath.EndsWith(".fb2", StringComparison.OrdinalIgnoreCase) ||
               filePath.EndsWith(".fb2.zip", StringComparison.OrdinalIgnoreCase); // FB2 can also be zipped
    }
    public IEnumerable<string> ReadWords(Stream stream)
    {
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });

        while (reader.Read())
        {
            // Шукаємо тільки текстові вузли всередині абзаців і секцій
            if (reader.NodeType == XmlNodeType.Text)
            {
                var words = Regex.Matches(reader.Value, @"[\p{L}\p{M}]+");
                foreach (Match match in words)
                {
                    yield return match.Value;
                }
            }
        }
    }

}
