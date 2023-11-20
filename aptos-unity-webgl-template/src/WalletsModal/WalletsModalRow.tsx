import {
  Box,
  Button,
  ListItem,
  ListItemAvatar,
  ListItemButton,
  ListItemText,
  useTheme,
  Grid,
} from "@mui/material";
import { grey } from "./aptosColorPalette";
import { useEffect, useState } from "react";
import { Wallet, WalletReadyState, isRedirectable } from "@aptos-labs/wallet-adapter-core";
import { useWalletAdapter } from "../hooks/useWalletAdapter";

export interface WalletsModalRowProps {
  wallet: Wallet;
  onClick: (wallet: Wallet) => void;
}

function ConnectWalletRow({ wallet, onClick }: WalletsModalRowProps) {
  const theme = useTheme();
  return (
    <ListItem disablePadding>
      <ListItemButton
        alignItems="center"
        disableGutters
        onClick={() => onClick(wallet)}
        sx={{
          background: theme.palette.mode === "dark" ? grey[700] : grey[200],
          padding: "1rem 1rem",
          borderRadius: `${theme.shape.borderRadius}px`,
          display: "flex",
          gap: "1rem",
        }}>
        <ListItemAvatar
          sx={{
            display: "flex",
            alignItems: "center",
            width: "2rem",
            height: "2rem",
            minWidth: "0",
            color: `${theme.palette.text.primary}`,
          }}>
          <Box
            component="img"
            src={wallet.icon}
            sx={{ width: "100%", height: "100%" }}
          />
        </ListItemAvatar>
        <ListItemText
          primary={wallet.name}
          primaryTypographyProps={{
            fontSize: 18,
          }}
        />
        <Button
          variant="contained"
          size="small"
          className="wallet-connect-button">
          Connect
        </Button>
      </ListItemButton>
    </ListItem>
  );
}

function InstallWalletRow({ wallet }: Omit<WalletsModalRowProps, 'onClick'>) {
  const theme = useTheme();

  return (
    <ListItem
      alignItems="center"
      sx={{
        borderRadius: `${theme.shape.borderRadius}px`,
        background: theme.palette.mode === "dark" ? grey[700] : grey[200],
        padding: "1rem 1rem",
        gap: "1rem",
      }}>
      <ListItemAvatar
        sx={{
          display: "flex",
          alignItems: "center",
          width: "2rem",
          height: "2rem",
          minWidth: "0",
          opacity: "0.25",
        }}>
        <Box
          component="img"
          src={wallet.icon}
          sx={{ width: "100%", height: "100%" }}
        />
      </ListItemAvatar>
      <ListItemText
        sx={{
          opacity: "0.25",
        }}
        primary={wallet.name}
        primaryTypographyProps={{
          fontSize: 18,
        }}
      />
      <Button
        LinkComponent={"a"}
        href={wallet.url}
        target="_blank"
        size="small"
        className="wallet-connect-install">
        Install
      </Button>
    </ListItem>
  );
}

export function WalletsModalRow({ wallet, onClick }: WalletsModalRowProps) {
  const walletAdapter = useWalletAdapter();
  const [walletState, setWalletState] = useState(wallet.readyState);

  useEffect(() => {
    const onStateChange = () => {
      setWalletState(wallet.readyState);
    };
    walletAdapter.on("readyStateChange", onStateChange);
    return () => {
      walletAdapter.off("readyStateChange", onStateChange);
    };
  }, [wallet, walletAdapter]);

  const hasMobileSupport = Boolean(wallet.deeplinkProvider);
  const isWalletReady =
    walletState === WalletReadyState.Installed ||
    walletState === WalletReadyState.Loadable;

  if (isRedirectable() && !isWalletReady && !hasMobileSupport) {
    return null;
  }

  return (
    <Grid xs={12} paddingY={0.5} item>
      {isWalletReady ? (
        <ConnectWalletRow
          wallet={wallet}
          onClick={onClick}
        />
      ) : (
        <InstallWalletRow wallet={wallet} />
      )}
    </Grid>
  );
}