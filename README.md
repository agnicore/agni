# Agni Cluster OS

Agni Cluster OS is a UNISTACK&reg; software platform for writing custom distributed, **SaaS**, and **PaaS** solutions. The sharp focus is kept on **business-oriented systems** which have much domain logic/messages/services/contracts and other rules.

Agni OS, is not just a library - it is a full stack framework(an **A**pplication **O**perating **S**ystem) for high scalability having all development done using **lean .NET** (free from 3rd parties)

Per definition of UNISTACK&reg; ideology - all code is written in **C#** and **JavaScript** (some Web Admin UI tools) only. All base libraries are used in the same consistent way throught the code base. This project **only uses language and runtime features** which are **standard to the CLR and language itself**, as provided by primary Microsoft implementation.

## Overview of Features

* **Hierarchy of geo-spread regions**, NOC, zones
* **Host** and **zone** governor **supervisor processes**
* Logical to physical addressing/**node name resolution**
* **Hierarchical version-controlled configuration** engine; data mounted via Virtual File System (e.g. SVN, Web, RDBMS etc.)
* **Global Distributed Unique ID** (GDID) generation - monotonic increasing integers
* **Distributed lock manager** (DLM) - operates in hierarchical cluster allowing to coordinate tasks on the relevant level of hierarchy/scope
* **WorkSets** - a set of virtual work which balances ietms a kin to C# Parallel.Foreach in cluster - automatically coordinates between nodes
* **HostSets** - sets of workers providing balancing and sharding on topic/key
* **Todo Queues** - queues of "serverless" Todo instances - similar to distributed Tasks
* **Processes** - distributed contexts which coordinate Todo execution strands. Process exchange Signals to 
* **Database Sharding Router** - splis data into range partitions and then shards - supports any RDBMS or NoSQL as leaf nodes backend
* **Key-Value Cluster Database** with expiration



See [Agnicore Documentation Site](http://agnicore.com/products/agnios/book/introduction.htm) for more details

## Runtime / Platform

Agni is built on [NFX](https://github.com/agnicore/nfx); the whole stack is based on **.NET Standard** and runs on **.NET Framework 4.7+** and **.NET Core 2+** runtimes.

Agni support on platforms is the same as for its base core library [NFX](https://github.com/agnicore/nfx) - **.NET Standard 2+** which works on different runtimes. Officially we support .NET Core and .NET Framework:

    .NET Standard 2 - supported
    .NET Core 2 - supported
    .NET Framework 4.7.1 - supports classic .NET Framework 4.7.1+

NFX supports cross-platform development and is tested on:

    Windows Win 2010 Core 2 and Net Fx
    Linux Ubuntu 16 LTS using Core 2
    Mac OS 14 using Core 2

NFX Builds on:

    Windows / MSBuild 15 / VS 2017
    in process - *nix (need scripts and build process refinements)







