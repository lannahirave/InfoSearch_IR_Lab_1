using Common.DS;

namespace ConsoleApplication.Workflow;

public class StatisticsReporter
{
    public void Report(
        InvertedIndex invertedIndex,
        TermDocumentMatrix termDocumentMatrix,
        IEnumerable<string> processedFilePaths,
        long totalCollectionSize)
    {
        Console.WriteLine("\n--- Статистика колекції та структур ---");

        int fileCount = processedFilePaths.Count();
        Console.WriteLine(
            $"Загальний розмір обробленої колекції файлів: {totalCollectionSize / (1024.0 * 1024.0):F2} MB ({fileCount} файлів)");

        Console.WriteLine("\n--- Інвертований індекс ---");
        if (invertedIndex != null)
        {
            Console.WriteLine(
                $"  Загальна кількість слів (токенів) в колекції (з індексу): {invertedIndex.GetTotalWordCount()}");
            Console.WriteLine($"  Розмір словника (унікальних термінів): {invertedIndex.GetVocabularySize()}");
            Console.WriteLine($"  Кількість унікальних документів: {invertedIndex.GetDocumentCount()}");
            // Add more specific stats for InvertedIndex if needed (e.g., total postings)
            long totalPostings = invertedIndex.GetIndex().Sum(kvp => (long)kvp.Value.Count);
            Console.WriteLine($"  Загальна кількість постінгів (термін -> документ посилань): {totalPostings}");
        }
        else
        {
            Console.WriteLine("  Інвертований індекс не було створено.");
        }

        Console.WriteLine("\n--- Матриця інцидентності \"термін-документ\" ---");
        if (termDocumentMatrix != null)
        {
            Console.WriteLine($"  Розмір словника (унікальних термінів): {termDocumentMatrix.GetVocabularySize()}");
            Console.WriteLine($"  Кількість унікальних документів: {termDocumentMatrix.GetDocumentCount()}");
            Console.WriteLine(
                $"  Кількість ненульових записів (термін зустрічається в документі): {termDocumentMatrix.GetNonZeroEntriesCount()}");
        }
        else
        {
            Console.WriteLine("  Матрицю інцидентності не було створено.");
        }
    }
}
