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
  private static PlayerPrefsDappStateAccessors IC_DAPP_STATE_ACCESSORS = new();

  private ICDappClient icDappClient;
  private ICPairingData activePairing;

  public event EventHandler OnDisconnect;

  public ICDappClient ICDappClient => icDappClient;

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

  public void OnEnable()
  {
    icDappClient.OnDisconnect += OnIcDisconnect;
  }

  public void OnDisable()
  {
    icDappClient.OnDisconnect -= OnIcDisconnect;
  }

  public void OnIcDisconnect(object sender, string pairingId)
  {
    Debug.Log("OnIcDisconnect");
    if (activePairing != null && activePairing.pairingId == pairingId)
    {
      Debug.Log($"Triggering with {pairingId}");
      activePairing = null;
      OnDisconnect?.Invoke(this, null);
    }
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

  public async Task<AccountData> Connect(CancellationToken? cancellationToken = null)
  {
    var pairingRequest = await icDappClient.CreatePairingRequest(cancellationToken);

    var environmentOrBaseUrl = Constants.IC_ENVIRONMENT_OR_BASE_URL;
    var url = $"identity-connect:///anonymous-pairings/{pairingRequest.pairingId}/finalize?environment={environmentOrBaseUrl}";

    var ui = GameObject.Find("[UI]");
    ui.BroadcastMessage("OnIcPairingInitialized", url);

    var pairing = await icDappClient.WaitForFinalizedPairingRequest(pairingRequest, cancellationToken);
    activePairing = await IC_DAPP_STATE_ACCESSORS.Get(pairing.account.accountAddress);
    return AccountData;
  }

  public async Task Disconnect()
  {
    if (activePairing != null)
    {
      await IC_DAPP_STATE_ACCESSORS.Update(activePairing.accountAddress, null);
    }
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
