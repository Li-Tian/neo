## 认识 neo-cli

neo 是一个基于点对点网络的区块链系统。它提供基于 UTXO 模型的数字资产记账功能，以及一个基于 neo 虚拟机的智能合约的执行环境。
这里描述这个网络中节点程序 neo-cli 的整体结构和基本行为。

## 整体结构
neo 的整体结构如下图。（由于版本升级，部分结构可能会有变化。）

![neo-cli structure](../images/neo_cli_structure/neo-cli.png)

#### neo-cli命令行
neo-cli 是一个命令行程序。通过命令行控制台提供与区块链交互的基本功能。可以通过下述链接找到 neo-cli 的命令的详细说明。

<http://docs.neo.org/en-us/node/cli/cli.html>

(如果发现有死链接，请联系 <feedback@neo.org>)

#### 账本 API


#### 用户钱包


#### NEP-6钱包

#### LevelDBBlockchain

#### ApplicationLog

#### LocalNode

#### RpcServer

#### ConsensusService

#### Plugin

#### NeoVM

#### ApplicationEngine



## 介绍配置文件



## 启动的基本过程



