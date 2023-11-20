import { NetworkName, WalletCore, WalletName } from "@aptos-labs/wallet-adapter-core";
import { IdentityConnectWallet } from "@identity-connect/wallet-adapter-plugin";
import { MartianWallet } from "@martianwallet/aptos-wallet-adapter";
import { PontemWallet } from "@pontem/wallet-adapter-plugin";
import { RiseWallet } from "@rise-wallet/wallet-adapter";
import { FewchaWallet } from "fewcha-plugin-wallet-adapter";
import { PetraWallet } from "petra-plugin-wallet-adapter";
import { createContext, useContext } from "react";

const IDENTITY_CONNECT_ID = import.meta.env.VITE_IC_DAPP_ID;

const plugins = [
  ... (IDENTITY_CONNECT_ID ? [
    new IdentityConnectWallet(IDENTITY_CONNECT_ID, {
      networkName: NetworkName.Mainnet,
    })] : []),
  new PetraWallet(),
  new MartianWallet(),
  new FewchaWallet(),
  new PontemWallet(),
  new RiseWallet(),
];

const connectedWalletName = localStorage.getItem("AptosWalletName") as WalletName;
const walletAdapter = new WalletCore(plugins);
if (connectedWalletName) {
  walletAdapter.connect(connectedWalletName);
}

export const WalletAdapterContext = createContext(walletAdapter);

export function useWalletAdapter() {
  return useContext(WalletAdapterContext);
}
