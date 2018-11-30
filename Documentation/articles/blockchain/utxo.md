<center><h2>UTXO Model</h2></center>

&emsp;&emsp;Different to account balance model, UTXO (Unspent Transaction Output) model computes user assets through unspent 'output' rather than recording account assets directly. Every UTXO type asset, i.e. global asset, is `input-output` association model, where `input` reveals asset source and `output` reveals asset destination. In the picture below, Alice gets 8 GAS's share from her holded NEO, which is recorded in the first output in transaction # 101. When Alice transfers 3 GAS to Bob, `input` of new transaction records the asset is 8 GAS, which is represented by output position 0 of transaction #101. Furthermore, in another transaction #201, one output points to the 3 GAS transferred to Bob, while another one to 5 GAS back to Alice herself (small change).

[![utxo](../../images/blockchain/utxo.jpg)](../../images/blockchain/utxo.jpg)

> [!IMPORTANT]
> 1. If transaction contains fee, input.GAS > output.GAS
> 2. If NEO holder claims GAS share, input.GAS < output.GAS
> 3. In case of asset issueing, input.Asset < output. Asset

&emsp;&emsp;UTXO transfering is actually consuming output which can unlock `Output.scriptHash` and filling in signature parameters in new transaction's validator. Account address is actually script hash's base58check encoding, representing a piece of signature verification script as follows. [`Op.CheckSig`](../neo_vm.md#checksig) execution requires public key & signature. Public key parameters are already included in address script, so only signature parameters need to be added in transaction.

[![utxo](../../images/blockchain/account_scripthash.jpg)](../../images/blockchain/account_scripthash.jpg)






