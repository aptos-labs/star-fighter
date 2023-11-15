using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuPageCtrl : MonoBehaviour
{
  [SerializeField] private Text _gamesPlayed;
  [SerializeField] private Text _maxTime;
  [SerializeField] private Text _balance;
  [SerializeField] private Text _pilotName;

  public async void Start()
  {
    var client = new BackendClient();

    // Load inventory first
    var (pilot, records) = await client.inventory();
    var pilotStruct = new Pilot
    {
      pilotAddress = pilot
    };
    // GameState.Set<Pilot>("pilot", pilotStruct);

    // Load pilot information on startup
    var (gamesPlayed, maxTime) = await client.pilot(pilot!);
    _pilotName.text = "Unknown";
    _gamesPlayed.text = $"{gamesPlayed}";
    _maxTime.text = $"{maxTime} ms";
    _balance.text = $"{await client.balance() / 100000000.0} APT";
  }

  public async void Disconnect()
  {
    // var icDappClient = new ICDappClient();
    // await icDappClient.disconnect();
    var backend = new BackendClient();

    // GameState.Set<Pilot>("pilot", null);
    SceneManager.LoadScene("Login");

    // Logout after switching scenes, in case it fails (user can always just login again)
    await backend.logout();
  }

  public async void MintPilot()
  {
    var backend = new BackendClient();

    // If there is currently no pilot
    if (false)
    {
      // Create a new pilot
      await backend.mintPilot();

      SceneManager.LoadScene("MainMenu");
    }
  }

  public void PlayNewGame()
  {
    Debug.Log("Clicked");
    SceneManager.LoadScene("Game");
  }
}
