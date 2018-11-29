<center><h2> Smart Contract </h2> </center>

&emsp;&emsp;A smart contract is a set of commitments that are defined in digital form, including the agreement on how contract participants shall fulfill these commitments. Blockchain technology gives us a decentralized, non-tampering, highly reliable system in which smart contracts are extremely useful. Smart contracts is one of the most important characteristics of blockchain technologies and the reason why blockchains can be called disruptive technology. It is increasing the efficiency of our social structure by each passing day.


# Restrictive Condition

The basic type limitation of the smart contract can be referred to the link: <http://docs.neo.org/zh-cn/sc/quickstart/limitation.html>


Meanwhile, due to security considerations, to prevent different nodes from obtaining different data from the outside, NEO does not support the smart contract access the Internet and the other blockchain data currently.

# Price Mechanism

Each instruction of the smart contract needs to pay fee (Gas). In the procedure of deploying or invokeing the smart contract, the NEO-GUI will try to run the smart contract's bytecodes in test mode, and calculate the gas consumed. 

The initial 10 GAS during each execution of every smart contract is always free, including smart contract deployment and invoking. That is, fees that sum up to 10 GAS or less will not require a service fee.

All Smart Contract fees are considered as Service fee to be put in a pool for re-distribution to all NEO holders. The distribution is proportional to amount of NEO.


## Fees for Instructions

| Instruction                           | Fee(Gas) |
| -------------------------------- | ----------- |
| OpCode.PUSH16 [or less]          | 0           |
| OpCode.NOP                       | 0           |
| OpCode.APPCALL                   | 0.01        |
| OpCode.TAILCAL                   | 0.01        |
| OpCode.SHA1                      | 0.01        |
| OpCode.SHA256                    | 0.01        |
| OpCode.HASH160                   | 0.02        |
| OpCode.HASH256                   | 0.02        |
| OpCode.CHECKSIG                  | 0.1         |
| OpCode.CHECKMULTISIG（per public-key） | 0.1         |
| (Default)         | 0.001       |

## Fees for System Calls

| SysCall                                | Fee(Gas) |
| ------------------------------------------- | ------------ |
| Runtime.CheckWitness                        | 0.2          |
| Blockchain.GetHeader                        | 0.1          |
| Blockchain.GetBlock                         | 0.2          |
| Blockchain.GetTransaction                   | 0.1          |
| Blockchain.GetTransactionHeight             | 0.1          |
| Blockchain.GetAccount                       | 0.1          |
| Blockchain.GetValidators                    | 0.2          |
| Blockchain.GetAsset                         | 0.1          |
| Blockchain.GetContract                      | 0.1          |
| Transaction.GetReferences                   | 0.2          |
| Transaction.GetUnspentCoins                 | 0.2          |
| Transaction.GetWitnesses                    | 0.2          |
| Witness.GetVerificationScript               | 0.1          |
| Account.IsStandard                          | 0.1          |
| Asset.Create(system asset)               | 5000         |
| Asset.Renew(system asset) [per year]     | 5000         |
| Contract.Create*                            | 100~1000     |
| Contract.Migrate*                           | 100~1000     |
| Storage.Get                                 | 0.1          |
| Storage.Put, Storage.PutEx [per KB]       | 1            |
| Storage.Delete                              | 0.1          |
| (Default)                                  | 0.001        |

* The cost of creating or migrating a smart contract is the basic 100 GAS plus fees of functions the contract requires. If the storage area is required, the function fee is 400 GAS, and if the dynamic call is needed, the function fee is 500 GAS.

* When deploy the contract which requires storage, dynamic invocation, etc., be sure to check the corresponding options. In the future, we will consider detection mechanism.

# Trigger

A smart contract trigger is a mechanism that triggers the execution of smart contracts. There are four triggers introduced in the NEO smart contract, `Verification`, `Application`, `VerificationR` and `ApplicationR`, the `VerificationR` and `ApplicationR` are added in version 2.9.

A blockchain that provides smart contract system should provide multiple triggers for the smart contracts running on it, makes them to function in different contexts.

`Verification` and `Application` enable smart contract to verify transiaction and change the state of the blockchain.

`VerificationR` and `ApplicationR` enable smart contract to reject a transfer or change the state of the blockchain when a transfer received.

For more information, please read: <http://docs.neo.org/zh-cn/sc/trigger.html>


## VerificationR

The purpose of the `VerificationR` trigger is to call the contract as a verification function, which accepts no parameter, and should return a valid Boolean value, indicating the validity of the transaction or block, as it is specified as a target of an output of the transaction. 

