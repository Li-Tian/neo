
### **Asset**

| 尺寸 | 字段 | 名称 | 类型 | 描述 |
|--|-------|-----|------|------|
| 32  | AssetId | 资产Id | UInt256 | `assetid = tx.hash` |
| 1 | AssetType | 类型 | AssetType | |
| ? | Name | 资产名字 | string | 存放的是公钥列表 |
| 8 | Amount | 总量 |Fixed8 |  `amount = -Fixed8.Satoshi = -1 = 无穷大`  |
| 8 | Available | 剩余量 | Fixed8 |   |
| 1 | Precision | 精度 | byte |   |
|1 | FeeMode | 费用模式 | const byte |   |
| 8 | Fee | 费用 | Fixed8 |   |
| 20 | FeeAddress |  | UInt160 | 默认null   |
| ? | Owner | 所有者 | ECPoint |   |
| 20 | Admin | 管理员 | UInt160  |   |
| 20 | Issuer | 发行者 | UInt160 |   |
| 4 | Expiration | 过期时间 | uint  |   |
| 1 | IsFrozen | 是否冻结 | bool |  资产是否冻结 |

> [!Note]
> 总量的模式有两种: 一种不限量，总量设置为` -Fixed8.Satoshi`， 表示无穷大. 另外一种是限定不可修改的总量。

### **AssetType**

| 字段 | 值 | 描述 |
|-------|-----|----|
| CreditFlag | 0 |  |
| DutyFlag | 0x80 |  |
| GoverningToken | 0x00 | Neo |
| UtilityToken | 0x01 | Gas |
| Currency | 0x08 |  |
| Share | DutyFlag &#124; 0x10 | 股权类 |
| Invoice | DutyFlag &#124; 0x18 |  |
| Token | CreditFlag &#124; 0x20 | 普通token |

> [!Note]
> 资产类型包含`DutyFlag`值时，都需要进行收款方签名。