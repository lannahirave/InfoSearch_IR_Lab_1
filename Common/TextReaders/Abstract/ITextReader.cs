namespace Common.TextReaders.Abstract;

public interface ITextReader
{
    /// <summary>
    /// Чи може цей рідер читати конкретний файл за шляхом.
    /// </summary>
    bool CanRead(string filePath);

    /// <summary>
    /// Потокове читання слів з файлу (по чанках або рядках).
    /// </summary>
    IEnumerable<string> ReadWords(Stream stream);
}
