import { BCS, TxnBuilderTypes } from 'aptos';
import { ADMIN_ACCOUNT_ADDRESS } from '../constants';

export function buildSaveSessionPayload(userAddress: string, survivalTimeMs: number) {
  const entryFunction = TxnBuilderTypes.EntryFunction.natural(
    `${ADMIN_ACCOUNT_ADDRESS}::star_fighter`,
    'save_game_session',
    [],
    [
      BCS.bcsToBytes(TxnBuilderTypes.AccountAddress.fromHex(userAddress)),
      BCS.bcsSerializeUint64(survivalTimeMs),
    ],
  );
  return new TxnBuilderTypes.TransactionPayloadEntryFunction(entryFunction);
}