## FAQ

### How is Authentication/Authorization implemented in the sample project?
There is currently no implementation for Authentication or Authorization in this solution. Each party that uses it with their
integration is responsible for protecting the endpoints from unauthorized usage. Some level of authorization can be achieved using the *Auth endpoint*

### What is the use of *Auth endpoint* and *IContext*?
Before every operation, Altium 365 will call the *Auth endpoint* to verify that the credentials the user has provided, will allow access to the PLM service.
The implementing party should verify that the credentials used during the *Auth endpoint* call are correct and give Altium 365 the green light to continue with the operation.
The *IContext* is picked up from the request via the gRPC interceptor. This allows the implementing party to access the provided PLM credentials by Dependency Injection
without the hassle of passing it through the entire invocation tree.

### My sync operation failed, yet the Generic Connector reported no errors. How can I check what went wrong?
In Altium 365 Workplace web view you can navigate to the **Admin->PLM Integration** (Synchronization Status tab) or the **Admin->Processes** (Browser tab) view. 
From there, after selecting the proper operation row, you can see more details about the failed operation by opening the *Data* tab.

![Failed Operation](./images/testing/FailedSync.png "Failed sync details")

### After opening the Altium 365 Workspace web view inside the *Admin* section, I do not see *PLM Integration* or *Processes*?
Please verify that you have an Enterprise license assigned. PLM Integration and Processes functionality is only possible with this license.

### While configuring my PLM Connection, I can't select the *Generic Connector* as my PLM Connection driver. Why isn't it visible there?
Please verify that you have a license for Generic Connector assigned. Generic Connector requires an extra license in order to be used for PLM Integration configuration.

### I have used the FilesystemPLMDriver for my tests with synchronization. At first, CreateItem operations were invoked, but now I only see UpdateItem calls. How can I force the CreateItem operations to appear again?
It is most probably caused by the FilesystemPLMDriver already having all your items stored in a file-based repository. In this case, Altium 365 will not create new items and will try to update their values.
In order to make FilesystemPLMDriver *forget* the components, delete the *items* folder that was created in the root directory of the project 
(If started in debug mode from IDE, it will be located at *FilesystemPLMDriver\bin\Debug\net8.0*.)

### I am receiving a lot of ReadItem requests with IDs that don't exist on my side. How should my implementation react?
It is normal that the *ReadItem* will be called with an unknown component ID. No exception should be thrown in these
cases. The expected result will be to return a *null* value, thus informing Altium 365 that the corresponding element needs to be created.
