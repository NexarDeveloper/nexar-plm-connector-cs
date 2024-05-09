using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models;
using CustomPLMService.Contract.Models.Metadata;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Altium.PLM.Custom.AuthResult;
using Auth = CustomPLMService.Contract.Models.Authentication.Auth;
using VoidTO = Altium.PLM.Custom.Void;
using AuthResultTO = Altium.PLM.Custom.AuthResult;
using FileResource = CustomPLMService.Contract.Models.Items.FileResource;
using Id = CustomPLMService.Contract.Models.Items.Id;
using OperationSupportedResponseTO = Altium.PLM.Custom.OperationSupportedResponse;
using RelationshipTable = CustomPLMService.Contract.Models.Relationship.RelationshipTable;
using RelationshipTableTO = Altium.PLM.Custom.RelationshipTable;
using RelationshipType = CustomPLMService.Contract.Models.Metadata.RelationshipType;
using TypeIdTO = Altium.PLM.Custom.TypeId;
using TypeTO = Altium.PLM.Custom.Type;
using Type = CustomPLMService.Contract.Models.Metadata.Type;
using IdTO = Altium.PLM.Custom.Id;
using ItemTO = Altium.PLM.Custom.Item;
using ItemResultTO = Altium.PLM.Custom.ItemResult;
using ErrorTO = Altium.PLM.Custom.Error;
using FileResourceResponseTO = Altium.PLM.Custom.FileResourceResponse;
using ItemCreateSpec = CustomPLMService.Contract.Models.Items.ItemCreateSpec;
using ItemUpdateSpec = CustomPLMService.Contract.Models.Items.ItemUpdateSpec;
using Query = CustomPLMService.Contract.Models.Query.Query;

namespace CustomPLMService;

public interface IHybridAgent
{
    Task Run(CancellationToken ct);
}

