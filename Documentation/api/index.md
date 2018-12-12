# NEO C# SDK 2.9.3.0

这里是 NEO C# SDK 2.9.3.0。下面给出一个 namespace 的简单描述。

 * **Neo.*** : Neo 所使用的基础数据结构。
 * **Neo.Cryptography.*** : Neo 所使用的密码算法的工具。
 * **Neo.IO.Data.LevelDB.*** : Neo 所使用的LevelDB 的 C# 封装接口。
 * **Neo.IO.Data.Json.*** : Neo 所使用的 JSON 的 C# 接口。
 * **Neo.Network.P2P.*** : 点对点网络。
 * **Neo.Network.P2P.Payloads.*** : 点对点网络传输的数据结构。
 * **Neo.Plugins.*** : 插件的接口定义。
 * **Neo.Wallets.*** : 钱包的接口定义。
 * **Neo.Wallets.NEP6.*** : NEP6钱包的实现。
 * **Neo.Wallets.SQLite.*** : SQLite钱包的实现。

# 应该如何使用这份文档

 * 如果你在学习 NEO 的源代码，这份文档可以帮助你理解一些细节。
 * 如果你在建设 NEO 相关的软件系统，这份文档可以帮助你加深理解，然后你可以修改源代码来实现你所需要的功能。

> [!IMPORTANT]
> 如果你想直接使用 Neo.dll，你需要特别 **注意版本**。NEO还在不断的迭代过程中，NEO团队可能会因为考虑实现某些功能或者提升性能而重构部分系统，因此部分接口有可能会发生改变，而不再向上兼容。但是我们会尽可能为每个重大改变提供对应的文档。
