public static class Constants
{
  public static string IC_DAPP_ID = "<redacted>";
  // public static string IC_DAPP_ID = "d6595c74-77f1-4d2d-a0b7-b59f604895b3";
  public static string IC_DAPP_HOSTNAME = "https://star-fighter.vercel.app";
  public static string IC_BASE_URL = "https://identityconnect.com";
  public static string BACKEND_BASE_URL = "https://star-fighter-api.vercel.app";
  // public static string IC_BASE_URL = "http://ic.com:8083";
  // public static string BACKEND_BASE_URL = "http://127.0.0.1:8080";

  public static string IC_ENVIRONMENT_OR_BASE_URL = IC_BASE_URL != null ? IC_BASE_URL : "production";
}
