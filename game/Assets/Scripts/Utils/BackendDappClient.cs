using System;
using IdentityConnect;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net;
using UnityEngine;

public class BackendPairingClient
{
  public delegate void OnPairingRequestCreated(string pairingId);

  private MyBackendClient myBackendClient;
  private OnPairingRequestCreated _callback;

  public BackendPairingClient(MyBackendClient myBackendClient, OnPairingRequestCreated callback)
  {
    this.myBackendClient = myBackendClient;
    this._callback = callback;
  }

  private async Task<string> createIcPairingRequest(string dappEd25519PublicKeyB64)
  {
    // var httpClient = new HttpClient();
    // var serializedRequestBody = JsonConvert.SerializeObject(new { dappEd25519PublicKeyB64 });
    // var content = new StringContent(serializedRequestBody, Encoding.UTF8, "application/json");
    // var response = await httpClient.PostAsync($"{BACKEND_BASE_URL}/v1/auth/ic-pairings", content);
    // var serializedResponseBody = await response.Content.ReadAsStringAsync();
    // var responseBody = JsonConvert.DeserializeObject<JObject>(serializedResponseBody);
    // if (response.StatusCode != HttpStatusCode.OK)
    // {
    //   throw new HttpRequestException(responseBody?.Value<string>("message"));
    // }
    var responseBody = await myBackendClient.PostAsync("v1/auth/ic-pairings", new { dappEd25519PublicKeyB64 });
    return responseBody!["pairingId"]!.ToString();
  }

  private async Task<PairingData> getPairing(string id, CancellationToken? cancellationToken = null)
  {
    try
    {
      var httpClient = new HttpClient();
      var response = await httpClient.GetAsync($"{Constants.IC_BASE_URL}/v1/pairing/{id}", cancellationToken ?? CancellationToken.None);
      var serializedResponseBody = await response.Content.ReadAsStringAsync();
      var responseBody = JsonConvert.DeserializeObject<JObject>(serializedResponseBody);

      if (response.StatusCode != HttpStatusCode.OK)
      {
        throw new HttpRequestException(responseBody?.Value<string>("message"));
      }
      return responseBody?["data"]?["pairing"]?.ToObject<PairingData>();
    }
    catch (HttpRequestException ex)
    {
      if (!ex.Message.Contains("404"))
      {
        throw ex;
      }
      return null;
    }
  }

  private async Task<PairingData> waitForPairingRequestFinalized(string pairingId, CancellationToken? cancellationToken = null)
  {
    while (true)
    {
      var pairing = await this.getPairing(pairingId, cancellationToken);
      if (pairing?.status == "FINALIZED")
      {
        return pairing;
      }
      await Task.Delay(5000, cancellationToken ?? CancellationToken.None);
    }
  }

  public async Task<PairingData> pairWithQrCode(byte[] dappEd25519PublicKey, CancellationToken? cancellationToken = null)
  {
    var dappEd25519PublicKeyB64 = Convert.ToBase64String(dappEd25519PublicKey);
    var pairingId = await createIcPairingRequest(dappEd25519PublicKeyB64);

    Debug.Log($"Created pairing with id {pairingId}");

    if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
    {
      return null;
    }

    var environmentOrBaseUrl = Constants.IC_ENVIRONMENT_OR_BASE_URL;
    var url = $"identity-connect:///anonymous-pairings/{pairingId}/finalize?environment={environmentOrBaseUrl}";
    this._callback(url);

    try
    {
      var pairing = await this.waitForPairingRequestFinalized(pairingId, cancellationToken);
      return pairing;
    }
    catch (OperationCanceledException)
    {
      return null;
    }
  }
}

