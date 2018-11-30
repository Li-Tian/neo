<center><h2>Neo Blockchain Model</h2></center>

&emsp;&emsp;Asset is the core of Neo system. Transaction, contract, account and walet serves asset registration, flowing & administration. Neo CLI, compiler & virtual machine, etc, are technical means for function realization. Their function is as follows:

[![neo system](../../images/blockchain/system1.jpg)](../../images/blockchain/system1.jpg)

&emsp;&emsp;All operations are accomplished by transaction in Neo blockchain network. Assets can be transferred between different accounts through `ContractTransaction`. Users can also extract deserved GAS through `ClaimTransaction`. Contracts are also invoked by `InvocationTransaction`.

&emsp;&emsp;In Bitcoin, Script is responsible for transaction signature verification. Contracts take corresponding responsibility in NEO. Contract can be simply regarded as an upgrade of Bitcoin Script. Bitcoin Script is not Turing complete: its functionality is limited even though able to complete transaction signature verification. Bitcoin has only UTXO model and concerns only transaction itself. Writing language for Neo smart contract like C# and Python, are all Turing-complete, and can satify varies needs in real world. In case of accounting method, Bitcoin uses UTXO model; Ethereum uses a widely-used model, account balance, or balance model. In Neo system, UTXO model and account balance model both exist. UTXO model is mainly used for global assets, while account balance model is mainly used for user-published NEP-5 assets like stock, token, etc.

&emsp;&emsp;In Neo system, assets exist in the token's form. There are many kinds of assets like NEO, GAS and NEP-5. There are also equity assets like stock. All assets, including NEP-5 assets, and corresponding transaction information of a specifed address can be shown in the following steps: open a wallet in Neo Tracker (https://neotracker.io/), and choose one `address`. Address is also called "account". An address can be computed from specified private key with encryption algotithm and code switching, and used directly in transaction settlement. It can also be smart contract address used during smart contract execution. Method computing address please refer to `Address` section in `Wallet` chapter.

> [!NOTE]
> In case of dead link, please contact <feedback@neo.org>

