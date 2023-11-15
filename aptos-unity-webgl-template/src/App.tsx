import { WalletConnector } from "@aptos-labs/wallet-adapter-mui-design";
import { useEffect, useRef } from 'react';
import { Unity } from 'react-unity-webgl';
import { useUnityInstanceContext } from "./providers/unity-instance";
import { useWalletRequestHandler } from "./hooks/useWalletRequestHandler";
import './App.css';

/* eslint-disable @typescript-eslint/no-unused-vars */

function App() {
  const { unityProvider, isLoaded, loadingProgression, config } = useUnityInstanceContext();
  const canvasRef = useRef<HTMLCanvasElement>(null);

  const openWalletSelector = useRef<() => void>(() => { });
  useEffect(() => {
    const walletButton = document.querySelector('.wallet-button') as HTMLButtonElement;
    if (walletButton) {
      walletButton.style.marginBottom = '16px';
      openWalletSelector.current = () => {
        walletButton.click();
      }
    }
  }, []);

  useWalletRequestHandler({ openWalletSelector: openWalletSelector.current });

  return (
    <div id="unity-container" className="unity-desktop">
      <WalletConnector />
      <Unity
        unityProvider={unityProvider}
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
          <div style={{ width: `${100 * loadingProgression}%` }} id="unity-progress-bar-full"></div>
        </div>
      </div>
      <div id="unity-warning"> </div>
      <div id="unity-footer">
        <div id="unity-webgl-logo"></div>
        <div id="unity-fullscreen-button"></div>
        <div id="unity-build-title">{config.productName}</div>
      </div>
    </div>
  )
}

export default App
