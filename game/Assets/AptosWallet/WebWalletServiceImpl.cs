using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class WebWalletServiceImpl : MonoBehaviour, IWalletService
{
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
    AccountData = JsonConvert.DeserializeObject<AccountData>(serializedAccountData);
    if (AccountData != null)
    {
      OnConnect?.Invoke(this, AccountData);
    }
    else
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
    var isConnected = await GetIsConnected();
    if (isConnected)
    {
      AccountData = await GetAccount();
      NetworkData = await GetNetwork();
    }
  }

  #endregion

  #region WalletService

  public event EventHandler<AccountData> OnConnect;
  public event EventHandler OnDisconnect;

  public bool IsConnected => AccountData != null;
  public AccountData AccountData { get; private set; }
  public NetworkData NetworkData { get; private set; }

  public Task Connect(CancellationToken? cancellationToken)
  {
    // Ideally this should load for as long as the popup is open
    return SendRequest<bool>("connect");
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
