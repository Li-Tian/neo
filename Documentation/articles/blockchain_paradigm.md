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

测试demo

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

![Base58Check编解码](..\images\blockchain_paradigm\Base58Check编解码.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/20)

测试demo

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

3、导出、导出NEP2格式密钥

