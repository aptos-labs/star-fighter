import { PropsWithChildren, createContext, useContext, useMemo } from "react";
import { useUnityContext } from "react-unity-webgl";

declare global {
  const unityBuildConfig: {
    codeUrl: string;
    dataUrl: string;
    frameworkUrl: string;
    loaderUrl: string;
    streamingAssetsUrl: string;
    companyName: string;
    productName: string;
    productVersion: string;
    showBanner: boolean;
  };
}

function getUnityConfig() {
  if (import.meta.env.DEV) {
    const subfolder = '/Build';
    const productName = 'star-fighter'
    return {
      codeUrl: `${subfolder}/${productName}.wasm.gz`,
      dataUrl: `${subfolder}/${productName}.data.gz`,
      frameworkUrl: `${subfolder}/${productName}.framework.js.gz`,
      loaderUrl: `${subfolder}/${productName}.loader.js`,
      productName,
    };
  }
  return unityBuildConfig;
}

export type UnityConfig = ReturnType<typeof getUnityConfig>;
export type UnityContextValue = ReturnType<typeof useUnityContext>;
export type UnityInstanceContextValue = UnityContextValue & { config: UnityConfig };

const UnityInstanceContext = createContext<UnityInstanceContextValue | undefined>(undefined);

export function UnityInstanceContextProvider({ children }: PropsWithChildren) {
  const unityConfig = getUnityConfig();
  const internalContextValue = useUnityContext(unityConfig);
  const contextValue = useMemo(() => ({ ...internalContextValue, config: unityConfig }), [internalContextValue, unityConfig]);

  return <UnityInstanceContext.Provider value={contextValue}>
    {children}
  </UnityInstanceContext.Provider>
}

export function useUnityInstanceContext() {
  const contextValue = useContext(UnityInstanceContext);
  if (contextValue === undefined) {
    throw new Error('No provider found for UnityInstanceContext');
  }
  return contextValue;
}
