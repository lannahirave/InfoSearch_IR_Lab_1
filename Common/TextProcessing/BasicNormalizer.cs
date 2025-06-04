using System.Text;
using System.Text.RegularExpressions;
using Common.TextProcessing.Abstract;

namespace Common.TextProcessing;

public class BasicNormalizer : ITokenNormalizer
{
 public string Normalize(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return string.Empty;

        // Залишаємо тільки букви (включно з юнікодними, українськими)
        var normalized = new string(token
            .Where(char.IsLetter)
            .ToArray());

        return normalized.ToLowerInvariant();
    }
}
