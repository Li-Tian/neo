# NEO C# SDK 2.9.1.0

Here is the NEO C# SDK 2.9.1.0. A brief description of a namespace is given below.

 - **Neo.*** : The underlying data structure used by Neo.
 - **Neo.Cryptography.*** : A tool set for the cryptographic algorithm used by Neo.
 - **Neo.IO.Data.LevelDB.*** : The C# wrapper interface for LevelDB used by Neo.
 - **Neo.IO.Data.Json.*** : The C# interface of JSON used by Neo.
 - **Neo.Network.P2P.*** : Peer-to-peer network.
 - **Neo.Network.P2P.Payloads.*** : Data structure for peer-to-peer network transmission.
 - **Neo.Plugins.*** : Interface definition for plugin.
 - **Neo.Wallets.*** : Interface definition for wallet.
 - **Neo.Wallets.NEP6.*** : The implementation of the NEP6 wallet.
 - **Neo.Wallets.SQLite.*** : The implementation of the SQLite wallet.

# How should I use this document?

 - If you are learning NEO's source code, this document can help you understand some of the details.
 - If you are building an NEO-related software system, this document can help you deepen your understanding, and then you can modify the source code to achieve the functionality you need.

> [!IMPORTANT]
> If you want to use Neo.dll directly, you need to pay special **attention to version**. NEO is still in the process of iterative process. The NEO team may reconstruct part of the system because they consider implementing certain functions or improving performance. Therefore, some interfaces may change and are not upward compatible. But we will try to provide the corresponding documentation for each major change.