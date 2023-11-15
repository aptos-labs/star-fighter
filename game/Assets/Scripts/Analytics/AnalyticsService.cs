using System;
using Segment.Analytics;
using Segment.Concurrent;
using Segment.Serialization;
using UnityEngine;

#if UNITY_EDITOR || !UNITY_WEBGL

public class AnalyticsService : MonoBehaviour
{
  private static Analytics Analytics { get; set; }

  private void Awake()
  {
    var mainThreadDispatcher = gameObject.AddComponent<MainThreadDispatcher>();
    var lifecycle = gameObject.AddComponent<Lifecycle>();
    var configuration =
    new Configuration("uzU8CACyPR5C1Fky3GjEUiGgCdhEOvu9",
        exceptionHandler: new ErrorHandler(),
        httpClientProvider: new UnityHTTPClientProvider(mainThreadDispatcher));
    Analytics = new Analytics(configuration);
    Analytics.Add(new LifecyclePlugin(lifecycle));
    Debug.Log("Analytics initialized");
  }

  class ErrorHandler : ICoroutineExceptionHandler
  {
    public void OnExceptionThrown(Exception e)
    {
      Debug.LogException(e);
    }
  }

  public static void Track(string name, JsonObject properties = null)
  {
    if (Analytics == null)
    {
      return;
    }

    Analytics.Track(name, properties);
  }

  public static void Screen(string name, JsonObject properties = null, string category = "")
  {
    if (Analytics == null)
    {
      return;
    }

    Analytics.Screen(name, properties, category);
  }

  public static void Identify(string userId, JsonObject traits = null)
  {
    if (Analytics == null)
    {
      return;
    }

    Analytics.Identify(userId, traits);
  }
}

#else

public class AnalyticsService : MonoBehaviour
{
  private void Awake()
  {
  }

  public void Start()
  {
  }

  public static void Track(string name, JsonObject properties = null)
  {
  }

  public static void Screen(string name, JsonObject properties = null, string category = "")
  {
  }

  public static void Identify(string userId, JsonObject traits = null)
  {
  }
}

#endif
