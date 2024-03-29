using System;
using System.Collections.Generic;
using System.Linq;
using Altium.PLM.Custom;
using AutoMapper;
using AutoMapper.Internal;
using CustomPLMService.Contract.Models.Authentication;
using Google.Protobuf.Collections;
using AttributeSpec = CustomPLMService.Contract.Models.Metadata.AttributeSpec;
using ItemTO = Altium.PLM.Custom.Item;
using ItemCreateSpecTO = Altium.PLM.Custom.ItemCreateSpec;
using ItemUpdateSpecTO = Altium.PLM.Custom.ItemUpdateSpec;
using QueryTO = Altium.PLM.Custom.Query;
using QueryAttributeTO = Altium.PLM.Custom.QueryAttribute;
using RelationshipTableTO = Altium.PLM.Custom.RelationshipTable;
using SupportedOperationTO = Altium.PLM.Custom.OperationSupportedRequest.Types.Operation;
using ValueTO = Altium.PLM.Custom.Value;
using AttributeSpecTO = Altium.PLM.Custom.AttributeSpec;
using AttributeValue = CustomPLMService.Contract.Models.Items.AttributeValue;
using AttributeValueTO = Altium.PLM.Custom.AttributeValue;
using Auth = CustomPLMService.Contract.Models.Authentication.Auth;
using BaseTypeTO = Altium.PLM.Custom.BaseType;
using AuthTO = Altium.PLM.Custom.Auth;
using BaseType = CustomPLMService.Contract.Models.Metadata.BaseType;
using Credentials = CustomPLMService.Contract.Models.Authentication.Credentials;
using CredentialsTO = Altium.PLM.Custom.Credentials;
using FileResource = CustomPLMService.Contract.Models.Items.FileResource;
using TypeTO = Altium.PLM.Custom.Type;
using TypeIdTO = Altium.PLM.Custom.TypeId;
using IdTO = Altium.PLM.Custom.Id;
using Type = CustomPLMService.Contract.Models.Metadata.Type;
using UomValueTO = Altium.PLM.Custom.UomValue;
using ListValueTO = Altium.PLM.Custom.ListValue;
using FileResourceTO = Altium.PLM.Custom.FileResource;
using Id = CustomPLMService.Contract.Models.Items.Id;
using Item = CustomPLMService.Contract.Models.Items.Item;
using ItemCreateSpec = CustomPLMService.Contract.Models.Items.ItemCreateSpec;
using ItemUpdateSpec = CustomPLMService.Contract.Models.Items.ItemUpdateSpec;
using ListValue = CustomPLMService.Contract.Models.Metadata.ListValue;
using NumberingFormat = CustomPLMService.Contract.Models.Items.NumberingFormat;
using RelationshipTypeTO = Altium.PLM.Custom.RelationshipType;
using RelationshipSpecTO = Altium.PLM.Custom.RelationshipSpec;
using NumberingFormatTO = Altium.PLM.Custom.NumberingFormat;
using Query = CustomPLMService.Contract.Models.Search.Query;
using RelationshipSpec = CustomPLMService.Contract.Models.Metadata.RelationshipSpec;
using RelationshipTable = CustomPLMService.Contract.Models.Relationship.RelationshipTable;
using RelationshipType = CustomPLMService.Contract.Models.Metadata.RelationshipType;
using RelationshipRowTO = CustomPLMService.Contract.Models.Relationship.RelationshipRow;
using TypeId = CustomPLMService.Contract.Models.Metadata.TypeId;
using UomValue = CustomPLMService.Contract.Models.Items.UomValue;
using Value = CustomPLMService.Contract.Models.Items.Value;


