# Aptos Star Fighter

An Aptos space survival game

Escape the rockets and try to survive as long as possible!
Your best score will be saved on-chain for everyone to see!

## Screenshot:
<img src="screenshot.png" />

## Project structure

The repository is subdivided into three main folders:
- [move](move): The smart contract that manages the game state and the high score.
- [server](server): The server that handles authentication and talks to the blockchain.
- [game](game): The game client, which is a Unity 3D project design to build for native desktop, as well as WebGL.

On top of that, we have the `aptos-unity-webgl-template` which is a WebGL
template that supports the [Aptos Wallet Adapter](https://github.com/aptos-labs/aptos-wallet-adapter).

## Special thanks:
[gnazario](https://github.com/gnazario)

[bowen](https://github.com/gnazario)

[Lex](https://github.com/gnazario)
