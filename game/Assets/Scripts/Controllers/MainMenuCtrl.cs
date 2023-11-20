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
    authService = FindFirstObjectByType<AuthService>();
    loginComponent.SetActive(false);
    dashboardComponent.SetActive(false);
  }

  private void Start()
  {
    if (authService.IsAuthenticated)
    {
      SetActiveComponent(dashboardComponent);
    }
    else
    {
      SetActiveComponent(loginComponent);
    }
  }

  public void OnAuthenticated()
  {
    SetActiveComponent(dashboardComponent);
  }

  public void OnLogout()
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
