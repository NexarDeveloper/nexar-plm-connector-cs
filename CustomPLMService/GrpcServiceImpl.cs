using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Altium.PLM.Custom;
using AutoMapper;
using CustomPLMService.Contract;
using Grpc.Core;
using TypeIdRequestTO = Altium.PLM.Custom.TypeIdRequest;
using TypeRequestTO = Altium.PLM.Custom.TypeRequest;
using IdRequestTO = Altium.PLM.Custom.IdRequest;
using QueryItemsRequestTO = Altium.PLM.Custom.QueryItemsRequest;
using ItemCreateRequestTO = Altium.PLM.Custom.ItemCreateRequest;
using CreateRelationshipsRequestTO = Altium.PLM.Custom.CreateRelationshipsRequest;
using OperationSupportedRequestTO = Altium.PLM.Custom.OperationSupportedRequest;
using AdvanceStateRequestTO = Altium.PLM.Custom.AdvanceStateRequest;
using RelationshipRequestTO = Altium.PLM.Custom.RelationshipRequest;
using TypeTO = Altium.PLM.Custom.Type;
using VoidTO = Altium.PLM.Custom.Void;
using AuthTO = Altium.PLM.Custom.Auth;
using AuthResultTO = Altium.PLM.Custom.AuthResult;
using TypeIdTO = Altium.PLM.Custom.TypeId;
using BaseTypeTO = Altium.PLM.Custom.BaseType;
using Credentials = CustomPLMService.Contract.Credentials;
using IdTO = Altium.PLM.Custom.Id;
using ItemTO = Altium.PLM.Custom.Item;
using ItemResultTO = Altium.PLM.Custom.ItemResult;
using ErrorTO = Altium.PLM.Custom.Error;
using ItemUpdateRequestTO = Altium.PLM.Custom.ItemUpdateRequest;
using FileResourceTO = Altium.PLM.Custom.FileResource;
using FileResourceResponseTO = Altium.PLM.Custom.FileResourceResponse;
using OperationSupportedResponseTO = Altium.PLM.Custom.OperationSupportedResponse;
using RelationshipTableTO = Altium.PLM.Custom.RelationshipTable;

namespace CustomPLMService
{
    public class PlmServiceImpl : PLMService.PLMServiceBase
    {
        private const string AltiumAfsPlmGenericConnector = "Altium_AFS_PLM_Generic_Connector";

        private readonly ICustomPlmMetadataService metadataService;
        private readonly ICustomPlmService service;

        protected PlmServiceImpl(ICustomPlmMetadataService metadataService, ICustomPlmService service)
        {
            this.metadataService = metadataService;
            this.service = service;
        }

        public override Task<AuthResultTO> TestAccess(AuthTO request, ServerCallContext context)
        {

            return Task.Run(() =>
            {
                var authResult = new AuthResultTO
                {
                    Success = false
                };

                if (request.Licenses.Contains(AltiumAfsPlmGenericConnector))
                {
                    if (service.TestAccess(Converter.Map<Contract.Auth>(request)))
                    {
                        authResult.Success = true;
                        authResult.Status = AuthResult.Types.Status.Success;
                    }
                    else
                    {
                        authResult.Status = AuthResult.Types.Status.InvalidCredentials;
                    }
                }
                else
                {
                    Console.Error.WriteLine("ERROR: No valid license for PLM generic connector available");
                    authResult.Status = AuthResult.Types.Status.NoLicense;
                }

                return authResult;
            });
        }

        public override async Task ReadTypeIdentifiers(TypeRequestTO request,
            IServerStreamWriter<TypeIdTO> responseStream, ServerCallContext context)
        {
            var ctx = Context(request.Auth);
            foreach (var plmTypeId in metadataService.ReadTypeIdentifiers(ctx,
                Converter.Map<Contract.BaseType>(request.BaseType)))
            {
                await responseStream.WriteAsync(Converter.Map<TypeIdTO>(plmTypeId));
            }
        }


        public override async Task ReadTypes(TypeIdRequestTO request, IServerStreamWriter<TypeTO> responseStream,
            ServerCallContext context)
        {
            var ctx = Context(request.Auth);
            foreach (var type in metadataService.ReadTypes(ctx, request.Data.Select(Converter.Map<Contract.TypeId>)))
            {
                await responseStream.WriteAsync(Converter.Map<TypeTO>(type));
            }
        }

        public override async Task ReadItems(IdRequestTO request, IServerStreamWriter<ItemTO> responseStream,
            ServerCallContext context)
        {
            var ctx = Context(request.Auth);

            foreach (var id in request.Data.Select(Converter.Map<Contract.Id>))
            {
                var item = service.ReadItem(ctx, id);
                if (item != null)
                {
                    var message = Converter.Map<ItemTO>(item);
                    await responseStream.WriteAsync(message);
                }
            }
        }

        public override async Task QueryItems(QueryItemsRequestTO request, IServerStreamWriter<IdTO> responseStream,
            ServerCallContext context)
        {
            var ctx = Context(request.Auth);
            foreach (var plmItem in service.QueryItems(ctx, Converter.Map<Contract.Query>(request.Query),
                Converter.Map<Contract.Type>(request.Type)))
            {
                if (plmItem != null)
                {
                    await responseStream.WriteAsync(Converter.Map<IdTO>(plmItem));
                }
            }
        }

