namespace ConsoleApplication.Workflow;

public class ApplicationConfig
{
    public string TextsDirectoryPath { get; init; } = string.Empty;
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public long MinFileSizeKB { get; init; } = 150;
    public int MinFileCount { get; init; } = 10;
    public bool FakeSingleThreaded { get; init; } = false;
    // Add other relevant configurations here, e.g., specific serializer choices
}

// Custom exception for configuration issues
public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message) { }
}
