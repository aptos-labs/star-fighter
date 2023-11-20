using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UI = UnityEngine.UI;

public struct UserStats
{
  public string name;
  public ulong aptBalance;
  public int gamesPlayed;
  public long bestSurvivalTimeMs;
  public int? rank;
}

public class DashboardCtrl : MonoBehaviour
{
  public UI.Text txtName;
  public UI.Text txtBalance;
  public UI.Text txtGamesPlayed;
  public UI.Text txtBestTimeMs;
  public UI.Text txtRank;
  public GameSessionPopupCtrl gameSessionPopupCtrl;

  private AuthService authService;
  private WalletService walletService;
  private BackendClient backendClient;

  # region MonoBehaviour

  private void Awake()
  {
    authService = FindFirstObjectByType<AuthService>();
    walletService = FindFirstObjectByType<WalletService>();
    backendClient = new BackendClient(authService);
  }

  private async void OnEnable()
  {
    await UpdateStats();
    walletService.OnDisconnect += OnDisconnected;
  }

  private void OnDisable()
  {
    walletService.OnDisconnect -= OnDisconnected;
  }

  # endregion

  public async Task UpdateStats()
  {
    var responseBody = await backendClient.GetAsync<UserStats>("v1/user");
    var address = responseBody.name;
    txtName.text = address[..8] + ".." + address[(address.Length - 6)..];
    txtBalance.text = (responseBody.aptBalance / 1e8).ToString("0.## APT");
    txtGamesPlayed.text = responseBody.gamesPlayed.ToString();
    txtBestTimeMs.text = responseBody.bestSurvivalTimeMs.ToString();
    txtRank.text = responseBody.rank.HasValue ? responseBody.rank.ToString() : "-";
  }

  public async void OnDisconnect()
  {
    authService.Logout();
    await walletService.Disconnect();
    SendMessageUpwards("OnLogout");
  }

  public async void OnStartGame()
  {
    await gameSessionPopupCtrl.RequestGameSession();
  }

  public void OnDisconnected(object sender, object args)
  {
    Debug.Log("Disconnect detected, logging out");
    authService.Logout();
    SendMessageUpwards("OnLogout");
  }
}
