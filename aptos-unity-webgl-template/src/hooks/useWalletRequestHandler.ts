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
    if (wallet.account) {
      const serializedAccount = JSON.stringify(wallet.account);
      unityContext.sendMessage('[Root]', 'OnAptosWalletConnect', serializedAccount);
    }
  }, [unityContext, wallet.account]);

  const handleRequest = useCallback(async (method: string, args?: any) => {
    console.log('Handling request', method, args);
    if (method === 'connect') {
      openWalletSelector();
      return;
    } else if (method === 'isConnected') {
      return wallet.connected;
    } else if (method === 'disconnect') {
      wallet.disconnect();
      return;
    } else if (method === 'getAccount') {
      return wallet.account;
    } else if (method === 'signAndSubmitTransaction') {
      const result = await wallet.signAndSubmitTransaction(args);
      console.log(result);
      return result;
    }
    throw new Error('Method not supported');
  }, [wallet]);

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
