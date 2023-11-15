using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using IdentityConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class WebWalletServiceImpl : MonoBehaviour, IWalletService
{
  [DllImport("__Internal")]
  private static extern void SendAptosWalletRequest(string serializedRequest);

  private TaskCompletionSource<string> completionSource;

  public event EventHandler<AccountData> OnConnect;
  public event EventHandler OnDisconnect;

  private async Task SendRequest(string method, string serializedArgs = null)
  {
    if (completionSource != null)
    {
      throw new InvalidOperationException("Another task is pending");
    }
    completionSource = new TaskCompletionSource<string>();
    var serializedRequest = JsonConvert.SerializeObject(new { method, args = serializedArgs });
    SendAptosWalletRequest(serializedRequest);
    await completionSource.Task;
  }

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

  // #region MonoBehavior

  // private async void Start()
  // {
  //   var isConnected = await GetIsConnected();
  //   Debug.Log($"IsConnected {isConnected}");
  //   if (isConnected)
  //   {
  //     var account = await GetAccount();
  //     Debug.Log(account.address);
  //   }
  // }

  // #endregion

  #region WalletService

  public AccountData AccountData => throw new NotImplementedException();

  public bool IsConnected => throw new NotImplementedException();

  public Task Connect(CancellationToken? cancellationToken)
  {
    return SendRequest("connect");
  }

  public Task Disconnect()
  {
    return SendRequest("disconnect");
  }

  private Task<bool> GetIsConnected()
  {
    return SendRequest<bool>("isConnected");
  }

  private Task<AccountData> GetAccount()
  {
    return SendRequest<AccountData>("getAccount");
  }

  public async Task<string> SignAndSubmitTransaction(string serializedPayload, CancellationToken? cancellationToken = null)
  {
    var response = await SendRequest<JToken>("signAndSubmitTransaction", serializedPayload);
    return response["hash"].ToString();
  }

  #endregion

  public void OnAptosWalletConnect(string serializedAccountData)
  {
    var accountData = JsonConvert.DeserializeObject<AccountData>(serializedAccountData);
    OnConnect?.Invoke(this, accountData);
  }

  public void OnAptosWalletDisconnect()
  {
    OnDisconnect?.Invoke(this, null);
  }

  public void OnAptosWalletResponseSuccess(string serializedResponse)
  {
    Debug.Log($"OnSuccess: {serializedResponse}");
    var prevCompletionSource = completionSource;
    completionSource = null;
    prevCompletionSource.SetResult(serializedResponse);
  }

  public void OnAptosWalletResponseError(string errorMessage)
  {
    Debug.Log($"OnError: {errorMessage}");
    var prevCompletionSource = completionSource;
    completionSource = null;
    prevCompletionSource.SetException(new Exception(errorMessage));
  }
}
