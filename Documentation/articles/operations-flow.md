# Operational Flows
To illustrate the relationship between **Altium 365 Generic PLM Connector** endpoints and the overall flow of the basic operations, 
this section presents the simplified flow of operations available using the Generic Connector API. It excludes more complicated
scenarios, but unless your *IsOperationSupported()* method returns true for those queries, Altium 365 will not start those 
operations and will be limited to the operations presented in the following section.

## Sync Operation
### To PLM
Sync to PLM starts with verifying *Auth* data. If the Generic Connector accepts the *Auth* data, 
the Sync process begins. First, it reads all the metadata types. Then, based on connector configuration, Altium 365 will query
its internal data store to select items that need to be synced. When the list of components that need to be synced to PLM is ready, 
the process will start fetching the items from PLM (based on the configured key). If the item does not exist on PLM, it will be created.
If it does exist, it will be updated. This is illustrated in the following diagram:

```plantuml
title Sync To PLM
skinparam sequenceMessageAlign center
skinparam ParticipantPadding 100

Participant "Altium 365" as Altium
Participant "Generic Connector" as Connector

    Altium -> Connector: TestAccess()
        activate Connector
    Connector --> Altium: <<Success>>
        deactivate Connector
    |||
    Altium -> Connector: ReadTypeIdentifiers()
        activate Connector
    Connector --> Altium: <<TypeID[]>>
        deactivate Connector

    Altium -> Connector: ReadTypes()
        activate Connector
    Connector --> Altium: <<Type[]>>
        deactivate Connector
    ||| 
    loop for all chunks with items
        Altium -> Connector: ReadItems()
            activate Connector
        Connector --> Altium: <<Item[]>>
            deactivate Connector
        
        Altium -> Connector: CreateItems() || UpdateItems()
            activate Connector
        Connector --> Altium: <<Item[]>>
            deactivate Connector
    end
```

### To Altium
Sync to PLM starts with verifying *Auth* data. If the Generic Connector accepts the *Auth* data,
the Sync process begins. First, it reads all the metadata types. Then, based on connector configuration, Altium 365 will query
PLM for a list of item IDs that need to be synced. For each of these IDs, the process will fetch the items from PLM 
(based on the configuration) and store them on the Altium side. This is illustrated in the following diagram:

```plantuml
title Sync To Altium
skinparam sequenceMessageAlign center
skinparam ParticipantPadding 100

Participant "Altium 365" as Altium
Participant "Generic Connector" as Connector

    Altium -> Connector: TestAccess()
        activate Connector
    Connector --> Altium: <<Success>>
        deactivate Connector
    |||
    Altium -> Connector: ReadTypeIdentifiers()
        activate Connector
    Connector --> Altium: <<TypeID[]>>
        deactivate Connector

    Altium -> Connector: ReadTypes()
        activate Connector
    Connector --> Altium: <<Type[]>>
        deactivate Connector
    |||
    Altium -> Connector: QueryItems()
        activate Connector
    Connector --> Altium: <<Id[]>>
        deactivate Connector
    
    loop for all chunks with items
        Altium -> Connector: ReadItems()
            activate Connector
        Connector --> Altium: <<Item[]>>
            deactivate Connector
    end
```

### Part Choices
#### To PLM
The *Part Choice* operation is run at the end of the Sync Operation. It consists of reading items from the PLM side and then
running *CreateItems* or *UpdateItems* operations. What is worth noticing is the format in which Part Choices are provided.
For every *Part Choice* entry, **Manufacturer Name** and **Manufacturer Part Number** attributes configured in **Attribute Definition** 
configuration row will contain the *Part Choice* data. 

Number of *Part Choices* that can be passed, is determined by the number of **Attribute Definition** rows in the configuration.
If only three rows are added to the **PLM Integration** config, up to three *Part Choices* will be provided 
(each occupying parameters from a single row of the configuration). Any additional Part Choice will be ignored.

