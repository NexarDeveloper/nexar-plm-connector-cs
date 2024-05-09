using CustomPLMService.Contract;
using CustomPLMService.Contract.Models.Authentication;
using CustomPLMService.Contract.Models.Items;
using CustomPLMService.Contract.Models.Metadata;
using CustomPLMService.Contract.Models.Relationship;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomPLMService.Contract.Models;
using CustomPLMService.Contract.Models.Query;
using FilesystemPLMDriver.Models;
using Microsoft.Extensions.Logging;
using Type = CustomPLMService.Contract.Models.Metadata.Type;

namespace FilesystemPLMDriver
{
    public class FileSystemPlmService(ItemRepository repository, ILogger<FileSystemPlmService> logger)
        : ICustomPlmService
    {
        public Task<IEnumerable<Item>> CreateItems(IEnumerable<ItemCreateSpec> items, CancellationToken cancellationToken)
        {
            var result = new List<Item>();
            foreach (var item in items)
            {
                var changes = item.Metadata.Id.BaseType.Equals(BaseType.Change);
                var dto = changes ? DtoConverter.ToObjectDto(item) : DtoConverter.ToItemDto(item);

                repository.Store(dto);
                result.Add(DtoConverter.ToPlmItem(dto));
            }

            return Task.FromResult((IEnumerable<Item>)result);
        }

        public Task<IEnumerable<Item>> ReadItems(IEnumerable<Id> plmIds, CancellationToken cancellationToken)
        {
            var result = new List<Item>();
            foreach (var plmId in plmIds)
            {
                var changes = plmId.TypeId.BaseType.Equals(BaseType.Change);
                var itemDto = repository.Load(plmId.PublicId, changes);
                if (itemDto is not null)
                {
                    result.Add(DtoConverter.ToPlmItem(itemDto));
                }
            }

            return Task.FromResult((IEnumerable<Item>)result);
        }

        public Task<IEnumerable<Item>> UpdateItems(IEnumerable<ItemUpdateSpec> updateSpecs, CancellationToken cancellationToken)
        {
            var result = new List<Item>();
            foreach (var updateSpecItem in updateSpecs)
            {
                var changes = updateSpecItem.Metadata.Id.BaseType.Equals(BaseType.Change);
                var dto = changes ? DtoConverter.ToObjectDto(updateSpecItem) : DtoConverter.ToItemDto(updateSpecItem);

                repository.Store(dto);
                result.Add(DtoConverter.ToPlmItem(dto));
            }

            return Task.FromResult((IEnumerable<Item>)result);
        }

        public Task DeleteItems(IEnumerable<Id> ids, CancellationToken cancellationToken)
        {
            foreach (var id in ids)
            {
                var dtoId = id.ToIdDto(null);
                var isChange = id.TypeId.BaseType.Equals(BaseType.Change);
                repository.DeleteItem(dtoId, isChange);
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<Id>> QueryItems(Query query, Type type, CancellationToken cancellationToken)
        {
            var allItems = repository.LoadAllItems();
            var filtered = new List<ItemDto>();
            foreach (var item in allItems)
            {
                if (query.MaxRows > 0 && filtered.Count > query.MaxRows)
                    break;
                var matches = true;
                if (type is not null)
                {
                    if (!type.Id.Id.Equals(item.Id.Type))
                    {
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

        public async Task CreateRelationships(IEnumerable<RelationshipTable> tables, CancellationToken cancellationToken)
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

        public Task<IEnumerable<RelationshipTable>> ReadRelationships(IEnumerable<Id> ids, RelationshipType type, CancellationToken cancellationToken)
        {
            var relationships = new List<RelationshipTable>();
            foreach (var id in ids)
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

                relationships.Add(table);
            }

            return Task.FromResult((IEnumerable<RelationshipTable>)relationships);
        }

        public Task AdvanceState(Id id, CancellationToken cancellationToken)
        {
            logger.LogWarning("Advance state is not implemented");
            return Task.CompletedTask;
        }

        public Task<bool> TestAccess(Auth auth, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsOperationSupported(SupportedOperation operationType, CancellationToken cancellationToken)
        {
            var isSupported = operationType is SupportedOperation.CreateChangeOrder
                or SupportedOperation.ExtractPartChoicesFromAttributes;
            logger.LogInformation($"Operation '{operationType}' is {(isSupported ? "" : "NOT ")}supported");
            return Task.FromResult(isSupported);
        }

        public async Task<string> UploadFile(FileResource request, CancellationToken cancellationToken)
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
                foreach (var directory in from relationshipTableRow in relationshipTable.Rows
                         where !string.IsNullOrWhiteSpace(relationshipTableRow.FileId)
                         select GetDirectoryForId(relationshipTableRow.FileId))
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