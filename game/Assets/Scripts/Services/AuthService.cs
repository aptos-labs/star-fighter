using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using IdentityConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class AuthService : MonoBehaviour
{
  private static string AUTH_TOKEN_PREFS_KEY = "AuthToken";

  private MyBackendClient backendClient;

  # region MonoBehavior

  private void Start()
  {
    backendClient = new MyBackendClient(this);
  }

  #endregion

  # region API

  public string AuthToken
  {
    get => LocalStorage.Get(AUTH_TOKEN_PREFS_KEY);
    private set => LocalStorage.Set(AUTH_TOKEN_PREFS_KEY, value);
  }

  public AccountData AccountData
  {
    get
    {
      var handler = new JwtSecurityTokenHandler();
      var jwtSecurityToken = handler.ReadJwtToken(AuthToken);
      var jwtPayload = JsonConvert.DeserializeObject<JToken>(jwtSecurityToken.Payload.SerializeToJson());
      return new AccountData
      {
        address = jwtPayload["address"].ToString(),
        publicKey = jwtPayload["publicKey"].ToString(),
        pairingId = jwtPayload["address"]?.ToString(),
      };
    }
  }

  public bool IsAuthenticated => (AuthToken ?? "").Length > 0;

  public async Task AuthenticateWithAddressAndPublicKey(string address, string publicKey)
  {
    var responseBody = await backendClient.PostAsync("v1/auth/tokens", new { address, publicKey });
    AuthToken = responseBody["token"].ToString();
  }

  public async Task AuthenticateWithIcPairing(string pairingId)
  {
    var responseBody = await backendClient.PostAsync("v1/auth/tokens", new { pairingId });
    AuthToken = responseBody["token"].ToString();
  }

  public void Logout()
  {
    AuthToken = null;
  }

  # endregion
}
