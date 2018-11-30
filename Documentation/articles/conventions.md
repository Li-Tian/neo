# Conventions

　The NEO network system uses the following basic technical conventions for transmission and storage.

## Byte Order

　All integer types of NEO are Little Endian except for IP address and port number, these 2 are Big Endian.

## Hash Algorithm

　Two different hash functions are used in the NEO system: SHA256 and RIPEMD160. The former is used to generate a longer hash value (32 bytes) and the latter is used to generate a shorter hash value (20 bytes). Usually when a hash value of an object is generated, hash functions are used twice. For example, when a hash of a block or transaction is generated, SHA256 is calculated twice; when a contract address is generated, the SHA256 hash of the script is calculated, then the NSPEMD160 hash of the previous hash is calculated.

　In addition, the block will also use a hash structure called a Merkle Tree. It computes the hash of each transaction and combines one with the next and then hash again. Repeats this process until there is only one root hash (Merkle Root).

　Details will be described in subsequent chapters.

## Variable Length Types

 * varint: A variable-length integer that can be encoded differently based on the value to save space.

      |Value|Length|Format|
      |---|---|---|
      |< 0xfd|1|uint8|
      |<= 0xffff|3|0xfd + uint16|
      |<= 0xffffffff|5|0xfe + uint32|
      |> 0xffffffff|9|0xff + uint64|

 * varstr: A variable-length string consisting of a variable-length integer followed by a string. The string is encoded in UTF8.

      |Size|Field|DataType|Description|
      |---|---|---|---|
      |?|length|varint|The length of the string in bytes|
      |length|string|uint8[length]|string itself|

 * Array: An array consisting of a variable-length integer followed by a sequence of elements.

      | Size | Field | Data Type | Description |
      | ---- | ----- | --------- | ----------- |
      | ?    | length | varint | The length of the array, in number of array members |
      | ?    | data | Array Element Type | Array Members |
      | ?    | data | Array Element Type | Array Members |
      | ... | ... | ... | ... |

## Fixed-point Number(Fixed8)

　Data in NEO such as amount or price are 64 bit fixed-point number and the precision of decimal part is 10<sup>-8</sup>，range：[-2<sup>63</sup>/10<sup>8</sup>, +2<sup>63</sup>/10<sup>8</sup>)