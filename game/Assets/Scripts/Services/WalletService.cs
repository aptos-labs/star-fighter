using System;
using System.Threading;
using System.Threading.Tasks;

public class AccountData
{
  public string address;
  public string publicKey;
  public string pairingId;
}

public interface IWalletService
{
  public event EventHandler<AccountData> OnConnect;
  public event EventHandler OnDisconnect;
  public AccountData AccountData { get; }
  public bool IsConnected { get; }
  public Task Connect(CancellationToken? cancellationToken);
  public Task Disconnect();
}

#if !UNITY_EDITOR && UNITY_WEBGL
public class WalletService : WebWalletServiceImpl { }
#else
public class WalletService : ICWalletServiceImpl { }
#endif
