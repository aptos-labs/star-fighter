using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionPopupCtrl : MonoBehaviour
{
  private GameSessionService gameSessionService;

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
    gameSessionService = FindFirstObjectByType<GameSessionService>();
#if !UNITY_EDITOR && UNITY_WEBGL
    icPrompt.SetActive(false);
    webPrompt.SetActive(true);
    cancelBtn.SetActive(false);
#else
    icPrompt.SetActive(true);
    webPrompt.SetActive(false);
    cancelBtn.SetActive(true);
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

  public async Task RequestGameSession()
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
        taskCompletionSource = new TaskCompletionSource<bool>();
        await taskCompletionSource.Task;
      }
    }
    catch (OperationCanceledException)
    {
      // Do nothing
      return;
    }
    catch (Exception ex)
    {
      // TODO: show error
      Debug.Log(ex.Message);
      return;
    }
    finally
    {
      gameObject.SetActive(false);
      createSessionCancellationTokenSource = null;
      taskCompletionSource = null;
    }

    var prevActiveScene = SceneManager.GetActiveScene();
    await SceneManager.UnloadSceneAsync(prevActiveScene);
    await SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
  }

  public void OnStartGame()
  {
    taskCompletionSource?.SetResult(true);
  }

  public void OnCancelRequest()
  {
    createSessionCancellationTokenSource?.Cancel();
  }
}
