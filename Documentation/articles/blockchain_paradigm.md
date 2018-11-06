# 地址编码：Base58Check
...

# 哈希算法：SHA256, RIPEMD160, Murmur3
...

# 签名算法：ECDSA
...

简单描述上述算法的基本原理，详细描述调用的输入输出接口。可以适当举例，或者列举程序的调用示范。

详细描述可以引用 wikipedia 等知名网站。不要引用个人博客。降低将来出现死链的风险。

然后描述一下各个算法在系统中的哪些地方使用，即描述各个算法的应用场景。

※以上内容为建议，可根据对概念的理解调整文章的框架。





## Base58

### 摘要

Base58是一种将非可视字符与可视化字符(ASCII化)相互转化的编解码方法

### 详述

Base58是一种将非可视字符与可视化字符(ASCII)相互转化的编解码方法。实现了数据的压缩、便于阅读，适用于抗自动监视的传输系统的底层编码机制，但缺乏效验机制，无法检测出传输过程中字符串的遗漏，需要配合改进算法Base58Check使用。

采用数字、大写字母、小写字母（去除歧义字符 0 (零), O (大写字母O), I (大写的字母i) and l (小写的字母L) ），总计58个字符作为编码的字母表。

neo使用的字母表为：123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz

接口定义：

1、编码方法：把byte[]数据编码成Base58字符串String数据

~~~
string Encode(byte[] input)
~~~



2、解码方法：Base58字符串String解码成byte[]数据

~~~
byte[] Decode(string input)
~~~



编码步骤：

1、把byte[]数据前添加一个0x00，生成一个新的byte数组，并将新数组做倒序排序

2、把数组的数据转成10进制BigInteger数

3、把BigInteger数按字母表转换成58进制字符串

4、统计原byte[]数据中0x00的个数count，在字符串前补count个字母表游标为零所对应的字符

解码步骤：

1、倒序输入的字符串，将其按字母表转换成10进制Biginteger数

2、把Biginteger数转换成byte[]数据，并将byte[]数据倒序排序

3、统计原输入的字符串中字母表游标为零所对应的字符的个数count

4、若byte[]数据的长度大于1，且byte[0]等于0，byte[1]大于等于0x80,则从byte[1+count]开始截取，

​      否则从byte[count]开始截取,得到结果

Example:

1、String-->byte[]

​                                                                                                     [0x17,0xad,0x5c,0xac,0x59,

​                                                                                                      0x6a,0x1e,0xf6,0xc1,0x8a,

AXaXZjZGA3qhQRTCsyG5uFKr9HeShgVhTF             --------->0xc1,0x74,0x6d,0xfd,0x30, 

​                                                                                                      0x4f,0x93,0x96,0x43,0x54,

​                                                                                                      0xb5,0x78,0xa5,0x83,0x22]

2、byte[]-->String

[0x17,0xad,0x5c,0xac,0x59,

 0x6a,0x1e,0xf6,0xc1,0x8a,

0xc1,0x74,0x6d,0xfd,0x30,           --------->AXaXZjZGA3qhQRTCsyG5uFKr9HeShgVhTF

0x4f,0x93,0x96,0x43,0x54,

0xb5,0x78,0xa5,0x83,0x22]

应用场景：

1、为Base58Check编解码方法提供服务。





## Base58Check

### 摘要

Base58Check是基于Base58的改进型编解码方法

### 详述

Base58Check是基于Base58的改进型编解码方法.通过对原数据添加数据的哈希值作为盐，弥补了Base58缺少效验机制的缺点。。



接口定义：

1、编码方法：把byte[]数据编码成带效验功能Base58字符串String数据

```
string Base58CheckEncode(byte[] input)
```

2、解码方法：把带效验功能Base58字符串String解码成byte[]数据

```
byte[] Base58CheckDecode(string input)
```



编码步骤：

1、通过对原byte[]数据做两次sha256得到原数据的哈希，取其前4字节作为版本前缀checksum，添加到原byte[]  

​     数据的末尾

2、把添加了版本前缀的byte[]数据做Base58编码得到对应的字符串

解码步骤：

1、把输入的字符串做Base58解码，得到byte[]数据

2、取byte[]数据收字节到到数第4字节前的所有数据byte[] data

3、把data做两次sha256得到的哈希值的前4字节作为版本前缀checksum，与

byte[]数据的后4字节比较是否相同，相同则返回data,否则抛出异常

![Base58Check编解码](../images/blockchain_paradigm/Base58CheckEncodeAndDecode.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/20)

Example:

1、String-->byte[]

​                                                                                                     [0x17,0xad,0x5c,0xac,0x59,

​                                                                                                      0x6a,0x1e,0xf6,0xc1,0x8a,

AXaXZjZGA3qhQRTCsyG5uFKr9HeShgVhTF             --------->0xc1,0x74,0x6d,0xfd,0x30, 

​                                                                                                      0x4f,0x93,0x96,0x43,0x54,

​                                                                                                      0xb5]

2、byte[]-->String

[0x17,0xad,0x5c,0xac,0x59,

 0x6a,0x1e,0xf6,0xc1,0x8a,

0xc1,0x74,0x6d,0xfd,0x30,           --------->AXaXZjZGA3qhQRTCsyG5uFKr9HeShgVhTF

0x4f,0x93,0x96,0x43,0x54,

0xb5]

应用场景：

1、导入、导出输出wif格式的密钥     

2、合约脚本哈希与地址字符串相互转换

