namespace Common.TextReaders.Abstract;

public interface ITextReaderFactory
{
    ITextReader? GetReader(string filePath);
}
