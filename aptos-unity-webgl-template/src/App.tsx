import { useCallback, useRef, useState } from 'react';
import { Unity } from 'react-unity-webgl';
import { useUnityContext } from './hooks/useUnityContext';
import { useWalletRequestHandler } from './hooks/useWalletRequestHandler';
import { WalletsModal, WalletsModalHandle } from "./WalletsModal";
import './App.css';

function App() {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const walletsModalRef = useRef<WalletsModalHandle>(null);

  const [isLoaded, setIsLoaded] = useState(false);
  const progressBarRef = useRef<HTMLDivElement>(null);

  const onLoadingProgress = useCallback((progress: number) => {
    if (progress >= 1) {
      setIsLoaded(true);
    } else if (progressBarRef.current)
      progressBarRef.current.style.width = `${progress * 100}%`;
  }, []);

  const unityContext = useUnityContext({ onLoadingProgress });

  useWalletRequestHandler({
    unityContext,
    onConnect: async () => walletsModalRef.current?.connect() ?? false
  });

  return (
    <div id="unity-container" className="unity-desktop">
      <WalletsModal ref={walletsModalRef} />
      <Unity
        unityProvider={unityContext.provider}
        style={{
          height: 768,
          width: 1024,
          background: '#231F20'
        }}
        devicePixelRatio={window.devicePixelRatio}
        disabledCanvasEvents={["dragstart"]}
        ref={canvasRef}
      />
      <div style={{ display: isLoaded ? 'none' : 'block' }} id="unity-loading-bar">
        <div id="unity-logo"></div>
        <div id="unity-progress-bar-empty">
          <div
            ref={progressBarRef}
            id="unity-progress-bar-full" />
        </div>
      </div>
      <div id="unity-warning"> </div>
      <div id="unity-footer">
        <div id="unity-webgl-logo"></div>
        <div id="unity-fullscreen-button"></div>
        <div id="unity-build-title">{unityContext.config.productName}</div>
      </div>
    </div>
  )
}

export default App;
