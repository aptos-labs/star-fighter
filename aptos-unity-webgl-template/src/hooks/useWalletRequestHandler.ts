import { useCallback, useEffect } from "react";
import { useWalletAdapter } from "./useWalletAdapter";
import type { UnityContext } from "./useUnityContext";

/* eslint-disable @typescript-eslint/no-explicit-any */

export interface AptosWalletSerializedRequest {
  method: string;
  args?: string;
}

function isAptosWalletSerializedRequest(request: any): request is AptosWalletSerializedRequest {
  return request !== undefined && typeof request.method === 'string';
}

export interface AptosWalletRequest {
  method: string;
  args?: any;
}

const rootGameObjectName = '[Root]';

export interface UseWalletRequestHandlerProps {
  unityContext: UnityContext,
  onConnect: () => Promise<boolean>;
}

export function useWalletRequestHandler({ unityContext, onConnect }: UseWalletRequestHandlerProps) {
  const walletAdapter = useWalletAdapter();


  // useEffect(() => {
  //   let activeWallet: Wallet | undefined;

  //   function setupListeners() {
  //     const connectedWalletName = walletAdapter.wallet?.name;
  //     activeWallet = connectedWalletName && walletAdapter.wallets.find((w) => w.name === connectedWalletName);
  //     console.log('setting up', activeWallet?.name);

  //     activeWallet?.provider?.onAccountChange((newAccount: AccountInfo) => {
  //       console.log(`account changed from ${walletAdapter.account?.address} to ${newAccount}`);
  //       console.log('Detected account change:', newAccount);
  //       const serializedAccount = JSON.stringify(newAccount ?? null);
  //       unityContext.sendMessage('[Root]', 'OnAptosWalletAccountChange', serializedAccount);
  //     });
  //     activeWallet?.provider?.onNetworkChange((newNetwork: NetworkInfo) => {
  //       console.log('Detected network change from', walletAdapter.network, 'to', newNetwork);
  //       const serializedNetwork = JSON.stringify(newNetwork ?? null);
  //       unityContext.sendMessage('[Root]', 'OnAptosWalletNetworkChange', serializedNetwork);
  //     });
  //   }

  //   function teardownListeners() {
  //     console.log('tearing down', activeWallet?.name);
  //     activeWallet?.provider?.onAccountChange(undefined);
  //     activeWallet?.provider?.onNetworkChange(undefined);
  //     activeWallet = undefined;
  //   }

  //   setupListeners();
  //   walletAdapter.on("connect", setupListeners);
  //   walletAdapter.on("disconnect", teardownListeners);
  //   return () => {
  //     teardownListeners();
  //     walletAdapter.off("connect", setupListeners);
  //     walletAdapter.off("disconnect", teardownListeners);
  //   };
  // }, [walletAdapter]);

  // const connectedAddress = useRef<string>();
  // useEffect(() => {
  //   let timeoutHandle: number | undefined;
  //   function async checkAddress() {
  //     const currWalletName = walletAdapter.wallet?.name;
  //     const currWallet = currWalletName && walletAdapter.wallets.find((w) => w.name === currWalletName);
  //     const currConnectedAddress = await(currWallet as any)?.account();
  //     console.log('checking', connectedAddress.current, currConnectedAddress);
  //     if (currConnectedAddress !== connectedAddress.current) {
  //       console.log(`[webgl] Account change detected, from ${connectedAddress.current} to ${currConnectedAddress}`);
  //       connectedAddress.current = currConnectedAddress;
  //     }
  //     timeoutHandle = setTimeout(checkAddress, 1000);
  //   }
  //   checkAddress();
  //   return () => clearTimeout(timeoutHandle);
  // }, [walletAdapter]);

  // useEffect(() => {
  //   // setConnectedWallet(walletAdapter.wallets.find((w) => w.name === walletAdapter.wallet?.name));
  //   const onConnect = (args: unknown) => {
  //     console.log('onconnect', args);
  //     // setConnectedWallet(walletAdapter.wallets.find((w) => w.name === walletAdapter.wallet?.name));
  //   };

  //   const onDisconnect = (args: unknown) => {
  //     console.log('ondisconnect', args);
  //     // setConnectedWallet(undefined);
  //   };

  //   walletAdapter.on("connect", onConnect);
  //   walletAdapter.on("disconnect", onDisconnect);
  //   return () => {
  //     walletAdapter.off("connect", onConnect);
  //     walletAdapter.off("disconnect", onDisconnect);
  //   };
  // }, [walletAdapter]);

  // useEffect(() => {
  //   const currConnectedWallet = connectedWallet;
  //   console.log('curr wallet change', currConnectedWallet?.name);

  //   if (currConnectedWallet) {
  //     console.log('setting up listeners', currConnectedWallet.name);
  //     connectedWallet.onAccountChange(async (newAccount) => {
  //       console.log('Detected account change:', newAccount);
  //       // const serializedAccount = JSON.stringify(newAccount ?? null);
  //       // unityContext.sendMessage('[Root]', 'OnAptosWalletAccountChange', serializedAccount);
  //     });
  //     connectedWallet.onNetworkChange(async (newNetwork) => {
  //       console.log('Detected network change:', newNetwork);
  //       // const serializedNetwork = JSON.stringify(newNetwork ?? null);
  //       // unityContext.sendMessage('[Root]', 'OnAptosWalletNetworkChange', serializedNetwork);
  //     });
  //   }


  //   return () => {
  //     console.log('tearing down listeners', currConnectedWallet?.name);
  //     currConnectedWallet?.onAccountChange(async () => { });
  //     currConnectedWallet?.onNetworkChange(async () => { });
  //     currConnectedWallet?.provider?.off('accountChange');
  //     currConnectedWallet?.provider?.off('networkChange');
  //   };
  // }, [connectedWallet, unityContext, walletAdapter]);

  // useEffect(() => {
  //   console.log('Detected account change:', wallet.account);
  //   const serializedAccount = JSON.stringify(wallet.account ?? null);
  //   unityContext.sendMessage('[Root]', 'OnAptosWalletAccountChange', serializedAccount);
  // }, [unityContext, wallet.account]);

  // useEffect(() => {
  //   console.log('Detected network change:', wallet.network);
  //   const serializedNetwork = JSON.stringify(wallet.network ?? null);
  //   unityContext.sendMessage('[Root]', 'OnAptosWalletNetworkChange', serializedNetwork);
  // }, [unityContext, wallet.network]);

  const handleRequest = useCallback(async (method: string, args?: any) => {
    console.log('[webgl] Handling deserialized request', { method, args });
    if (method === 'connect') {
      return onConnect();
    } else if (method === 'isConnected') {
      return walletAdapter.isConnected();
    } else if (method === 'disconnect') {
      return walletAdapter.disconnect();
    } else if (method === 'getAccount') {
      return walletAdapter.account;
    } else if (method === 'getNetwork') {
      return walletAdapter.network;
    } else if (method === 'signAndSubmitTransaction') {
      return await walletAdapter.signAndSubmitTransaction(args);
    }
    throw new Error('Method not supported');
  }, [onConnect, walletAdapter]);

  const onAptosWalletRequest = useCallback(async (request: unknown) => {
    try {
      console.log('[webgl] Received request', request);
      if (!isAptosWalletSerializedRequest(request)) {
        throw new Error('Request malformed');
      }

      const requestArgs = request.args && JSON.parse(request.args);
      const responseArgs = await handleRequest(request.method, requestArgs);

      console.log('[webgl] Request handled with response', responseArgs);

      const serializedResponseArgs = JSON.stringify(responseArgs ?? true)
      unityContext.sendMessage(rootGameObjectName, "OnAptosWalletResponseSuccess", serializedResponseArgs);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : JSON.stringify(err);
      unityContext.sendMessage(rootGameObjectName, "OnAptosWalletResponseError", errorMessage);
    }
  }, [handleRequest, unityContext]);

  useEffect(() => {
    unityContext.addEventListener("aptosWalletRequest", onAptosWalletRequest as any);
    return () => {
      unityContext.removeEventListener("aptosWalletRequest", onAptosWalletRequest as any);
    };
  }, [onAptosWalletRequest, unityContext]);
}