        public override async Task CreateItems(ItemCreateRequestTO request,
            IServerStreamWriter<ItemResultTO> responseStream, ServerCallContext context)
        {
            try
            {
                var ctx = Context(request.Auth);
                foreach (var itemSpec in request.Data.Select(Converter.Map<Contract.ItemCreateSpec>))
                {
                    var result = new ItemResultTO();
                    try
                    {
                        var createdItem = Converter.Map<ItemTO>(service.CreateItem(ctx, itemSpec));
                        result.Item = createdItem;
                    }
                    catch (Exception e)
                    {
                        result.Error = new ErrorTO
                        {
                            Message = e.Message
                        };
                    }
                    await responseStream.WriteAsync(result);
                }
            }
            catch (Exception e)
            {
                throw new RpcException(new Status(StatusCode.Internal, e.Message));
            }
        }


        public override async Task UpdateItems(ItemUpdateRequestTO request,
            IServerStreamWriter<ItemResultTO> responseStream, ServerCallContext context)
        {
            var ctx = Context(request.Auth);
            foreach (var item in request.Data.Select(Converter.Map<Contract.ItemUpdateSpec>))
            {
                var result = new ItemResultTO();
                try
                {
                    var updatedItem = service.UpdateItem(ctx, item);
                    result.Item = Converter.Map<ItemTO>( updatedItem);
                }
                catch (Exception e)
                {
                    result.Error = new ErrorTO
                    {
                        Message = e.Message
                    };
                }
                await responseStream.WriteAsync(result);
            }
        }

        public override Task<FileResourceResponseTO> UploadFile(FileResourceTO request, ServerCallContext context)
        {
            var id = Guid.NewGuid().ToString();

            var filePath = PrepareFileLocation(id, request.FileName);
            using (var output = new FileStream(filePath, FileMode.Create))
            {
                request.Data.WriteTo(output);
            }

            return Task.Run(() => new FileResourceResponseTO
            {
                Id = id
            });
        }

        private static string PrepareFileLocation(string id, string fileName)
        {
            var directory = GetDirectoryForId(id);
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, fileName);
        }

        private static string GetDirectoryForId(string id)
        {
            return Path.Combine("data", id);
        }

        private static void CleanupFiles(CreateRelationshipsRequestTO request)
        {
            foreach (var relationshipTable in request.Relationships)
            {
                foreach (var relationshipTableRow in relationshipTable.Rows)
                {
                    if (string.IsNullOrWhiteSpace(relationshipTableRow.FileId))
                    {
                        continue;
                    }

                    var directory = GetDirectoryForId(relationshipTableRow.FileId);
                    Directory.Delete(directory, true);
                }
            }
        }

        public override Task<VoidTO> CreateRelationships(CreateRelationshipsRequestTO request,
            ServerCallContext context)
        {
            var ctx = Context(request.Auth);
            try
            {
                service.CreateRelationships(ctx, request.Relationships.Select(Mapper.Map<Contract.RelationshipTable>));
            }
            finally
            {
                CleanupFiles(request);
            }

            return Task.Run(() => new VoidTO());
        }

        public override Task<OperationSupportedResponseTO> IsOperationSupported(OperationSupportedRequestTO request,
            ServerCallContext context)
        {
            return Task.Run(() => new OperationSupportedResponseTO
            {
                IsSupported = service.IsOperationSupported(Converter.Map<SupportedOperation>(request.Operation))
            });
        }

        public override Task<VoidTO> DeleteItems(IdRequestTO request, ServerCallContext context)
        {
            var ctx = Context(request.Auth);
            foreach (var id in request.Data)
            {
                service.DeleteItem(ctx, Converter.Map<Contract.Id>(id));
            }

            return Task.Run(() => new VoidTO());
        }

        public override Task<VoidTO> AdvanceState(AdvanceStateRequestTO request, ServerCallContext context)
        {
            var ctx = Context(request.Auth);
            service.AdvanceState(ctx, Converter.Map<Contract.Id>(request.Id));
            return Task.Run(() => new VoidTO());
        }

        public override async Task ReadRelationships(RelationshipRequestTO request,
            IServerStreamWriter<RelationshipTableTO> responseStream, ServerCallContext context)
        {
            var ctx = Context(request.Auth);
            foreach (var id in request.Ids)
            {
                await responseStream.WriteAsync(Converter.Map<RelationshipTableTO>(
                    service.ReadRelationship(ctx, Converter.Map<Contract.Id>(id),
                        Converter.Map<Contract.RelationshipType>(request.Type))));
            }
        }

        private static Context Context(AuthTO requestAuth)
        {
            if (requestAuth.AuthToken != null)
            {
                var token = requestAuth.AuthToken;
                var ctx = new Context(token);
                return ctx;
            }
            else
            {
                var credentials = requestAuth.Credentials;
                var ctx = new Context(new Credentials { Username = credentials.Username, Password = credentials.Password });
                return ctx;
            }
        }
    }
}