3、导入、导出NEP2格式密钥



## ECC椭圆曲线加密

### 摘要

ECC椭圆曲线加密算法是一种基于离散对数问题的非对称加密算法

### 详述

​     ECC椭圆曲线加密算法是一种非对称加密算法。利用其K=k*G过程不可逆的特性（其中K为公钥，G为基点（常数点)),可以预防通过公钥暴力求解私钥。相较于RSA等其他加密算法，在相同密钥长度情况下，其具备更高的安全性，同时更节约算力。ECC结合其他算法广泛应用于签名等领域，例如ECDsa数字签名。

　NEO与比特币一样都采用ECC作为其公钥生成算法，NEO采用了secp256k1 标准所定义的⼀条特殊的椭圆曲线，使用的参数：

　素数Ｑ：00FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFF

　椭圆曲线的系数Ａ：00FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFC

　椭圆曲线的系数Ｂ：005AC635D8AA3A93E7B3EBBD55769886BC651D06B0CC53B0F63BCE3C3E27D2604B

　阶数Ｎ：00FFFFFFFF00000000FFFFFFFFFFFFFFFFBCE6FAADA7179E84F3B9CAC2FC632551

　基点G：(０ｘ6B17D1F2E12C4247F8BCE6E563A440F277037D812DEB33A0F4A13945D898C296，

　　　　　０ｘ4FE342E2FE1A7F9B8EE7EB4A7C0F9E162BCE33576B315ECECBB6406837BF51F5)



注：1、NEO私钥采用32字节长度密钥

​       2、NEO公钥采用两种格式：压缩形：02or03+x坐标+y坐标

​                                                       非压缩形：04 +x坐标

Example:

1、private-->public

​      输入：c7134d6fd8e73d819e82755c64c93788d8db0961929e025a53363c4cc02a6962
​      输出（压缩型）：035a928f201639204e06b4368b1a93365462a8ebbff0b8818151b74faab3a2b61a
​      输出（非压缩型）：045a928f201639204e06b4368b1a93365462a8ebbff0b8818151b74faab3a2b61a35dfabcb79ac492a

2a88588d2f2e73f045cd8af58059282e09d693dc340e113f                                                                                         

应用场景：

1、私钥生成公钥

２、签名和验证签名

参考文献：

1、[一个关于椭圆曲线密码学的初级读本 ](https://arstechnica.com/information-technology/2013/10/a-relatively-easy-to-understand-primer-on-elliptic-curve-cryptography/)



## ECDSA 签名

### 摘要

​    椭圆曲线数字签名算法（ECDSA）是使用椭圆曲线密码（ECC）对数字签名算法（DSA）的模拟

### 详述

​     椭圆曲线数字签名算法（ECDSA）是使用椭圆曲线密码（ECC）对数字签名算法（DSA）的模拟。

其优点是速度快，强度高，签名短。其基本使用方法如下：

​     假设私钥、公钥、基点分别为k、K、G，根据ECC算法可知有K = k·G。

签名过程：
　　1、选择随机数r，计算点r·G(x, y)。
　　2、根据随机数r、消息M的哈希h、私钥k，计算s = (h + k·x)/r。
　　3、将消息M、和签名{r·G, s}发给接收方。

验证过程：
　　1、接收方收到消息M、以及签名{r·G=(x,y), s}。
　　2、根据消息求哈希h。
　　3、使用发送方公钥K计算：h·G/s + x·K/s，并与r·G比较，如相等即验签成功。

　　推导原理如下：
　　h·G/s + x·K/s = h·G/s + x(k·G)/s = (h+x·k)G/s
　　= r(h+x·k)G / (h+k·x) = r·G

​      NEO与比特币一样都采用ECDSA作为其数字签名的算法 。作为一个通用算法，多数高级语言会提供其对应的算法包，NEO使用了微软提供的System.Security.Cryptography算法库，其使用的求解消息的哈希算法为SHA256，公钥由ECC算法转化私钥求得。​                                                                       

应用场景：

1、交易的签名。

２、共识



# RIPEMD160 

### 摘要

   RIPEMD160是一种加密哈希函数

### 详述

​     椭圆曲线数字签名算法（ECDSA）是使用椭圆曲线密码（ECC）对数字签名算法（DSA）的模拟。

其优点是速度快，强度高，签名短。其基本使用方法如下：

​     假设私钥、公钥、基点分别为k、K、G，根据ECC算法可知有K = k·G。

签名过程：
　　1、选择随机数r，计算点r·G(x, y)。
　　2、根据随机数r、消息M的哈希h、私钥k，计算s = (h + k·x)/r。
　　3、将消息M、和签名{r·G, s}发给接收方。

验证过程：
　　1、接收方收到消息M、以及签名{r·G=(x,y), s}。
　　2、根据消息求哈希h。
　　3、使用发送方公钥K计算：h·G/s + x·K/s，并与r·G比较，如相等即验签成功。

　　推导原理如下：
　　h·G/s + x·K/s = h·G/s + x(k·G)/s = (h+x·k)G/s
　　= r(h+x·k)G / (h+k·x) = r·G

​      NEO与比特币一样都采用ECDSA作为其数字签名的算法 。作为一个通用算法，多数高级语言会提供其对应的算法包，NEO使用了微软提供的System.Security.Cryptography算法库，其使用的求解消息的哈希算法为SHA256，公钥由ECC算法转化私钥求得。​                                                                       

应用场景：

1、交易的签名。

２、共识