import { GetPairingSerializedResponse, CreatePairingSerializedResponse } from '@identity-connect/api';
import axios, { AxiosInstance } from 'axios';
import {
  IDENTITY_CONNECT_DAPP_ID,
  IDENTITY_CONNECT_REFERER,
  IDENTITY_CONNECT_BASE_URL
} from '../constants';

export class ICDappBackendClient {
  private readonly axiosInstance: AxiosInstance;

  public constructor() {
    this.axiosInstance = axios.create({
      baseURL: IDENTITY_CONNECT_BASE_URL,
    });
  }

  public async getPairing(id: string) {
    const response = await this.axiosInstance.get<GetPairingSerializedResponse>(`v1/pairing/${id}`, {
      validateStatus: (status) => status === 200 || status === 404,
    });
    return response?.data.data.pairing;
  }

  public async createPairing(dappEd25519PublicKeyB64: string) {
    const response = await this.axiosInstance.post<CreatePairingSerializedResponse>('v1/pairing', {
      dappEd25519PublicKeyB64,
      dappId: IDENTITY_CONNECT_DAPP_ID,
    }, {
      headers: {
        Referer: IDENTITY_CONNECT_REFERER
      }
    });
    return response.data.data.pairing;
  }
}