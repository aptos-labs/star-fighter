# The server

A simple expressjs server for handling communications with the fullnode, caching
and preventing cheating.

Note: depending on the nature of your web3 game, you might not need to have a backend.
In a way, the blockchain can act as a backend and you can design your game around that.

# Structure

The main controllers are:
- `AuthController`: handles authentication, either using IdentityConnect or account data.
  The account should be able to provide a proof of ownership of the secret key, but that is not being
  checked for now, for simplicity
- `UserController`: get info about the currently authenticated user
- `SessionController`: handles a game session. In order to start a game, the user needs to call an endpoint
  which will return them a transaction payload to sign in order to start the game.
- `LeaderboardController`: fetch the leaderboard from the blockchain. 
  The server acts as an intermediate caching layer

# Development

You can start a local development server by running
```zsh
pnpm dev 
```

You can then build a static website by running the following command.
This will generate a dist folder that you can upload to your web server.
```zsh
pnpm build 
```

If you're using vercel, you can manually deploy your server by running
```zsh
vercel
# Then follow the instructions..
```
