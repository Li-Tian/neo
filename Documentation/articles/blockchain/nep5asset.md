### **NEP5 资产**


| 尺寸 | 字段 | 名称 | 类型 | 描述 |
|--|-------|-----|------|------|
| 20  | ScriptHash | 地址脚本hash | UInt160 |   |
| 1 | IsFrozen | 是否冻结 | bool |  冻结用户的资产不能转账  |
| ? * ? | Votes | 投票地址 | ECPoint[] | 投票地址列表 |
| ? | Balances | UTXO资产 |Dict<UInt256, Fixed8> | 资产Id -> 数量  |


