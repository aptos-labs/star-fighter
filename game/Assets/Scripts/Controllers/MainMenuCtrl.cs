using Segment.Serialization;
using UnityEngine;

public class MainMenuCtrl : MonoBehaviour
{
  private AuthService authService;

  public GameObject loginComponent;
  public GameObject dashboardComponent;

  private GameObject activeObject;

  #region MonoBehaviour

  private void Awake()
  {
    authService = GetComponentInParent<AuthService>();
    loginComponent.SetActive(false);
    dashboardComponent.SetActive(false);
  }

  private void Start()
  {
    if (authService.IsAuthenticated)
    {
      var x = authService.AccountData;
      Debug.Log(x);
      SetActiveComponent(dashboardComponent);
      // AnalyticsService.Identify(authService.AuthToken, new JsonObject
      // {

      // });
    }
    else
    {
      SetActiveComponent(loginComponent);
    }
  }

  private void OnAuthenticated()
  {
    SetActiveComponent(dashboardComponent);
  }

  private void OnLogout()
  {
    SetActiveComponent(loginComponent);
  }

  #endregion

  private void SetActiveComponent(GameObject value)
  {
    activeObject?.SetActive(false);
    activeObject = value;
    activeObject.SetActive(true);
  }

}
