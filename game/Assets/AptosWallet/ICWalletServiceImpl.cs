using IdentityConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uniffi;
using UnityEngine;

public abstract class ICWalletServiceImpl : MonoBehaviour, IWalletService
{
  private static PlayerPrefsDappStateAccessors IC_DAPP_STATE_ACCESSORS = new PlayerPrefsDappStateAccessors();

  private ICDappClient icDappClient;
  private ICPairingData activePairing;

  public event EventHandler<AccountData> OnConnect;
  public event EventHandler OnDisconnect;

  public async void Awake()
  {
    icDappClient = new ICDappClient(
      Constants.IC_DAPP_ID,
      Constants.IC_DAPP_HOSTNAME,
      IC_DAPP_STATE_ACCESSORS,
      new ICDappClientConfig { baseUrl = Constants.IC_BASE_URL });

    var pairings = await IC_DAPP_STATE_ACCESSORS.GetAll();
    activePairing = pairings.Values.FirstOrDefault();
  }

  #region WalletService

  public bool IsConnected => activePairing != null;

  public AccountData AccountData
  {
    get
    {
      if (activePairing == null)
      {
        return null;
      }

      var publicKeyBytes = Convert.FromBase64String(activePairing.accountEd25519PublicKeyB64);
      return new AccountData
      {
        address = activePairing.accountAddress,
        pairingId = activePairing.pairingId,
        publicKey = IdentityConnectMethods.EncodeHex(publicKeyBytes),
      };
    }
  }

  public async Task Connect(CancellationToken? cancellationToken = null)
  {
    var pairingRequest = await icDappClient.CreatePairingRequest(cancellationToken);
    var dappEd25519PublicKeyBytes = Convert.FromBase64String(pairingRequest.dappEd25519PublicKeyB64);

    var environmentOrBaseUrl = Constants.IC_ENVIRONMENT_OR_BASE_URL;
    var url = $"identity-connect:///anonymous-pairings/{pairingRequest.pairingId}/finalize?environment={environmentOrBaseUrl}";
    BroadcastMessage("OnIcPairingInitialized", url);

    var pairing = await icDappClient.WaitForFinalizedPairingRequest(pairingRequest, cancellationToken);

    activePairing = await IC_DAPP_STATE_ACCESSORS.Get(pairing.account.accountAddress);
    var publicKeyBytes = Convert.FromBase64String(pairing.account.ed25519PublicKeyB64);
    var accountData = new AccountData
    {
      address = pairing.account.accountAddress,
      publicKey = IdentityConnectMethods.EncodeHex(publicKeyBytes),
    };
    OnConnect?.Invoke(this, accountData);
  }

  public async Task Disconnect()
  {
    await IC_DAPP_STATE_ACCESSORS.Update(activePairing.accountAddress, null);
  }

  public async Task<string> SignAndSubmitTransaction(string serializedPayload, CancellationToken? cancellationToken = null)
  {
    var payload = JsonConvert.DeserializeObject<JToken>(serializedPayload);
    var requestArgs = new SignAndSubmitTransactionWithPojoPayloadRequestArgs(payload);
    var requestOptions = new SignatureRequestOptions { cancellationToken = cancellationToken };
    var response = await icDappClient.SignAndSubmitTransaction(
      activePairing.accountAddress,
      requestArgs,
      requestOptions
    );
    return response.hash;
  }

  #endregion
}
