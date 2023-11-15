using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GameSessionPopupCtrl : MonoBehaviour
{
  private GameSessionService gameSessionService;
  private NavigationService navigationService;

  public GameObject icPrompt;
  public GameObject webPrompt;
  public GameObject pendingState;
  public GameObject readyState;
  public GameObject errorState;
  public GameObject cancelBtn;

  # region MonoBehaviour

  private void Awake()
  {
    // gameObject.SetActive(false);
    gameSessionService = GetComponentInParent<GameSessionService>();
#if !UNITY_EDITOR && UNITY_WEBGL
      icPrompt.gameObject.SetActive(false);
      webPrompt.gameObject.SetActive(true);
      cancelBtn.SetActive(false);
#else
    icPrompt.gameObject.SetActive(true);
    webPrompt.gameObject.SetActive(false);
#endif
  }

  # endregion

  private bool IsReadyToPlay
  {
    set
    {
      var isReadyToPlay = value;
      pendingState.SetActive(!isReadyToPlay);
      readyState.SetActive(isReadyToPlay);
    }
  }

  private CancellationTokenSource createSessionCancellationTokenSource;
  private TaskCompletionSource<bool> taskCompletionSource;

  public async Task<string> CreateSession()
  {
    gameObject.SetActive(true);
    IsReadyToPlay = false;
    try
    {
      createSessionCancellationTokenSource = new CancellationTokenSource();
      using (createSessionCancellationTokenSource)
      {
        await gameSessionService.CreateSession(createSessionCancellationTokenSource.Token);
        if (!gameSessionService.HasActiveSession)
        {
          throw new Exception("No active game session");
        }
        IsReadyToPlay = true;
      }
    }
    catch (OperationCanceledException)
    {
      // Do nothing
      return null;
    }
    catch (Exception ex)
    {
      // TODO: show error
      Debug.Log(ex.Message);
      gameObject.SetActive(false);
    }

    this.taskCompletionSource = new TaskCompletionSource<bool>();
    await taskCompletionSource.Task;
    return gameSessionService.ActiveSessionId;
  }

  public void StartGame()
  {
    this.taskCompletionSource.SetResult(true);
    this.taskCompletionSource = null;
    gameObject.SetActive(false);
  }

  public void OnCancelGameSession()
  {
    createSessionCancellationTokenSource?.Cancel();
    createSessionCancellationTokenSource = null;
    gameObject.SetActive(false);
  }
}
