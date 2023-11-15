using System.Diagnostics;
using UnityEngine;

namespace K4.Threading
{
  using SelfType = K4UnityThreadDispatcher;

  public partial class K4UnityThreadDispatcher : MonoBehaviour
  {
    public static SelfType Instance
    {
      get;
      private set;
    }

    public static double AllotedTimeEachWindow => 3;

    private static readonly Stopwatch windowTimeStopwatch = new Stopwatch();

    private void Update()
    {
      windowTimeStopwatch.Restart();

      while (globalPendingActions.Count != 0 && windowTimeStopwatch.Elapsed.TotalMilliseconds < AllotedTimeEachWindow)
      {
        if (globalPendingActions.TryDequeue(out System.Action action))
        {
          action();
        }
      }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void CreateDispatcher()
    {
      if (Instance == null)
      {
        SelfType dispatcher = FindObjectOfType<SelfType>() ?? new GameObject("Unity Thread Dispatcher").AddComponent<SelfType>();
        DontDestroyOnLoad(dispatcher);
        Instance = dispatcher;
      }
    }
  }
}
