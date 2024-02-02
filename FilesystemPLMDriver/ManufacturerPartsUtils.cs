using System;
using System.Collections.Generic;
using CustomPLMService.Contract;

namespace CustomPLMDriver
{
    internal static class ManufacturerPartsUtils
    {
        public static List<RelationshipRow> GenerateSampleData()
        {
            var rows = new List<RelationshipRow>();
            var generator = new Random();
            var randomRows = generator.Next(1, 12);

            for (var i = 0; i < randomRows; i++)
            {
                rows.Add(CreateSampleRow(i));
            }

            return rows;
        }

        private static RelationshipRow CreateSampleRow(int rowCounter)
        {
            var value = new Value($"MFR {rowCounter}");

            var attribute = new AttributeValue
            {
                AttributeId = "ManufacturerName"
            };

            attribute.Value = value;

            var childId = new Id
            {
                PublicId = $"MPN-{rowCounter}"
            };

            var row = new RelationshipRow
            {
                ChildId = childId
            };

            row.Attributes.Add(attribute);

            return row;
        }
    }
}
