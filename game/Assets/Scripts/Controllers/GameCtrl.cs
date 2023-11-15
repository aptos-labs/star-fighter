using UnityEngine;
using UnityEngine.UI;
using System;
using Segment.Serialization;

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

  private NavigationService navigationService;
  private GameSessionService gameSessionService;
  private long startTime;

  private void Awake()
  {
    navigationService = GetComponentInParent<NavigationService>();
    gameSessionService = GetComponentInParent<GameSessionService>();
  }

  private void OnEnable()
  {
    StartGame();
  }

  private void StartGame()
  {
    AnalyticsService.Track("GameStart");
    endGamePanel.SetActive(false);
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
    this.endGamePanel.SetActive(false);
    var sessionId = await gameSessionPopupCtrl.CreateSession();

    if (sessionId != null)
    {
      StartGame();
    }
    else
    {
      this.endGamePanel.SetActive(true);
    }
  }

  public void NavigateToMainMenu()
  {
    navigationService.Navigate(NavigationRoute.MainMenu);
  }
}
