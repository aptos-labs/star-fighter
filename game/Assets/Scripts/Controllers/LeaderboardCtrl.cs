using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Segment.Serialization;
using UnityEngine;
using UI = UnityEngine.UI;

public class LeaderboardEntry
{
  public long bestSurvivalTimeMs;
  public long gamesPlayed;
  public string address;
}

public class GetLeaderboardPageResponse
{
  public LeaderboardEntry[] rows;
  public long totalCount;
}

public class LeaderboardCtrl : MonoBehaviour
{
  public UI.Button prevButton;
  public UI.Button nextButton;

  public UI.Text totalCount;

  public Transform leaderboardContent;
  public GameObject leaderboardRowPrefab;
  private List<LeaderboardRow> LeaderboardRows { get; set; } = new List<LeaderboardRow>();

  private static int PAGE_SIZE = 10;
  private AuthService authService;
  private MyBackendClient myBackendClient;

  private int currentPage = 0;
  private int CurrentPage
  {
    get => currentPage;
    set
    {
      var maxPage = (CurrentPageData?.totalCount ?? 0 + PAGE_SIZE - 1) / PAGE_SIZE;
      if (value < 0 || value > maxPage)
      {
        return;
      }

      currentPage = value;
      prevButton.interactable = currentPage > 0;
      nextButton.interactable = currentPage < maxPage;
    }
  }

  private string totalCountFormat;

  private GetLeaderboardPageResponse currentPageData;
  private GetLeaderboardPageResponse CurrentPageData
  {
    get => currentPageData;
    set
    {
      currentPageData = value;
      var textValue = CurrentPageData?.totalCount.ToString() ?? "-";
      totalCount.text = totalCountFormat.Replace("{count}", textValue);
    }
  }

  private void Awake()
  {
    totalCountFormat = totalCount.text;
    authService = GetComponentInParent<AuthService>();
    myBackendClient = new MyBackendClient(authService);
    for (int i = 0; i < leaderboardContent.childCount; ++i)
    {
      var child = leaderboardContent.GetChild(i);
      GameObject.Destroy(child.gameObject);
    }

    for (int i = 0; i < PAGE_SIZE; ++i)
    {
      var instance = Instantiate(this.leaderboardRowPrefab, leaderboardContent);
      var row = instance.GetComponent<LeaderboardRow>();
      LeaderboardRows.Add(row);
      row.gameObject.SetActive(false);
    }
  }

  private async void OnEnable()
  {
    CurrentPage = 0;
    await GetPageData(currentPage);
    CurrentPage = 0;
    AnalyticsService.Screen("Leaderboard");
  }

  private async Task GetPageData(int page)
  {
    var from = page * PAGE_SIZE;
    var to = from + PAGE_SIZE;
    CurrentPageData = await myBackendClient.GetAsync<GetLeaderboardPageResponse>($"v1/leaderboard?from={from}&to={to}");

    for (int i = 0; i < PAGE_SIZE; ++i)
    {
      if (i < CurrentPageData.rows.Length)
      {
        var currRowData = CurrentPageData.rows[i];
        var currLeaderboardRow = LeaderboardRows[i];
        currLeaderboardRow.gameObject.SetActive(true);
        currLeaderboardRow.address.text = currRowData.address.Substring(0, 16) + ".." + currRowData.address.Substring(50, 14);
        currLeaderboardRow.rank.text = (page * PAGE_SIZE + i + 1).ToString();
      }
      else
      {
        LeaderboardRows[i].gameObject.SetActive(false);
      }
    }
  }

  public async void PrevPage()
  {
    CurrentPage -= 1;
    await GetPageData(currentPage);
  }

  public async void NextPage()
  {
    CurrentPage += 1;
    await GetPageData(currentPage);
  }
}
