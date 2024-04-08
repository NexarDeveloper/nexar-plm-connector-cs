# Operation flows
To illustrate the relationship between **Nexar Generic PLM Connector** endpoints, and the overall flow of the basic operations, 
this section will present the simplified flow of operations available via the generic connector API. These flows omit more complicated
scenarios, but unless your *IsOperationSupported()* method returns true for those queries, A365 won't start those operations and will limit
to operations presented in these flows.

## Sync operation
### To PLM
Sync to PLM starts with verifying Auth data. If the generic connector accepts the Auth data, 
the Sync process starts. First, it reads all the metadata types. Then, based on connector configuration, A365 will query
its internal data store to select items that need syncing. When the list of components that need to be synced to PLM is ready, 
process will start fetching the items from PLM (based on the configured key). If the item doesn't exist on PLM, it will be created.
In other case, updated. This is illustrated on the following diagram:

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
Sync to PLM starts with verifying Auth data. If the generic connector accepts the Auth data,
the Sync process starts. First, it reads all the metadata types. Then, based on connector configuration, A365 will query
PLM for list of item ids that need syncing. For each of these ids, process will fetch the items from PLM 
(based on the configuration) and store them on Altium side. This is illustrated on the following diagram:

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

### Parts Choice
   TBD
## Publish BOM
Publish BOM operation starts with verifying Auth data. If the generic connector accepts the Auth data,
the Sync process starts. First, it reads all the metadata types. It will then read from PLM all items that are present
in the BOM. Based on the response, it will create missing items (like project file, components that weren't returned by ReadItems etc.)
and request creation of BOM type relationship between the project and the items. This is illustrated on the following diagram:

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
Publish BOM operation starts with verifying Auth data. If the generic connector accepts the Auth data,
the Sync process starts. First, it reads all the metadata types. Then, A365 lists all components that exist in the project
and tries to read them from the Generic Connector side. Worth noticing that the project itself is also threaded as an item. 
Based on result, it will either create or update items. When the sync part finishes, A365 will start uploading the relationship
information. To do so, when needed, it will upload a file (attachment) and later run *CreateRelationships* command that will
refer to the uploaded file. Example of such files could be *.pcba assembly files. The number and type of those files depends on the publish
template configuration. When the *CreateRelationships* operation is ended, the basic publish operation is over. If *PropagateChangeOrderToNextStep*
would be configured on A365 and CustomPLMService would support operations such as *CreateChangeOrder*, *AdvanceWorkflowState* operation would also be started.

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