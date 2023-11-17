using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityConnect;
using UnityEngine;

public class CreateGameSessionResponse
{
  public string sessionId;
  public string fundTransferPayload;
}

public class GameSessionService : MonoBehaviour
{
  private MyBackendClient backendClient;
  private WalletService walletService;

  private string fundTransferTxnHash;

  # region MonoBehavior

  private void Awake()
  {
    var authService = FindFirstObjectByType<AuthService>();
    backendClient = new MyBackendClient(authService);
    walletService = FindFirstObjectByType<WalletService>();
  }

  #endregion

  public string ActiveSessionId { get; private set; }
  public bool HasActiveSession => ActiveSessionId != null;

  public async Task CreateSession(CancellationToken? cancellationToken = null)
  {
    var responseBody = await backendClient.PostAsync<CreateGameSessionResponse>("v1/sessions");
    ActiveSessionId = responseBody.sessionId;
    fundTransferTxnHash = await walletService.SignAndSubmitTransaction(responseBody.fundTransferPayload, cancellationToken);
  }

  public async Task EndSession(long survivalTimeMs)
  {
    if (ActiveSessionId == null)
    {
      throw new InvalidOperationException("No active game session");
    }

    try
    {
      var responseBody = await backendClient.PatchAsync($"v1/sessions/{ActiveSessionId}", new
      {
        survivalTimeMs,
        fundTransferTxnHash
      });
    }
    finally
    {
      ActiveSessionId = null;
      fundTransferTxnHash = null;
    }
  }
}
