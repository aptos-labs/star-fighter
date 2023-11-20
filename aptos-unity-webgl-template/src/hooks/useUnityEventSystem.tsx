import { useUnityContext } from "react-unity-webgl";
import { unityConfig } from "./useUnityContext";

export function useUnityEventSystem() {
  const { addEventListener, removeEventListener } = useUnityContext(unityConfig);
  return { addEventListener, removeEventListener };
}
