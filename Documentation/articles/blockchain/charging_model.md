<center><h2>Charging Model</h2></center>

&emsp;&emsp;Participants in NEO ecology need to pay network fee and system fee by GAS when using NEO network. Network fee is paid to consensus node as new block excitation. System fee and UTXO, which can be claimed by corresponding account with `ClaimTransaction`, are shares of NEO holders. Fee distribution rule is as follows:

[![economic model](../../images/blockchain/economic_model.jpg)](../../images/blockchain/economic_model.jpg)

&emsp;&emsp;According to "Transaction" chapter, inputs and outputs data of a transaction reveals GAS amount change before & after transaction. Total fee can be calculated from GAS change:

&emsp;&emsp;&emsp;&emsp;Total fee = Network fee + System fee = sum(GAS in inputs) - sum(GAS in outputs)

### **Network Fee**

&emsp;&emsp;Network fee is the fee for transaction encapsultion. User can define the amount of network fee. Theoretically the higher network fee per byte is, corresponding transaction is easier to be encapsulated. A block supports at most 500 transactions, in which at most 20 free ones, in current main net.

### **System Fee**
&emsp;&emsp;System fee is the fee for consumed network resources in NEO network. It can be divided in 2 parts. Firstly, system fee of special transactions can be set in configuration file `protocol.json`, including: 

| Transaction Type          |  System Fee |
|-----------------------|---------------|
| EnrollmentTransaction |      1000     |
| IssueTransaction      |       500     |
| PublishTransaction    |       500     |
| RegisterTransaction   |     10000     | 

&emsp;&emsp;Besides, system invoking / VM command execution occured in smart contract execution produce fee. Such fee is also sorted as system fee. About detailed fee standard please refer to "Smart Contract" chapter.