namespace CustomPLMService
{
    public class PlmServiceMappingProfile : Profile
    {
        public PlmServiceMappingProfile()
        {
            // Authentication
            CreateMap<AuthTO, Auth>().ReverseMap();
            CreateMap<CredentialsTO, Credentials>().ReverseMap();
            CreateMap<SupportedOperationTO, SupportedOperation>().ReverseMap();

            // Items
            CreateMap<AttributeValueTO, AttributeValue>().ConvertUsing((source, destination, resolutionContext) => MapAttributeValueTO(source, resolutionContext));
            CreateMap<AttributeValue, AttributeValueTO>().ConvertUsing((source, destination, resolutionContext) => MapAttributeValue(source, resolutionContext));
            CreateMap<FileResourceTO, FileResource>().ForMember(fileResource => fileResource.Data,
                opt => opt.MapFrom(src => src.Data.Memory));
            CreateMap<IdTO, Id>().ReverseMap();
            CreateMap<ItemTO, Item>().ReverseMap();
            CreateMap<ItemCreateSpecTO, ItemCreateSpec>().ReverseMap();
            CreateMap<ItemUpdateSpecTO, ItemUpdateSpec>().ReverseMap();
            CreateMap<NumberingFormatTO, NumberingFormat>().ReverseMap();
            CreateMap<UomValueTO, UomValue>().ReverseMap();
            CreateMap<ValueTO, Value>().ConvertUsing((source, destination, resolutionContext) => MapValueTo(source, resolutionContext));
            CreateMap<Value, ValueTO>().ConvertUsing((source, destination, resolutionContext) => MapValue(source, resolutionContext));

            // Metadata
            CreateMap<AttributeSpecTO, AttributeSpec>().ReverseMap();
            CreateMap<AttributeSpecTO.Types.Datatype, AttributeSpec.Datatype>().ReverseMap();
            CreateMap<AttributeSpecTO.Types.Valueset, AttributeSpec.Valueset>().ReverseMap();
            CreateMap<BaseTypeTO, BaseType>().ReverseMap();
            CreateMap<ListValueTO, ListValue>().ReverseMap();
            CreateMap<RelationshipSpecTO, RelationshipSpec>().ReverseMap();
            CreateMap<RelationshipTypeTO, RelationshipType>().ReverseMap();
            CreateMap<TypeTO, Type>().ReverseMap();
            CreateMap<TypeIdTO, TypeId>().ReverseMap();

            // Relationships
            CreateMap<RelationshipRowTO, RelationshipRow>().ReverseMap();
            CreateMap<RelationshipTableTO, RelationshipTable>().ReverseMap();

            // Search
            CreateMap<QueryTO, Query>().ReverseMap();
            CreateMap<QueryAttributeTO, QueryAttribute>().ReverseMap();


            this.Internal().ForAllPropertyMaps(pm => true,
                (propertyMap, opts) =>
                    opts.Condition((source, dest, sourceMember, destMember) => sourceMember is not null));
            this.Internal().ForAllPropertyMaps(IsToRepeatedField, (propertyMap, opts) => opts.UseDestinationValue());
        }

        private static bool IsToRepeatedField(PropertyMap pm)
        {
            if (!pm.DestinationType.IsConstructedGenericType)
                return false;

            var destGenericBase = pm.DestinationType.GetGenericTypeDefinition();
            return destGenericBase == typeof(RepeatedField<>);
        }

        AttributeValue MapAttributeValueTO(AttributeValueTO source, ResolutionContext context)
        {
            return new AttributeValue
            {
                AttributeId = source.AttributeId,
                Value = MapValueTo(source.Value[0], context)
            };
        }

        AttributeValueTO MapAttributeValue(AttributeValue source, ResolutionContext context)
        {
            var dest = new AttributeValueTO
            {
                AttributeId = source.AttributeId,
            };
            if (source.Value != null)
            {
                dest.Value.Add(MapValue(source.Value, context));
            }

            return dest;
        }

        private static ValueTO MapValue(Value source, ResolutionContext context)
        {
            if (source.IsNull())
            {
                return new ValueTO();
            }

            var destination = new ValueTO();
            switch (source.TypedValueCase)
            {
                case Value.Type.None:
                    break;
                case Value.Type.String:
                    destination.StringValue = source;
                    break;
                case Value.Type.Bool:
                    destination.BoolValue = source;
                    break;
                case Value.Type.Double:
                    destination.DoubleValue = source;
                    break;
                case Value.Type.Float:
                    destination.FloatValue = source;
                    break;
                case Value.Type.Int:
                    destination.IntValue = source;
                    break;
                case Value.Type.Date:
                    destination.DateValue = source;
                    break;
                case Value.Type.Reference:
                    destination.ReferenceValue = context.Mapper.Map<IdTO>((Id)source);
                    break;
                case Value.Type.Uom:
                    destination.UomValue = context.Mapper.Map<UomValueTO>((UomValue)source);
                    break;
                case Value.Type.ListValue:
                    destination.ListValue = context.Mapper.Map<ListValueTO>((ListValue)source);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return destination;
        }

        private static Value MapValueTo(ValueTO source, ResolutionContext context)
        {
            if (source == null)
            {
                return null;
            }

            var value = new Value();
            switch (source.TypedValueCase)
            {
                case ValueTO.TypedValueOneofCase.None:
                    break;
                case ValueTO.TypedValueOneofCase.StringValue:
                    value = new Value(source.StringValue);
                    break;
                case ValueTO.TypedValueOneofCase.BoolValue:
                    value = new Value(source.BoolValue);
                    break;
                case ValueTO.TypedValueOneofCase.DoubleValue:
                    value = new Value(source.DoubleValue);
                    break;
                case ValueTO.TypedValueOneofCase.FloatValue:
                    value = new Value(source.FloatValue);
                    break;
                case ValueTO.TypedValueOneofCase.IntValue:
                    value = new Value((int)source.IntValue);
                    break;
                case ValueTO.TypedValueOneofCase.DateValue:
                    value = new Value(source.DateValue);
                    break;
                case ValueTO.TypedValueOneofCase.ReferenceValue:
                    value = new Value(context.Mapper.Map<Id>(source.ReferenceValue));
                    break;
                case ValueTO.TypedValueOneofCase.UomValue:
                    value = new Value(context.Mapper.Map<UomValue>(source.UomValue));
                    break;
                case ValueTO.TypedValueOneofCase.ListValue:
                    value = new Value(context.Mapper.Map<ListValue>(source.ListValue));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return value;
        }
    }
}