If the smart contract is triggered by `VerificationR`, the smart contract entry point will be invoked:

```c#
main("receiving", new object[0])
```

The `receiving` function should have the following signature:

```c#
public bool receiving()
```

the `receiving` function will be invoked automatically when a contract is receiving assets from a transfer.

## ApplicationR

The `ApplicationR` trigger indicates that the default function `received` of the contract is being invoked because it is specified as a target of an output of the transaction. The `received` function accepts no parameter, changes the states of the blockchain, and returns any type of value.

The entry point of the contract will be invoked if the contract is triggered by `ApplicationR`:

```c#
main("received", new object[0])
```

The `received` function should have the following signature:

```c#
public byte[] received()
```

The `received` function will be invoked automatically when a contract is receiving assets from a transfer.


# Interoperable service layer


The interoperable service layer provides some APIs for accessing the blockchain data of the smart contract. It can access block information, transaction information, contract information, asset information, and so on.

## Runtime
### System.Runtime.GetTrigger

| old api：  | "Neo.Runtime.GetTrigger"   |
|------------|------------------------------------------------------------------|
| Binding Method: | Runtime_GetTrigger                             |
| Function Description: | Get trigger of the smart contract(Application or Verification) |
| C\# Method：  | TriggerType Trigger;     |
| Remark:     |  Verification = 0x00, Application = 0x10 |

### System.Runtime.CheckWitness

| old api：  | "Neo.Runtime.CheckWitness", "AntShares.Runtime.CheckWitness"     |
|------------|---------------------------------------------|
| Binding Method: | Runtime_CheckWitness   |
|  Function Description: | Verify the transaction/Whether the block contains the verification script's hash   |
| C\# Method：  | bool CheckWitness(byte[] hashOrPubkey);      |
| Remark:     | Check whether the transaction's verification scripts contain the hash.  |

### System.Runtime.Notify

| old api：  | "Neo.Runtime.Notify", "AntShares.Runtime.Notify"       |
|------------|----------------------------------------------------------------------|
| Binding Method: | Runtime_Notify                    |
|  Function Description: |  Send notification to the client that execute the smart contract.   |
| C\# Method：  | void Notify(params object[] state)        |
| Remark:     | Create a notifcation with the `EvaluationStack` state.</br>Trigger a notify event and add the notification into the `notifications` array. |

### System.Runtime.Log

| old api：  | "Neo.Runtime.Log"， "AntShares.Runtime.Log"       |
|------------|---------------------------------------------------|
| Binding Method: | Runtime_Log                                       |
|  Function Description: | Send a log to the client executing the smart contract  |
| C\# Method：  | void Log(string message)                          |
| Remark:     | Get the message from the `EvaluationStack` and trigger `log` event to the client with the message. |

### System.Runtime.GetTime

| old api：  | "Neo.Runtime.GetTime"                                  |
|------------|--------------------------------------------------------|
| Binding Method: | Runtime_GetTime                                    |
|  Function Description: | Get the current time                  |
| C\# Method：  | uint Time                                         |
| Remark:     |  Get block header by the current block height. The current time is the current block's timestamp plus the `block time`, which default is 15 seconds.  |

### System.Runtime.Serialize

| old api：  | "Neo.Runtime.Serialize"                                |
|------------|---------------------------------------------------------|
| Binding Method: | Runtime_Serialize                                  |
|  Function Description: | Serialize the object                        |
| C\# Method：  | byte[] Serialize(this object source)                                                      |


### System.Runtime.Deserialize**

| old api：  | "Neo.Runtime.Deserialize"                               |
|------------|---------------------------------------------------------|
| Binding Method: | Runtime_Deserialize                                |
|  Function Description: | Deserialize data from the source            |
| C\# Method：  | object Deserialize(this byte[] source)               |

## Blockchain

### System.Blockchain.GetHeight

| old api：  | "Neo.Blockchain.GetHeight"， "AntShares.Blockchain.GetHeight"     |
|------------|-------------------------------------------------------------------|
| Binding Method: | Blockchain_GetHeight                                         |
|  Function Description: | Get the current block height                          |
| C\# Method：  | uint GetHeight()                                                |

### System.Blockchain. GetHeader

| old api：  | " Neo.Blockchain.GetHeader"， " AntShares.Blockchain.GetHeader"   |
|------------|-------------------------------------------------------------------|
| Binding Method: | Blockchain_GetHeader                                         |
| Function Description: | Get block header by block height or block hash           |
| C\# Method：  | Header GetHeader(uint height); </br> Header GetHeader(byte[] hash)  |

