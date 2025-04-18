namespace Borderly.Config
{
    public class Settings
    {
        public required string InputDirectory { get; set; }
        public required string OutputDirectory { get; set; }
        public string? ProcessedDirectory { get; set; }
        public ProcessedFileOption ProcessedFileOption { get; set; } = ProcessedFileOption.None;
    }
}
