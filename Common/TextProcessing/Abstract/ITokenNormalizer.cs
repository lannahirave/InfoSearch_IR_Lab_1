namespace Common.TextProcessing.Abstract;

public interface ITokenNormalizer
{
    string Normalize(string token);
}
