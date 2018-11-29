<center><h2> Hash Algorithm </h2></center>

　Hash function, or hash algorithm, is a method creating digital finger print from any kind data. Hash function compresses message or data  into abstract to shrink data size & fix data size. This function disorgnizes & remixes data, rebuilding a finger print with the name Hash value. Hash value is always represented by a short string consisted of random letters and digits.

### RIPEMD160 

​　RIPEMD is an encryption hash function published by Hans Dobbertin, Antoon Bosselaers Bart Prenee from COSIC research team, University of Leuven in 1996.

　RIPEMD160 is a 160-bit improvement based on RIPEMD. This algorithm produces a 160-bit hash, which can be presented in hexadecimal format. One feature of this algorithm is avalanche effect, i.e. any slight changes can result in a totally different hash value.

​　NEO generates 160-bit hash of contract script with RIPEMD160.

Example:

| String value | Hash value                                   |
| ----------- | ---------------------------------------- |
| Hello World | 98c615784ccb5fe5936fbc0cbe9dfdb408d92f0f |


Scenarios：

1. Generate contract hash.



### SHA256 

　SHA256 is a kind of SHA-2 algorithm. SHA-2 is an encryption hash function algorithm standard produced by NSA. It belongs to SHA family & is a successor of SHA-1. SHA-2 has 6 different algorithm standards, including SHA-224, SHA-256, SHA-384, SHA-512, SHA-512/224 and SHA-512/256.

　SHA256 produces a 256-bit hash, which can be shown in hexadecimal format, for ang message length.

Example:

| String value | Hash value                                                       |
| ----------- | ------------------------------------------------------------ |
| Hello World | a591a6d40bf420404a011733cfb7b190d62c65bf0bcda32b57b277d9ad9f146e |

Scenarios:

1. Compute contract hash.

2. Signing & Signing validation.

3. Base58Check encoding / decoding.

4. db3、NEP6 wallet secret key storation, export & validation.



### Murmur3 

　  Murmur is kind of non-encryption hash algorithm and suits general hash indexing. It is proposed by Austin Appleby in 2008. These has been several derived variants published to public domain. Murmur's random distribution feature works better for key with strong regularity compared to other popular hash algorithms.

Features：

   1. Low collision probability.
   
   2. Fast computing rate.

   3. Good peformance for large files.

Example:

| String | Hash value |
| ---|---|
| Hello World |ce837619 |


Scenarios：

1. Bloom filter

2. leveldb storage

### Scrypt

　  Scrypt is a kind of secure-encryption algorithm based on PBKDF2-HMAC-SHA-256 algorithm. It's developed by Colin Percival, a famous FreeBSD hacker, for his backup service Tarsnap. Original designing intention is computing during CPU idle time to reduce CPU load and the rely upon CPU computing. Scrypt's long computing time & heavy RAM cost makes parallel computing very difficult, which results in Scrypt's decent defensibility against rainbow table attacks.

​　Neo mainly use SCRYPT algorithm to generate encryption secret key satisfying NEP-2 standard. Parameters are defined as follows:

　　N: CPU/RAM cost，usually 2 ^ N. Default value is 16384.

　　p: Parallelization parameter, a positive integer ranges from 1 to 255. Bigger value represents heavier rely upon concurrent computation. Default value is 8.

　　r: Block size，theoretically ranges from 1 to 255. Bigger value represents heavier rely upon RAM & bandwidth. Default value is 8.

Example:

| Data | Parameters  | Hash value  |
|---|---|---|
| Hello World | key:"I love code"<br>N:16384<br>p:8<br>r:8 | 17b94895fab004e035b3630a718b498f6<br>647458351f04b84b4a2c0bf1db963630fa<br>7bfd1c29663c7bf3556fd7ba6131e5ddfd6<br>40b9f6a2a9ad75d3d59b65f932 |

> [!NOTE]
> The hash value above is in one line.



Scenarios：

1. NEP2 format secret key export.

2. Password verification for NEP6 wallet.

Reference

1. <https://en.wikipedia.org/wiki/Scrypt>

> [!NOTE]
> In case of dead links, please contact <feedback@neo.org>
