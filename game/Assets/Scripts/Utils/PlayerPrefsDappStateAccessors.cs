using IdentityConnect;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using K4.Threading;
using System.Threading.Tasks;

public static class LocalStorage
{
  public static T Get<T>(string key) where T : class
  {
    var serialized = PlayerPrefs.GetString(key, null);
    return serialized != null
      ? JsonConvert.DeserializeObject<T>(serialized)
      : null;
  }

  public static void Set<T>(string key, T value) where T : class
  {
    var serialized = JsonConvert.SerializeObject(value);
    if (value == null)
    {
      PlayerPrefs.DeleteKey(key);
    }
    else
    {
      PlayerPrefs.SetString(key, serialized);
    }
  }

  public static string Get(string key)
  {
    var value = PlayerPrefs.GetString(key, null);
    return value;
  }

  public static void Set(string key, string value)
  {
    if (value == null)
    {
      PlayerPrefs.DeleteKey(key);
    }
    else
    {
      PlayerPrefs.SetString(key, value);
    }
  }

  public static void Remove(string key)
  {
    PlayerPrefs.DeleteKey(key);
  }
}

public class PlayerPrefsDappStateAccessors : IDappStateAccessors
{
  private static string IC_PAIRINGS_PLAYER_PREFS_KEY = "icDappPairings";

  public async Task<ICPairingData> get(string address)
  {
    var pairings = await this.getAll();
    return pairings[address];
  }

  public async Task<IDictionary<string, ICPairingData>> getAll()
  {
    var serializedPairings = await K4UnityThreadDispatcher.Execute(PlayerPrefs.GetString, IC_PAIRINGS_PLAYER_PREFS_KEY);
    var pairings = JsonConvert.DeserializeObject<Dictionary<string, ICPairingData>>(serializedPairings) ?? new Dictionary<string, ICPairingData>();
    return pairings;
  }

  public async Task update(string address, ICPairingData pairingData)
  {
    var pairings = await this.getAll();
    if (pairingData == null)
    {
      pairings.Remove(address);
    }
    else
    {
      pairings[address] = pairingData;
    }
    var serializedPairings = JsonConvert.SerializeObject(pairings);
    await K4UnityThreadDispatcher.Execute(PlayerPrefs.SetString, IC_PAIRINGS_PLAYER_PREFS_KEY, serializedPairings);
  }
}
