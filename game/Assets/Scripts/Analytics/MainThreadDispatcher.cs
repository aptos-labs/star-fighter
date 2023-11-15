using System.Collections.Concurrent;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
  private ConcurrentQueue<NetworkRequest> _tasks;

  protected void Awake()
  {
    _tasks = new ConcurrentQueue<NetworkRequest>();
  }

  public void Post(NetworkRequest task)
  {
    _tasks.Enqueue(task);
  }


  private void Update()
  {
    while (!_tasks.IsEmpty)
    {
      if (_tasks.TryDequeue(out NetworkRequest task))
      {
        StartCoroutine(task.Run());
      }
    }
  }
}