### System.Blockchain.GetBlock

| old api：  | "Neo.Blockchain.GetBlock"， "AntShares.Blockchain.GetBlock"      |
|------------|------------------------------------------------------------------|
| Binding Method: | Blockchain_GetBlock                                         |
| Function Description: | Get block by block height or block hash               |
| C\# Method：  | Block GetBlock(uint height);</br> Block GetBlock(byte[] hash)  |

### System.Blockchain.GetTransaction

| old api：  | "Neo.Blockchain.GetTransaction"， "AntShares.Blockchain.GetTransaction"  |
|------------|--------------------------------------------------------------------------|
| Binding Method: | Blockchain_GetTransaction                                           |
| Function Description: | Get transaction by txid                                       |
| C\# Method：  | Transaction GetTransaction(byte[] hash)                               |


### System.Blockchain.GetTransactionHeight

| old api：  | "Neo.Blockchain.GetTransactionHeight"                       |
|------------|-------------------------------------------------------------|
| Binding Method: | Blockchain_GetTransactionHeight                        |
| Function Description: | Get transaction height by txid                   |

### System.Blockchain.GetContract

| old api：  | "Neo.Blockchain.GetContract", "AntShares.Blockchain.GetContract"    |
|------------|---------------------------------------------------------------------|
| Binding Method: | Blockchain_GetContract                                         |
| Function Description: | Get contract by script hash                              |
| C\# Method：  | Contract GetContract(byte[] script_hash)                         |

### Neo.Blockchain.GetAccount

| old api：  | "AntShares.Blockchain.GetAccount"                    |
|------------|------------------------------------------------------|
| Binding Method: | Blockchain_GetAccount                           |
|  Function Description: | Get account by address script hash       |
| C\# Method：  | Account GetAccount(byte[] script_hash)            |
| Remark:     |  If the account is not exist, create a new AccountState with this script_hash, and add it into the accounts array. Finally, return the account.  |

### Neo.Blockchain.GetValidators

| old api：  | "AntShares.Blockchain.GetValidators"        |
|------------|---------------------------------------------|
| Binding Method: | Blockchain_GetValidators               |
|  Function Description: | Get validators                  |
| C\# Method：  | byte[][] GetValidators()                 |

### Neo.Blockchain.GetAsset

| old api：  | "AntShares.Blockchain.GetAsset"             |
|------------|---------------------------------------------|
| Binding Method: | Blockchain_GetAsset                    |
|  Function Description: | Get asset by id                 |
| C\# Method：  | Asset GetAsset(byte[] asset_id)          |



## Header

### System.Header.GetIndex

| old api：  | "Neo.Header.GetIndex"                      |
|------------|--------------------------------------------|
| Binding Method: | Header_GetIndex                       |
| Function Description: | Get current block index         |
| C\# Method：  | uint Index                              |

### System.Header.GetHash

| old api：  | "Neo.Header.GetHash"， "AntShares.Header.GetHash"     |
|------------|-------------------------------------------------------|
| Binding Method: | Header_GetHash                                   |
|  Function Description: | Get block hash                            |
| C\# Method：  | byte[] Hash                                        |


### System.Header.GetPrevHash

| old api：  | "Neo.Header.GetPrevHash"， "AntShares.Header.GetPrevHash"     |
|------------|---------------------------------------------------------------|
| Binding Method: | Header_GetPrevHash                                       |
|  Function Description: | Get the previous block hash                       |
| C\# Method：  | byte[] PrevHash                                            |

### System.Header.GetTimestamp

| old api：  | "Neo.Header.GetTimestamp"，"AntShares.Header.GetTimestamp"   |
|------------|--------------------------------------------------------------|
| Binding Method: | Header_GetTimestamp                                     |
|  Function Description: | Get the block timestamp                          |
| C\# Method：  | uint Timestamp                                                                            |

### Neo.Header.GetVersion

| old api：  | "AntShares.Header.GetVersion"            |
|------------|------------------------------------------|
| Binding Method: | Header_GetVersion                   |
|  Function Description: | Get the block version        |
| C\# Method：  | uint Version                          |

### Neo.Header.GetMerkleRoot

| old api：  | "AntShares.Header.GetMerkleRoot"           |
|------------|--------------------------------------------|
| Binding Method: | Header_GetMerkleRoot                  |
| Function Description: | Get the merkle tree root        |
| C\# Method：  | byte[] MerkleRoot                       |

