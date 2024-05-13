using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace FilesystemPLMDriver
{
    [ExcludeFromCodeCoverage]
    public class MetadataConfig
    {
        public const string Key = "Metadata";

        public IReadOnlyCollection<string> ItemTypes { get; init; }
        public IReadOnlyCollection<string> ChangeTypes { get; init; }
        public IReadOnlyCollection<string> AttributeNames { get; init; }
    }
}
