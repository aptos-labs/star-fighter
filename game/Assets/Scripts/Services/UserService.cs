using System.Threading.Tasks;
using UnityEngine;

public struct UserStats
{
  public string name;
  public ulong aptBalance;
  public int gamesPlayed;
  public long bestSurvivalTimeMs;
  public int? rank;
}

public class UserService : MonoBehaviour
{
  private AuthService authService;
  private MyBackendClient myBackendClient;

  public UserStats UserStats { get; private set; }

  # region MonoBehavior

  private void Awake()
  {
    authService = GetComponentInParent<AuthService>();
    myBackendClient = new MyBackendClient(authService);
  }

  private async void Start()
  {
    await UpdateStats();
  }

  #endregion

  public async Task UpdateStats()
  {
    UserStats = await myBackendClient.GetAsync<UserStats>("v1/user");
  }
}
