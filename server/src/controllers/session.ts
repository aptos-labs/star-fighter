import { BCS, HexString, Provider, TxnBuilderTypes, Types, Network } from 'aptos';
import express from 'express';
import expressAsyncHandle from 'express-async-handler';
import { ADMIN_ACCOUNT_ADDRESS, ADMIN_ACCOUNT_SECRET_KEY } from '../constants';
import { ensureAuthenticated } from '../middlewares';
import { getUser } from '../utils';
import { randomUUID } from 'crypto';
import { buildSaveSessionPayload } from '../utils/buildSaveGameSessionPayload';
import { simulateAdminTransactionWithPayload, submitAdminTransactionWithPayload } from '../utils/adminOperations';
import { buildCoinTransferPayload, buildCoinTransferPojoPayload } from '../utils/buildCoinTransferPayload';
import { updateUserStats } from '../utils/usersStats';

export async function getGameSessionMaximumCost(userAddress: string) {
  const payload = buildSaveSessionPayload(userAddress, 100000);
  const { gasUnitPrice, gasUsed } = await simulateAdminTransactionWithPayload(payload);
  return 2n * gasUsed * gasUnitPrice;
}

interface ActiveSession {
  address: string;
  requestedFunds: bigint;
  fundTransferPayload: Types.EntryFunctionPayload;
  createdAt: number;
};

const activeSessions: { [sessionId: string]: ActiveSession } = {};

export const sessionCtrl = express.Router();

sessionCtrl.post<{}>('', ensureAuthenticated, expressAsyncHandle(async (req, res) => {
  const user = getUser(req);

  const requestedFunds = await getGameSessionMaximumCost(user.address);

  const sessionId = randomUUID();
  // const fundTransferPayload = buildCoinTransferPayload(ADMIN_ACCOUNT_ADDRESS, requestedFunds);
  // const serializedFundTransferPayload = HexString.fromUint8Array(BCS.bcsToBytes(fundTransferPayload)).toString();

  const fundTransferPayload = buildCoinTransferPojoPayload(ADMIN_ACCOUNT_ADDRESS, requestedFunds);
  const serializedFundTransferPayload = JSON.stringify(fundTransferPayload);

  activeSessions[sessionId] = {
    address: user.address,
    createdAt: Date.now(),
    requestedFunds,
    fundTransferPayload,
  };

  res.status(200).json(
    {
      sessionId,
      fundTransferPayload: serializedFundTransferPayload,
    }
  );
}));

type PatchSessionRequestParams = {
  id: string;
};

interface PatchSessionRequestBody {
  survivalTimeMs: number;
  fundTransferTxnHash: string;
}

sessionCtrl.patch<PatchSessionRequestParams, {}, PatchSessionRequestBody>('/:id', ensureAuthenticated, expressAsyncHandle(async (req, res) => {
  const user = getUser(req);
  const sessionId = req.params.id;
  const session = activeSessions[sessionId];
  if (!session) {
    res.status(404).json();
    return;
  }

  const gasUnitPrice = 100;
  const maxGasAmount = Math.floor(Number(session.requestedFunds) / gasUnitPrice);

  const payload = buildSaveSessionPayload(user.address, req.body.survivalTimeMs);
  const userTxn = await submitAdminTransactionWithPayload(payload, {
    gasUnitPrice,
    maxGasAmount,
  });

  if (!userTxn.success) {
    res.status(500).json(userTxn.vm_status);
    return;
  }

  await updateUserStats(user.address, req.body.survivalTimeMs);

  // const signatureBytes = new HexString(req.body.signature).toUint8Array();
  // const deserializer = new BCS.Deserializer(signatureBytes);
  // const signature = TxnBuilderTypes.Ed25519Signature.deserialize(deserializer);

  // const publicKeyBytes = new HexString(user.publicKey).toUint8Array();
  // const txnAuthenticator = new TxnBuilderTypes.TransactionAuthenticatorEd25519(
  //   new TxnBuilderTypes.Ed25519PublicKey(publicKeyBytes),
  //   signature,
  // );

  // const signedTxn = new TxnBuilderTypes.SignedTransaction(
  //   session.fundTxn,
  //   txnAuthenticator,
  // );

  // const signedTxnBytes = BCS.bcsToBytes(signedTxn);
  // const pendingTxn = await aptosProvider.submitSignedBCSTransaction(signedTxnBytes);
  // const userTxn = (await aptosProvider.waitForTransactionWithResult(pendingTxn.hash)) as Types.UserTransaction;

  // if (!userTxn.success) {
  //   res.status(500).json();
  //   return;
  // }

  // generate game end payload
  // simulate game end txn cost
  // if funded amount is enough, submit transaction
  // possibly send money back? probably not worth bothering

  res.status(204).json();
}));

