### **NEP5 资产**


这里需要根据之后的内容调整。

NEP5资产与UTXO不同，它没有采用UTXO模型，而是在合约存储区内记账，通过对存储区内不同账户 hash所记录余额数值的变化，完成交易。

​参照NEP5协议的要求，在NEP5资产智能合约时必需实现以下方法：

totalSupply

public static BigInteger totalSupply()
​Returns 部署在系统内该token的总数。

name

public static string name()
​Returns token的名称. e.g. "MyToken"。 该方法每次被调用时必需返回一样的值。

symbol

public static string symbol()
​Returns 合约所管理的token的短字符串符号 . e.g. "MYT"。 该符号需要应该比较短小 (建议3-8个字符), 没有空白字符或换行符 ，并限制为大写拉丁字母 (26个英文字符)。 该方法每次被调用时必需返回一样的值。

decimals

public static byte decimals()
​Returns token使用的小数位数 - e.g. 8，意味着把token数量除以100,000,000来获得它的表示值。 该方法每次被调用时必需返回一样的值。

balanceOf

public static BigInteger balanceOf(byte[] account)
Returns 账户的token金额。 参数账户必需是一个20字节的地址。如果不是，该方法会抛出一个异常。 如果该账户是个未被使用的地址，该方法会返回0。

transfer

public static bool transfer(byte[] from, byte[] to, BigInteger amount)
​从一个账户转移一定数量的token到另一个账户. 参数from和to必需是20字节的地址，否则，该方法会报错。 ​参数amount必需大于等于0.否则，该方法会报错。 ​如果账户没有足够的支付金额，该函数会返回false。 ​如果方法执行成功，会触发转移事件，并返回true，即使数量为0或者from和to是同一个地址。 ​函数会检查from的地址是否等于调用合约的hash.如果是，则转移会被处理；否则，函数会调用SYSCALL Neo.Runtime.CheckWitness来确认转移。 ​如果to地址是一个部署合约，函数会检查其payable标志位来决定是否把token转移到该合约。 ​如果转移没有被处理，函数会返回false。

事件 transfer

public static event transfer(byte[] from, byte[] to, BigInteger amount)
​会在token被转移时触发，包括零值转移。 ​一个创建新token的token合约在创建token时会触发转移事件，并将from的地址设置为null。 ​一个销毁token的token合约在销毁token时会触发转移事件，并将to的地址设置为null。



