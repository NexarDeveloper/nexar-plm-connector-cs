using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CustomPLMService.Contract;
using Type = CustomPLMService.Contract.Type;

namespace CustomPLMDriver
{
    public class FileSystemPlmService : ICustomPlmService
    {
        private readonly ItemRepository repository;

        public FileSystemPlmService(ItemRepository repository)
        {
            this.repository = repository;
        }

        public Item CreateItem(Context context, ItemCreateSpec item)
        {
            var changes = item.Metadata.Id.BaseType.Equals(BaseType.Change);
            var dto = changes ? DtoConverter.ToObjectDto(item) : DtoConverter.ToItemDto(item);

            repository.Store(dto);
            return DtoConverter.ToPlmItem(dto);
        }

        public Item ReadItem(Context context, Id plmId)
        {
            var changes = plmId.TypeId.BaseType.Equals(BaseType.Change);
            var itemDto = repository.Load(plmId.PublicId, changes);
            if (itemDto != null)
            {
                return DtoConverter.ToPlmItem(itemDto);
            }

            return null;
        }


        public Item UpdateItem(Context context, ItemUpdateSpec item)
        {
            var changes = item.Metadata.Id.BaseType.Equals(BaseType.Change);
            var dto = changes ? DtoConverter.ToObjectDto(item) : DtoConverter.ToItemDto(item);

            repository.Store(dto);
            return DtoConverter.ToPlmItem(dto);
        }

        public IEnumerable<Id> QueryItems(Context context, Query query, Type type)
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
                    foreach (var value in item.Attributes)
                    {
                        if (value.Name.Equals(att.Name))
                        {
                            foundValue = true;
                            if (!value.Value.Equals(att.Value))
                            {
                                matches = false;
                                break;
                            }
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

            var dtos = filtered.Select(item => item.Id).ToList();
            var output = dtos.Select(id => DtoConverter.ToPlmId(id, false)).ToList();
            return output;
        }

        public void CreateRelationships(Context context, IEnumerable<RelationshipTable> tables)
        {
            foreach (var table in tables)
            {
                var parentId = table.Id;
                var isChange = table.Type.Equals(RelationshipType.AffectedItems);
                var itemDto = repository.Load(parentId.PublicId, isChange);

                if (itemDto != null)
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

        public void DeleteItem(Context context, Id id)
        {
            var dtoId = id.ToIdDto(null);
            var isChange = id.TypeId.BaseType.Equals(BaseType.Change);
            repository.DeleteItem(dtoId, isChange);
        }

        public void AdvanceState(Context context, Id id)
        {
            Console.WriteLine("Advance state is not implemented");
        }

        public RelationshipTable ReadRelationship(Context context, Id id, RelationshipType type)
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

            return table;
        }

        public bool TestAccess(Auth auth)
        {
            return true;
        }

        public bool IsOperationSupported(SupportedOperation operationType)
        {
            return operationType.Equals(SupportedOperation.CreateChangeOrder) || operationType.Equals(SupportedOperation.ExtractPartChoicesFromAttributes);
        }
    }
}
