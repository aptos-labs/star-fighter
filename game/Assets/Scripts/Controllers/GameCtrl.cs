using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public static class DateTimeUtils
{
  public static long Now()
  {
    return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  }
}

public class GameCtrl : MonoBehaviour
{
  public GameSessionPopupCtrl gameSessionPopupCtrl;
  public GameObject endGamePanel;

  // public Canvas endGameUi;
  [SerializeField]
  private Text _text;

  private GameSessionService gameSessionService;
  private long startTime;

  private void Awake()
  {
    gameSessionService = FindFirstObjectByType<GameSessionService>();
  }

  private void OnEnable()
  {
    StartGame();
  }

  private void StartGame()
  {
    AnalyticsService.Track("GameStart");
    startTime = DateTimeUtils.Now();
    BroadcastMessage("OnGameStart", SendMessageOptions.DontRequireReceiver);
  }

  private async void OnPlayerExploded()
  {
    var currentTime = DateTimeUtils.Now();
    var totalTime = currentTime - startTime;

    BroadcastMessage("OnGameEnd", SendMessageOptions.DontRequireReceiver);
    AnalyticsService.Track("GameEnd");
    _text.text = $"Survived {totalTime} ms...\n Try again?";

    var missileSpawner = GetComponentInChildren<MissileSpawnerCtrl>();
    missileSpawner.enabled = false;

    // endGameUi.gameObject.SetActive(true);
    this.endGamePanel.SetActive(true);
    await gameSessionService.EndSession(totalTime);
  }

  public async void PlayAgain()
  {
    endGamePanel.SetActive(false);
    try
    {
      var isNewGame = await gameSessionPopupCtrl.RequestGameSession();
      if (!isNewGame)
      {
        endGamePanel.SetActive(true);
      }
    }
    catch (Exception ex)
    {
      endGamePanel.SetActive(true);
      throw ex;
    }
  }

  public async void NavigateToMainMenu()
  {
    await SceneManager.LoadSceneAsync("MainMenu");
  }
}
