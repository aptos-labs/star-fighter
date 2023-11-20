using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class WebWalletServiceImpl : MonoBehaviour, IWalletService
{
  public static readonly string CONNECTED_ACCOUNT_KEY = "WebWallet_ConnectedAccount";

  #region Bridge

  /// <summary>
  /// Completion source for the pending wallet request
  /// </summary>
  private TaskCompletionSource<string> completionSource;

  [DllImport("__Internal")]
  private static extern void SendAptosWalletRequest(string serializedRequest);

  /// <summary>
  /// Send a wallet request and wait for the response
  /// </summary>
  /// <param name="method">Method name</param>
  /// <param name="serializedArgs">Optional JSON-serialized args</param>
  /// <returns>Deserialized response args</returns>
  /// <exception cref="InvalidOperationException">Raised when a previous request has not completed yet</exception>
  private async Task<TResponse> SendRequest<TResponse>(string method, string serializedArgs = null)
  {
    if (completionSource != null)
    {
      throw new InvalidOperationException("Another task is pending");
    }
    completionSource = new TaskCompletionSource<string>();
    var serializedRequest = JsonConvert.SerializeObject(new { method, args = serializedArgs });
    SendAptosWalletRequest(serializedRequest);
    var serializedResponse = await completionSource.Task;
    return JsonConvert.DeserializeObject<TResponse>(serializedResponse);
  }

  /// <summary>
  /// Callback for when an account is connected using the wallet adapter
  /// </summary>
  /// <param name="serializedAccountData">JSON-serialized account</param>
  public void OnAptosWalletAccountChange(string serializedAccountData)
  {
    var connectedAccount = AccountData;
    var activeAccount = JsonConvert.DeserializeObject<AccountData>(serializedAccountData);
    Debug.Log($"OnAccountChange: from {connectedAccount?.address} to {activeAccount?.address}");
    if (connectedAccount != null && activeAccount?.address != connectedAccount.address)
    {
      OnDisconnect?.Invoke(this, null);
    }
  }

  /// <summary>
  /// Callback for when the connected wallet switches network
  /// </summary>
  /// <param name="serializedNetworkData">JSON-serialized network</param>
  public void OnAptosWalletNetworkChange(string serializedNetworkData)
  {
    NetworkData = JsonConvert.DeserializeObject<NetworkData>(serializedNetworkData);
  }

  /// <summary>
  /// Generic request success callback. This will resolve the pending request.
  /// </summary>
  /// <param name="serializedResponse">The generic JSON-serialized response</param>
  public void OnAptosWalletResponseSuccess(string serializedResponse)
  {
    Debug.Log($"OnSuccess: {serializedResponse}");
    var prevCompletionSource = completionSource;
    completionSource = null;
    prevCompletionSource.SetResult(serializedResponse);
  }

  /// <summary>
  /// Generic request error callback. This will reject the pending request.
  /// </summary>
  /// <param name="errorMessage">The error message coming from JS</param>
  public void OnAptosWalletResponseError(string errorMessage)
  {
    Debug.Log($"OnError: {errorMessage}");
    var prevCompletionSource = completionSource;
    completionSource = null;
    prevCompletionSource.SetException(new Exception(errorMessage));
  }

  #endregion

  #region MonoBehaviour

  private async void Start()
  {
    var connectedAccount = AccountData;
    if (connectedAccount == null)
    {
      return;
    }

    var activeAccount = await GetIsConnected() ? await GetAccount() : null;
    if (activeAccount?.address != connectedAccount.address)
    {
      AccountData = null;
      OnDisconnect?.Invoke(this, null);
      return;
    }

    NetworkData = await GetNetwork();
  }

  #endregion

  #region WalletService

  public event EventHandler OnDisconnect;

  public bool IsConnected => AccountData != null;
  public AccountData AccountData
  {
    get => LocalStorage.Get<AccountData>(CONNECTED_ACCOUNT_KEY);
    private set => LocalStorage.Set(CONNECTED_ACCOUNT_KEY, value);
  }
  public NetworkData NetworkData { get; private set; }

  public async Task<AccountData> Connect(CancellationToken? cancellationToken)
  {
    await SendRequest<bool>("connect");
    var isConnected = await GetIsConnected();
    Debug.Log($"right after connecting, isConnected: {isConnected}");
    if (isConnected)
    {
      AccountData = await GetAccount();
      NetworkData = await GetNetwork();
      Debug.Log($"right after connecting, address: {AccountData.address}");
      return AccountData;
    }
    return null;
  }

  public Task Disconnect()
  {
    return SendRequest<bool>("disconnect");
  }

  public Task<bool> GetIsConnected()
  {
    return SendRequest<bool>("isConnected");
  }

  public Task<AccountData> GetAccount()
  {
    return SendRequest<AccountData>("getAccount");
  }

  public Task<NetworkData> GetNetwork()
  {
    return SendRequest<NetworkData>("getNetwork");
  }

  public async Task<string> SignAndSubmitTransaction(string serializedPayload, CancellationToken? cancellationToken = null)
  {
    var response = await SendRequest<JToken>("signAndSubmitTransaction", serializedPayload);
    return response["hash"].ToString();
  }

  #endregion
}