### Neo.Header.GetConsensusData

| old api：  | "AntShares.Header.GetConsensusData"         |
|------------|---------------------------------------------|
| Binding Method: | Header_GetConsensusData                |
| Function Description: | Get the ConsensusData            |

### Neo.Header.GetNextConsensus

| old api：  | "AntShares.Header.GetNextConsensus"          |
|------------|----------------------------------------------|
| Binding Method: | Header_GetNextConsensus                 |
|  Function Description: | Get the script hash of the next round validators' multi-signature contract  |
| C\# Method： | ulong ConsensusData                        |

## Block

### System.Block.GetTransactionCount

| old api：  | "Neo.Block.GetTransactionCount"，"AntShares.Block.GetTransactionCount" |
|------------|-----------------------------------------------------------------------|
| Binding Method: | Block_GetTransactionCount                                        |
| Function Description: | Get the number of the current block's transactions         |
| C\# Method：  | int GetTransactionCount()                                          |

### System.Block.GetTransactions

| old api：  | "Neo.Block.GetTransactions"                        |
|------------|----------------------------------------------------|
| Binding Method: | Block_GetTransactions                         |
| Function Description: | Get transactions of the current block   |
| C\# Method：  | Transaction[] GetTransactions()                 |

### System.Block.GetTransaction

| old api：  | "Neo.Block.GetTransaction"， "AntShares.Block.GetTransaction"  |
|------------|----------------------------------------------------------------|
| Binding Method: | Block_GetTransaction                                      |
| Function Description: | Get the specified index transaction in the current block  |
| C\# Method：  | Transaction GetTransaction(int index)                        |



## Transaction

### System.Transaction.GetHash

| old api：  | "Neo.Transaction.GetHash"，"AntShares.Transaction.GetHash"  |
|------------|-------------------------------------------------------------|
| Binding Method: | Transaction_GetHash                                    |
| Function Description: | Get the current transaction hash                 |
| C\# Method：  | byte[] Hash                                              |

### Neo.Transaction.GetType

| old api：  | "AntShares.Transaction.GetType"                            |
|------------|------------------------------------------------------------|
| Binding Method: | Transaction_GetType                                   |
|  Function Description: | Get type of the current transaction        |
| C\# Method：  | byte Type                                               |

### Neo.Transaction.GetAttributes

| old api：  | "AntShares.Transaction.GetAttributes"                       |
|------------|-------------------------------------------------------------|
| Binding Method: | Transaction_GetAttributes                              |
|  Function Description: | Get attributes of the current transaction   |
| C\# Method：  | TransactionAttribute[] GetAttributes()                   |


### Neo.Transaction.GetInputs

| old api：  | "AntShares.Transaction.GetInputs"                           |
|------------|-------------------------------------------------------------|
| Binding Method: | Transaction_GetInputs                                  |
|  Function Description: | Get all inputs of the current transaction   |
| C\# Method：  | TransactionInput[] GetInputs()                           |


### Neo.Transaction.GetOutputs

| old api：  | "AntShares.Transaction.GetOutputs"                           |
|------------|--------------------------------------------------------------|
| Binding Method: | Transaction_GetOutputs                                  |
| Function Description: | Get all outputs of the current transaction    |
| C\# Method：  | TransactionOutput[] GetOutputs()                          |

### Neo.Transaction.GetReferences

| old api：  | "AntShares.Transaction.GetReferences"                      |
|------------|------------------------------------------------------------|
| Binding Method: | Transaction_GetReferences                             |
| Function Description: | Get all outputs which the current transaction's inputs referenced |
| C\# Method：  | TransactionOutput[] GetReferences()                     |

### Neo.Transaction.GetUnspentCoins

| Binding Method: | Transaction_GetUnspentCoins                   |
|------------|----------------------------------------------------|
| Function Description: | Get all UTXOs of the current transaction |
| C\# Method：  | TransactionOutput[] GetUnspentCoins()           |

## Storage

### System.Storage.GetContext

| old api：  | "Neo.Storage.GetContext"， "AntShares.Storage.GetContext"  |
|------------|------------------------------------------------------------|
| Binding Method: | Storage_GetContext                                    |
| Function Description: |Get the current storage context                  |
| C\# Method：  | StorageContext CurrentContext                           |
| Remark:     | The StorageContext's `IsReadOnly` must be equal to false   |

### System.Storage.GetReadOnlyContext

