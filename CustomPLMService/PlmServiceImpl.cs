using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models;
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
    public class PlmServiceImpl(
        ICustomPlmMetadataService metadataService,
        ICustomPlmService service,
        IMapper mapper,
        ILogger<PlmServiceImpl> logger) : PLMService.PLMServiceBase
    {
        public async override Task<AuthResultTO> TestAccess(AuthTO request, ServerCallContext context)
        {
            var authResult = new AuthResultTO();

            if (await service.TestAccess(mapper.Map<Auth>(request), CancellationToken.None))
            {
                authResult.Success = true;
                authResult.Status = Types.Status.Success;
            }
            else
            {
                authResult.Success = false;
                authResult.Status = Types.Status.InvalidCredentials;
                logger.LogInformation("Invalid Credentials Provided");
            }

            return authResult;
        }

        public async override Task ReadTypeIdentifiers(TypeRequestTO request,
            IServerStreamWriter<TypeIdTO> responseStream, ServerCallContext context)
        {
            var typeIdentifiers = await metadataService.ReadTypeIdentifiers(mapper.Map<BaseType>(request.BaseType));
            foreach (var typeId in typeIdentifiers.Select(mapper.Map<TypeIdTO>))
            {
                await responseStream.WriteAsync(typeId);
            }
        }

        public async override Task ReadTypes(TypeIdRequestTO request, IServerStreamWriter<TypeTO> responseStream,
            ServerCallContext context)
        {
            var types = await metadataService.ReadTypes(request.Data.Select(mapper.Map<TypeId>));
            foreach (var type in types.Select(mapper.Map<TypeTO>))
            {
                await responseStream.WriteAsync(type);
            }
        }

        public async override Task ReadItems(IdRequestTO request, IServerStreamWriter<ItemTO> responseStream,
            ServerCallContext context)
        {
            var items = await service.ReadItems(request.Data.Select(mapper.Map<Id>), CancellationToken.None);
            foreach (var item in items.Select(mapper.Map<ItemTO>))
            {
                await responseStream.WriteAsync(item);
            }
        }

        public async override Task QueryItems(QueryItemsRequestTO request, IServerStreamWriter<IdTO> responseStream,
            ServerCallContext context)
        {
            var plmItems = await service.QueryItems(mapper.Map<Query>(request.Query),
                mapper.Map<Type>(request.Type), CancellationToken.None);
            foreach (var plmItem in plmItems.Select(mapper.Map<IdTO>))
            {
                await responseStream.WriteAsync(plmItem);
            }
        }

        public async override Task CreateItems(ItemCreateRequestTO request,
            IServerStreamWriter<ItemResultTO> responseStream, ServerCallContext context)
        {
            try
            {
                var createdItems =
                    await service.CreateItems(request.Data.Select(mapper.Map<ItemCreateSpec>), CancellationToken.None);
                foreach (var createdItem in createdItems.Select(mapper.Map<ItemTO>))
                {
                    await responseStream.WriteAsync(new ItemResultTO
                    {
                        Item = createdItem
                    });
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception while trying to create items");
                await responseStream.WriteAsync(new ItemResultTO
                {
                    Error = new ErrorTO
                    {
                        Message = e.Message
                    }
                });
            }
        }

        public async override Task UpdateItems(ItemUpdateRequestTO request,
            IServerStreamWriter<ItemResultTO> responseStream, ServerCallContext context)
        {
            try
            {
                var updatedItems = await service.UpdateItems(request.Data.Select(mapper.Map<ItemUpdateSpec>), CancellationToken.None);
                foreach (var updatedItem in updatedItems.Select(mapper.Map<ItemTO>))
                {
                    await responseStream.WriteAsync(new ItemResultTO
                    {
                        Item = updatedItem
                    });
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception while trying to update items");
                await responseStream.WriteAsync(new ItemResultTO
                {
                    Error = new ErrorTO
                    {
                        Message = e.Message
                    }
                });
            }
        }

        public async override Task<FileResourceResponseTO> UploadFile(FileResourceTO request, ServerCallContext context)
        {
            var id = await service.UploadFile(mapper.Map<FileResource>(request), CancellationToken.None);

            return new FileResourceResponseTO
            {
                Id = id
            };
        }

        public async override Task<VoidTO> CreateRelationships(CreateRelationshipsRequestTO request,
            ServerCallContext context)
        {
            await service.CreateRelationships(request.Relationships.Select(mapper.Map<RelationshipTable>), CancellationToken.None);
            return new VoidTO();
        }

        public async override Task<OperationSupportedResponseTO> IsOperationSupported(
            OperationSupportedRequestTO request,
            ServerCallContext context)
        {
            return new OperationSupportedResponseTO
            {
                IsSupported = await service.IsOperationSupported(mapper.Map<SupportedOperation>(request.Operation), CancellationToken.None)
            };
        }

        public async override Task<VoidTO> DeleteItems(IdRequestTO request, ServerCallContext context)
        {
            await service.DeleteItems(request.Data.Select(mapper.Map<Id>), CancellationToken.None);
            return new VoidTO();
        }

        public async override Task<VoidTO> AdvanceState(AdvanceStateRequestTO request, ServerCallContext context)
        {
            await service.AdvanceState(mapper.Map<Id>(request.Id), CancellationToken.None);
            return new VoidTO();
        }

        public async override Task ReadRelationships(RelationshipRequestTO request,
            IServerStreamWriter<RelationshipTableTO> responseStream, ServerCallContext context)
        {
            var relationships = await service.ReadRelationships(request.Ids.Select(mapper.Map<Id>),
                mapper.Map<RelationshipType>(request.Type), CancellationToken.None);
            
            foreach (var relationship in relationships.Select(mapper.Map<RelationshipTableTO>))
            {
                await responseStream.WriteAsync(relationship);
            }
        }
    }
}