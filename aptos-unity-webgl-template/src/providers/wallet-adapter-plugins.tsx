// Copyright © Aptos
// SPDX-License-Identifier: Apache-2.0

import { AptosWalletAdapterProvider, NetworkName } from "@aptos-labs/wallet-adapter-react";
import { IdentityConnectWallet } from "@identity-connect/wallet-adapter-plugin";
import { MartianWallet } from "@martianwallet/aptos-wallet-adapter";
import { PontemWallet } from "@pontem/wallet-adapter-plugin";
import { RiseWallet } from "@rise-wallet/wallet-adapter";
import { FewchaWallet } from "fewcha-plugin-wallet-adapter";
import { PetraWallet } from "petra-plugin-wallet-adapter";
import { PropsWithChildren, useMemo } from "react";

const IDENTITY_CONNECT_ID = import.meta.env.VITE_IC_DAPP_ID;

export function WalletAdapterPluginsProvider({ children }: PropsWithChildren) {
  const wallets = useMemo(
    () => [
      ... (IDENTITY_CONNECT_ID ? [
        new IdentityConnectWallet(IDENTITY_CONNECT_ID, {
          networkName: NetworkName.Mainnet,
        })] : []),
      new PetraWallet(),
      new MartianWallet(),
      new FewchaWallet(),
      new PontemWallet(),
      new RiseWallet(),
    ],
    [],
  );

  return (
    <AptosWalletAdapterProvider plugins={wallets} autoConnect={true}>
      {children}
    </AptosWalletAdapterProvider>
  );
}