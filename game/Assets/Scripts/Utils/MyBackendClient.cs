using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System.Threading;
using UnityEngine;

// public class MyBackendClient : HttpClient
// {
//   private static string BACKEND_BASE_URL = "http://localhost:8080/";

//   private AuthService authService;

//   public MyBackendClient(AuthService authService)
//   {
//     this.authService = authService;
//     BaseAddress = new Uri(BACKEND_BASE_URL);
//   }

//   public async Task<TResponseBody> GetAsync<TResponseBody>(string requestUri)
//   {
//     var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
//     if (authService.IsAuthenticated)
//     {
//       request.Headers.Add("Authorization", $"Bearer {authService.AuthToken}");
//     }
//     var response = await base.SendAsync(request);
//     var serializedResponseBody = await response.Content.ReadAsStringAsync();
//     return JsonConvert.DeserializeObject<TResponseBody>(serializedResponseBody);
//   }

//   new public async Task<JToken> GetAsync(string requestUri)
//   {
//     return await GetAsync<JToken>(requestUri);
//   }

//   public async Task<TResponseBody> PostAsync<TResponseBody>(string requestUri, object requestBody)
//   {
//     var serializedRequestBody = JsonConvert.SerializeObject(requestBody);
//     var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
//     request.Content = new StringContent(serializedRequestBody, Encoding.UTF8, "application/json");
//     if (authService.IsAuthenticated)
//     {
//       request.Headers.Add("Authorization", $"Bearer {authService.AuthToken}");
//     }
//     var response = await base.SendAsync(request);
//     var serializedResponseBody = await response.Content.ReadAsStringAsync();
//     return JsonConvert.DeserializeObject<TResponseBody>(serializedResponseBody);
//   }

//   public async Task<JToken> PostAsync(string requestUri, object requestBody)
//   {
//     return await PostAsync<JToken>(requestUri, requestBody);
//   }
// }

public class MyBackendClient
{
  private AuthService authService;

  public MyBackendClient(AuthService authService)
  {
    this.authService = authService;
  }

  public async Task<TResponseBody> GetAsync<TResponseBody>(string requestUri, CancellationToken? cancellationToken = null)
  {
    using var request = UnityWebRequest.Get($"{Constants.BACKEND_BASE_URL}/{requestUri}");
    if (authService.IsAuthenticated)
    {
      request.SetRequestHeader("Authorization", $"Bearer {authService.AuthToken}");
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

  public async Task<JToken> GetAsync(string requestUri, CancellationToken? cancellationToken = null)
  {
    return await GetAsync<JToken>(requestUri, cancellationToken);
  }

  public async Task<TResponseBody> PostAsync<TResponseBody>(string requestUri, object requestBody = null)
  {
    var serializedRequestBody = requestBody != null ? JsonConvert.SerializeObject(requestBody) : null;
    using var request = UnityWebRequest.Post($"{Constants.BACKEND_BASE_URL}/{requestUri}", serializedRequestBody, "application/json");
    if (authService.IsAuthenticated)
    {
      request.SetRequestHeader("Authorization", $"Bearer {authService.AuthToken}");
    }

    var operation = request.SendWebRequest();
    while (!operation.isDone)
    {
      await Task.Yield();
    }

    if (request.result != UnityWebRequest.Result.Success)
    {
      throw new HttpRequestException(request.error);
    }

    return JsonConvert.DeserializeObject<TResponseBody>(request.downloadHandler.text);
  }

  public async Task<JToken> PostAsync(string requestUri, object requestBody = null)
  {
    return await PostAsync<JToken>(requestUri, requestBody);
  }

  public async Task<TResponseBody> PatchAsync<TResponseBody>(string requestUri, object requestBody = null)
  {
    var serializedRequestBody = requestBody != null ? JsonConvert.SerializeObject(requestBody) : null;
    using var request = UnityWebRequest.Post($"{Constants.BACKEND_BASE_URL}/{requestUri}", serializedRequestBody, "application/json");
    request.method = "PATCH";
    if (authService.IsAuthenticated)
    {
      request.SetRequestHeader("Authorization", $"Bearer {authService.AuthToken}");
    }

    var operation = request.SendWebRequest();
    while (!operation.isDone)
    {
      await Task.Yield();
    }

    if (request.result != UnityWebRequest.Result.Success)
    {
      throw new HttpRequestException(request.error);
    }

    return JsonConvert.DeserializeObject<TResponseBody>(request.downloadHandler.text);
  }

  public async Task<JToken> PatchAsync(string requestUri, object requestBody = null)
  {
    return await PatchAsync<JToken>(requestUri, requestBody);
  }
}
