# The contract
The contract is built around the `user_stats` module, which exposes views and entry functions
to fetch and mutate the users stats on-chain.

The design is very basic: only the admin account can mutate users stats on chain.
Before the start of a game, the user needs to send funds to the server, and once a game is over
and verified, the server will update the stats on-chain.

Future improvements:
- Create a token for holding the user stats, so that the user can look at them on their wallet.
- Add client - server encryption (to prevent cheating)
- Design a more sophisticated smart contract flow for preventing loss of funds

# Deployment

```bash
# Create profile
aptos init --profile devnet
# Confirm that code compiles
aptos move compile --named-addresses space_fighters=devnet
# Deploy
yes | aptos move publish --named-addresses space_fighters=devnet --profile=devnet
```