| old api：  | "Neo.Storage.GetReadOnlyContext"                           |
|------------|------------------------------------------------------------|
| Binding Method: | Storage_GetReadOnlyContext                            |
| Function Description: | Get the current readonly storage context        |
| Remark:     | The StorageContext's  IsReadOnly equals true              |

### System.Storage.Get

| old api：  | "Neo.Storage.Get"， "AntShares.Storage.Get"   |
|------------|-----------------------------------------------|
| Binding Method: | Storage_Get                              |
|  Function Description: | Get value by key, stored in the private stroage   |
| C\# Method：  | byte[] Get(StorageContext context, byte[] key); </br> byte[] Get(StorageContext context, string key);                                                 |
| Remark:     | If not exist, return an empty byte array.       |

### System.Storage.Put

| old api：  | "Neo.Storage.Put"，"AntShares.Storage.Put"   |
|------------|----------------------------------------------|
| Binding Method: | Storage_Put                             |
| Function Description: | put key-value pair                |
| C\# Method：  | void Put(StorageContext context, byte[] key, byte[] value); </br>void Put(StorageContext context, byte[] key, BigInteger value); </br>void Put(StorageContext context, byte[] key, string value);</br> void Put(StorageContext context, string key, byte[] value); </br>void Put(StorageContext context, string key, BigInteger value); </br>void Put(StorageContext context, string key, string value); |
| Remark:     | If the context is readonly, return false;</br>If the contract is not exist or has no storage, return false;</br> The length of the `key` cannot more than 1024. |

### System.Storage.Delete

| old api：  | "Neo.Storage.Delete"， "AntShares.Storage.Delete"   |
|------------|-----------------------------------------------------|
| Binding Method: | Storage_Delete                                 |
| Function Description: |Delete the value by key in the private storage  |
| C\# Method：  | void Delete(StorageContext context, byte[] key); void Delete(StorageContext context, string key);                                                        |
| Remark:     | If the context is readonly, return false;</br>If the contract is not exist or has no storage, return false;                                             |

### Neo.Storage.Find

| Binding Method: | Storage_Find                                |
|------------|-------------------------------------------------------------------------------|
|  Function Description: | Get contents of the specified prefix in the current storage   |
| C\# Method：  | Iterator < byte[], byte[] \> Find(StorageContext context, byte[] prefix); </br> Iterator < string, byte[] \> Find(StorageContext context, string prefix);   |



## StorageContext

### System.StorageContext.AsReadOnly


| old api：  | "Neo.StorageContext.AsReadOnly"               |
|------------|-----------------------------------------------|
| Binding Method: | StorageContext_AsReadOnly                |
| Function Description: | Set the current storage readonly  |

## InvocationTransaction

### Neo.InvocationTransaction.GetScript

| Binding Method: | InvocationTransaction_GetScript           |
|------------|------------------------------------------------|
| Function Description: | Get script of the current contract  |
| C\# Method：  | byte[] Script                               |

## Attribute

### Neo.Attribute.GetUsage

| old api：  | "AntShares.Attribute.GetUsage"           |
|------------|------------------------------------------|
| Binding Method: | Attribute_GetUsage                  |
| Function Description: | Get usage of the transaction's attribute |
| C\# Method：  | byte Usage                             |

### Neo.Attribute.GetData

| old api：  | "AntShares.Attribute.GetData"          |
|------------|----------------------------------------|
| Binding Method: | Attribute_GetData                 |
|  Function Description: | Get data of the transaction's attribute |
| C\# Method：  | byte[] Data                         |


## Input

### Neo.Input.GetHash

| old api：  | "AntShares.Input.GetHash"              |
|------------|----------------------------------------|
| Binding Method: | Input_GetHash                     |
| Function Description: |  Get transaction's hash of the input referenced   |
| C\# Method：  | byte[] PrevHash                     |

### Neo.Input.GetIndex

| old api：  | "AntShares.Input.GetIndex"             |
|------------|----------------------------------------|
| Binding Method: | Input_GetIndex                    |
| Function Description: | Get the index of the output referenced by the current input   |
| C\# Method：  | ushort PrevIndex                    |

## Output

### Neo.Output.GetAssetId

| old api：  | "AntShares.Output.GetAssetId"               |
|------------|---------------------------------------------|
| Binding Method: | Output_GetAssetId                      |
| Function Description: |  Get `AssetId` of the transaction's output |
| C\# Method：  | byte[] AssetId                           |

### Neo.Output.GetValue

