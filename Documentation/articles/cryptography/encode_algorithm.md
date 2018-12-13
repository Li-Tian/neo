<center> <h2> Encoding Algorithm </h2> </center>

##  Base58

　Base58 is a kind of encoding/decoding algorithm used to switch data between unvisualizable / visualizable format(ASCII). Base58 enables data compressing, easy reading, and is suitable for Infrastructure encoding mechanism of anti-automatic monitoring transmission system. Nevertheless, lack of checking mechanism induce the defect of disability to check string omit in transmission process. Thus Base58Check, an improved algorithm is needed to cooperate.

　There are 58 letters in base58's alphabet including single digit numbers (From 1 to 9), English letters except O(capitalized o) / I(capitalized i) / l(lower-cased L). This letters are ommited to avoid misreading.

　Neo's alphabet is as follows: **123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz**

Interface definition：

1.  Encoding method: Encoding byte[] format data into Base58 string format.

```c#
string Encode(byte[] input)
```


2.  Decoding Method: Decoding Base58 string format data into byte[] format

```c#
byte[] Decode(string input)
```


**Encoding Steps**：

1.  Add 0x00 before byte[] data to generate a new byte array, and then reverse its order.(little endian)

2.  Convert array data to BigInteger Object.

3.  Convert BigInteger format number to 58-based number according to Base58 alphabet.

4.  Count the number of 0x00 in original byte array format data. In the head of the Base58 format data generated in step 3, for each 0x00, add a letter '1', which is the first charater in Base58 alphabet.

**Decoding Steps**：

1.  Invert input string and convert it into Biginteger format according to Base58 alphabet.

2.  Convert from Biginteger format to byte[] format and then reverse the order to big endian.

3.  If byte[] format data's length is more than 1 & byte[0] = 0 & byte[1] >= 0x80, start from byte[1], otherwise start from byte[0] to get the decoded result.

4.  Count the number of the first letter of Base58 alphabet in original input data as count and remove leading zeros from the decoded data.

Example:

| String Content | byte[] |
| --- | --- |
| <nobr>AXaXZjZGA3qhQRTCsyG5uFKr9HeShgVhTF</nobr> |  [0x17, 0xad, 0x5c, 0xac, 0x59, 0x6a, 0x1e, 0xf6, 0xc1, 0x8a, 0xc1, 0x74, 0x6d, 0xfd, 0x30, 0x4f, 0x93, 0x96, 0x43, 0x54, 0xb5, 0x78, 0xa5, 0x83, 0x22] |

Scenarios：

1.  Serves Base58Check encoding / decoding method.

## Base58Check

　  Base58Check is an improved encoding / decoding algorithm base on Base58. Base58Check solved the lack of checking mechanism in Base58, by adding hash value to original data as salt.


Interface definition:

1. Encoding method: encode byte array data into checkable Base58 string format.

```c#
string Base58CheckEncode(byte[] input)
```

2.  Decoding method：decode checkable Bse58 string data into byte array format.

```c#
byte[] Base58CheckDecode(string input)
```
**Encoding Steps**:

1.  Encode input byte array twice with Sha256 algorithm. Take the first 4 bytes of result hash as version prefix checksum and append it to the end of original byte array.

2.  Base58-encode the byte array with version prefix to get corresponding encoded result.

**Decoding Steps**:

1.  Base58-decode input string to get byte array format decoded result.

2.  Take the content of byte array except the last 4 bytes as data.

3.  Encode data twice with Sha256 algorithm and check whether the first 4 bytes are the same with the last 4 bytes in byte array of step 1. If so return the decoded data, otherwise regard data as illegal.

[![Base58Check Encoding & Decoding](../../images/blockchain_paradigm/Base58CheckEncodeAndDecode-en.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/20)](../../images/blockchain_paradigm/Base58CheckEncodeAndDecode.png)

Example:

| String content | byte[] |
| --- | --- |
| <nobr>AXaXZjZGA3qhQRTCsyG5uFKr9HeShgVhTF</nobr>   |  [0x17, 0xad, 0x5c, 0xac, 0x59, 0x6a, 0x1e, 0xf6, 0xc1, 0x8a, 0xc1, 0x74, 0x6d, 0xfd, 0x30, 0x4f, 0x93, 0x96, 0x43, 0x54, 0xb5] |


Scenarios：

1. Import / export wif format secret key

2. Switch between contract script hash and address

3. Import / export NEP2 format secret key
