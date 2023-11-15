using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace K4.Threading
{
  public partial class K4UnityThreadDispatcher
  {
    private static readonly ConcurrentQueue<Action> globalPendingActions = new ConcurrentQueue<Action>();

    public static Task<R> Execute<R>(Func<R> func)
    {
      TaskCompletionSource<R> tcs = new TaskCompletionSource<R>();
      void InternalAction()
      {
        try
        {
          R returnValue = func();
          tcs.SetResult(returnValue);
        }
        catch (Exception e)
        {
          tcs.SetException(e);
        }
      }

      globalPendingActions.Enqueue(InternalAction);
      return tcs.Task;
    }

    public static Task<R> Execute<T1, R>(Func<T1, R> func, T1 arg1)
    {
      return Execute(() => func(arg1));
    }

    public static Task<R> Execute<T1, T2, R>(Func<T1, T2, R> func, T1 arg1, T2 arg2)
    {
      return Execute(() => func(arg1, arg2));
    }

    public static Task Execute(Action action)
    {
      return Execute(
        () =>
        {
          action();
          return true;
        }
      );
    }

    public static Task Execute<T1>(Action<T1> action, T1 arg1)
    {
      return Execute(
        () =>
        {
          action(arg1);
          return true;
        }
      );
    }

    public static Task Execute<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
    {
      return Execute(
        () =>
        {
          action(arg1, arg2);
          return true;
        }
      );
    }
  }
}