| old api：  | "AntShares.Output.GetValue"                  |
|------------|----------------------------------------------|
| Binding Method: | Output_GetValue                         |
| Function Description: | Get `Amount` of the transaction's output  |
| C\# Method：  | long Value                                |

### Neo.Output.GetScriptHash

| old api：  | "AntShares.Output.GetScriptHash"             |
|------------|----------------------------------------------|
| Binding Method: | Output_GetScriptHash                    |
|  Function Description: | Get `ScriptHash` of the transaction's output |
| C\# Method：  | byte[] ScriptHash                         |

## Account

### Neo.Account.GetScriptHash

| old api：  | "AntShares.Account.GetScriptHash"             |
|------------|-----------------------------------------------|
| Binding Method: | Account_GetScriptHash                    |
| Function Description: | Get `ScriptHash` of the account's contract  |
| C\# Method：  | byte[] ScriptHash;                         |


### Neo.Account.GetVotes

| old api：  | "AntShares.Account.GetVotes"                |
|------------|---------------------------------------------|
| Binding Method: | Account_GetVotes                       |
|  Function Description: |  Get votes of the account   |
| C\# Method：  | byte[][] Votes;                          |

### Neo.Account.GetBalance

| old api：  | "AntShares.Account.GetBalance"              |
|------------|---------------------------------------------|
| Binding Method: | Account_GetBalance                     |
|  Function Description: | Get balance of the account by asset id  |
| C\# Method：  | long GetBalance(byte[] asset_id);        |

## Asset

### Neo.Asset.GetAssetId

| old api：  | "AntShares.Asset.GetAssetId"              |
|------------|-------------------------------------------|
| Binding Method: | Asset_GetAssetId                     |
| Function Description: | Get asset id                   |

### Neo.Asset.GetAssetType

| old api：  | "AntShares.Asset.GetAssetType"            |
|------------|-------------------------------------------|
| Binding Method: | Asset_GetAssetType                   |
|  Function Description: | Get asset type                |
| C\# Method：  | byte AssetType                         |

### Neo.Asset.GetAmount

| old api：  | "AntShares.Asset.GetAmount"                |
|------------|--------------------------------------------|
| Binding Method: | Asset_GetAmount                       |
|  Function Description: | Get asset amount               |
| C\# Method：  | long Amount                             |

### Neo.Asset.GetAvailable

| old api：  | "AntShares.Asset.GetAvailable"            |
|------------|-------------------------------------------|
| Binding Method: | Asset_GetAvailable                   |
| Function Description: | Get avaiable amount of the asset |
| C\# Method：  | long Available                         |

### Neo.Asset.GetPrecision

| old api：  | "AntShares.Asset.GetPrecision"             |
|------------|--------------------------------------------|
| Binding Method: | Asset_GetPrecision                    |
|  Function Description: | Get percision of the asset |
| C\# Method：  | byte Precision                          |

### Neo.Asset.GetOwner

| old api：  | "AntShares.Asset.GetOwner"                 |
|------------|--------------------------------------------|
| Binding Method: | Asset_GetOwner                        |
|  Function Description: | Get owner of the asset     |
| C\# Method：  | byte[] Owner                            |

### Neo.Asset.GetAdmin

| old api：  | "AntShares.Asset.GetAdmin"                |
|------------|-------------------------------------------|
| Binding Method: | Asset_GetAdmin                       |
|  Function Description: | Get admin of the asset    |
| C\# Method：  | byte[] Admin                           |

### Neo.Asset.GetIssuer

| old api：  | "AntShares.Asset.GetIssuer"               |
|------------|-------------------------------------------|
| Binding Method: | Asset_GetIssuer                      |
|  Function Description: | Get issuer of the asset   |
| C\# Method：  | byte[] Issuer                          |

### Neo.Asset.Create

| old api：  | "AntShares.Asset.Create"                 |
|------------|------------------------------------------|
| Binding Method: | Asset_Create                        |
|  Function Description: | Register a asset             |
| C\# Method：  | Asset Create(byte asset_type, string name, long amount, </br> byte precision, byte[] owner, byte[] admin, byte[] issuer);                    |

### Neo.Asset.Renew

| old api：  | "AntShares.Asset.Renew"                    |
|------------|--------------------------------------------|
| Binding Method: | Asset_Renew                           |
|  Function Description: | Renewal the asset           |
| C\# Method：  | uint Renew(byte years);                 |
| Remark:     | The renewal fee is calculated by the number of blocks, 1 year being 2 million blocks |

