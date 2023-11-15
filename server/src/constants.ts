import { Network } from "@aptos-labs/ts-sdk";

export const SERVER_PORT = process.env.SERVER_PORT || 8080;

export const ADMIN_ACCOUNT_ADDRESS = process.env.ADMIN_ADDRESS!;
export const ADMIN_ACCOUNT_SECRET_KEY = process.env.ADMIN_SECRET_KEY!;

export const JWT_SECRET = process.env.JWT_SECRET!;

export const IDENTITY_CONNECT_DAPP_ID = process.env.IDENTITY_CONNECT_DAPP_ID!;
export const IDENTITY_CONNECT_REFERER = process.env.IDENTITY_CONNECT_REFERER!;
const IDENTITY_CONNECT_ENVIRONMENTS_URLS = {
  production: 'https://identityconnect.com',
  staging: 'https://identity-connect.staging.gcp.aptosdev.com',
};
// export const IDENTITY_CONNECT_ENVIRONMENT_OR_BASE_URL = 'http://ic.com:8083';
export const IDENTITY_CONNECT_ENVIRONMENT_OR_BASE_URL = 'production';

function isIcEnvironment(environmentOrBaseUrl: string): environmentOrBaseUrl is keyof typeof IDENTITY_CONNECT_ENVIRONMENTS_URLS {
  return Object.keys(IDENTITY_CONNECT_ENVIRONMENTS_URLS).includes(environmentOrBaseUrl);
}

export const IDENTITY_CONNECT_BASE_URL = isIcEnvironment(IDENTITY_CONNECT_ENVIRONMENT_OR_BASE_URL)
  ? IDENTITY_CONNECT_ENVIRONMENTS_URLS[IDENTITY_CONNECT_ENVIRONMENT_OR_BASE_URL]
  : IDENTITY_CONNECT_ENVIRONMENT_OR_BASE_URL;

export const APTOS_NETWORK = Network.MAINNET;