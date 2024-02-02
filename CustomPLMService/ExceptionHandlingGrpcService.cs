using System;
using System.Threading.Tasks;
using Altium.PLM.Custom;
using CustomPLMService.Contract;
using Grpc.Core;
using Auth = Altium.PLM.Custom.Auth;
using Id = Altium.PLM.Custom.Id;
using Item = Altium.PLM.Custom.Item;
using ItemResult = Altium.PLM.Custom.ItemResult;
using RelationshipTable = Altium.PLM.Custom.RelationshipTable;
using Type = Altium.PLM.Custom.Type;
using TypeId = Altium.PLM.Custom.TypeId;
using Void = Altium.PLM.Custom.Void;

namespace CustomPLMService
{
    public class ExceptionHandlingGrpcService : PlmServiceImpl
    {
        public ExceptionHandlingGrpcService(ICustomPlmMetadataService metadataService, ICustomPlmService service) : base(metadataService, service)
        {
        }

        public override Task<AuthResult> TestAccess(Auth request, ServerCallContext context)
        {
            return HandleException(() => base.TestAccess(request, context));
        }

        public override Task ReadTypeIdentifiers(TypeRequest request, IServerStreamWriter<TypeId> responseStream, ServerCallContext context)
        {
            return HandleException(() => base.ReadTypeIdentifiers(request, responseStream, context));
        }

        public override Task ReadTypes(TypeIdRequest request, IServerStreamWriter<Type> responseStream, ServerCallContext context)
        {
            return HandleException(() => base.ReadTypes(request, responseStream, context));
        }

        public override Task ReadItems(IdRequest request, IServerStreamWriter<Item> responseStream, ServerCallContext context)
        {
            return HandleException(() => base.ReadItems(request, responseStream, context));
        }

        public override Task QueryItems(QueryItemsRequest request, IServerStreamWriter<Id> responseStream, ServerCallContext context)
        {
            return HandleException(() => base.QueryItems(request, responseStream, context));
        }

        public override Task CreateItems(ItemCreateRequest request, IServerStreamWriter<ItemResult> responseStream, ServerCallContext context)
        {
            return HandleException(() => base.CreateItems(request, responseStream, context));
        }

        public override Task UpdateItems(ItemUpdateRequest request, IServerStreamWriter<ItemResult> responseStream, ServerCallContext context)
        {
            return HandleException(() => base.UpdateItems(request, responseStream, context));
        }

        public override Task<FileResourceResponse> UploadFile(FileResource request, ServerCallContext context)
        {
            return HandleException(() => base.UploadFile(request, context));
        }

        public override Task<Void> CreateRelationships(CreateRelationshipsRequest request, ServerCallContext context)
        {
            return HandleException(() => base.CreateRelationships(request, context));
        }

        public override Task<OperationSupportedResponse> IsOperationSupported(OperationSupportedRequest request, ServerCallContext context)
        {
            return HandleException(() => base.IsOperationSupported(request, context));
        }

        public override Task<Void> DeleteItems(IdRequest request, ServerCallContext context)
        {
            return HandleException(() => base.DeleteItems(request, context));
        }

        public override Task<Void> AdvanceState(AdvanceStateRequest request, ServerCallContext context)
        {
            return HandleException(() => base.AdvanceState(request, context));
        }

        public override Task ReadRelationships(RelationshipRequest request, IServerStreamWriter<RelationshipTable> responseStream, ServerCallContext context)
        {
            return HandleException(() => base.ReadRelationships(request, responseStream, context));
        }

        private static TResult HandleException<TResult>(Func<TResult> function)
        {
            try
            {
                return function.Invoke();
            }
            catch (Exception e)
            {
                throw new RpcException(new Status(StatusCode.Internal, e.Message));
            }
        }
    }
}
