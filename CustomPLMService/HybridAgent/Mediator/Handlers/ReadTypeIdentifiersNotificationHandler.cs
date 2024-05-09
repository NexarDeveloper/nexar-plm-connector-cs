﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.Contract.Models.Metadata;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using TypeIdTO = Altium.PLM.Custom.TypeId;

namespace CustomPLMService.HybridAgent.Mediator.Handlers;

public class ReadTypeIdentifiersNotificationHandler(
    ReversePLMService.ReversePLMServiceClient grpcClient,
    ICustomPlmMetadataService plmMetadataService,
    IMapperBase mapper,
    ILogger<ReadTypeIdentifiersNotificationHandler> logger) : INotificationHandler<ReadTypeIdentifiersNotification>
{

    public async Task Handle(ReadTypeIdentifiersNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Read Type Identifiers request");

        var responseStream = grpcClient.ReturnReadTypeIdentifiers(cancellationToken: cancellationToken).RequestStream;
        try
        {
            var typeIdentifiers =
                await plmMetadataService.ReadTypeIdentifiers(
                    mapper.Map<BaseType>(notification.Request.BaseType));
            foreach (var typeId in typeIdentifiers.Select(mapper.Map<TypeIdTO>))
            {
                await responseStream.WriteAsync(new TypeIdEx
                {
                    CorrelationId = notification.CorrelationId,
                    Value = typeId
                }, cancellationToken);
            }
        }
        finally
        {
            await responseStream.CompleteAsync();
        }
    }
}
