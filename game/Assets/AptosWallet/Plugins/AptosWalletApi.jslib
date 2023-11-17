mergeInto(LibraryManager.library, {
  SendAptosWalletRequest: async function (serializedRequestPtr) {
    const serializedRequest = JSON.parse(UTF8ToString(serializedRequestPtr));
    dispatchReactUnityEvent('aptosWalletRequest', serializedRequest);
  },
});