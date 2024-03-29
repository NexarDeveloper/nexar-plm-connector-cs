using CustomPLMService.Contract;
using CustomPLMService.Contract.Models.Authentication;
using CustomPLMService.Contract.Models.Items;
using CustomPLMService.Contract.Models.Metadata;
using CustomPLMService.Contract.Models.Relationship;
using CustomPLMService.Contract.Models.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FilesystemPLMDriver.Models;
using Microsoft.Extensions.Logging;
using Type = CustomPLMService.Contract.Models.Metadata.Type;

namespace FilesystemPLMDriver
{
    public class FileSystemPlmService(ItemRepository repository, ILogger<FileSystemPlmService> logger) : ICustomPlmService
    {
        public Task<Item> CreateItem(ItemCreateSpec item)
        {
            var changes = item.Metadata.Id.BaseType.Equals(BaseType.Change);
            var dto = changes ? DtoConverter.ToObjectDto(item) : DtoConverter.ToItemDto(item);

            repository.Store(dto);
            return Task.FromResult(DtoConverter.ToPlmItem(dto));
        }

        public Task<Item> ReadItem(Id plmId)
        {
            var changes = plmId.TypeId.BaseType.Equals(BaseType.Change);
            var itemDto = repository.Load(plmId.PublicId, changes);
            return Task.FromResult(itemDto != null ? DtoConverter.ToPlmItem(itemDto) : null);
        }

        public Task<Item> UpdateItem(ItemUpdateSpec item)
        {
            var changes = item.Metadata.Id.BaseType.Equals(BaseType.Change);
            var dto = changes ? DtoConverter.ToObjectDto(item) : DtoConverter.ToItemDto(item);

            repository.Store(dto);
            return Task.FromResult(DtoConverter.ToPlmItem(dto));
        }

        public Task<IEnumerable<Id>> QueryItems(Query query, Type type)
        {
            var allItems = repository.LoadAllItems();
            var filtered = new List<ItemDto>();
            foreach (var item in allItems)
            {
                if (query.MaxRows > 0 && filtered.Count > query.MaxRows)
                    break;
                var matches = true;
                if (null != type)
                {
                    if (!type.Id.Id.Equals(item.Id.Type))
                    {
                        matches = false;
                        continue;
                    }
                }

                foreach (var att in query.Attrs)
                {
                    var foundValue = false;
                    foreach (var value in item.Attributes.Where(value => value.Name.Equals(att.Name)))
                    {
                        foundValue = true;
                        if (!value.Value.Equals(att.Value))
                        {
                            matches = false;
                            break;
                        }
                    }

                    if (!foundValue)
                    {
                        matches = false;
                        break;
                    }
                }

                if (query.ModifyDate > 0)
                {
                    if (query.ModifyDate > item.ModifyDate)
                    {
                        matches = false;
                    }
                }

                if (matches)
                {
                    filtered.Add(item);
                }
            }

            var dtos = filtered.Select(item => item.Id);
            var output = dtos.Select(id => DtoConverter.ToPlmId(id, false)).ToList();
            return Task.FromResult<IEnumerable<Id>>(output);
        }

        public async Task CreateRelationships(IEnumerable<RelationshipTable> tables)
        {
            var relationshipTables = tables.ToList();
            try
            {
                foreach (var table in relationshipTables)
                {
                    var parentId = table.Id;
                    var isChange = table.Type.Equals(RelationshipType.AffectedItems);
                    var itemDto = repository.Load(parentId.PublicId, isChange);

                    if (itemDto is not null)
                    {
                        var dto = DtoConverter.ToRelationshipTableDto(table);
                        itemDto.RelationshipTables.Add(dto);
                        repository.Store(itemDto);
                    }
                    else
                    {
                        throw new Exception($"Cannot create relationships for not existing entity: {parentId}");
                    }
                }
            }
            finally
            {
                await CleanupFiles(relationshipTables);
            }
        }

        public Task DeleteItem(Id id)
        {
            var dtoId = id.ToIdDto(null);
            var isChange = id.TypeId.BaseType.Equals(BaseType.Change);
            repository.DeleteItem(dtoId, isChange);

            return Task.CompletedTask;
        }

        public Task AdvanceState(Id id)
        {
            logger.LogWarning("Advance state is not implemented");
            return Task.CompletedTask;
        }

        public Task<RelationshipTable> ReadRelationship(Id id, RelationshipType type)
        {
            var table = new RelationshipTable
            {
                Id = id,
                Type = type
            };

            if (type == RelationshipType.ManufacturerParts)
            {
                table.Rows.AddRange(ManufacturerPartsUtils.GenerateSampleData());
            }

            return Task.FromResult(table);
        }

        public Task<bool> TestAccess(Auth auth)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsOperationSupported(SupportedOperation operationType)
        {
            var isSupported = operationType is SupportedOperation.CreateChangeOrder or SupportedOperation.ExtractPartChoicesFromAttributes;
            logger.LogInformation($"Operation '{operationType}' is {(isSupported ? "": "NOT ")}supported");
            return Task.FromResult(isSupported);
        }

        public async Task<string> UploadFile(FileResource request)
        {
            var id = Guid.NewGuid().ToString();

            var filePath = PrepareFileLocation(id, request.FileName);
            await using var output = new FileStream(filePath, FileMode.Create);
            await output.WriteAsync(request.Data);

            return id;
        }


        private static Task CleanupFiles(IEnumerable<RelationshipTable> tables)
        {
            foreach (var relationshipTable in tables)
            {
                foreach (var directory in from relationshipTableRow in relationshipTable.Rows where !string.IsNullOrWhiteSpace(relationshipTableRow.FileId) select GetDirectoryForId(relationshipTableRow.FileId))
                {
                    Directory.Delete(directory, true);
                }
            }

            return Task.CompletedTask;
        }

        private static string PrepareFileLocation(string id, string fileName)
        {
            var directory = GetDirectoryForId(id);
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, fileName);
        }

        private static string GetDirectoryForId(string id)
        {
            return Path.Combine("data", id);
        }
    }
}
