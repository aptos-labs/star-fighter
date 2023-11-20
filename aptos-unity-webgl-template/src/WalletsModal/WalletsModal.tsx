import {
  Box,
  Typography,
  useTheme,
  IconButton,
  Dialog,
  Stack,
} from "@mui/material";
import { grey } from "./aptosColorPalette";
// reported bug with loading mui icons with esm, therefore need to import like this https://github.com/mui/material-ui/issues/35233
import { LanOutlined as LanOutlinedIcon } from "@mui/icons-material";
import { Close as CloseIcon } from "@mui/icons-material";
import { forwardRef, useImperativeHandle, useRef, useState } from "react";
import { Wallet } from "@aptos-labs/wallet-adapter-core";
import { useWalletAdapter } from "../hooks/useWalletAdapter";
import { Deferred, makeDeferred } from "../utils";
import { WalletsModalRow } from "./WalletsModalRow";

export interface WalletsModalHandle {
  connect: () => Promise<boolean>;
}

export interface WalletsModalProps {
  networkSupport?: string;
}

export const WalletsModal = forwardRef<WalletsModalHandle, WalletsModalProps>(({
  networkSupport,
}, ref) => {
  const walletAdapter = useWalletAdapter();
  const theme = useTheme();

  const [isOpen, setIsOpen] = useState(false);
  const deferredConnectResponse = useRef<Deferred<boolean>>();

  useImperativeHandle(ref, () => ({
    connect: async () => {
      const deferred = makeDeferred<boolean>();
      deferredConnectResponse.current?.resolve(false);
      deferredConnectResponse.current = deferred;
      setIsOpen(true);
      return deferred.promise;
    }
  }), []);

  const onWalletClick = async (wallet: Wallet) => {
    setIsOpen(false);
    try {
      await walletAdapter.connect(wallet.name);
      const isConnected = await walletAdapter.isConnected();
      deferredConnectResponse.current?.resolve(isConnected);
    } catch (err) {
      console.error('Connection error', err);
      deferredConnectResponse.current?.resolve(false);
    }
  };

  const onClose = () => {
    setIsOpen(false);
    deferredConnectResponse.current?.resolve(false);
  };

  return (
    <Dialog
      open={isOpen}
      onClose={onClose}
      aria-labelledby="wallet selector modal"
      aria-describedby="select a wallet to connect"
      sx={{ borderRadius: `${theme.shape.borderRadius}px` }}
      maxWidth="xs"
      fullWidth>
      <Stack
        sx={{
          display: "flex",
          flexDirection: "column",
          top: "50%",
          left: "50%",
          bgcolor: "background.paper",
          boxShadow: 24,
          p: 3,
        }}>
        <IconButton
          onClick={onClose}
          sx={{
            position: "absolute",
            right: 12,
            top: 12,
            color: grey[450],
          }}>
          <CloseIcon />
        </IconButton>
        <Typography align="center" variant="h5" pt={2}>
          Connect Wallet
        </Typography>
        <Box
          sx={{
            display: "flex",
            gap: 0.5,
            alignItems: "center",
            justifyContent: "center",
            mb: 4,
          }}>
          {networkSupport && (
            <>
              <LanOutlinedIcon
                sx={{
                  fontSize: "0.9rem",
                  color: grey[400],
                }}
              />
              <Typography
                sx={{
                  display: "inline-flex",
                  fontSize: "0.9rem",
                  color: grey[400],
                }}
                align="center">
                {networkSupport} only
              </Typography>
            </>
          )}
        </Box>
        <Box>{
          walletAdapter.wallets.map((wallet) =>
            <WalletsModalRow key={wallet.name} wallet={wallet} onClick={onWalletClick} />
          )
        }</Box>
      </Stack>
    </Dialog>
  );
});
