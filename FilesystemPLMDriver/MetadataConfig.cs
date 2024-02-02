using System.Collections.Generic;

namespace CustomPLMDriver
{
    public class MetadataConfig
    {
        public IReadOnlyCollection<string> ItemTypes { get; init; }
        public IReadOnlyCollection<string> ChangeTypes { get; init; }
        public IReadOnlyCollection<string> AttributeNames { get; init; }
    }
}
