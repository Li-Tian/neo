<center><h2>Asset and Account</h2></center>

### **Asset**

NEO中资产包含两种：一种是用户发行的UTXO类型的全局资产，NEO与GAS在创世块中被定义发行。另外一种是，用户通过智能合约发布的如NEP-5资产。前者信息记录在资产账户信息中，见下表，后者存储在合约的存储空间。

There are two kinds of assets in NEO, 

| Size | Field  | Type | Description |
|--|-------|------|------|
| 32  | AssetId | UInt256 | `assetid = tx.hash` |
| 1 | AssetType | AssetType | |
| ? | Name | string | |
| 8 | Amount  |Fixed8 |  `amount = -Fixed8.Satoshi = -1 = infinity`  |
| 8 | Available | Fixed8 |   |
| 1 | Precision | byte |   |
|1 | FeeMode | const byte |   |
| 8 | Fee  | Fixed8 |   |
| 20 | FeeAddress | UInt160 | default `null`   |
| ? | Owner  | ECPoint |   |
| 20 | Admin  | UInt160  |   |
| 20 | Issuer  | UInt160 |   |
| 4 | Expiration  | uint  |   |
| 1 | IsFrozen  | bool | Frozen assets, cannot be transferred  |

> [!NOTE]
> There are two models of `Amount`: One is unlimited, the amount is set to `-Fixed8.Satoshi`, means infinity. The other is to fixed amount cannot be modified currently.

### **AssetType**

| Type | Value | Description |
|-------|-----|----|
| CreditFlag | 0 |  |
| DutyFlag | 0x80 |  |
| GoverningToken | 0x00 | Neo |
| UtilityToken | 0x01 | Gas |
| Currency | 0x08 |  |
| Share | DutyFlag &#124; 0x10 | Equity-like assets |
| Invoice | DutyFlag &#124; 0x18 |  |
| Token | CreditFlag &#124; 0x20 | Normal token |

> [!NOTE]
>  Asset with `DutyFlag`, needs the signature of the payee. 

| Asset Name | Type | Value |  Amount | Description |
|-------|----|-----|-------|--------|
| NEO |  AssetType.GoverningToken | 0x00 | 100 million | All of which are transferred to the address of the standby consensus nodes' multi-signature contract in Genesis Block | 
| GAS | AssetType.UtilityToken | 0x01 | 100 million | By block release, NEO holders claim GAS through `ClaimTransacion`. |



### **Account**

In NEO, the account model and UTXO model coexist. Accounts record the UTXO-type global assets, and NEP5 TOKEN-type assets also.

| Size | Field  | Type | Descriptoin |
|--|-------|-----|------|
| 20  | ScriptHash  | UInt160 | The hash of account's script contract.  |
| 1 | IsFrozen  | bool |  Fronzen accounts cannot transfer.  |
| ? * ? | Votes  | ECPoint[] | the voted address list |
| ? | Balances  |Dict<UInt256, Fixed8> | UTXO assets, mapping from assetId to amount.  |