## Contract

### Neo.Contract.GetScript

| old api：  | "AntShares.Contract.GetScript"              |
|------------|---------------------------------------------|
| Binding Method: | Contract_GetScript                     |
|  Function Description: | Get script hash of the contract |
| C\# Method：  | byte[] Script                            |

### Neo.Contract.IsPayable

| old api：  | "Neo.Contract.IsPayable"                    |
|------------|---------------------------------------------|
| Binding Method: | Contract_IsPayable                     |
|  Function Description: | Whether the contract is payable |
| C\# Method：  | bool IsPayable                           |

### System.Contract.GetStorageContext

| old api：  | "Neo.Contract.GetStorageContext", "AntShares.Contract.GetStorageContext"  |
|------------|---------------------------------------------|
| Binding Method: | Contract_GetStorageContext             |
|  Function Description: | Get storage context of the contract  |
| C\# Method：  | StorageContext StorageContext            |
| Remark:     | The StorageContext's IsReadOnly = false    |

### System.Contract.Destroy

| old api：  | "Neo.Contract.Destroy", "AntShares.Contract.Destroy" |
|------------|------------------------------------------------------|
| Binding Method: | Contract_Destroy                                |
|  Function Description: | Destory contract                         |
| C\# Method：  | void Destroy();                                   |
| Remark:     | The storage will be destory also.                   |

### Neo.Contract.Create

| old api：  | "AntShares.Contract.Create"                  |
|------------|----------------------------------------------|
| Binding Method: | Contract_Create                         |
|  Function Description: | Publish a contract               |
| C\# Method：  | Contract Create(byte[] script, byte[] parameter_list, byte return_type, </br> ContractPropertyState contract_property_state, string name, </br> string version, string author, string email, string description);                                 |

### Neo.Contract.Migrate

| old api：  | "AntShares.Contract.Migrate"                |
|------------|---------------------------------------------|
| Binding Method: | Contract_Migrate                       |
|  Function Description: | Migrate/Upgrade the contract    |
| C\# Method：  | Contract Migrate(byte[] script, byte[] parameter_list, byte return_type, </br> ContractPropertyState contract_property_state, string name, </br> string version, string author, string email, string description);                                |

## Enumerator

### Neo.Enumerator.Create

| Binding Method: | Enumerator_Create                     |
|------------|--------------------------------------------|
|  Function Description: | Create an enumerator           |

### Neo.Enumerator.Next

| Aliases：  | "Neo.Iterator.Next"                        |
|------------|--------------------------------------------|
| Binding Method: | Enumerator_Next                       |
|  Function Description: | Get the next element           |
| C\# Method：  | bool Next();                            |

### Neo.Enumerator.Value

| Aliases：  | "Neo.Iterator.Value"                       |
|------------|--------------------------------------------|
| Binding Method: | Enumerator_Value                      |
|  Function Description: | Get the current value          |
| C\# Method：  | TValue Value                            |

### Neo.Enumerator.Concat

| Binding Method: | Enumerator_Concat                     |
|------------|--------------------------------------------|
|  Function Description: | Concat the two enumerators     |

### Neo.Iterator.Create

| Binding Method: | Iterator_Create                       |
|------------|--------------------------------------------|
|  Function Description: | Create a iterator              |

### Neo.Iterator.Key

| Binding Method: | Iterator_Key                                |
|------------|--------------------------------------------------|
|  Function Description: | Get current key of the iterator  |
| C\# Method：  | TKey Key                                      |

### Neo.Iterator.Keys

| Binding Method: | Iterator_Keys                               |
|------------|--------------------------------------------------|
|  Function Description: | Get all keys of the iterator    |

### Neo.Iterator.Values

| Binding Method: | Iterator_Values                             |
|------------|--------------------------------------------------|
|  Function Description: | Get all values of the iterator   |
| C\# Method：  | TValue Value                                  |

## ExecutionEngine

### System.ExecutionEngine.GetScriptContainer

| Binding Method: | GetScriptContainer                               |
|------------|-------------------------------------------------------|
|  Function Description: |  Get the script container of the contract |
| C\# Method：  | IScriptContainer ScriptContainer                   |

### System.ExecutionEngine.GetExecutingScriptHash

| Binding Method: | GetExecutingScriptHash                           |
|------------|-------------------------------------------------------|
|  Function Description: | Get the current executing scrit hash      |
| C\# Method：  | byte[] ExecutingScriptHash                         |
| Remark:     | Get the engine.CurrentContext.ScriptHash             |

