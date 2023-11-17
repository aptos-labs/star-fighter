import { useWallet } from "@aptos-labs/wallet-adapter-react";
import { useCallback, useEffect } from "react";
import { useUnityInstanceContext } from "../providers/unity-instance";

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
  openWalletSelector: () => void;
}

export function useWalletRequestHandler({ openWalletSelector }: UseWalletRequestHandlerProps) {
  const unityContext = useUnityInstanceContext();
  const wallet = useWallet();

  useEffect(() => {
    console.log('Detected account change:', wallet.account);
    const serializedAccount = JSON.stringify(wallet.account ?? null);
    unityContext.sendMessage('[Root]', 'OnAptosWalletAccountChange', serializedAccount);
  }, [unityContext, wallet.account]);

  useEffect(() => {
    console.log('Detected network change:', wallet.network);
    const serializedNetwork = JSON.stringify(wallet.network ?? null);
    unityContext.sendMessage('[Root]', 'OnAptosWalletNetworkChange', serializedNetwork);
  }, [unityContext, wallet.network]);

  const handleRequest = useCallback(async (method: string, args?: any) => {
    console.log('Handling request', method, args);
    if (method === 'connect') {
      return openWalletSelector();
    } else if (method === 'isConnected') {
      return wallet.connected;
    } else if (method === 'disconnect') {
      return wallet.disconnect();
    } else if (method === 'getAccount') {
      return wallet.account;
    } else if (method === 'getNetwork') {
      return wallet.network;
    } else if (method === 'signAndSubmitTransaction') {
      return await wallet.signAndSubmitTransaction(args);
    }
    throw new Error('Method not supported');
  }, [openWalletSelector, wallet]);

  const onAptosWalletRequest = useCallback(async (request: unknown) => {
    try {
      console.log('Received request', request);
      if (!isAptosWalletSerializedRequest(request)) {
        throw new Error('Request malformed');
      }

      const requestArgs = request.args && JSON.parse(request.args);
      const responseArgs = await handleRequest(request.method, requestArgs);
      console.log('Request handled with response', responseArgs);

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
