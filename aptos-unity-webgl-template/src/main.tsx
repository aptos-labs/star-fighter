import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import { WalletAdapterPluginsProvider } from './providers/wallet-adapter-plugins.tsx'
import { UnityInstanceContextProvider } from './providers/unity-instance.tsx'
import './index.css'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <WalletAdapterPluginsProvider>
      <UnityInstanceContextProvider>
        <App />
      </UnityInstanceContextProvider>
    </WalletAdapterPluginsProvider>
  </React.StrictMode>,
)
