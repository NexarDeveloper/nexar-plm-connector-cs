using Altium.PLM.Custom.Reverse;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using MediatR;
namespace CustomPLMService.HybridAgent;

public static class RequestExtensions
{
    public static INotification AsNotification(this Request request)
    {
        if (request.TestAccess is not null)
        {
            return new TestAccessNotification
            {
                CorrelationId = request.CorrelationId,
                Request = request.TestAccess
            };
        }

        if (request.AdvanceState is not null)
        {
            return new AdvanceStateNotification()
            {
                CorrelationId = request.CorrelationId,
                Request = request.AdvanceState
            };
        }

        if (request.IsOperationSupported is not null)
        {
            return new IsOperationSupportedNotification()
            {
                CorrelationId = request.CorrelationId,
                Request = request.IsOperationSupported
            };
        }

        if (request.CreateRelationships is not null)
        {
            return new CreateRelationshipsNotification()
            {
                CorrelationId = request.CorrelationId,
                Request = request.CreateRelationships
            };
        }

        if (request.ReadRelationships is not null)
        {
            return new ReadRelationshipNotification()
            {
                CorrelationId = request.CorrelationId,
                Request = request.ReadRelationships
            };
        }

        if (request.UploadFile is not null)
        {
            return new UploadFileNotification()
            {
                CorrelationId = request.CorrelationId,
                Request = request.UploadFile
            };
        }

        if (request.CreateItems is not null)
        {
            return new CreateItemsNotification()
            {
                CorrelationId = request.CorrelationId,
                Request = request.CreateItems
            };
        }

        if (request.DeleteItems is not null)
        {
            return new DeleteItemsNotification()
            {
                CorrelationId = request.CorrelationId,
                Request = request.DeleteItems
            };
        }

        if (request.QueryItems is not null)
        {
            return new QueryItemsNotification()
            {
                CorrelationId = request.CorrelationId,
                Request = request.QueryItems
            };
        }

        if (request.ReadItems is not null)
        {
            return new ReadItemsNotification()
            {
                CorrelationId = request.CorrelationId,
                Request = request.ReadItems
            };
        }

        if (request.UpdateItems is not null)
        {
            return new UpdateItemsNotification()
            {
                CorrelationId = request.CorrelationId,
                Request = request.UpdateItems
            };
        }

        if (request.ReadTypes is not null)
        {
            return new ReadTypesNotification()
            {
                CorrelationId = request.CorrelationId,
                Request = request.ReadTypes
            };
        }

        if (request.ReadTypeIdentifiers is not null)
        {
            return new ReadTypeIdentifiersNotification()
            {
                CorrelationId = request.CorrelationId,
                Request = request.ReadTypeIdentifiers
            };
        }

        throw new RequestMappingFailedException(request);
    }
}
