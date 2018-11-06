<center><h2>投票，验证人，议员，议长</h2></center>

投票和共识，议员和议长的确认方法。（投票去头去尾的逻辑细节）





## 投票


* **EnrollmentTransaction** 类型


| 尺寸 | 字段 | 类型 | 说明 |
|-----|------|------|------|
| 1 | Type | uint8 | 交易类型， `0x20` |
| 1 | Version | uint8 | 	交易版本，目前为 0 |
| ? | PublicKey | ECPoint | 申请验证人地址 |
| ?*? | Attributes | tx_attr[]| 该交易所具备的额外特性 |
| 34*? | Inputs |  tx_in[] | 输入 |
| 60 * ? | Outputs | tx_out[] | 输出 |
| ?*? | Scripts | script[] | 用于验证该交易的脚本列表 |



1. StateTransanction


* **StateTransaction** 类型


| 尺寸 | 字段 | 类型 | 说明 |
|-----|------|------|------|
| 1 | Type | uint8 | 交易类型， `0x90` |
| 1 | Version | uint8 | 	交易版本，目前为 0 |
| ?*?   | Descriptors | StateDescriptor[] | 投票信息  |
| ?*? | Attributes | tx_attr[]| 该交易所具备的额外特性 |
| 34*? | Inputs |  tx_in[] | 输入 |
| 60 * ? | Outputs | tx_out[] | 输出 |
| ?*? | Scripts | script[] | 用于验证该交易的脚本列表 |



* **StateDescriptor** 类型


| 尺寸  |   字段  | 类型 |  说明 |
|-------|---------|------|-------|
| 1  | Type |  StateType/byte | 投票类型: `0x40` 投票， `0x48` 申请验证人 |
| ? |  Key | byte[] |  投票人地址  | 
| ? | Field | string | `Registered`, `Votes` |
| ? | Value | byte[] | 投票地址列表 |




2. NEO 资产变动时， 相应的变化








## 验证人到议员






## 议员到议长



