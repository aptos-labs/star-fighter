using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityConnect;
using uniffi;

public class BackendPairingClient
{
  public delegate void OnPairingRequestCreated(string pairingId);

  private readonly MyBackendClient _backendClient;

  public BackendPairingClient(MyBackendClient backendClient)
  {
    _backendClient = backendClient;
  }

  public async Task<ICPairingRequest> CreatePairingRequest(CancellationToken? cancellationToken = null)
  {
    var dappEd25519Keypair = IdentityConnectMethods.CreateEd25519keyPair();
    string dappEd25519SecretKeyB64 = Convert.ToBase64String(dappEd25519Keypair.secretKey);
    string dappEd25519PublicKeyB64 = Convert.ToBase64String(dappEd25519Keypair.publicKey);

    var responseBody = await _backendClient.PostAsync("v1/auth/ic-pairings", new { dappEd25519PublicKeyB64 }, cancellationToken);
    var pairingId = responseBody!["pairingId"]!.ToString();
    return new ICPairingRequest(dappEd25519PublicKeyB64, dappEd25519SecretKeyB64, pairingId);
  }
}

