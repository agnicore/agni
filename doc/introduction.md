# Introduction

Back to [Documentation Index](./)


## Audience

The target audience of this documentation is anyone who is interested in distributed applications. Although we concentrate on our implementation called Agni Cluster OS, the general principles and techniques described in here are really applicable to other systems as well.

The reader is expected to be acquainted with general programming concepts, OOD (object-oriented design) and must have general understanding of services (i.e. SOAP, REST etc.). The knowledge of C# is beneficial but not required, as Java developers can as-easily grasp the concept as .NET ones.

## Focus

We hear many buzz-words and technology names daily: "Kafka", "Akka", "Hadoop", "Zookeeper", "Google Spanner" etc.. What do all of these names have in common? - the distributed architecture that makes them "hot" - the ability to process so much data that a single computer, no matter how fast, can never accomplish on its own.

We have selected .NET, wait, **not that .NET** that you think about in terms of <s>IIS/AD and Fortune 500 companies (J2EE anyone?)</s>, but modern cloud-ready *nix-friendly system. Our writing is going to concentrate around C#, but one can use any CLR language.

We are going to focus on the way to the infinite scalability on all tiers of the system: your mind, architecture, design, code, support, etc. One can scale hardware (i.e. IaaS) but if the app is not scalable - alas! - the solution is not really as "hot" as advertised.

## Why?
 
Why did we write this?
Let's start with a trite, yet very important topic: the customers of my business do not care about how I get our services provided to them. It is my problem. They don't care about technology and tools that I use (unless I decide to become a software technology company). And here comes the problem - **fragmentation and too much choice (which is sometime equivalent to having no choice at all)** for me, as a business owner. Honestly, all I need is a good stable solution that I don’t have to pour millions into every year just to update 3rd party libs to keep the thing afloat.

Matters get worse at scale. **High-scalability** adds additional dimension of complexity. The developers are harder to find, the various PaaS offerings lock-in for years. The architecture has not 100% crystallized - there are microservices, REST (but someone argues for RPC), and now **serverless computing** altogether!

The cost of distributed cloud systems development has skyrocketed. Many companies decided to use simple languages like PHP and Ruby that facilitate quick Web site construction but are not suitable for high-computational loads facing big problems down the road (e.g. PHP@Facebook). To this day there is no unified way for organizing/operating clusters of server nodes which would have been as standardized as C APIs for IO tasks.

We believe that the construction of distributed systems should be easier and we think that currently there is no set application-level approach to building such systems. Many great technologies have emerged - primarily **targeting only some aspect** of what we (as a business) need to accomplish, yet **many projects repeat the most tedious and cumbersome parts over and over**.

All of the popular distributed projects (Akka, Kafka, Spark etc.) have a concept of a "cluster" - to spread the workload or storage load or both. The problem is, as we see it: **every project implements it in its own "private" way**. For example, can a web server farm be hosted by Hadoop Yarn? Or maybe Zookeeper? Probably it could, but neither Yarn, no Zookeeper were meant to be used this way. What we posit with Agni OS is that the **concept of "clustering"/distributed nodes must be provided to ANY kind of application** which runs on it, just like a regular OS provides processes/threads to ANY kind of application that can run on top.

We chose .NET, or more specifically CLR(Common Language Runtime) for **its performance** being close if not equal (when used properly) to native languages like C++, and the **high-level of abstraction** along with **rich reflection/metadata** which is very important for making run-time decisions/services. The abundance of **developers who understand C#** and simple base class library concepts like logging, collections and DB access really cuts the cost. Microsoft is moving very aggressively towards making .NET as a whole a first-class citizen of a large scale world, a market that was dominated by Java/C++/Erlang /NodeJs technologies.

Read More on:

[Why Companies face Problems 2 Years Later](companies-not-designed.md)

## Our Philosophy + Unistack 

We want to reduce complexity but without feature loss. The way to achieve this effect is this - reduce the number of standards and repetition used in the system. The less standards we need to support/keep in mind/remember - the simpler the whole system becomes. For example: instead of supporting 4 different logging frameworks where this one uses one kind of configuration and that one uses a completely different set of configuration - we use just one. Once developers read logging tutorial they can easily understand how logging works in any tier of the whole system.

UNI-STACK = a Unifed Stack of software to get your job done. Use one library instead of 10s of 3rd parties that all increase complexity. In Unistack everything is named, configured, and operates the same way, thus reducing complexity 10-fold. Unistack was purposely coded to facilitate distributed apps creation, yet (unlike many PaaS) it can be used to write basic client-server apps that all run on one machine without any bloat. 


Concern|Remidiation
--|--
Transaction processing|share nothing, scale horizontally
Configuration/management |share everything or as much as possible
Unify|patterns, languages, components
Avoid|3rd parties as much as possible - direct and transitive dependencies
Watch For|<ul> <li> Edge cases - indicate that something is wrong (i.e. many "IF"s)</li><li>Coupling/Monolith</li><li>Appearance of 3rd party - must be justified</li><li>Introduction of new compilers/languages (need to justify the purpose)</li> </ul>
Priorities|<ol><li>Reuse</li><li>Build using Agni</li><li>Use open source</li><li>Buy proprietary</li><ol>

## Points To Consider 

There are a few important points that are usually overlooked.

Why do we need distributed applications at all? If computers were infinitely scalable then would that have not sufficed for serving millions of users from a single box? It probably would, and the app dev paradigm could have stayed in the "client/server" age. But computational power of single units is finite, therefore we need to devise **new ways of writing applications - that are architected in order to scale on many computers**.

Now, let’s talk about IaaS. Many think that they are "in the cloud" because they install their XYZ software on AWS or Azure. While that may qualify you for "cloud", **the software alone is not scalable if it is not architected/implemented for scalability**. The IaaS does not scale (e.g.  MS DOS programs) by itself, although you could spawn in on 100s of EC instances. This is not how Google is written. It is a time to **scale your application architecture** and then **IaaS will naturally support it**.

Consider PaaS - it does make a particular service of interest scalable, but only that part of the service which is offered as a PaaS (for example a SQL database). An application, as a whole, is still not scalable completely if only some of its pieces are. Another problem is coupling. Once you start using some PaaS feature, you are **now dependent on this service**, and if you have **not provided an abstraction** - then you will not be able to replace that service provider at all. So the apps that rely on various PaaS offerings are better to be architected properly to avoid hard-coupling (even logically) with particular provider. Of course this may not be a problem if you purposely want to go with, say Amazon, Google or Microsoft forever. 




Back to [Documentation Index](./)