### System.ExecutionEngine.GetCallingScriptHash

| Binding Method: | GetScriptContainer                               |
|------------|-------------------------------------------------------|
|  Function Description: | Get the script hash of the caller, which invoked the current script  |
| C\# Method：  | byte[] CallingScriptHash                           |
| Remark:     | Get the engine.CallingContext.ScriptHash             |

### System.ExecutionEngine.GetEntryScriptHash

| Binding Method: | GetEntryScriptHash                                 |
|------------|---------------------------------------------------------|
|  Function Description: | Get the entry point of the current contract |
| C\# Method：  | byte[] EntryScriptHash                               |
| Remark:     | Get the engine.EntryContext.ScriptHash                 |


# NEP-5

The NEP-5 proposal outlines a token standard for the NEO blockchain that will provide systems with a generalized interaction mechanism for tokenized Smart Contracts. 

Different from UTXO, the NEP5 assets are recorded in the contract storage area, through updating account balance in the storage area, to complete the transaction.

In the method definitions below, we provide both the definitions of the functions as they are defined in the contract as well as the invoke parameters.

**totalSupply**
    
```c#
public static BigInteger totalSupply()
```

Returns the total token supply deployed in the system.

**name**
    
```c#
public static string name()
```

Returns the name of the token. e.g. "MyToken".

This method MUST always return the same value every time it is invoked.

**symbol**

```c#
public static string symbol()
```

Returns a short string symbol of the token managed in this contract. e.g. "MYT". This symbol SHOULD be short (3-8 characters is recommended), with no whitespace characters or new-lines and SHOULD be limited to the uppercase latin alphabet (i.e. the 26 letters used in English).

**decimals**

```c#
public static byte decimals()
```

Returns the number of decimals used by the token - e.g. 8, means to divide the token amount by 100,000,000 to get its user representation.

This method MUST always return the same value every time it is invoked.

**balanceOf**

```c#
public static BigInteger balanceOf(byte[] account)
```

Returns the token balance of the `account`.

The parameter `account` SHOULD be a 20-byte address. If not, this method SHOULD throw an exception.

If the `account` is an unused address, this method MUST return 0.

**transfer**

```c#
public static bool transfer(byte[] from, byte[] to, BigInteger amount)
```

Transfers an amount of tokens from the from account to the to account.<br/>
The parameters from and to SHOULD be 20-byte addresses. If not, this method SHOULD throw an exception.<br/>
The parameter amount MUST be greater than or equal to 0. If not, this method SHOULD throw an exception.<br/>
The function MUST return false if the from account balance does not have enough tokens to spend.<br/>
If the method succeeds, it MUST fire the transfer event, and MUST return true, even if the amount is 0, or from and to are the same address.<br/>
The function SHOULD check whether the from address equals the caller contract hash. If so, the transfer SHOULD be processed; If not, the function SHOULD use the SYSCALL `Neo.Runtime.CheckWitness` to verify the transfer.<br/>
If the to address is a deployed contract, the function SHOULD check the payable flag of this contract to decide whether it should transfer the tokens to this contract.<br/>
If the transfer is not processed, the function SHOULD return false.


**Transfer Event**

```c#
public static event transfer(byte[] from, byte[] to, BigInteger amount)
```

MUST trigger when tokens are transferred, including zero value transfers.<br/>
A token contract which creates new tokens MUST trigger a `transfer` event with the from address set to null when tokens are created.<br/>
A token contract which burns tokens MUST trigger a `transfer` event with the to address set to null when tokens are burned.

# Upgrade

## Contract Migrate

Smart contract support upgrade operations after release, but upgrade interfaces need to be reserved in the old contract.<br/>
The contract upgrade mainly calls the `Neo.Contract.Migrate` method:

```c#
Contract Migrate(byte[] script, byte[] parameter_list, byte return_type, ContractPropertyState contract_property_state, string name, string version, string author, string email, string description);
```

When the upgrade interface invoked in the old contract, it will create a new contract based on the parameters passed in. If the old contract has a storage area, it will move to the new contract. After upgrade, the old contract and storage will be deleted. 

## Contract Destory

Smart contract support destruction operations after release, but need to reserve the destruction interfaces in the old contract.


The contract destruction mainly calls the `Neo.Contract.Destroy` method:

```c#
void Destroy();
```

The `Destory` method accepts no parameters, and it will delete contract and the storage area.

> [!NOTE]
> 如果发现有死链接，请联系 <feedback@neo.org>
