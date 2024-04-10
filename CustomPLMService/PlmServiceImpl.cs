using System;
using System.Linq;
using System.Threading.Tasks;
using Altium.PLM.Custom;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models;
using CustomPLMService.Contract.Models.Authentication;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Altium.PLM.Custom.AuthResult;
using TypeIdRequestTO = Altium.PLM.Custom.TypeIdRequest;
using TypeRequestTO = Altium.PLM.Custom.TypeRequest;
using IdRequestTO = Altium.PLM.Custom.IdRequest;
using QueryItemsRequestTO = Altium.PLM.Custom.QueryItemsRequest;
using ItemCreateRequestTO = Altium.PLM.Custom.ItemCreateRequest;
using CreateRelationshipsRequestTO = Altium.PLM.Custom.CreateRelationshipsRequest;
using OperationSupportedRequestTO = Altium.PLM.Custom.OperationSupportedRequest;
using AdvanceStateRequestTO = Altium.PLM.Custom.AdvanceStateRequest;
using Auth = CustomPLMService.Contract.Models.Authentication.Auth;
using RelationshipRequestTO = Altium.PLM.Custom.RelationshipRequest;
using TypeTO = Altium.PLM.Custom.Type;
using VoidTO = Altium.PLM.Custom.Void;
using AuthTO = Altium.PLM.Custom.Auth;
using AuthResultTO = Altium.PLM.Custom.AuthResult;
using BaseType = CustomPLMService.Contract.Models.Metadata.BaseType;
using TypeIdTO = Altium.PLM.Custom.TypeId;
using IdTO = Altium.PLM.Custom.Id;
using ItemTO = Altium.PLM.Custom.Item;
using ItemResultTO = Altium.PLM.Custom.ItemResult;
using ErrorTO = Altium.PLM.Custom.Error;
using FileResource = CustomPLMService.Contract.Models.Items.FileResource;
using ItemUpdateRequestTO = Altium.PLM.Custom.ItemUpdateRequest;
using FileResourceTO = Altium.PLM.Custom.FileResource;
using FileResourceResponseTO = Altium.PLM.Custom.FileResourceResponse;
using Id = CustomPLMService.Contract.Models.Items.Id;
using ItemCreateSpec = CustomPLMService.Contract.Models.Items.ItemCreateSpec;
using ItemUpdateSpec = CustomPLMService.Contract.Models.Items.ItemUpdateSpec;
using OperationSupportedResponseTO = Altium.PLM.Custom.OperationSupportedResponse;
using Query = CustomPLMService.Contract.Models.Query.Query;
using RelationshipTable = CustomPLMService.Contract.Models.Relationship.RelationshipTable;
using RelationshipTableTO = Altium.PLM.Custom.RelationshipTable;
using RelationshipType = CustomPLMService.Contract.Models.Metadata.RelationshipType;
using Type = CustomPLMService.Contract.Models.Metadata.Type;
using TypeId = CustomPLMService.Contract.Models.Metadata.TypeId;

namespace CustomPLMService
{
    public class PlmServiceImpl(ICustomPlmMetadataService metadataService, ICustomPlmService service, IMapper mapper, ILogger<PlmServiceImpl> logger) : PLMService.PLMServiceBase
    {
        public async override Task<AuthResultTO> TestAccess(AuthTO request, ServerCallContext context)
        {
            var authResult = new AuthResultTO
            {
                Success = false
            };

            if (await service.TestAccess(mapper.Map<Auth>(request)))
            {
                authResult.Success = true;
                authResult.Status = Types.Status.Success;
            }
            else
            {
                authResult.Status = Types.Status.InvalidCredentials;
                logger.LogInformation("Invalid Credentials Provided");
            }
            
            return authResult;
        }

        public async override Task ReadTypeIdentifiers(TypeRequestTO request,
            IServerStreamWriter<TypeIdTO> responseStream, ServerCallContext context)
        {
            var typeIdentifiers = await metadataService.ReadTypeIdentifiers(mapper.Map<BaseType>(request.BaseType));
            foreach (var plmTypeId in typeIdentifiers)
            {
                await responseStream.WriteAsync(mapper.Map<TypeIdTO>(plmTypeId));
            }
        }


