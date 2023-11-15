using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using UnityEngine;

public struct SigningInfo
{
  public SigningInfo(string signingMessage, string nonce)
  {
    this.signingMessage = signingMessage;
    this.nonce = nonce;
  }

  public string signingMessage { get; }
  public string nonce { get; }
}

public struct TxnHash
{
  public TxnHash(string hash)
  {
    this.hash = hash;
  }

  private string hash { get; }
}

[Serializable]
public struct Pilot
{
  public string pilotAddress;
}

public struct AuthData
{
  public string accountAddress;
  public string authToken;
}

public class BackendClient
{
  private const string BASE_URL = "http://localhost:8080";

  private HttpClient _httpClient = new HttpClient();

  public static string accountAddress = null;
  public static string staticAuthToken = null;

  public BackendClient()
  {
  }

  public async Task<string> faucet(string address)
  {
    var serializedBody = JsonConvert.SerializeObject(new { });
    var content = new StringContent(serializedBody, Encoding.UTF8, "application/json");
    var response = await this._httpClient.PostAsync($"https://faucet.devnet.aptoslabs.com/mint?amount=100000000&address={address}", content);
    await response.Content.ReadAsStringAsync();

    if (response.StatusCode != HttpStatusCode.OK)
    {
      throw new HttpRequestException("Failed to use faucet");
    }

    return "";
  }

  private AuthData getAuthData()
  {
    // var authData = GameState.Get<AuthData>("authData");
    // if (authData is null)
    // {
    throw new Exception("No auth data to load!");
    // }

    // return (AuthData)authData;
  }

  private void setAuthData(string address, string authToken)
  {
    var authData = new AuthData
    {
      accountAddress = address,
      authToken = authToken,
    };
    // GameState.Set<AuthData>("authData", authData);
  }

  public async Task<SigningInfo> createSession(string address)
  {
    var path = "create";
    var query = $"accountAddress={address}&newSession=true";
    var body = new { };
    setAuthData(address, null);

    var response = await post(body, path, query);
    return new SigningInfo(response.Value<string>("signingMessage"), response.Value<string>("nonce"));
  }

  public async Task<string> login(string message, string signature, string publicKey)
  {
    var authData = getAuthData();
    var path = "login";
    var query =
      $"accountAddress={authData.accountAddress}&message={message}&signature={signature}&publicKey={publicKey}";
    var body = new
    {
      accountAddress = authData.accountAddress,
      message = message,
      publicKey = publicKey,
      signature = signature,
    };

    var response = await post(body, path, query);
    var authToken = response.Value<string>("authToken");
    setAuthData(authData.accountAddress, authToken);
    return authToken;
  }

  public async Task<TxnHash> endGame(long gameTime, string pilotAddress)
  {
    var authData = getAuthData();
    var path = "endGame";
    var query = $"accountAddress={authData.accountAddress}&authToken={authData.authToken}";
    var body = new { gameTime, pilot = pilotAddress };

    var response = await post(body, path, query);

    return new TxnHash(response.Value<string>("hash"));
  }

  public async Task<(string, string)> inventory()
  {
    var authData = getAuthData();
    var path = "inventory";
    var query = $"accountAddress={authData.accountAddress}";

    var response = await get(path, query);

    return (response.Value<string>("pilot"), response.Value<string>("records"));
  }

  public async Task<TxnHash> mintPilot()
  {
    var authData = getAuthData();
    var path = "mint/pilot";
    var query = $"accountAddress={authData.accountAddress}&authToken={authData.authToken}";
    var body = new { };

    var response = await post(body, path, query);

    return new TxnHash(response.Value<string>("hash"));
  }

  public async Task<TxnHash> mintBody()
  {
    var authData = getAuthData();
    var path = "mint/body";
    var query = $"accountAddress={authData.accountAddress}&authToken={authData.authToken}";
    var body = new { };

    var response = await post(body, path, query);

    return new TxnHash(response.Value<string>("hash"));
  }

  public async Task<TxnHash> mintWing()
  {
    var authData = getAuthData();
    var path = "mint/wing";
    var query = $"accountAddress={authData.accountAddress}&authToken={authData.authToken}";
    var body = new { };

    var response = await post(body, path, query);

    return new TxnHash(response.Value<string>("hash"));
  }

  public async Task<TxnHash> mintFighter()
  {
    var authData = getAuthData();
    var path = "mint/fighter";
    var query = $"accountAddress={authData.accountAddress}&authToken={authData.authToken}";
    var body = new { };

    var response = await post(body, path, query);

    return new TxnHash(response.Value<string>("hash"));
  }

  public async Task<TxnHash> swap(string fighter, string wing = null,
    string body = null)
  {
    var authData = getAuthData();
    var path = "swap";
    var query = $"accountAddress={authData.accountAddress}&authToken={authData.authToken}";
    var input = new
    {
      owner = accountAddress,
      fighter = fighter,
      wing = wing,
      body = body
    };

    var response = await post(input, path, query);

    return new TxnHash(response.Value<string>("hash"));
  }

  public async Task<int> logout()
  {
    var authData = getAuthData();
    var path = "logout";
    var query = $"accountAddress={authData.accountAddress}&authToken={authData.authToken}";
    var body = new { };
    await post(body, path, query);
    staticAuthToken = null;
    accountAddress = null;
    return 0;
  }

  public async Task<(long, long)> pilot(string address)
  {
    var path = "pilot";
    var query = $"accountAddress={address}";
    var response = await get(path, query);
    return (response.Value<long>("gamesPlayed"), response.Value<long>("longestSurvival"));
  }

  public async Task<long> balance()
  {
    var authData = getAuthData();
    var path = "balance";
    var query = $"accountAddress={authData.accountAddress}";
    var response = await get(path, query);
    return response.Value<long>("balance");
  }

  private async Task<JObject> get(string path, string query)
  {
    var response = await this._httpClient.GetAsync($"http://localhost:8080/{path}?{query}");
    var serializedResponseBody = await response.Content.ReadAsStringAsync();
    var responseBody = JsonConvert.DeserializeObject<JObject>(serializedResponseBody);

    if (response.StatusCode != HttpStatusCode.OK)
    {
      throw new HttpRequestException(responseBody.Value<string>("message"));
    }

    return responseBody;
  }

  private async Task<JObject> post(object body, string path, string query)
  {
    var serializedBody = JsonConvert.SerializeObject(body);
    var content = new StringContent(serializedBody, Encoding.UTF8, "application/json");
    var response = await this._httpClient.PostAsync($"http://localhost:8080/{path}?{query}", content);
    var serializedResponseBody = await response.Content.ReadAsStringAsync();
    var responseBody = JsonConvert.DeserializeObject<JObject>(serializedResponseBody);

    if (response.StatusCode != HttpStatusCode.OK)
    {
      throw new HttpRequestException(responseBody.Value<string>("message"));
    }

    return responseBody;
  }
}
