# Agni OS Overview

Back to [Documentation Index](./)

Why is Agni Cluster is called an "OS"? Our goal is to make distributed programming available to any application type. When you write a software that gets installed locally, you don’t really care about inter-machine problems. The inter-process/thread/file APIs are provided within the machine to you by "regular OS", and you don’t need to think twice. 

Agni OS extends this simple "OS" approach to writing distributed programs. Those programs are distributed only because a single server can not serve that many users and can crash and burn because of hardware failures. Why not create an Operating System for running distributed programs? Provide APIs to devs so that apps can now logically scale on the architecture level - this is exactly what Agni OS does. 

## What is Agni OS? 

* A platform /OS for building large systems, systems that may get very large
* Unify the concept of distributed "cluster" for any application type: web, db, service, etc.
* Start dev on local PC - scale out to 1000s of nodes. Built for unlimited sizes and data centers
* It is just a library/framework - a set of a few .NET assemblies under 8mb total size
* Written in Plain C# + some js, css,less for UI
* Free from "heavy" dependencies, such as : AD, IIS, DataCenter SQL etc.
* Uses only the very base stuff: CLR(jit, gc), BCL: Type, List<>,Dictionary<>, Thread, Task, Socket,Interlocked, + primitives (int, string, decimal etc..)
* UNISTACK&reg; approach to building distributed apps: from serialization, logging, to web MVC, APIs, REST and security - done the same way in any app 

## Structure
The first question that needs to be answered - how to organize data, metadata being the first data type that we need to build a massively scalable system. A proper structuring of metadata is needed. 

A hierarchy is a natural multi-level organization pattern self-evident in nature. It has been used in computer science since its dawn. The tree-like structures usually yield O(log(n)) search complexities. Hierarchies are easy to understand, the management nature of "supervisors" - or "nodes that are above me" are used extensively in Actor-model systems like Erlang. 

Single-level horizontal scalability is limited because at some point you will have so many nodes that will require too many managers. Flat designs are limited. 

There are other patterns, like grid which are suitable for some tasks (i.e. peer-to-peer sharing), however hierarchy can be thought of as an index (i.e. BTree) overimposed on top of a grid. This way parts of the grid may be managed/addressed more efficiently. In other words, hierarchical cluster topology does not impose a hierarchical restriction on data flow or functionality when it is not desired, but practically in very many applications we see the opposite - the "large problem" gets mapped and then reduced by subordinate workers. 

To summarize, hierarchy is a good choice because: 

* Logical organization of a large system
* Geo/physical layout of large system: regions/data centers/zones
* Chain of command/supervision/governor pattern
* Natural MAP:REDUCE for many functions, i.e. telemetry/instrumentation data
* Update Deployment and general management 

There is one concern that needs to be discussed right away. It may seem that in a hierarchy, a node at the higher level poses a single point of failure risk to its subordinate nodes. If one cuts the tree at the trunk - the whole tree falls down. So the higher-level nodes aggregate the features of the lower-level nodes, and that includes the failures. In reality, the higher-level nodes do not need to be a single point of failure as we can make a secondary and tertiary backup copies of the layer, for example we may have 2 or more zone controllers sitting on the zone governor level. The details of the failure modes are described in the next sections.

## Topology
Agni OS organizes geographical and logical topology of the system as a hierarchical tree of sections that start at the world level, sub-split by regions, the structure akin to a file system: 

<img src="/doc/img/cluster-map.svg">

Regions have sub-regions. They also contain Network Operation Centers (NOCs): 

<img src="/doc/img/cluster-noc.svg">
 NOCs contain zones which can also have sub-zones and hosts (actual servers).

The following typical Agni OS topology is used as an example:

<img src="/doc/img/cluster-topology.svg">

In this example, the "World.r" is the root region that contains two sub-regions: "US.r" and "EU.r" and global governor NOC "Glob.noc".

Every NOC contains zones which contains sub-zones and/or hosts. Zones provide logical grouping of hosts - for example, in the example above, the "DB.z" has sub-zones representing areas of a database handling User, Financial and Social data stores.

The red boxes represent zone governor processes that control the underlying hosts. It is not required to have a zone gov in every zone. 

## Topology Navigation

Since cluster topology is hierarchical, and a kin to a file system structure, it makes sense to address particular resources in the system using a logical path.

Agni OS does not use physical addresses or DNS names for host identification, because these are lower-level network related things. Instead, logical paths are used to traverse the hierarchy and address particular resources. Notice that section name suffixes (i.e. "*.r","*.z" etc.) may be left out.

The metabase API (discussed below) provide the detailed coverage for navigation and working with logical cluster regional paths (i.e. get the parent, NOC, compare paths, add paths etc.)(see Metabase API Reference)

The following diagram illustrates the logical naming scheme: 

<img src="/doc/img/cluster-topology-nav.svg">

Logical naming is a higher-level abstraction than DNS, it allows to use unified scheme of addressing nodes of the system not only for the purposes of communication between nodes but also locating entities in the whole system tree, regardless of actual means of communication used.

Read more on:

 [The Metabase](/mbase)

 [Application Configuration](/src/Agni/AppModel#metabase-application-container-configuration)


Back to [Documentation Index](./)