        public async override Task ReadTypes(TypeIdRequestTO request, IServerStreamWriter<TypeTO> responseStream,
            ServerCallContext context)
        {
            var types = await metadataService.ReadTypes(request.Data.Select(mapper.Map<TypeId>));
            foreach (var type in types)
            {
                await responseStream.WriteAsync(mapper.Map<TypeTO>(type));
            }
        }

        public async override Task ReadItems(IdRequestTO request, IServerStreamWriter<ItemTO> responseStream,
            ServerCallContext context)
        {
            foreach (var id in request.Data.Select(mapper.Map<Id>))
            {
                var item = await service.ReadItem(id);
                if (item is not null)
                {
                    var message = mapper.Map<ItemTO>(item);
                    await responseStream.WriteAsync(message);
                }
            }
        }

        public async override Task QueryItems(QueryItemsRequestTO request, IServerStreamWriter<IdTO> responseStream,
            ServerCallContext context)
        {
            var plmItems = await service.QueryItems(mapper.Map<Query>(request.Query),
                mapper.Map<Type>(request.Type));
            foreach (var plmItem in plmItems)
            {
                if (plmItem is not null)
                {
                    await responseStream.WriteAsync(mapper.Map<IdTO>(plmItem));
                }
            }
        }

        public async override Task CreateItems(ItemCreateRequestTO request,
            IServerStreamWriter<ItemResultTO> responseStream, ServerCallContext context)
        {
            foreach (var itemSpec in request.Data.Select(mapper.Map<ItemCreateSpec>))
            {
                var result = new ItemResultTO();
                try
                {
                    var createdItem = mapper.Map<ItemTO>(await service.CreateItem(itemSpec));
                    result.Item = createdItem;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception while trying to create items");
                    result.Error = new ErrorTO
                    {
                        Message = e.Message
                    };
                }

                await responseStream.WriteAsync(result);
            }
        }


        public async override Task UpdateItems(ItemUpdateRequestTO request,
            IServerStreamWriter<ItemResultTO> responseStream, ServerCallContext context)
        {
            foreach (var item in request.Data.Select(mapper.Map<ItemUpdateSpec>))
            {
                var result = new ItemResultTO();
                try
                {
                    var updatedItem = await service.UpdateItem(item);
                    result.Item = mapper.Map<ItemTO>(updatedItem);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception while trying to update items");
                    result.Error = new ErrorTO
                    {
                        Message = e.Message
                    };
                }

                await responseStream.WriteAsync(result);
            }
        }

        public async override Task<FileResourceResponseTO> UploadFile(FileResourceTO request, ServerCallContext context)
        {
            var id = await service.UploadFile(mapper.Map<FileResource>(request));

            return new FileResourceResponseTO
            {
                Id = id
            };
        }

        public async override Task<VoidTO> CreateRelationships(CreateRelationshipsRequestTO request,
            ServerCallContext context)
        {
            await service.CreateRelationships(request.Relationships.Select(mapper.Map<RelationshipTable>));
            return new VoidTO();
        }

        public async override Task<OperationSupportedResponseTO> IsOperationSupported(OperationSupportedRequestTO request,
            ServerCallContext context)
        {
            return new OperationSupportedResponseTO
            {
                IsSupported = await service.IsOperationSupported(mapper.Map<SupportedOperation>(request.Operation))
            };
        }

        public async override Task<VoidTO> DeleteItems(IdRequestTO request, ServerCallContext context)
        {
            foreach (var id in request.Data)
            {
                await service.DeleteItem(mapper.Map<Id>(id));
            }

            return new VoidTO();
        }

        public async override Task<VoidTO> AdvanceState(AdvanceStateRequestTO request, ServerCallContext context)
        {
            await service.AdvanceState(mapper.Map<Id>(request.Id));
            return new VoidTO();
        }

        public async override Task ReadRelationships(RelationshipRequestTO request,
            IServerStreamWriter<RelationshipTableTO> responseStream, ServerCallContext context)
        {
            foreach (var id in request.Ids)
            {
                var relationship = await service.ReadRelationship(mapper.Map<Id>(id),
                    mapper.Map<RelationshipType>(request.Type));
                await responseStream.WriteAsync(mapper.Map<RelationshipTableTO>(relationship));
            }
        }
    }
}
