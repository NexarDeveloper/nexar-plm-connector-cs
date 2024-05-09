using System.Collections.Generic;
namespace FilesystemPLMDriver
{
    public class MetadataConfig
    {
        public const string Key = "Metadata";

        public IReadOnlyCollection<string> ItemTypes { get; init; }
        public IReadOnlyCollection<string> ChangeTypes { get; init; }
        public IReadOnlyCollection<string> AttributeNames { get; init; }
    }
}
