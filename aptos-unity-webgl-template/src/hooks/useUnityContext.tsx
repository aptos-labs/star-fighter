import { useMemo, useRef } from "react";
import { useUnityContext as useInternalUnityContext } from "react-unity-webgl";
import type { UnityConfig } from "react-unity-webgl";
import type { UnityInstance } from "react-unity-webgl/declarations/unity-instance";
import type { ReactUnityEventParameter } from "react-unity-webgl/distribution/types/react-unity-event-parameters";
import type { UnityProvider } from "react-unity-webgl/distribution/types/unity-provider";

declare global {
  const unityBuildConfig: UnityConfig;
}

function getUnityConfig(): UnityConfig {
  if (import.meta.env.DEV) {
    const subfolder = '/Build';
    const productName = 'star-fighter'
    return {
      codeUrl: `${subfolder}/${productName}.wasm.gz`,
      dataUrl: `${subfolder}/${productName}.data.gz`,
      frameworkUrl: `${subfolder}/${productName}.framework.js.gz`,
      loaderUrl: `${subfolder}/${productName}.loader.js`,
      productName,
      companyName: 'Aptos',
      productVersion: '0.0.1'
    };
  }
  return unityBuildConfig;
}

export const unityConfig = getUnityConfig();

export interface UseUnityProviderProps {
  onLoadingProgress?: (progress: number) => void;
  onLoaded?: () => void;
  onError?: (err: unknown) => void;
}

export function useUnityContext({
  onLoadingProgress,
  onError,
}: UseUnityProviderProps) {
  const unityInstance = useRef<UnityInstance | null>(null);
  const { addEventListener, removeEventListener } = useInternalUnityContext(unityConfig);
  const unityProvider = useMemo<UnityProvider>(() => ({
    setLoadingProgression: (progress: number) => onLoadingProgress?.(progress),
    setInitialisationError: (err) => onError?.(err),
    setUnityInstance: (newValue) => unityInstance.current = newValue,
    setIsLoaded: () => { },
    unityConfig,
  }), [onLoadingProgress, onError]);
  return {
    provider: unityProvider,
    instance: unityInstance,
    config: unityConfig,
    sendMessage: (
      gameObjectName: string,
      methodName: string,
      parameter?: ReactUnityEventParameter
    ) => unityInstance.current?.SendMessage(gameObjectName, methodName, parameter),
    addEventListener,
    removeEventListener,
  }
}

export type UnityContext = ReturnType<typeof useUnityContext>;
