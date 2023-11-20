using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public record CreateGameSessionResponse
{
  public string sessionId;
  public string fundTransferPayload;
}

public class GameSessionService : MonoBehaviour
{
  private static readonly string ACTIVE_SESSION_ID_KEY = "ActiveSessionId";

  private BackendClient _backendClient;
  private WalletService _walletService;
  private string _fundTransferTxnHash;

  # region MonoBehavior

  private void Awake()
  {
    var authService = FindFirstObjectByType<AuthService>();
    _backendClient = new BackendClient(authService);
    _walletService = FindFirstObjectByType<WalletService>();
  }

  #endregion

  public string ActiveSessionId
  {
    get => LocalStorage.Get(ACTIVE_SESSION_ID_KEY);
    private set => LocalStorage.Set(ACTIVE_SESSION_ID_KEY, value);
  }

  public bool HasActiveSession => ActiveSessionId != null;

  public async Task CreateSession(CancellationToken? cancellationToken = null)
  {
    var responseBody = await _backendClient.PostAsync<CreateGameSessionResponse>("v1/sessions");
    ActiveSessionId = responseBody.sessionId;
    _fundTransferTxnHash = await _walletService.SignAndSubmitTransaction(responseBody.fundTransferPayload, cancellationToken);
  }

  public async Task EndSession(long survivalTimeMs)
  {
    if (ActiveSessionId == null)
    {
      throw new InvalidOperationException("No active game session");
    }

    try
    {
      var responseBody = await _backendClient.PatchAsync($"v1/sessions/{ActiveSessionId}", new
      {
        survivalTimeMs,
        fundTransferTxnHash = _fundTransferTxnHash,
      });
    }
    finally
    {
      ActiveSessionId = null;
      _fundTransferTxnHash = null;
    }
  }
}
