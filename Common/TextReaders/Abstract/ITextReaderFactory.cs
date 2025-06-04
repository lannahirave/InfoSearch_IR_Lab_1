namespace Common.TextReaders.Interfaces;

public interface ITextReaderFactory
{
    ITextReader? GetReader(string filePath);
}
