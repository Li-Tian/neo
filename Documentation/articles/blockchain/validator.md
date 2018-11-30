<center><h2>Validator</h2></center>

In NEO, the holders can apply to be validators (consensus node candidates), and then vote to be become consensus nodes. In NEO system, the voting of validators and number of consensus nodes are stored in blockchain.

### **Validator**

Validator, the candidate of consensus node, decided by the voting of the holders.

| Size | Field  | Type | Descriptoin |
|--|-------|-----|------|------|
| ?  | PublicKey  | ECPoint | Validator's public key |
| 1 | Registered  | bool |Check registered. Only registerd validators can be vote |
| 8 | Votes | Fixed8 |  |


### **Validator_Count**

The voting record of number of consensus nodes.

| Size | Field | Type | Descriptoin |
|--|-------|-----|------|------|
| 1024 * 8 | Votes  | Fixed8[] | Voting list, up to 1024 consensus nodes.  |


The process of validators voting, please read["Voting, Validator, Delegates, Speaker"](../consensus/vote_validator.md) section.