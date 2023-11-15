import axios, { isAxiosError } from 'axios';
import express from 'express';
import handleAsync from 'express-async-handler';
import { body, oneOf } from 'express-validator';
import jwt from 'jsonwebtoken';
import { IDENTITY_CONNECT_BASE_URL, IDENTITY_CONNECT_ENVIRONMENT_OR_BASE_URL, JWT_SECRET } from '../constants';
import { ensureValidRequest } from '../middlewares';
import { ICDappBackendClient } from '../utils/ICDappBackendClient';
import { PairingStatus } from '@identity-connect/api';
import { Hex } from '@aptos-labs/ts-sdk';

export const authCtrl = express.Router();

export interface CreateTokenResponseBody {
  token: string;
}

export interface CreateTokenWithNoProofOfOwnershipRequestBody {
  address: string;
  publicKey: string;
}

export interface CreateTokenWithIcPairingRequestBody {
  pairingId: string;
}

export type CreateTokenRequestBody = CreateTokenWithNoProofOfOwnershipRequestBody | CreateTokenWithIcPairingRequestBody;

authCtrl.post<{}, CreateTokenResponseBody, CreateTokenRequestBody>('/tokens',
  oneOf([
    [
      body('address').isHexadecimal(),
      body('publicKey').isHexadecimal(),
    ],
    [
      body('pairingId').isUUID(),
    ]
  ]),
  ensureValidRequest,
  handleAsync(async (req, res) => {
    if ('address' in req.body) {
      const token = jwt.sign({
        address: req.body.address,
        publicKey: req.body.publicKey,
      }, JWT_SECRET);
      res.status(200).json({ token });
      return;
    }

    const icDappBackendClient = new ICDappBackendClient();
    const pairing = await icDappBackendClient.getPairing(req.body.pairingId);

    if (!pairing || pairing.status !== PairingStatus.Finalized) {
      res.status(404).json();
      return;
    }

    const publicKeyBytes = Buffer.from(pairing.account.ed25519PublicKeyB64, 'base64');
    const publicKey = Hex.fromHexInput(publicKeyBytes).toString();

    const token = jwt.sign({
      address: pairing.account.accountAddress,
      publicKey,
      pairingId: pairing.id,
    }, JWT_SECRET);
    res.status(200).json({ token });
  }));

export interface CreateIcPairingRequestBody {
  dappEd25519PublicKeyB64: string,
}

export interface CreateIcPairingResponseBody {
  pairingId: string;
  environment?: string;
}

authCtrl.post<{}, CreateIcPairingResponseBody, CreateIcPairingRequestBody>('/ic-pairings',
  body('dappEd25519PublicKeyB64').notEmpty().isBase64(),
  ensureValidRequest,
  handleAsync(async (req, res) => {
    const { dappEd25519PublicKeyB64 } = req.body;
    const icDappBackendClient = new ICDappBackendClient();
    const pairing = await icDappBackendClient.createPairing(dappEd25519PublicKeyB64);
    console.log("created pairing", pairing);
    res.status(200).json({ pairingId: pairing.id, environment: IDENTITY_CONNECT_ENVIRONMENT_OR_BASE_URL });
  }));
