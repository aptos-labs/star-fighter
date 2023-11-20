using System;
using System.Threading;
using System.Threading.Tasks;
using Aptos.Accounts;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class LocalAccountData
{
  public string address;
  public string secretKey;
  public string publicKey;
}

public abstract class LocalWalletServiceImpl : MonoBehaviour, IWalletService
{
  private static readonly string LOCAL_ACCOUNT_IS_CONNECTED_KEY = "LocalAccount_IsConnected";
  private static readonly string LOCAL_ACCOUNT_KEY = "LocalAccount_Data";
  private LocalAccountData LocalAccount
  {
    get
    {
      var localAccount = LocalStorage.Get<LocalAccountData>(LOCAL_ACCOUNT_KEY);
      if (localAccount == null)
      {
        var aptosAccount = new Account();
        localAccount = new LocalAccountData
        {
          address = aptosAccount.AccountAddress.ToString(),
          publicKey = aptosAccount.PublicKey.Key,
          secretKey = aptosAccount.PrivateKey.Key,
        };
        LocalStorage.Set(LOCAL_ACCOUNT_KEY, localAccount);
      }
      return localAccount;
    }
  }

  public async void Start()
  {
    await Task.Delay(200);
    if (IsConnected)
    {
      var localAccount = LocalAccount;
      AccountData = new AccountData
      {
        address = localAccount.address,
        publicKey = localAccount.publicKey,
      };
      Debug.Log($"Local account address: {localAccount.address}");
    }
  }

  #region WalletService

  public event EventHandler OnDisconnect;

  public bool IsConnected
  {
    get => LocalStorage.Get(LOCAL_ACCOUNT_IS_CONNECTED_KEY) == "True";
    set => LocalStorage.Set(LOCAL_ACCOUNT_IS_CONNECTED_KEY, value.ToString());
  }
  public AccountData AccountData { get; private set; }
  public NetworkData NetworkData => new()
  {
    name = "Mainnet",
    chainId = "1",
    url = "https://fullnode.mainnet.aptoslabs.com/v1",
  };

  public async Task<AccountData> Connect(CancellationToken? cancellationToken)
  {
    await Task.Delay(500, cancellationToken ?? CancellationToken.None);
    IsConnected = true;
    var localAccount = LocalAccount;
    AccountData = new AccountData
    {
      address = localAccount.address,
      publicKey = localAccount.publicKey,
    };
    Debug.Log($"Local account address: {AccountData.address}");
    return AccountData;
  }

  public async Task Disconnect()
  {
    await Task.Delay(100);
    IsConnected = false;
  }

  public async Task<bool> GetIsConnected()
  {
    await Task.Delay(200);
    return IsConnected;
  }

  public async Task<AccountData> GetAccount()
  {
    await Task.Delay(200);
    return AccountData;
  }

  public async Task<NetworkData> GetNetwork()
  {
    await Task.Delay(200);
    return NetworkData;
  }

  public async Task<string> SignAndSubmitTransaction(string serializedPayload, CancellationToken? cancellationToken = null)
  {
    // var jsonPayload = JsonConvert.DeserializeObject<JObject>(serializedPayload);
    // var payload = new TransactionPayload
    // {
    //   Type = jsonPayload["type"].ToString(),
    //   Function = jsonPayload["function"].ToString(),
    //   Arguments = new Arguments
    //   {
    //     ArgumentStrings = jsonPayload["arguments"].ToString().Select((value) => value.ToString()).ToArray(),
    //   },
    //   TypeArguments = jsonPayload["type_arguments"].Select((value) => value.ToString()).ToArray(),
    // };

    var localAccountData = LocalAccount;
    var localAccount = new Account(localAccountData.secretKey, localAccountData.publicKey);

    var payload = JToken.Parse(serializedPayload);
    var recipient = payload["arguments"][0].ToString();
    var amount = payload["arguments"][1].ToObject<long>();

    var client = Aptos.Unity.Rest.RestClient.Instance;
    client.SetEndPoint(NetworkData.url);

    var completionSource = new TaskCompletionSource<string>();
    StartCoroutine(client.Transfer((txn, response) =>
    {
      Debug.Log(response.message);
      var userTxn = JObject.Parse(response.message);
      completionSource.SetResult(userTxn["hash"].ToString());
    }, localAccount, recipient, amount));
    return await completionSource.Task;
  }

  #endregion
}