public class HybridAgent(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    ICustomPlmService plmService,
    ICustomPlmMetadataService plmMetadataService,
    IMapperBase mapper,
    ILogger<HybridAgent> logger) : IHybridAgent
{
    public async Task Run(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var getRequestResponse = grpcClient.GetRequest(new VoidTO(), cancellationToken: ct);
                while (await getRequestResponse.ResponseStream.MoveNext())
                {
                    var request = getRequestResponse.ResponseStream.Current;
                    logger.LogInformation($"Received request {request}");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    (Task.Run(() => Dispatch(request, ct), ct)).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                logger.LogDebug(ex, "Deadline Exceeded");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(ex, "Cancellation request received");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occured");
            }
        }
    }

    private async Task Dispatch(Request request, CancellationToken ct)
    {
        //TODO: use MediatR here
        logger.LogTrace($"Dispatching request: {request}");
        if (request.TestAccess is not null)
        {
            var authResult = new AuthResultTO();

            if (await plmService.TestAccess(mapper.Map<Auth>(request)))
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

            await grpcClient.ReturnTestAccessAsync(new AuthResultEx
            {
                Value = authResult,
                CorrelationId = request.CorrelationId
            }, cancellationToken: ct);
        }
        else if (request.AdvanceState is not null)
        {
            await plmService.AdvanceState(
                mapper.Map<Id>(request.AdvanceState.Id));
            await grpcClient.ReturnAdvanceStateAsync(new VoidEx
            {
                CorrelationId = request.CorrelationId,
                Value = new VoidTO()
            }, cancellationToken: ct);
        }
        else if (request.IsOperationSupported is not null)
        {
            var isSupported =
                await plmService.IsOperationSupported(
                    mapper.Map<SupportedOperation>(request.IsOperationSupported.Operation));
            await grpcClient.ReturnIsOperationSupportedAsync(new OperationSupportedResponseEx
            {
                CorrelationId = request.CorrelationId,
                Value = new OperationSupportedResponseTO
                {
                    IsSupported = isSupported
                }
            }, cancellationToken: ct);
        }
        else if (request.CreateRelationships is not null)
        {
            await plmService.CreateRelationships(
                request.CreateRelationships.Relationships.Select(mapper.Map<RelationshipTable>));
            await grpcClient.ReturnCreateRelationshipsAsync(new VoidEx
            {
                CorrelationId = request.CorrelationId,
                Value = new VoidTO()
            }, cancellationToken: ct);
        }
        else if (request.ReadRelationships is not null)
        {
            var responseStream = grpcClient.ReturnReadRelationships(cancellationToken: ct).RequestStream;

            try
            {
                var relationships = await plmService.ReadRelationships(
                    request.ReadRelationships.Ids.Select(mapper.Map<Id>),
                    mapper.Map<RelationshipType>(request.ReadRelationships.Type));
                foreach (var relationship in relationships.Select(mapper.Map<RelationshipTableTO>))
                {
                    await responseStream.WriteAsync(new RelationshipTableEx
                    {
                        CorrelationId = request.CorrelationId,
                        Value = relationship
                    }, ct);
                }
            }
            finally
            {
                await responseStream.CompleteAsync();
            }
        }
        else if (request.UploadFile is not null)
        {
            var id = await plmService.UploadFile(mapper.Map<FileResource>(request.UploadFile));
            await grpcClient.ReturnUploadFileAsync(new FileResourceResponseEx
            {
                CorrelationId = request.CorrelationId,
                Value = new FileResourceResponseTO
                {
                    Id = id
                }
            }, cancellationToken: ct);
        }
        else if (request.CreateItems is not null)
        {
            var responseStream = grpcClient.ReturnCreateItems(cancellationToken: ct).RequestStream;
            try
            {
                var createdItems =
                    await plmService.CreateItems(request.CreateItems.Data.Select(mapper.Map<ItemCreateSpec>));
                foreach (var createdItem in createdItems.Select(mapper.Map<ItemTO>))
                {
                    await responseStream.WriteAsync(new ItemResultEx
                        {
                            CorrelationId = request.CorrelationId,
                            Value = new ItemResultTO
                            {
                                Item = createdItem
                            }
                        }
                        , ct);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception while trying to create items");
                await responseStream.WriteAsync(new ItemResultEx
                {
                    CorrelationId = request.CorrelationId,
                    Value = new ItemResultTO
                    {
                        Error = new ErrorTO
                        {
                            Message = e.Message
                        }
                    }
                }, ct);
            }
            finally
            {
                await responseStream.CompleteAsync();
            }
        }
        else if (request.DeleteItems is not null)
        {
            await plmService.DeleteItems(request.DeleteItems.Data.Select(mapper.Map<Id>));
            await grpcClient.ReturnDeleteItemsAsync(new VoidEx
            {
                CorrelationId = request.CorrelationId,
                Value = new VoidTO()
            }, cancellationToken: ct);
        }
        else if (request.QueryItems is not null)
        {
            var responseStream = grpcClient.ReturnQueryItems(cancellationToken: ct).RequestStream;

            try
            {
                var plmItems = await plmService.QueryItems(mapper.Map<Query>(request.QueryItems.Query),
                    mapper.Map<Type>(request.QueryItems.Type));
                foreach (var plmItem in plmItems.Select(mapper.Map<IdTO>))
                {
                    await responseStream.WriteAsync(new IdEx
                    {
                        CorrelationId = request.CorrelationId,
                        Value = plmItem
                    }, ct);
                }
            }
            finally
            {
                await responseStream.CompleteAsync();
            }
        }
        else if (request.ReadItems is not null)
        {
            var responseStream = grpcClient.ReturnReadItems(cancellationToken: ct).RequestStream;

            try
            {
                var items = await plmService.ReadItems(request.ReadItems.Data.Select(mapper.Map<Id>));
                foreach (var item in items.Select(mapper.Map<ItemTO>))
                {
                    await responseStream.WriteAsync(new ItemEx
                    {
                        CorrelationId = request.CorrelationId,
                        Value = item
                    }, ct);
                }
            }
            finally
            {
                await responseStream.CompleteAsync();
            }
        }
        else if (request.UpdateItems is not null)
        {
            var responseStream = grpcClient.ReturnUpdateItems(cancellationToken: ct).RequestStream;
            try
            {
                var updatedItems =
                    await plmService.UpdateItems(request.UpdateItems.Data.Select(mapper.Map<ItemUpdateSpec>));
                foreach (var updatedItem in updatedItems.Select(mapper.Map<ItemTO>))
                {
                    await responseStream.WriteAsync(new ItemResultEx
                    {
                        CorrelationId = request.CorrelationId,
                        Value = new ItemResultTO
                        {
                            Item = updatedItem
                        }
                    }, ct);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception while trying to update items");
                await responseStream.WriteAsync(new ItemResultEx
                {
                    CorrelationId = request.CorrelationId,
                    Value = new ItemResultTO
                    {
                        Error = new ErrorTO
                        {
                            Message = e.Message
                        }
                    }
                }, ct);
            }
            finally
            {
                await responseStream.CompleteAsync();
            }
        }
        else if (request.ReadTypes is not null)
        {
            var responseStream = grpcClient.ReturnReadTypes(cancellationToken: ct).RequestStream;
            try
            {
                var types = await plmMetadataService.ReadTypes(request.ReadTypes.Data.Select(mapper.Map<TypeId>));
                foreach (var type in types.Select(mapper.Map<TypeTO>))
                {
                    await responseStream.WriteAsync(new TypeEx
                    {
                        CorrelationId = request.CorrelationId,
                        Value = type
                    }, ct);
                }
            }
            finally
            {
                await responseStream.CompleteAsync();
            }
        }
        else if (request.ReadTypeIdentifiers is not null)
        {
            var responseStream = grpcClient.ReturnReadTypeIdentifiers(cancellationToken: ct).RequestStream;
            try
            {
                var typeIdentifiers =
                    await plmMetadataService.ReadTypeIdentifiers(
                        mapper.Map<BaseType>(request.ReadTypeIdentifiers.BaseType));
                foreach (var typeId in typeIdentifiers.Select(mapper.Map<TypeIdTO>))
                {
                    await responseStream.WriteAsync(new TypeIdEx
                    {
                        CorrelationId = request.CorrelationId,
                        Value = typeId
                    }, ct);
                }
            }
            finally
            {
                await responseStream.CompleteAsync();
            }
        }
    }
}