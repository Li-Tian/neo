# Reading instructions

NEO's goal is to build an economic ecosystem based on smart contract. The positioning of this Yellow Paper is to hope to give a specification of the NEO network for technicians. After starting the work of the Yellow Paper, I found it is more challenging than I had expected. This system contains too many information, and due to the limited ability, it is difficult to cover all the details in a limited paragraph. Therefore, we tried to describe the technical characteristics of NEO from different angles as much as possible. We hope that readers can grasp the overall structure of NEO system from scattered technical details.

# Targeted Reader

This is a technical document. The targeted readers are mainly non-blockchain technicians, or other blockchain technicians. After reading this document, I hope you can understand the technical details of NEO and you can quickly participate in the construction of NEO's economic ecosystem. By the way, the C# version of the NEO SDK [API Documentation](../api/index.md) is also provided for easy reference.

If you are not a technician, you can read [NEO's White Paper](http://docs.neo.org/en-us/whitepaper.html) and then come back to read this document. Technical details can be skipped. It will be good to get the design concepts of NEO networks.

# Other instructions

This document is mainly used to describe the design of NEO network nodes, but does not cover the list of command-line functions and JSON-RPC functions of neo-cli, and the usage of neo-gui. If you need to understand these parts, you can refer to the links below.

 - [NEO-CLI Command Reference](http://docs.neo.org/en-us/node/cli/cli.html)<BR>
 - [NEO JSON-RPC API Reference](http://docs.neo.org/en-us/node/cli/2.9.0/api.html)<BR>
 - [NEO-GUI](http://docs.neo.org/en-us/node/gui/install.html)<BR>

> [!NOTE]
> - The documentation is constantly being updated. If you find errors or omissions, problems with links 404, or you have suggestions for improvement, or you are willing to translate it into other languages, please write to <feedback@neo.org>.
> - The Yellow Paper is based on NEO 2.9.2.0. Although we intended to make it less dependent on a specific programming language, there is still a small amount of C# language habits referenced in type definitions and so on.
> - The API documentation is the C# programming interface documentation for NEO 2.9.3.0.
