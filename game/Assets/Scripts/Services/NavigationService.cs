using System;
using System.Linq;
using UnityEngine;

public enum NavigationRoute
{
  MainMenu,
  Game,
}

[Serializable]
public class NavigationMapEntry
{
  public NavigationRoute route;
  public GameObject element;
}

public class NavigationService : MonoBehaviour
{
  public NavigationRoute defaultRoute = NavigationRoute.MainMenu;
  public NavigationMapEntry[] routes;

  public void Start()
  {
    Navigate(defaultRoute);
  }

  public void Navigate(NavigationRoute route)
  {
    foreach (var entry in routes)
    {
      entry.element.SetActive(false);
    }
    routes.FirstOrDefault(entry => entry.route == route)?.element?.SetActive(true);
  }
}
