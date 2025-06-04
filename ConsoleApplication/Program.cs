using System.Diagnostics;
using Common.DS;
using Common.DS.Abstract;
using Common.Serialization;
using Common.TextProcessing;
using Common.TextProcessing.Abstract;
using Common.TextReaders;
using Common.TextReaders.Interfaces;

Console.OutputEncoding = System.Text.Encoding.UTF8;
// Navigate up to project root
var textsDirectoryPath = Path.Combine( "D:\\Texts");
var outputDirectoryPath = Path.Combine("Output");

const long MinFileSizeKB = 150;
const int MinFileCount = 10; // Вимога з завдання
const bool toFakeSingleThreaded = false;

// Створення папки для вихідних файлів, якщо її немає
Directory.CreateDirectory(outputDirectoryPath);

Console.WriteLine($"Очікувана папка з текстами: {textsDirectoryPath}");
Console.WriteLine($"Папка для збереження словників: {outputDirectoryPath}");
Console.WriteLine("--- Початок програми ---");

// 1. Ініціалізація залежностей
ITextReaderFactory readerFactory = new TextReaderFactory();

ITokenNormalizer tokenNormalizer = new BasicNormalizer(); // Використовуємо базовий нормалізатор
IInvertedIndexBuilder indexBuilder = new InvertedIndexBuilder(readerFactory, tokenNormalizer);

// 2. Отримання списку файлів
if (!Directory.Exists(textsDirectoryPath))
{
    Console.WriteLine($"ПОМИЛКА: Папка з текстами не знайдена за шляхом: {textsDirectoryPath}");
    return 1; // Код помилки
}

List<string> filePaths;
try
{
    filePaths = Directory.GetFiles(textsDirectoryPath, "*.*", SearchOption.TopDirectoryOnly)
        .Where(f => new FileInfo(f).Length >= MinFileSizeKB * 1024) // Фільтр за розміром
        .ToList();
}
catch (Exception ex)
{
    Console.WriteLine($"ПОМИЛКА при отриманні списку файлів: {ex.Message}");
    return 1;
}


if (filePaths.Count == 0)
{
    Console.WriteLine($"Не знайдено файлів розміром > {MinFileSizeKB}KB у папці {textsDirectoryPath}.");
    return 1;
}

if (filePaths.Count < MinFileCount)
{
    Console.WriteLine($"ПОПЕРЕДЖЕННЯ: Знайдено лише {filePaths.Count} файлів, що відповідають критерію розміру (потрібно щонайменше {MinFileCount}).");
}
else
{
    Console.WriteLine($"Знайдено {filePaths.Count} файлів для обробки:");
}

foreach (var filePath in filePaths)
{
    //Console.WriteLine($"  - {Path.GetFileName(filePath)} ({(new FileInfo(filePath).Length / 1024.0):F2} KB)");
}


// 3. Побудова інвертованого індексу
Console.WriteLine("\n--- Побудова індексу ---");
var stopwatch = Stopwatch.StartNew();
InvertedIndex? invertedIndex = null;
try
{
    invertedIndex = await indexBuilder.BuildAsync(filePaths, CancellationToken.None, toFakeSingleThreaded);
}
catch (Exception ex)
{
    Console.WriteLine($"ПОМИЛКА під час побудови індексу: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    // Тут можна перевірити, чи були помилки обробки окремих файлів, якщо InvertedIndexBuilder їх логує
    if (invertedIndex == null || invertedIndex.GetVocabularySize() == 0)
    {
         Console.WriteLine("Індекс не було побудовано або він порожній. Перевірте логіку рідерів та нормалізатора.");
         return 1;
    }
}
stopwatch.Stop();

Console.WriteLine($"Індекс успішно побудовано за: {stopwatch.ElapsedMilliseconds} мс");

// 4. Оцінка розмірів
Console.WriteLine("\n--- Статистика колекції та словника ---");

long totalCollectionSize = 0;
foreach (var filePath in filePaths) // Перераховуємо лише ті файли, що реально пішли в обробку
{
    try
    {
        totalCollectionSize += new FileInfo(filePath).Length;
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine($"Попередження: Файл {filePath} не знайдено під час розрахунку розміру колекції.");
    }
}

Console.WriteLine($"Загальний розмір обробленої колекції файлів: {totalCollectionSize / (1024.0 * 1024.0):F2} MB");
Console.WriteLine($"Загальна кількість слів (токенів) в колекції: {invertedIndex.GetTotalWordCount()}");
Console.WriteLine($"Розмір словника (кількість унікальних термінів): {invertedIndex.GetVocabularySize()}");

// 5. Збереження словника та порівняння форматів
Console.WriteLine("\n--- Збереження словника ---");

// JSON Serializer
var jsonSerializer = new JsonIndexSerializer();
var jsonFilePath = Path.Combine(outputDirectoryPath, "inverted_index.json");
Console.WriteLine($"Збереження у JSON форматі: {jsonFilePath}");
stopwatch.Restart();
try
{
    await jsonSerializer.SerializeAsync(invertedIndex, jsonFilePath);
    stopwatch.Stop();
    Console.WriteLine($"  Збережено за: {stopwatch.ElapsedMilliseconds} мс");
    Console.WriteLine($"  Розмір файлу: {new FileInfo(jsonFilePath).Length / 1024.0:F2} KB");
}
catch (Exception ex)
{
    stopwatch.Stop();
    Console.WriteLine($"  ПОМИЛКА при збереженні у JSON: {ex.Message}");
}


// Custom Text Serializer
var customTextSerializer = new CustomTextIndexSerializer();
var customTextFilePath = Path.Combine(outputDirectoryPath, "inverted_index_custom.txt");
Console.WriteLine($"Збереження у власному текстовому форматі: {customTextFilePath}");
stopwatch.Restart();
try
{
    await customTextSerializer.SerializeAsync(invertedIndex, customTextFilePath);
    stopwatch.Stop();
    Console.WriteLine($"  Збережено за: {stopwatch.ElapsedMilliseconds} мс");
    Console.WriteLine($"  Розмір файлу: {new FileInfo(customTextFilePath).Length / 1024.0:F2} KB");
}
catch (Exception ex)
{
    stopwatch.Stop();
    Console.WriteLine($"  ПОМИЛКА при збереженні у Custom Text: {ex.Message}");
}

// Додайте тут інші серіалізатори для порівняння, якщо потрібно

Console.WriteLine("\n--- Програма завершена ---");
return 0; // Успішне завершення
