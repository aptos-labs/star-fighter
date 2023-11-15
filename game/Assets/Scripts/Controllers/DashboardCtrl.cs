using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UI = UnityEngine.UI;

public class DashboardCtrl : MonoBehaviour
{
  private AuthService authService;
  private WalletService walletService;
  private GameSessionService gameSessionService;
  private NavigationService navigationService;
  private MyBackendClient myBackendClient;

  public UI.Text txtName;
  public UI.Text txtBalance;
  public UI.Text txtGamesPlayed;
  public UI.Text txtBestTimeMs;
  public UI.Text txtRank;
  public GameSessionPopupCtrl gameSessionPopupCtrl;

  public UI.Button startNewGameButton;

  public GameObject leaderboardPopup;

  # region MonoBehaviour

  private void Awake()
  {
    authService = GetComponentInParent<AuthService>();
    walletService = GetComponentInParent<WalletService>();
    myBackendClient = new MyBackendClient(authService);
    gameSessionService = GetComponentInParent<GameSessionService>();
    navigationService = GetComponentInParent<NavigationService>();
  }

  private async void OnEnable()
  {
    await UpdateStats();
    walletService.OnDisconnect += OnDisconnectDetected;
  }

  private void OnDisable()
  {
    walletService.OnDisconnect -= OnDisconnectDetected;
  }

  # endregion

  public async Task UpdateStats()
  {
    var responseBody = await myBackendClient.GetAsync<UserStats>("v1/user");
    var address = responseBody.name;
    this.txtName.text = address[..8] + ".." + address[(address.Length - 6)..];
    this.txtBalance.text = (responseBody.aptBalance / 1e8).ToString("0.## APT");
    this.txtGamesPlayed.text = responseBody.gamesPlayed.ToString();
    this.txtBestTimeMs.text = responseBody.bestSurvivalTimeMs.ToString();
    this.txtRank.text = responseBody.rank.HasValue ? responseBody.rank.ToString() : "-";
  }

  public async void OnPlayNewGame()
  {
    var sessionId = await gameSessionPopupCtrl.CreateSession();
    if (sessionId != null)
    {
      navigationService.Navigate(NavigationRoute.Game);
    }
  }

  public async void OnDisconnect()
  {
    authService.Logout();
    await walletService.Disconnect();
    SendMessageUpwards("OnLogout");
  }

  public async void OnDisconnectDetected(object sender, object args)
  {
    authService.Logout();
    SendMessageUpwards("OnLogout");
  }

  public void ShowLeaderboard()
  {
    leaderboardPopup.SetActive(true);
  }

  public void HideLeaderboard()
  {
    leaderboardPopup.SetActive(false);
  }
}
