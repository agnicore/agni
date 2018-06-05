# The Metabase

Agni OS makes an extensive use of metadata (data about data) to describe its’ instance. An instance
is a distinct installation of Agni OS on a set of physical and/or virtual resources. One may operate
an instance from many different data centers running various applications. The logical decision about 
the instance composition is dictated by a particular business solution.

There are many various factors to consider and configure in a distributed system. We took an approach
akin to reflection - a set of data about the system with APIs that can programmatically extract that
data for various OS functions. This concept is called "Metabase" - a special kind of database that 
stores metadata about the Agni OS system. 

Metabase is a hierarchical data structure that gets mounted by every Agni OS process via a **Virtual File System**
(VFS) access. It can be depicted as a structured system of config files which describe various system
 entities. In code, metabase is represented by the `Agni.Metabase.Metabank` class. Application developers
 never create an instance of `Metabank`, as this is done by the `BootConfLoader` as a part of `IAgniApplication`
 container setup. The VFS is a software-mode component that abstracts the access to files on various
 PaaS and physical layers (i.e. SVN, Amazon S3, Google Drive etc.). 
 The VFS usually supports version control, so any changes to the whole metabase are version-controlled.
 This is beneficial for keeping track of releases and rolling-back changes in case of emergencies, so
 all changes in the cluster setup, packages and configuration are always available.