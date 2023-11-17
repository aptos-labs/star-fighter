using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class MyBackendClient
{
  private readonly AuthService _authService;

  public MyBackendClient(AuthService authService)
  {
    _authService = authService;
  }

  public async Task<TResponseBody> GetAsync<TResponseBody>(string requestUri, CancellationToken? cancellationToken = null)
  {
    return await SendAsync<TResponseBody>("GET", requestUri, null, cancellationToken);
  }

  public async Task<JToken> GetAsync(string requestUri, CancellationToken? cancellationToken = null)
  {
    return await GetAsync<JToken>(requestUri, cancellationToken);
  }

  public async Task<TResponseBody> PostAsync<TResponseBody>(string requestUri, object requestBody = null, CancellationToken? cancellationToken = null)
  {
    return await SendAsync<TResponseBody>("POST", requestUri, requestBody, cancellationToken);
  }

  public async Task<JToken> PostAsync(string requestUri, object requestBody = null, CancellationToken? cancellationToken = null)
  {
    return await PostAsync<JToken>(requestUri, requestBody, cancellationToken);
  }

  public async Task<TResponseBody> PatchAsync<TResponseBody>(string requestUri, object requestBody = null, CancellationToken? cancellationToken = null)
  {
    return await SendAsync<TResponseBody>("PATCH", requestUri, requestBody, cancellationToken);
  }

  public async Task<JToken> PatchAsync(string requestUri, object requestBody = null, CancellationToken? cancellationToken = null)
  {
    return await PatchAsync<JToken>(requestUri, requestBody, cancellationToken);
  }

  public async Task<TResponseBody> SendAsync<TResponseBody>(string method, string requestUri, object requestBody = null, CancellationToken? cancellationToken = null)
  {
    var serializedRequestBody = requestBody != null ? JsonConvert.SerializeObject(requestBody) : null;
    using var request = requestBody != null
      ? UnityWebRequest.Post($"{Constants.BACKEND_BASE_URL}/{requestUri}", serializedRequestBody, "application/json")
      : UnityWebRequest.Get($"{Constants.BACKEND_BASE_URL}/{requestUri}");
    request.method = method;

    if (_authService.IsAuthenticated)
    {
      request.SetRequestHeader("Authorization", $"Bearer {_authService.AuthToken}");
    }

    var operation = request.SendWebRequest();
    while (!operation.isDone)
    {
      cancellationToken?.ThrowIfCancellationRequested();
      await Task.Yield();
    }

    if (request.result != UnityWebRequest.Result.Success)
    {
      throw new HttpRequestException(request.error);
    }

    return JsonConvert.DeserializeObject<TResponseBody>(request.downloadHandler.text);
  }
}
