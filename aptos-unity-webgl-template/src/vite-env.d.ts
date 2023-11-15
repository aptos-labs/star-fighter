/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_IC_DAPP_ID?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
