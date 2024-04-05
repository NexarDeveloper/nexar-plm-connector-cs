## FAQ

### How is Authentication/Authorization implemented in the sample project?
There is currently no implementation for Authentication nor Authorization in this solution. Each party that is planning to use it with their
integration, is responsible for protecting the endpoints from unauthorised usage. Some level of Authorization can be achieved using the *Auth endpoint*

### What is the use of *Auth endpoint* and *IContext*?
Before every operation, A365 will call the *Auth endpoint*, to verify that the credentials user has provided, will in fact allow access to PLM service.
The implementing party, should verify those credentials during the *Auth endpoint* call are correct, and give A365 green light to continue with the operation.
The *IContext* is being picked up from the request via the gRPC interceptor. This allows the implementing party to access the provided PLM credentials, by Dependency Injection
without the hassle of passing it through the whole invocation tree.

### My sync operation fails, yet there were no errors reported by the Generic Connector. How can I check what went wrong?
Inside A365 Workplace web view, one can navigate to **Admin->PLM Integration** (Synchronization Status tab) or **Admin->Processes** (Browser tab) views. 
From there, after selecting the proper operation row, one can see more details about the failed operation by opening the *Data* tab.

![Failed Operation](./images/testing/FailedSync.png "Failed sync details")

### After opening the A365 Workspace web view, inside the *Admin* section, I cant find *PLM Integration* nor *Processes* rows?
Please verify that you have Enterprise licence assigned. PLM Integration and Processes functionality is only possible with this license being assigned.

### While configuring my PLM Connection, I can't select the *Generic Connector* as my PLM Connection driver. Why isn't it visible there?
Please verify that you have a licence for Generic Connector assigned. Generic Connector requires an extra license in order to be used for PLM Integration configuration.

### I have used the FilesystemPLMDriver for my tests with synchronization. At first, CreateItem operations were invoked, but now I only see UpdateItem calls. How to force the CreateItem operations to appear again?
It is most probably caused by the FilesystemPLMDriver already having all your items stored in a file based repository. In such case, A365 will not create new items and will try to update their values.
In order to make FilesystemPLMDriver *forget* the components, simply delete the *items* folder that was created in the root directory of the project 
(If started in debug mode from IDE, it will be located at *FilesystemPLMDriver\bin\Debug\net8.0*).

### I am receiving a lot of ReadItem requests with ids that doesn't exist on my side. How should my implementation react?
This is totally normal that the *ReadItem* will be called with yet not known component id. No exception should be thrown in
such case. The expected result will be to return *null* value, thus informing A365 that corresponding element needs to be created.
