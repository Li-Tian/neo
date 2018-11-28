<center><h2>收费模型</h2></center>

&emsp;&emsp;NEO生态的各参与方，在使用NEO网络时，需要支付网络费(Network Fee)和系统费(System Fee)。网络费将作为选举的共识节点出块奖励，而系统费和GAS的区块奖励，将作为持有NEO的用户权益分红，可通过`ClaimTransaction`交易提取相应GAS到对应账户。我们用下图来描述费用的分配。

[![economic model](../../images/blockchain/economic_model.jpg)](../../images/blockchain/economic_model.jpg)

&emsp;&emsp;参照`交易`部分，我们知道在一个交易的inputs和outputs的数据信息中给出了相关地址上交易前后的GAS数量。手续费可以求其差值得出：

&emsp;&emsp;&emsp;&emsp;手续费 = 网络费 + 系统费 = sum(inputs 中的 GAS) - sum(outputs 中的 GAS)

### **网络费**
&emsp;&emsp;交易是可以收费的。在后面的交易类型中，可以看到进行一些特定的交易需要付很高的系统手续费。而默认情况下，交易手续费为0。用户可以自愿给一些交易费。在Neo GUI中可以设置。如下图。交易的输入=交易的输出+网络交易费。增发情况下，不一样。具体之后再看。

### **系统费**
&emsp;&emsp;系统费由两部分算得。首先，在系统设置`protocol.json`中给出了4种交易的收费额。列举如下： 

| 交易类型               |     系统费    |
|-----------------------|---------------|
| EnrollmentTransaction |      1000     |
| IssueTransaction      |       500     |
| PublishTransaction    |       500     |
| RegisterTransaction   |     10000     | 

&emsp;&emsp;其次，系统调用收取的费用也会加到系统费中。具体信息可以参见`智能合约`部分。




