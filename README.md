# Agni Cluster OS

Agni Cluster OS is a UNISTACK&reg; software platform for writing custom distributed, **SaaS**, and **PaaS** solutions. The sharp focus is kept on **business-oriented systems** which have much domain logic/messages/services/contracts and other rules.

Agni OS, is not just a library - it is a full stack framework(an **A**pplication **O**perating **S**ystem) for high scalability having all development done using **lean .NET** (free from 3rd parties)

According to UNISTACK&reg; ideology - all code is written only in **C#** and **JavaScript** (used in some Web Admin UI tools). The same base libraries are used in the same consistent way throught the code base. This project **only uses language and runtime features** which are **standard to the CLR and language itself**, as provided by primary Microsoft implementation.

## Overview of Features

* **Hierarchy of geo-spread regions**, NOC, zones
* **Host** and **zone** governor **supervisor processes**
* Logical to physical addressing/**node name resolution**
* **Hierarchical version-controlled configuration** engine; data mounted via Virtual File System (e.g. SVN, Web, RDBMS etc.) the configuration bank is called [**"The Metabase"**](mbase/)
* [**Global Distributed Unique ID**](src/Agni/Identification) (GDID) generation - monotonic increasing integers
* [**Distributed lock manager**](src/Agni/Locking) (DLM) - operates in hierarchical cluster allowing to coordinate tasks on the relevant level of hierarchy/scope
* [**WorkSets**](src/Agni/Coordination/WorkSet.cs) - a set of virtual work which coordinates item execution among multiple nodes - akin to **C# Parallel.Foreach in a cluster**
* [**HostSets**](src/Agni/Coordination/HostSet.cs) - sets of workers providing balancing and sharding on topic/key
* [**Todo Queues**](src/Agni/Workers/Todo.cs) - queues of "serverless" Todo instances - similar to distributed Tasks
* [**Processes**](src/Agni/Workers/Process.cs) - distributed contexts which coordinate Todo execution strands. Process exchange Signals to 
* [**Database Sharding Router**](src/Agni/MDB) - splis data into range partitions and then shards - supports any RDBMS or NoSQL as leaf nodes backend
* [**Key-Value Cluster Database**](src/Agni/KDB) with expiration

Agni includes a module called [**Agni.Social**](src/Agni.Social) - a core for custom systems like Twitter/Facebook which have much social network logic. It is important to mention  scalability - the social component uses Agni sharding, Processes and Todos executed in a cluster to serve a truly unlimited number of clients/social profiles.

Social features:

* [**Social Graph Node**](src/Agni.Social/Graph) - cluster database solution for storing business entities (e.g. "friends", "groups", "rooms", "organizations" etc) and their relationships. The concrete systems provide [**GraphHost**](src/Agni.Social/Graph/Server/GraphHost.cs) implementations which map graph nodes to concrete business entities
* Graph [**node subscriptions**](src/Agni.Social/Graph/IGraphEventSystem.cs) and [**event delivery**](src/Agni.Social/Graph/Server/GraphHost.cs#L33) - done in the cloud, e.g. the system can handle users with as low as a few to multi-multi million node subscriptions (think Facebook or Twitter)
* Node ["**Friendship**"](src/Agni.Social/Graph/IGraphFriendSystem.cs)
* [**Comment system**](src/Agni.Social/Graph/IGraphCommentSystem.cs) - voting like/dislike. Comments, Questions, Answers, Moderation
* [**Trending**](src/Agni.Social/Trending) - special-purpose OLAP engine [harvesting](src/Agni.Social/Trending/SocialTrendingGauge.cs), [storing](src/Agni.Social/Trending/ITrendingSystem.cs#L27) and [querying data](src/Agni.Social/Trending/ITrendingSystem.cs#L39) in real time, "top prodyucts in xyz category in the past week/month/year" etc.



See [Agnicore Documentation Site](http://agnicore.com/products/agnios/book/introduction.htm) for more details

## Runtime / Platform

Agni is built on [NFX](https://github.com/agnicore/nfx); the whole stack is based on **.NET Standard** and runs on **.NET Framework 4.7+** and **.NET Core 2+** runtimes.

Agni support on platforms is the same as for its base core library [NFX](https://github.com/agnicore/nfx) - **.NET Standard 2+** which works on different runtimes. Officially we support .NET Core and .NET Framework:

* .NET Standard 2 - supported
* .NET Core 2 - supported
* .NET Framework 4.7.1 - supports classic .NET Framework 4.7.1+

NFX supports cross-platform development and is tested on:

* Windows Win 2010 Core 2 and Net Fx
* Linux Ubuntu 16 LTS using Core 2
* Mac OS 14 using Core 2

NFX Builds on:

* Windows / MSBuild 15 / VS 2017
* in process - *nix (need scripts and build process refinements)







