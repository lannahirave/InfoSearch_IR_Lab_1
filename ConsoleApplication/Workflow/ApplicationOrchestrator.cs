using System.Diagnostics;
using Common.DS;
using Common.DS.Abstract;
using Common.Serialization;
using Common.TextProcessing;
using Common.TextProcessing.Abstract;
using Common.TextReaders;
using Common.TextReaders.Abstract;
using Common.Search; // For QueryParser, BooleanSearchService
using Common.Search.AST; // For QueryNode (though not directly used here, good to be aware)

namespace ConsoleApplication.Workflow;

public class ApplicationOrchestrator
{
    private readonly ApplicationConfig _config;
    private readonly ITextReaderFactory _readerFactory;
    private readonly ITokenNormalizer _tokenNormalizer;
    private readonly IIndexStructuresBuilder _indexBuilder;
    private readonly StatisticsReporter _statsReporter;
    private readonly IndexSerializationManager _serializationManager;
    private readonly QueryParser _queryParser; // Added
    private readonly BooleanSearchService _searchService; // Added

    public ApplicationOrchestrator(ApplicationConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));

        _readerFactory = new TextReaderFactory();
        _tokenNormalizer = new BasicNormalizer(); // This normalizer instance will be used for parsing queries too
        _indexBuilder = new IndexStructuresBuilder(_readerFactory, _tokenNormalizer);
        _statsReporter = new StatisticsReporter();
        
        var jsonInvertedIndexSerializer = new JsonIndexSerializer();
        var customTextInvertedIndexSerializer = new CustomTextIndexSerializer();
        var termDocumentMatrixJsonSerializer = new TermDocumentMatrixJsonSerializer();
        var termDocumentMatrixCustomTextSerializer = new TermDocumentMatrixCustomTextSerializer(); 
        
        _serializationManager = new IndexSerializationManager(
            jsonInvertedIndexSerializer, 
            customTextInvertedIndexSerializer,
            termDocumentMatrixJsonSerializer,
            termDocumentMatrixCustomTextSerializer,
            _config.OutputDirectoryPath);

        _queryParser = new QueryParser(_tokenNormalizer); // Initialize QueryParser with the same normalizer
        _searchService = new BooleanSearchService();    // Initialize BooleanSearchService
    }

    public async Task RunAsync()
    {
        Console.WriteLine($"Очікувана папка з текстами: {_config.TextsDirectoryPath}");
        Console.WriteLine($"Папка для збереження словників: {_config.OutputDirectoryPath}");
        Console.WriteLine("--- Початок програми ---");

        ValidateConfiguration();
        Directory.CreateDirectory(_config.OutputDirectoryPath);

        var filePaths = GetFilePaths();
        if (!filePaths.Any())
        {
            Console.WriteLine("Не знайдено файлів для обробки. Завершення програми.");
            return;
        }

        (InvertedIndex invertedIndex, TermDocumentMatrix termDocumentMatrix) = await BuildStructuresAsync(filePaths);

        long totalCollectionSize = CalculateTotalCollectionSize(filePaths);
        _statsReporter.Report(invertedIndex, termDocumentMatrix, filePaths, totalCollectionSize);

        await _serializationManager.SerializeInvertedIndexAsync(invertedIndex);
        await _serializationManager.SerializeTermDocumentMatrixAsync(termDocumentMatrix);

        // Run the search loop
        // Pass the IIndexAccessor implementations (InvertedIndex and TermDocumentMatrix)
        await RunSearchLoopAsync(invertedIndex, termDocumentMatrix); 

        Console.WriteLine("\n--- Програма завершена ---"); // Updated comment
    }
    
    private async Task RunSearchLoopAsync(IIndexAccessor invertedIndexAccessor, IIndexAccessor termDocumentMatrixAccessor)
    {
        Console.WriteLine("\n--- Пошук за індексом ---");
        Console.WriteLine("Введіть ваш булевий запит (наприклад, 'слово1 AND (слово2 OR NOT слово3)').");
        Console.WriteLine("Оператори: AND, OR, NOT. Дужки () підтримуються.");
        Console.WriteLine("Введіть 'exit' або порожній рядок для виходу з режиму пошуку.");

        while (true)
        {
            Console.Write("\nЗапит> ");
            string? rawQuery = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(rawQuery) || rawQuery.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            try
            {
                QueryNode astRoot = _queryParser.Parse(rawQuery);
                //Console.WriteLine($"  AST: {astRoot}"); // Optional: print AST for debugging

                Console.WriteLine("\n  Результати з Інвертованого Індексу:");
                PerformAndDisplaySearch(astRoot, invertedIndexAccessor);

                Console.WriteLine("\n  Результати з Матриці Інцидентності:");
                PerformAndDisplaySearch(astRoot, termDocumentMatrixAccessor);
            }
            catch (QueryParseException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"  Помилка парсингу запиту: {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex) // Catch other potential errors during search execution
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"  Помилка виконання запиту: {ex.Message}");
                // Console.Error.WriteLine(ex.StackTrace); // For debugging
                Console.ResetColor();
            }
        }
        Console.WriteLine("Вихід з режиму пошуку.");
    }

    private void PerformAndDisplaySearch(QueryNode astRoot, IIndexAccessor indexAccessor)
    {
        var stopwatch = Stopwatch.StartNew();
        IReadOnlySet<string> results = _searchService.ExecuteQuery(astRoot, indexAccessor);
        stopwatch.Stop();

        if (results.Any())
        {
            Console.WriteLine($"    Знайдено {results.Count} документів ({stopwatch.ElapsedMilliseconds} мс):");
            // Sort results for consistent display
            foreach (var docId in results.OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"      - {docId}");
            }
        }
        else
        {
            Console.WriteLine($"    Документів не знайдено ({stopwatch.ElapsedMilliseconds} мс).");
        }
    }

    // ... (ValidateConfiguration, GetFilePaths, BuildStructuresAsync, CalculateTotalCollectionSize remain the same) ...
    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_config.TextsDirectoryPath))
            throw new ConfigurationException("Texts directory path is not configured.");
        if (string.IsNullOrWhiteSpace(_config.OutputDirectoryPath))
            throw new ConfigurationException("Output directory path is not configured.");
    }

    private List<string> GetFilePaths()
    {
        Console.WriteLine("\n--- Пошук файлів для обробки ---");
        if (!Directory.Exists(_config.TextsDirectoryPath))
        {
            Console.WriteLine($"ПОМИЛКА: Папка з текстами не знайдена за шляхом: {_config.TextsDirectoryPath}");
            return new List<string>();
        }

        List<string> paths;
        try
        {
            paths = Directory.GetFiles(_config.TextsDirectoryPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f =>
                {
                    try { return new FileInfo(f).Length >= _config.MinFileSizeKB * 1024; }
                    catch (FileNotFoundException) { return false; }
                })
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ПОМИЛКА при отриманні списку файлів: {ex.Message}");
            return new List<string>();
        }

        if (paths.Count == 0)
        {
            Console.WriteLine($"Не знайдено файлів розміром > {_config.MinFileSizeKB}KB у папці {_config.TextsDirectoryPath}.");
        }
        else if (paths.Count < _config.MinFileCount)
        {
            Console.WriteLine($"ПОПЕРЕДЖЕННЯ: Знайдено лише {paths.Count} файлів, що відповідають критерію розміру (потрібно щонайменше {_config.MinFileCount}).");
        }
        else
        {
            Console.WriteLine($"Знайдено {paths.Count} файлів для обробки.");
        }
        return paths;
    }

    private async Task<(InvertedIndex InvertedIndex, TermDocumentMatrix TermDocumentMatrix)> BuildStructuresAsync(IEnumerable<string> filePaths)
    {
        Console.WriteLine("\n--- Побудова індексу та матриці інцидентності ---");
        var stopwatch = Stopwatch.StartNew();
        InvertedIndex? invertedIndex = null;
        TermDocumentMatrix? termDocumentMatrix = null;

        try
        {
            (invertedIndex, termDocumentMatrix) = await _indexBuilder.BuildAsync(filePaths, CancellationToken.None, _config.FakeSingleThreaded);
        }
        catch (Exception ex) 
        {
            stopwatch.Stop();
            Console.WriteLine($"ПОМИЛКА під час побудови структур: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}"); 
            throw; 
        }
        
        stopwatch.Stop();

        if (invertedIndex == null || termDocumentMatrix == null)
        {
            throw new InvalidOperationException("Не вдалося побудувати індексні структури.");
        }
        
        Console.WriteLine($"Структури успішно побудовано за: {stopwatch.ElapsedMilliseconds} мс");
        return (invertedIndex, termDocumentMatrix);
    }
    
    private long CalculateTotalCollectionSize(IEnumerable<string> filePaths)
    {
        long totalSize = 0;
        foreach (var filePath in filePaths)
        {
            try
            {
                totalSize += new FileInfo(filePath).Length;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Попередження: Файл {Path.GetFileName(filePath)} не знайдено під час розрахунку розміру колекції.");
            }
        }
        return totalSize;
    }
}
