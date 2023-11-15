import { Aptos, AptosConfig } from '@aptos-labs/ts-sdk';
import express from 'express';
import expressAsyncHandle from 'express-async-handler';
import { ensureAuthenticated } from '../middlewares';
import { getUser } from '../utils';
import { getUserStats } from '../utils/usersStats';
import { APTOS_NETWORK } from '../constants';

export const userCtrl = express.Router();

export interface GetUserResponseBody {
  name: string;
  aptBalance: string;
  gamesPlayed: number;
  bestSurvivalTimeMs: number;
  rank?: number;
}

userCtrl.get<{}, GetUserResponseBody>('', ensureAuthenticated, expressAsyncHandle(async (req, res) => {
  const user = getUser(req);
  const config = new AptosConfig({ network: APTOS_NETWORK });
  const aptos = new Aptos(config);

  const coinResource = await aptos.getAccountResource({
    accountAddress: user.address,
    resourceType: '0x1::coin::CoinStore<0x1::aptos_coin::AptosCoin>',
  });
  const aptBalance = BigInt(coinResource.coin.value).toString();

  const { bestSurvivalTimeMs, gamesPlayed, rank } = await getUserStats(user.address);

  res.status(200).json({
    name: user.address,
    aptBalance,
    bestSurvivalTimeMs,
    gamesPlayed,
    rank,
  });
}));
