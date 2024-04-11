# Solution Description

## 1000-foot view
The solution in this repository consists of two main parts. One, is the base implementation of the gRPC service needed by the *Altium 365 Generic PLM Connector* to start 
the connection between Altium 365 and the Generic Connector. The second is an example implementation of a file-based PLM system that can act as a point of reference. 
It requires .NET 8.0 SDK to be installed on the system which can be downloaded from [Microsoft .NET download page](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) 

When using the template project, the implementing party only has to implement two interfaces that will later be used for the communication process. 
These interfaces are as described below.

| Interface                 | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
|:--------------------------|:----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ICustomPlmMetadataService | This interface is responsible for providing metadata information about your PLM system. <br/>It provides information about types and their attributes that are exposed by your implementation of the Generic Connector solution. Based on the information received from this interface, inside the Altium 365 configuration, you will be able to create mapping between the types known in the Altium ecosystem (capacitors, resistors etc.) and the types native to your PLM implementation. |
| ICustomPlmService         | This interface is the heart of communication between Altium 365 and your PLM service. It is responsible for storing and retrieving items from and to Altium 365.                                                                                                                                                                                                                                                                                                                              |

This template solution provides some extension methods that will make it easy to register all the necessary middleware to run the gRPC service:

![Startup.cs with registration of Custom PLM service](images/solution/solution_register_middleware.jpg "Middleware registration")

Those extension methods will register your implementations, the Global Exception filter, Automapper profiles, Logger configuration etc, creating a functioning service out of the box.  
It creates a distinct separation between the gRPC-required datamodel and application logic. It also ensures that the *Auth* data received by the
gRPC controller is always available by a simple injection of <code>IContext</code> instance.

## Models
To hide the details of gRPC DTO models, **CustomPLMService** project is mapping all API-related transfer objects into models defined in **CustomPLMService.Contract**. 
This is done using the automapper profile (located at **CustomPLMService/PlmServiceMappingProfile.cs**).

The most interesting models from this list are as outlined below.
### Item (Item.cs)
Represents object in PLM system

![Item diagram](./images/solution/ItemDiagram.jpg "Item Diagram")

### RelationshipTable (RelationshipTable.cs)
Represents object relationships table in the PLM system. This table contains a list of relationships of the object with other objects.

![RelationshipTable diagram](./images/solution/RelationshipTableDiagram.jpg "Relationship Table Diagram")

### FileResource (FileResource.cs)
Represents file uploaded to the custom connector side. It is used as a staging area mainly for attachments that later are 
defined by the RelationshipTable (during a CreateRelationships operation)

![FileResource diagram](./images/solution/FileResourceDiagram.jpg "File Resource Diagram")

## Filesystem PLM Driver
This project represents a very simple file-based implementation of PLM integration. Its role is to provide an easy way
of learning the way the Altium 365 runs sync/publish operations giving hands-on debugging experience without 
too much development time.

The first interesting part of this implementation (for testing purposes) is how the metadata is handled.
The list of component types and their attributes is defined in the *appsettings.json* file. When started, the service will
read this data, and when asked for supported data types, return all of the defined ItemTypes each containing the same list
of AttributeNames. While this is not a perfect solution, as all ItemTypes share the same attributes, it is enough to configure
a simple PLM Connection.

The second part of the Filesystem PLM Driver is the implementation of the file-based PLM service. All the data stored by this solution
is kept in the *items* folder, located at the root of the working directory. The data is stored in human-readable form as a simple XML
document. 

It is worth mentioning that while almost all methods read data from the xml files, the *ReadRelationship* method generates fake
data on the fly. Keep this in mind when running Publish operations.
