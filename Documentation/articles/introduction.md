# 阅读说明

　NEO 的目标是构建一个基于智能合约的经济生态系统。而开发指南的定位是希望能给这个经济生态系统的主干 NEO 网络整理出面向技术人员的协议。开始开发指南的工作以后发现这项工作比想象的要更加困难。 NEO 这个系统覆盖的细节太多，由于水平有限，很难在有限的篇幅描述到所有细节。因此，尽量更多地从不同的角度描述 NEO 的技术特点，希望读者能从零散的技术细节掌握 NEO 系统的整体构架。

# 面向的读者

　这是一篇技术文档，面向的读者主要是非区块链方向的技术人员，或者其它区块链的技术人员，通过阅读本文档以后，能够理解掌握 NEO 的技术细节，从而可以快速的参与 NEO 的经济生态系统的建设。同时还提供了 NEO SDK 的 C# 版 [API 文档](../api/index.md)，以方便查阅。

　如果你不是技术人员，可以先阅读 [NEO的白皮书](http://docs.neo.org/en-us/whitepaper.html)，然后再回来阅读本文档，遇到技术性的细节可以跳过，希望也能理解一些 NEO 网络的设计理念。

# 其它说明

　本文档主要用来描述 NEO 网络节点的设计构造，但是没有涵盖 neo-cli 的命令行功能列表和 JSON-RPC 功能列表，以及 neo-gui 的用法。如果需要了解这些部分，可以参考下述链接。

 * [NEO-CLI 的命令参考](http://docs.neo.org/en-us/node/cli/cli.html)
 * [NEO JSON-RPC API 参考](http://docs.neo.org/en-us/node/cli/2.9.0/api.html)
 * [NEO-GUI](http://docs.neo.org/en-us/node/gui/install.html)

> [!NOTE]
> * 文档还在不断的更新中，如果发现有错误或遗漏，或者有改善的建议，发现链接404等问题，或者有意向合作翻译成其它语言，请写信到 <feedback@neo.org>。
> * 特别感谢 Robert Kofler, Johann Loy 和 Marcin Behrens 帮助我们指出了文档中的一处错误。
> * 开发指南基于 NEO 2.9.3 写成。虽然希望写成不依赖于特定的编程语言，但是在类型定义等处还是少量引用了 C# 的语言习惯。
> * API 文档是 NEO 2.9.3 C# 的编程接口文档。
