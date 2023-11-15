import { BCS, HexString, Provider, TxnBuilderTypes, Types, Network, AptosAccount, TransactionBuilder } from 'aptos';
import express from 'express';
import expressAsyncHandle from 'express-async-handler';
import { ADMIN_ACCOUNT_ADDRESS, ADMIN_ACCOUNT_SECRET_KEY, APTOS_NETWORK } from '../constants';
import { ensureAuthenticated } from '../middlewares';
import { getUser } from '../utils';
import { randomUUID } from 'crypto';
import { buildSaveSessionPayload } from './buildSaveGameSessionPayload';

export const aptosProvider = new Provider(APTOS_NETWORK as any);

const adminSecretKeyBytes = new HexString(ADMIN_ACCOUNT_SECRET_KEY).toUint8Array();
const adminSigner = new AptosAccount(adminSecretKeyBytes, ADMIN_ACCOUNT_ADDRESS);

let adminPendingOperations: Promise<any> | undefined = undefined;

interface TransactionOptions {
  gasUnitPrice: bigint | number;
  maxGasAmount: bigint | number;
}

async function simulateAdminTransactionWithPayloadInternal(payload: TxnBuilderTypes.TransactionPayloadEntryFunction) {
  const accountData = await aptosProvider.getAccount(ADMIN_ACCOUNT_ADDRESS);
  const sequenceNumber = BigInt(accountData.sequence_number);

  const expirationTimestamp = Math.floor(Date.now() / 1000) + 120;
  const rawTxn = new TxnBuilderTypes.RawTransaction(
    TxnBuilderTypes.AccountAddress.fromHex(ADMIN_ACCOUNT_ADDRESS),
    sequenceNumber,
    payload,
    BigInt(0),
    BigInt(0),
    BigInt(expirationTimestamp),
    new TxnBuilderTypes.ChainId(await aptosProvider.getChainId())
  );

  const txnAuthenticator = new TxnBuilderTypes.TransactionAuthenticatorEd25519(
    new TxnBuilderTypes.Ed25519PublicKey(adminSigner.signingKey.publicKey),
    new TxnBuilderTypes.Ed25519Signature(new Uint8Array(64)),
  );

  const signedTxn = new TxnBuilderTypes.SignedTransaction(rawTxn, txnAuthenticator);

  const [simulatedUserTxn] = await aptosProvider.submitBCSSimulation(BCS.bcsToBytes(signedTxn), {
    estimateGasUnitPrice: true,
    estimateMaxGasAmount: true,
  });

  if (!simulatedUserTxn.success) {
    throw new Error(simulatedUserTxn.vm_status);
  }

  return {
    gasUnitPrice: BigInt(simulatedUserTxn.gas_unit_price),
    gasUsed: BigInt(simulatedUserTxn.gas_used)
  };
}

async function submitAdminTransactionWithPayloadInternal(payload: TxnBuilderTypes.TransactionPayloadEntryFunction, options: TransactionOptions) {
  const accountData = await aptosProvider.getAccount(ADMIN_ACCOUNT_ADDRESS);
  const sequenceNumber = BigInt(accountData.sequence_number);

  const expirationTimestamp = Math.floor(Date.now() / 1000) + 120;
  const rawTxn = new TxnBuilderTypes.RawTransaction(
    TxnBuilderTypes.AccountAddress.fromHex(ADMIN_ACCOUNT_ADDRESS),
    sequenceNumber,
    payload,
    BigInt(options.maxGasAmount),
    BigInt(options.gasUnitPrice),
    BigInt(expirationTimestamp),
    new TxnBuilderTypes.ChainId(await aptosProvider.getChainId())
  );

  const txnSigningMessage = TransactionBuilder.getSigningMessage(rawTxn);
  const signatureBytes = adminSigner.signBuffer(txnSigningMessage).toUint8Array();
  const txnAuthenticator = new TxnBuilderTypes.TransactionAuthenticatorEd25519(
    new TxnBuilderTypes.Ed25519PublicKey(adminSigner.signingKey.publicKey),
    new TxnBuilderTypes.Ed25519Signature(signatureBytes),
  );

  const signedTxn = new TxnBuilderTypes.SignedTransaction(rawTxn, txnAuthenticator);

  const pendingTxn = await aptosProvider.submitSignedBCSTransaction(BCS.bcsToBytes(signedTxn));
  const userTxn = await aptosProvider.waitForTransactionWithResult(pendingTxn.hash);
  return userTxn as Types.UserTransaction;
}

export async function simulateAdminTransactionWithPayload(payload: TxnBuilderTypes.TransactionPayloadEntryFunction) {
  const currPendingOperations = adminPendingOperations?.catch() ?? Promise.resolve();
  const newPendingOperations = currPendingOperations.then(() => simulateAdminTransactionWithPayloadInternal(payload));
  adminPendingOperations = newPendingOperations;
  return newPendingOperations;
}

export async function submitAdminTransactionWithPayload(payload: TxnBuilderTypes.TransactionPayloadEntryFunction, options: TransactionOptions) {
  const currPendingOperations = adminPendingOperations?.catch() ?? Promise.resolve();
  const newPendingOperations = currPendingOperations.then(() => submitAdminTransactionWithPayloadInternal(payload, options));
  adminPendingOperations = newPendingOperations;
  return newPendingOperations;
}
