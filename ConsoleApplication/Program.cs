using System.Text;
using ConsoleApplication.Workflow;

Console.OutputEncoding = Encoding.UTF8;

// 1. Setup Configuration
var config = new ApplicationConfig // Or load from args/file
{
    TextsDirectoryPath = "D:\\Texts2", // Ensure this path is correct
    OutputDirectoryPath = "Output",
    MinFileSizeKB = 150,
    MinFileCount = 10,
    FakeSingleThreaded = false
};

// Create the main orchestrator
var orchestrator = new ApplicationOrchestrator(config);

try
{
    await orchestrator.RunAsync();
    return 0; // Success
}
catch (ConfigurationException ex) // Custom exception for config errors
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"CONFIGURATION ERROR: {ex.Message}");
    Console.ResetColor();
    // Log ex.StackTrace if needed for debugging
    return 2; // Configuration error code
}
catch (OperationCanceledException)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Error.WriteLine("Operation was cancelled.");
    Console.ResetColor();
    return 3; // Cancellation error code
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"UNHANDLED ERROR: {ex.Message}");
    Console.Error.WriteLine($"Stack Trace: {ex.StackTrace}");
    Console.ResetColor();
    return 1; // General error code
}
