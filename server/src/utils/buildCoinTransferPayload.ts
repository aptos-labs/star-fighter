import { BCS, TxnBuilderTypes } from 'aptos';

export function buildCoinTransferPayload(recipient: string, amount: number | bigint) {
  const coinTypeTag = new TxnBuilderTypes.TypeTagStruct(
    TxnBuilderTypes.StructTag.fromString('0x1::aptos_coin::AptosCoin')
  );

  const entryFunction = TxnBuilderTypes.EntryFunction.natural(
    '0x1::coin',
    'transfer',
    [coinTypeTag],
    [
      BCS.bcsToBytes(TxnBuilderTypes.AccountAddress.fromHex(recipient)),
      BCS.bcsSerializeUint64(amount),
    ],
  );
  return new TxnBuilderTypes.TransactionPayloadEntryFunction(entryFunction);
}

export function buildCoinTransferPojoPayload(recipient: string, amount: number | bigint) {
  return {
    type: 'entry_function_payload',
    function: '0x1::coin::transfer',
    type_arguments: ['0x1::aptos_coin::AptosCoin'],
    arguments: [recipient, amount.toString()]
  };
}
