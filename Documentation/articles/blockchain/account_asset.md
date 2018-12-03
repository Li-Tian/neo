<center><h2>Asset and Account</h2></center>

### **Asset**

There are two kinds of assets in NEO: one is UTXO-type global assets issued by users, such as NEO, GAS defined in Genesis Block. Another is published by smart contract, such as NEP5 assets. The former are recorded in transactoin and users' account, while the latter are stored in the contracts' storage space.

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
| NEO |  AssetType.GoverningToken | 0x00 | 100 million | All of which are transferred to the address of the standby consensus nodes' multi-parity signature contract in Genesis Block | 
| GAS | AssetType.UtilityToken | 0x01 | 100 million | By block release, NEO holders claim GAS through `ClaimTransacion`. |



### **Account**

In NEO, the account model and UTXO model coexist. Accounts record the UTXO-type global assets, and NEP5 TOKEN-type assets also.
NEO网络中，账户(account)模型和UTXO模型并存。账户记录了UTXO类型的全局资产的用户资金和用户投票。

| Size | Field  | Type | Descriptoin |
|--|-------|-----|------|
| 20  | ScriptHash  | UInt160 | The hash of account's script contract.  |
| 1 | IsFrozen  | bool |  Fronzen accounts cannot transfer.  |
| ? * ? | Votes  | ECPoint[] | the voted address list |
| ? | Balances  |Dict<UInt256, Fixed8> | UTXO assets, mapping from assetId to amount.  |

