using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Altium.PLM.Custom;
using Altium.PLM.Custom.Reverse;
using AutoMapper;
using CustomPLMService.Contract;
using CustomPLMService.HybridAgent.Mediator.Handlers;
using CustomPLMService.HybridAgent.Mediator.Notifications;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
namespace CustomPLMService.Tests.HybridAgent.Mediator.Handlers;

[ExcludeFromCodeCoverage]
public class UploadFileNotificationHandlerTests
{
    private readonly Mock<ReversePLMService.ReversePLMServiceClient> grpcClientMock = new();
    private readonly Mock<ICustomPlmService> plmServiceMock = new();
    private readonly Mock<IMapper> mapperMock = new();
    private readonly Mock<ILogger<UploadFileNotificationHandler>> loggerMock = new();

    private readonly UploadFileNotificationHandler handler;

    public UploadFileNotificationHandlerTests()
    {
        var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Altium.PLM.Custom.Void()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => [], () => { });
        grpcClientMock.Setup(m => m.ReturnUploadFileAsync(It.IsAny<FileResourceResponseEx>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(fakeCall);

        handler = new UploadFileNotificationHandler(grpcClientMock.Object, plmServiceMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CallsGrpcService()
    {
        // Arrange
        var notification = new UploadFileNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new FileResource()
        };
        const string fileId = "mockFileId";
        plmServiceMock.Setup(m => m.UploadFile(It.IsAny<CustomPLMService.Contract.Models.Items.FileResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileId);
        var cancellationToken = (new CancellationTokenSource()).Token;

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        grpcClientMock.Verify(m => m.ReturnUploadFileAsync(
                It.Is<FileResourceResponseEx>(v =>
                    v.Value.Id == fileId),
                It.Is<Metadata>(metadata => metadata.Get(Constants.CorrelationIdKey).Value == notification.CorrelationId),
                It.IsAny<DateTime?>(),
                cancellationToken)
            , Times.Once);
    }

    [Fact]
    public async Task Handle_CallsPlmService()
    {
        // Arrange
        var cancellationToken = (new CancellationTokenSource()).Token;
        var mappedFileResource = new Contract.Models.Items.FileResource
        {
            FileName = "TestFileName"
        };
        var notification = new UploadFileNotification
        {
            CorrelationId = "TestCorrelationId",
            Request = new FileResource()
        };

        plmServiceMock.Setup(m => m.UploadFile(It.IsAny<CustomPLMService.Contract.Models.Items.FileResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("testId");

        mapperMock.Setup(m => m.Map<Contract.Models.Items.FileResource>(It.IsAny<FileResource>())).Returns(mappedFileResource);

        // Act
        await handler.Handle(notification, cancellationToken);

        // Assert
        plmServiceMock.Verify(m =>
                m.UploadFile(It.Is<Contract.Models.Items.FileResource>(resource => resource.FileName == mappedFileResource.FileName),
                    cancellationToken),
            Times.Once);
    }
}
