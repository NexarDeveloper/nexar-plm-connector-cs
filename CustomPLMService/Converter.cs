using System;
using AutoMapper;
using CustomPLMService.Contract;
using Google.Protobuf.Collections;
using ItemTO = Altium.PLM.Custom.Item;
using ItemCreateSpecTO = Altium.PLM.Custom.ItemCreateSpec;
using ItemUpdateSpecTO = Altium.PLM.Custom.ItemUpdateSpec;
using ItemResultTO = Altium.PLM.Custom.ItemResult;
using QueryTO = Altium.PLM.Custom.Query;
using RelationshipTableTO = Altium.PLM.Custom.RelationshipTable;
using SupportedOperationTO = Altium.PLM.Custom.OperationSupportedRequest.Types.Operation;
using ValueTO = Altium.PLM.Custom.Value;
using AttributeValueTO = Altium.PLM.Custom.AttributeValue;
using TypeTO = Altium.PLM.Custom.Type;
using IdTO = Altium.PLM.Custom.Id;
using Type = CustomPLMService.Contract.Type;
using UomValueTO = Altium.PLM.Custom.UomValue;
using ListValueTO = Altium.PLM.Custom.ListValue;

namespace CustomPLMService
{
    public static class Converter
    {
        public static TDestination Map<TDestination>(object source)
        {
            return Mapper.Map<TDestination>(source);
        }

        public static void Init()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<TypeTO, Type>().ReverseMap();
                cfg.CreateMap<ItemCreateSpecTO, ItemCreateSpec>().ReverseMap();
                cfg.CreateMap<ItemUpdateSpecTO, ItemUpdateSpec>().ReverseMap();
                cfg.CreateMap<ItemTO, Item>().ReverseMap();
                cfg.CreateMap<QueryTO, Query>().ReverseMap();
                cfg.CreateMap<RelationshipTableTO, RelationshipTable>().ReverseMap();
                cfg.CreateMap<SupportedOperationTO, SupportedOperation>().ReverseMap();
                cfg.CreateMap<AttributeValueTO, AttributeValue>().ConvertUsing((source, destination) => MapAttributeValueTO(source));
                cfg.CreateMap<AttributeValue, AttributeValueTO>().ConvertUsing((source, destination) => MapAttributeValue(source));
                cfg.CreateMap<ValueTO, Value>().ConvertUsing((source, destination) => MapValueTo(source));
                cfg.CreateMap<Value, ValueTO>().ConvertUsing((source, destination) => MapValue(source));

                AttributeValue MapAttributeValueTO(AttributeValueTO source)
                {
                    return new AttributeValue
                    {
                        AttributeId = source.AttributeId,
                        Value = MapValueTo(source.Value[0])
                    };
                }

                AttributeValueTO MapAttributeValue(AttributeValue source)
                {
                    var dest =  new AttributeValueTO
                    {
                        AttributeId = source.AttributeId,
                    };
                    if (source.Value != null)
                    {
                        dest.Value.Add(MapValue(source.Value));
                    }
                    return dest;
                }

                ValueTO MapValue(Value source)
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
                            destination.ReferenceValue = Mapper.Map<IdTO>((Id) source);
                            break;
                        case Value.Type.Uom:
                            destination.UomValue = Mapper.Map<UomValueTO>((UomValue) source);
                            break;
                        case Value.Type.ListValue:
                            destination.ListValue = Mapper.Map<ListValueTO>((ListValue) source);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return destination;
                }

                Value MapValueTo(ValueTO source)
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
                            value = new Value((int) source.IntValue); // TODO add real int value to the cRPC
                            break;
                        case ValueTO.TypedValueOneofCase.DateValue:
                            value = new Value(source.DateValue);
                            break;
                        case ValueTO.TypedValueOneofCase.ReferenceValue:
                            value = new Value(Mapper.Map<Id>(source.ReferenceValue));
                            break;
                        case ValueTO.TypedValueOneofCase.UomValue:
                            value = new Value(Mapper.Map<UomValue>(source.UomValue));
                            break;
                        case ValueTO.TypedValueOneofCase.ListValue:
                            value = new Value(Mapper.Map<ListValue>(source.ListValue));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return value;
                }


                bool IsToRepeatedField(PropertyMap pm)
                {
                    if (pm.DestinationPropertyType.IsConstructedGenericType)
                    {
                        var destGenericBase = pm.DestinationPropertyType.GetGenericTypeDefinition();
                        return destGenericBase == typeof(RepeatedField<>);
                    }


                    return false;
                }

                cfg.ForAllPropertyMaps(pm => true,
                    (propertyMap, opts) =>
                        opts.Condition((source, dest, sourceMember, destMember) => sourceMember != null));
                cfg.ForAllPropertyMaps(IsToRepeatedField, (propertyMap, opts) => opts.UseDestinationValue());
            });
        }

        public static void IsConfigurationValid()
        {
            Mapper.Configuration.AssertConfigurationIsValid();
        }
    }
}
