using System;
using System.Threading;
using System.Threading.Tasks;

public class AccountData
{
  public string address;
  public string publicKey;
  public string pairingId;
}

public class NetworkData
{
  public string name;
  public string chainId;
  public string url;
}

public interface IWalletService
{
  public event EventHandler OnDisconnect;
  public AccountData AccountData { get; }
  public bool IsConnected { get; }
  public Task<AccountData> Connect(CancellationToken? cancellationToken);
  public Task Disconnect();
}

// public class WalletService : LocalWalletServiceImpl { }
#if !UNITY_EDITOR && UNITY_WEBGL
public class WalletService : WebWalletServiceImpl { }
#else
public class WalletService : ICWalletServiceImpl { }
#endif
