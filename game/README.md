# Unity game

The game client, built using Unity3D.

Everything needed to integrate the game with Aptos wallets is under the `AptosWallet` subfolder.
There are two different implementations of `WalletService` that will be
selected automatically based on the build system:
- WebWalletService (WebGL targets): connects to the Aptos wallet adapter
  initialized in the AptosDapp template (built from [aptos-unity-webgl-template](../aptos-unity-webgl-template))
- ICWalletService (all other targets): uses IdentityConnect to connect to any supported wallet (e.g. Petra Mobile).

The `WalletService` exposes the following methods:
- `Connect` starts a connection to an Aptos wallet
- `Disconnect` disconnects from the currently connected wallet
- `IsConnected` returns whether the wallet is connected or not 
- `GetAccount` gets the currently connected account
- `SignAndSubmitTransaction` requests the wallet to sign and submit a serialized transaction payload

all the functions are implemented using async/await for better developer experience.

In order for `ICWalletService` to work properly, you need to set some necessary constants,
like `IC_DAPP_HOSTNAME` and `IC_BASE_URL`.
See [Constants.cs](Assets/Scripts/Constants.cs) for reference.

Currently, `ICWalletService` depends on a `BackendDappService` for generating a pairing request.
This ensures that the Dapp id generated for your specific dapp stays hidden from users if needed,
but will soon add the option to generate a pairing request fully from Unity.
