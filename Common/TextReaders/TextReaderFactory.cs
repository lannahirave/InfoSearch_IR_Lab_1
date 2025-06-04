using Common.TextProcessing;
using Common.TextReaders.Interfaces;

namespace Common.TextReaders;

public class TextReaderFactory : ITextReaderFactory
{
    private readonly List<ITextReader> _readers;

    public TextReaderFactory()
    { 
        _readers = [
            new TxtTextReader(),
            new CsvTextReader(textColumnIndex: 1),
            new Fb2TextReader(), // Я КОЛИ ЦЕ ПОЧУВ В ЗАПИСІ ГУГЛИВ ЩО ЗА МБ2 формат текстів!!!!!!!!!!!!!!!!!!
        ];
    }

    public ITextReader? GetReader(string filePath)
    {
        return _readers.FirstOrDefault(reader => reader.CanRead(filePath));
    }
}
