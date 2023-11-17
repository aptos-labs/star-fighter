using Aptos;
using QRCoder;
using System.Threading;
using UnityEngine;
using UI = UnityEngine.UI;

public class LoginCtrl : MonoBehaviour
{
  public GameObject welcomeSection;
  public GameObject icPairingSection;
  public UI.Button loginButton;
  public UI.RawImage qrCode;

  private WalletService walletService;
  private AuthService authService;
  private CancellationTokenSource pairingCancellationTokenSource;

  private bool IsConnecting
  {
    set
    {
      var isPairing = value;
      loginButton.interactable = !isPairing;
      var buttonText = loginButton.GetComponentInChildren<UI.Text>();
      buttonText.text = isPairing ? "Connecting..." : "Connect wallet";
    }
  }

  private string QrCodeContent
  {
    set
    {
      welcomeSection.SetActive(value == null);
      icPairingSection.SetActive(value != null);

      if (value == null)
      {
        qrCode.texture = null;
        return;
      }

      QRCodeGenerator qrGenerator = new QRCodeGenerator();
      QRCodeData qrCodeData = qrGenerator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);
      UnityQRCode unityQRCode = new UnityQRCode(qrCodeData);

      var texture = unityQRCode.GetGraphic(20);
      qrCode.texture = texture;
    }
  }

  # region MonoBehavior

  private void Awake()
  {
    authService = FindFirstObjectByType<AuthService>();
    walletService = FindFirstObjectByType<WalletService>();
  }

  private void OnEnable()
  {
    IsConnecting = false;
    walletService.OnConnect += OnConnect;
  }

  private void OnDisable()
  {
    walletService.OnConnect -= OnConnect;
  }

  # endregion

  # region Action handlers

  public async void ConnectWallet()
  {
    IsConnecting = true;
    try
    {
      pairingCancellationTokenSource = new CancellationTokenSource();
      using (pairingCancellationTokenSource)
      {
        var cancellationToken = pairingCancellationTokenSource.Token;
        await walletService.Connect(cancellationToken);
      }
    }
    finally
    {
      pairingCancellationTokenSource = null;
      IsConnecting = false;
    }
  }

  public void OnCancelPairing()
  {
    pairingCancellationTokenSource?.Cancel();
    QrCodeContent = null;
  }

  # endregion

  public void OnIcPairingInitialized(string url)
  {
    QrCodeContent = url;
  }

  private async void OnConnect(object sender, AccountData account)
  {
    if (account.pairingId != null)
    {
      await authService.AuthenticateWithIcPairing(account.pairingId);
    }
    else
    {
      await authService.AuthenticateWithAddressAndPublicKey(account.address, account.publicKey);
    }
    SendMessageUpwards("OnAuthenticated");
  }
}