```plantuml
title Sync To Altium
skinparam sequenceMessageAlign center
skinparam ParticipantPadding 100

Participant "Altium 365" as Altium
Participant "Generic Connector" as Connector

    ==End of Sync Operation==
    |||
    Altium -> Connector: ReadItems()
        activate Connector
    Connector --> Altium: <<Item[]>>
        deactivate Connector
    
    loop for all chunks with items
        Altium -> Connector: CreateItems() || UpdateItems()
            activate Connector
        Connector --> Altium: <<Item[]>>
            deactivate Connector
    end
```


## Publish BOM
Publishing BOM is a part of Publishing a Project. During the Publish Project operation,
there will be additional request for creation of a BOM type relationship between the project and the created items. 
This is illustrated in the following diagram:

```plantuml
title Publish BOM
skinparam sequenceMessageAlign center
skinparam ParticipantPadding 100

Participant "Altium 365" as Altium
Participant "Generic Connector" as Connector

    Altium -> Connector: TestAccess()
        activate Connector
    Connector --> Altium: <<Success>>
        deactivate Connector
    |||
    Altium -> Connector: ReadTypeIdentifiers()
        activate Connector
    Connector --> Altium: <<TypeID[]>>
        deactivate Connector

    Altium -> Connector: ReadTypes()
        activate Connector
    Connector --> Altium: <<Type[]>>
        deactivate Connector
    |||
    Altium -> Connector: ReadItems()
        activate Connector
    Connector --> Altium: <<Item[]>>
        deactivate Connector
    
    Altium -> Connector: CreateItems()
        activate Connector
    Connector --> Altium: <<Item[]>>
        deactivate Connector
        
    Altium -> Connector: CreateRelationships()
        activate Connector
    Connector --> Altium: <<return>>
        deactivate Connector        
```

## Publish Project
Project Publishing starts with verifying *Auth* data. If the Generic Connector accepts the *Auth* data,
the Sync process begins. First, it reads all the metadata types. Then, Altium 365 lists all components that exist in the project
and tries to read them from the Generic Connector side. It is worth mentioning that the project itself is also threaded as an item. 
Based on the result, it will either create or update items. When the sync finishes, Altium 365 will start uploading the relationship
information. To do so, when needed, it will upload a file as an attachment and later run the *CreateRelationships* command that will
refer to the uploaded file. An example of such files is *.pcba assembly file. The number and type of the files depends on the publish
template configuration. When the *CreateRelationships* operation finishes, the basic publish operation is over. If *PropagateChangeOrderToNextStep*
would be configured on Altium 365 and CustomPLMService supported operations such as *CreateChangeOrder*, the *AdvanceWorkflowState* operation would also begin.

```plantuml
title Publish Project
skinparam sequenceMessageAlign center
skinparam ParticipantPadding 100

Participant "Altium 365" as Altium
Participant "Generic Connector" as Connector

    Altium -> Connector: TestAccess()
        activate Connector
    Connector --> Altium: <<Success>>
        deactivate Connector
    |||
    Altium -> Connector: ReadTypeIdentifiers()
        activate Connector
    Connector --> Altium: <<TypeID[]>>
        deactivate Connector

    Altium -> Connector: ReadTypes()
        activate Connector
    Connector --> Altium: <<Type[]>>
        deactivate Connector
    |||
    Altium -> Connector: ReadItems()
        activate Connector
    Connector --> Altium: <<Item[]>>
        deactivate Connector
    
    loop
        Altium -> Connector: CreateItems() || UpdateItems()
            activate Connector
        Connector --> Altium: <<Item[]>>
            deactivate Connector
    end
    
    loop Called one time per file to upload
        Altium -> Connector: UploadFile()
            activate Connector
        Connector --> Altium: <<return>>
            deactivate Connector
    end
    
    Altium -> Connector: CreateRelationships()
        activate Connector
    Connector --> Altium: <<return>>
        deactivate Connector
```
