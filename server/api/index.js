"use strict";
var __create = Object.create;
var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __getProtoOf = Object.getPrototypeOf;
var __hasOwnProp = Object.prototype.hasOwnProperty;
var __export = (target, all) => {
  for (var name in all)
    __defProp(target, name, { get: all[name], enumerable: true });
};
var __copyProps = (to, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames(from))
      if (!__hasOwnProp.call(to, key) && key !== except)
        __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
  }
  return to;
};
var __toESM = (mod, isNodeMode, target) => (target = mod != null ? __create(__getProtoOf(mod)) : {}, __copyProps(
  // If the importer is in node compatibility mode or this is not an ESM
  // file that has been converted to a CommonJS file using a Babel-
  // compatible transform (i.e. "__esModule" has not been set), then set
  // "default" to the CommonJS "module.exports" for node compatibility.
  isNodeMode || !mod || !mod.__esModule ? __defProp(target, "default", { value: mod, enumerable: true }) : target,
  mod
));
var __toCommonJS = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);

// src/index.ts
var src_exports = {};
__export(src_exports, {
  default: () => src_default
});
module.exports = __toCommonJS(src_exports);
var import_config = require("dotenv/config");

// src/app.ts
var import_ts_sdk3 = require("@aptos-labs/ts-sdk");
var import_cors = __toESM(require("cors"));
var import_express4 = __toESM(require("express"));

// src/controllers/auth.ts
var import_express = __toESM(require("express"));
var import_express_async_handler = __toESM(require("express-async-handler"));
var import_express_validator2 = require("express-validator");
var import_jsonwebtoken = __toESM(require("jsonwebtoken"));

// src/constants.ts
var SERVER_PORT = process.env.SERVER_PORT || 8080;
var ADMIN_ACCOUNT_ADDRESS = process.env.ADMIN_ADDRESS;
var ADMIN_ACCOUNT_SECRET_KEY = process.env.ADMIN_SECRET_KEY;
var JWT_SECRET = process.env.JWT_SECRET;
var IDENTITY_CONNECT_DAPP_ID = process.env.IDENTITY_CONNECT_DAPP_ID;
var IDENTITY_CONNECT_REFERER = process.env.IDENTITY_CONNECT_REFERER;
var IDENTITY_CONNECT_ENVIRONMENTS_URLS = {
  production: "https://identityconnect.com",
  staging: "https://identity-connect.staging.gcp.aptosdev.com"
};
var IDENTITY_CONNECT_ENVIRONMENT_OR_BASE_URL = "production";
function isIcEnvironment(environmentOrBaseUrl) {
  return Object.keys(IDENTITY_CONNECT_ENVIRONMENTS_URLS).includes(environmentOrBaseUrl);
}
var IDENTITY_CONNECT_BASE_URL = isIcEnvironment(IDENTITY_CONNECT_ENVIRONMENT_OR_BASE_URL) ? IDENTITY_CONNECT_ENVIRONMENTS_URLS[IDENTITY_CONNECT_ENVIRONMENT_OR_BASE_URL] : IDENTITY_CONNECT_ENVIRONMENT_OR_BASE_URL;

// src/middlewares/ensureAuthenticated.ts
var import_express_jwt = require("express-jwt");
var ensureAuthenticated = (0, import_express_jwt.expressjwt)({
  secret: JWT_SECRET,
  algorithms: ["HS256"]
});

// src/middlewares/ensureValidRequest.ts
var import_express_validator = require("express-validator");
function ensureValidRequest(req, res, next) {
  const errors = (0, import_express_validator.validationResult)(req);
  if (!errors.isEmpty()) {
    res.status(400).json({
      name: "ValidationError",
      errors: errors.array({ onlyFirstError: true })
    });
    return;
  }
  next();
}

// src/utils/ICDappBackendClient.ts
var import_axios = __toESM(require("axios"));
var ICDappBackendClient = class {
  constructor() {
    this.axiosInstance = import_axios.default.create({
      baseURL: IDENTITY_CONNECT_BASE_URL
    });
  }
  async getPairing(id) {
    const response = await this.axiosInstance.get(`v1/pairing/${id}`, {
      validateStatus: (status) => status === 200 || status === 404
    });
    return response?.data.data.pairing;
  }
  async createPairing(dappEd25519PublicKeyB64) {
    const response = await this.axiosInstance.post("v1/pairing", {
      dappEd25519PublicKeyB64,
      dappId: IDENTITY_CONNECT_DAPP_ID
    }, {
      headers: {
        Referer: IDENTITY_CONNECT_REFERER
      }
    });
    return response.data.data.pairing;
  }
};

// src/controllers/auth.ts
var import_api = require("@identity-connect/api");
var import_ts_sdk = require("@aptos-labs/ts-sdk");
var authCtrl = import_express.default.Router();
authCtrl.post(
  "/tokens",
  (0, import_express_validator2.oneOf)([
    [
      (0, import_express_validator2.body)("address").isHexadecimal(),
      (0, import_express_validator2.body)("publicKey").isHexadecimal()
    ],
    [
      (0, import_express_validator2.body)("pairingId").isUUID()
    ]
  ]),
  ensureValidRequest,
  (0, import_express_async_handler.default)(async (req, res) => {
    if ("address" in req.body) {
      const token2 = import_jsonwebtoken.default.sign({
        address: req.body.address,
        publicKey: req.body.publicKey
      }, JWT_SECRET);
      res.status(200).json({ token: token2 });
      return;
    }
    const icDappBackendClient = new ICDappBackendClient();
    const pairing = await icDappBackendClient.getPairing(req.body.pairingId);
    if (!pairing || pairing.status !== import_api.PairingStatus.Finalized) {
      res.status(404).json();
      return;
    }
    const publicKeyBytes = Buffer.from(pairing.account.ed25519PublicKeyB64, "base64");
    const publicKey = import_ts_sdk.Hex.fromHexInput(publicKeyBytes).toString();
    const token = import_jsonwebtoken.default.sign({
      address: pairing.account.accountAddress,
      publicKey,
      pairingId: pairing.id
    }, JWT_SECRET);
    res.status(200).json({ token });
  })
);
authCtrl.post(
  "/ic-pairings",
  (0, import_express_validator2.body)("dappEd25519PublicKeyB64").notEmpty().isBase64(),
  ensureValidRequest,
  (0, import_express_async_handler.default)(async (req, res) => {
    const { dappEd25519PublicKeyB64 } = req.body;
    const icDappBackendClient = new ICDappBackendClient();
    const pairing = await icDappBackendClient.createPairing(dappEd25519PublicKeyB64);
    res.status(200).json({ pairingId: pairing.id, environment: IDENTITY_CONNECT_ENVIRONMENT_OR_BASE_URL });
  })
);

// src/controllers/user.ts
var import_ts_sdk2 = require("@aptos-labs/ts-sdk");
var import_express2 = __toESM(require("express"));
var import_express_async_handler2 = __toESM(require("express-async-handler"));

// src/utils/getUser.ts
function getUser(req) {
  const jwtRequest = req;
  if (jwtRequest.auth === void 0) {
    throw new Error("User not authenticated, are you using `ensureAuthenticated`");
  }
  return jwtRequest.auth;
}

// src/utils/adminOperations.ts
var import_aptos = require("aptos");
var aptosProvider = new import_aptos.Provider(import_aptos.Network.TESTNET);
var adminSecretKeyBytes = new import_aptos.HexString(ADMIN_ACCOUNT_SECRET_KEY).toUint8Array();
var adminSigner = new import_aptos.AptosAccount(adminSecretKeyBytes, ADMIN_ACCOUNT_ADDRESS);
var adminPendingOperations = void 0;
async function simulateAdminTransactionWithPayloadInternal(payload) {
  const accountData = await aptosProvider.getAccount(ADMIN_ACCOUNT_ADDRESS);
  const sequenceNumber = BigInt(accountData.sequence_number);
  const expirationTimestamp = Math.floor(Date.now() / 1e3) + 120;
  const rawTxn = new import_aptos.TxnBuilderTypes.RawTransaction(
    import_aptos.TxnBuilderTypes.AccountAddress.fromHex(ADMIN_ACCOUNT_ADDRESS),
    sequenceNumber,
    payload,
    BigInt(0),
    BigInt(0),
    BigInt(expirationTimestamp),
    new import_aptos.TxnBuilderTypes.ChainId(await aptosProvider.getChainId())
  );
  const txnAuthenticator = new import_aptos.TxnBuilderTypes.TransactionAuthenticatorEd25519(
    new import_aptos.TxnBuilderTypes.Ed25519PublicKey(adminSigner.signingKey.publicKey),
    new import_aptos.TxnBuilderTypes.Ed25519Signature(new Uint8Array(64))
  );
  const signedTxn = new import_aptos.TxnBuilderTypes.SignedTransaction(rawTxn, txnAuthenticator);
  const [simulatedUserTxn] = await aptosProvider.submitBCSSimulation(import_aptos.BCS.bcsToBytes(signedTxn), {
    estimateGasUnitPrice: true,
    estimateMaxGasAmount: true
  });
  if (!simulatedUserTxn.success) {
    throw new Error(simulatedUserTxn.vm_status);
  }
  return {
    gasUnitPrice: BigInt(simulatedUserTxn.gas_unit_price),
    gasUsed: BigInt(simulatedUserTxn.gas_used)
  };
}
async function submitAdminTransactionWithPayloadInternal(payload, options) {
  const accountData = await aptosProvider.getAccount(ADMIN_ACCOUNT_ADDRESS);
  const sequenceNumber = BigInt(accountData.sequence_number);
  const expirationTimestamp = Math.floor(Date.now() / 1e3) + 120;
  const rawTxn = new import_aptos.TxnBuilderTypes.RawTransaction(
    import_aptos.TxnBuilderTypes.AccountAddress.fromHex(ADMIN_ACCOUNT_ADDRESS),
    sequenceNumber,
    payload,
    BigInt(options.maxGasAmount),
    BigInt(options.gasUnitPrice),
    BigInt(expirationTimestamp),
    new import_aptos.TxnBuilderTypes.ChainId(await aptosProvider.getChainId())
  );
  const txnSigningMessage = import_aptos.TransactionBuilder.getSigningMessage(rawTxn);
  const signatureBytes = adminSigner.signBuffer(txnSigningMessage).toUint8Array();
  const txnAuthenticator = new import_aptos.TxnBuilderTypes.TransactionAuthenticatorEd25519(
    new import_aptos.TxnBuilderTypes.Ed25519PublicKey(adminSigner.signingKey.publicKey),
    new import_aptos.TxnBuilderTypes.Ed25519Signature(signatureBytes)
  );
  const signedTxn = new import_aptos.TxnBuilderTypes.SignedTransaction(rawTxn, txnAuthenticator);
  const pendingTxn = await aptosProvider.submitSignedBCSTransaction(import_aptos.BCS.bcsToBytes(signedTxn));
  const userTxn = await aptosProvider.waitForTransactionWithResult(pendingTxn.hash);
  return userTxn;
}
async function simulateAdminTransactionWithPayload(payload) {
  const currPendingOperations = adminPendingOperations?.catch() ?? Promise.resolve();
  const newPendingOperations = currPendingOperations.then(() => simulateAdminTransactionWithPayloadInternal(payload));
  adminPendingOperations = newPendingOperations;
  return newPendingOperations;
}
async function submitAdminTransactionWithPayload(payload, options) {
  const currPendingOperations = adminPendingOperations?.catch() ?? Promise.resolve();
  const newPendingOperations = currPendingOperations.then(() => submitAdminTransactionWithPayloadInternal(payload, options));
  adminPendingOperations = newPendingOperations;
  return newPendingOperations;
}

// src/utils/compare.ts
function compareAscending(lhs, rhs) {
  if (lhs < rhs) {
    return -1;
  }
  if (lhs > rhs) {
    return 1;
  }
  return 0;
}
function compareDescending(lhs, rhs) {
  return compareAscending(rhs, lhs);
}

// src/utils/usersStats.ts
var cachedUsersStats;
async function fetchUsersStats() {
  const responseBody = await aptosProvider.view({
    function: `${ADMIN_ACCOUNT_ADDRESS}::star_fighter::get_all_user_stats`,
    type_arguments: [],
    arguments: []
  });
  const rawUsersStats = responseBody[0];
  cachedUsersStats = {
    byAddress: {},
    byScoreDescending: []
  };
  for (const userStats of rawUsersStats) {
    const entry = {
      address: userStats.addr,
      bestSurvivalTimeMs: Number(userStats.best_survival_time_ms),
      gamesPlayed: Number(userStats.games_played)
    };
    cachedUsersStats.byScoreDescending.push(entry);
    cachedUsersStats.byAddress[entry.address] = entry;
  }
  ensureLeaderboardSorted();
  return cachedUsersStats;
}
function ensureLeaderboardSorted() {
  cachedUsersStats?.byScoreDescending.sort((lhs, rhs) => {
    if (lhs.bestSurvivalTimeMs !== rhs.bestSurvivalTimeMs) {
      return compareDescending(lhs.bestSurvivalTimeMs, rhs.bestSurvivalTimeMs);
    }
    if (lhs.gamesPlayed !== rhs.gamesPlayed) {
      return compareAscending(lhs.bestSurvivalTimeMs, rhs.bestSurvivalTimeMs);
    }
    return compareAscending(lhs.address, rhs.address);
  });
}
async function getUsersStats() {
  if (!cachedUsersStats) {
    cachedUsersStats = await fetchUsersStats();
  }
  return cachedUsersStats;
}
async function getUserStats(address) {
  const usersStats = await getUsersStats();
  if (address in usersStats.byAddress) {
    const userStats = usersStats.byAddress[address];
    const rank = usersStats.byScoreDescending.indexOf(userStats) + 1;
    return { ...userStats, rank };
  }
  return {
    gamesPlayed: 0,
    bestSurvivalTimeMs: 0,
    rank: void 0
  };
}
async function updateUserStats(address, survivalTimeMs) {
  const userStats = await getUsersStats();
  let entry = userStats.byAddress[address];
  if (!entry) {
    entry = {
      address,
      bestSurvivalTimeMs: survivalTimeMs,
      gamesPlayed: 1
    };
    userStats.byAddress[address] = entry;
    userStats.byScoreDescending.push(entry);
  } else {
    entry.gamesPlayed += 1;
    if (survivalTimeMs > entry.bestSurvivalTimeMs) {
      entry.bestSurvivalTimeMs = survivalTimeMs;
    }
  }
  ensureLeaderboardSorted();
}

// src/controllers/user.ts
var userCtrl = import_express2.default.Router();
userCtrl.get("", ensureAuthenticated, (0, import_express_async_handler2.default)(async (req, res) => {
  const user = getUser(req);
  const config = new import_ts_sdk2.AptosConfig({ network: import_ts_sdk2.Network.TESTNET });
  const aptos = new import_ts_sdk2.Aptos(config);
  const coinResource = await aptos.getAccountResource({
    accountAddress: user.address,
    resourceType: "0x1::coin::CoinStore<0x1::aptos_coin::AptosCoin>"
  });
  const aptBalance = BigInt(coinResource.coin.value).toString();
  const { bestSurvivalTimeMs, gamesPlayed, rank } = await getUserStats(user.address);
  res.status(200).json({
    name: user.address,
    aptBalance,
    bestSurvivalTimeMs,
    gamesPlayed,
    rank
  });
}));

// src/controllers/session.ts
var import_express3 = __toESM(require("express"));
var import_express_async_handler3 = __toESM(require("express-async-handler"));
var import_crypto = require("crypto");

// src/utils/buildSaveGameSessionPayload.ts
var import_aptos2 = require("aptos");
function buildSaveSessionPayload(userAddress, survivalTimeMs) {
  const entryFunction = import_aptos2.TxnBuilderTypes.EntryFunction.natural(
    `${ADMIN_ACCOUNT_ADDRESS}::star_fighter`,
    "save_game_session",
    [],
    [
      import_aptos2.BCS.bcsToBytes(import_aptos2.TxnBuilderTypes.AccountAddress.fromHex(userAddress)),
      import_aptos2.BCS.bcsSerializeUint64(survivalTimeMs)
    ]
  );
  return new import_aptos2.TxnBuilderTypes.TransactionPayloadEntryFunction(entryFunction);
}

// src/utils/buildCoinTransferPayload.ts
var import_aptos3 = require("aptos");
function buildCoinTransferPojoPayload(recipient, amount) {
  return {
    type: "entry_function_payload",
    function: "0x1::coin::transfer",
    type_arguments: ["0x1::aptos_coin::AptosCoin"],
    arguments: [recipient, amount.toString()]
  };
}

// src/controllers/session.ts
async function getGameSessionMaximumCost(userAddress) {
  const payload = buildSaveSessionPayload(userAddress, 1e5);
  const { gasUnitPrice, gasUsed } = await simulateAdminTransactionWithPayload(payload);
  return 2n * gasUsed * gasUnitPrice;
}
var activeSessions = {};
var sessionCtrl = import_express3.default.Router();
sessionCtrl.post("", ensureAuthenticated, (0, import_express_async_handler3.default)(async (req, res) => {
  const user = getUser(req);
  const requestedFunds = await getGameSessionMaximumCost(user.address);
  const sessionId = (0, import_crypto.randomUUID)();
  const fundTransferPayload = buildCoinTransferPojoPayload(ADMIN_ACCOUNT_ADDRESS, requestedFunds);
  const serializedFundTransferPayload = JSON.stringify(fundTransferPayload);
  activeSessions[sessionId] = {
    address: user.address,
    createdAt: Date.now(),
    requestedFunds,
    fundTransferPayload
  };
  res.status(200).json(
    {
      sessionId,
      fundTransferPayload: serializedFundTransferPayload
    }
  );
}));
sessionCtrl.patch("/:id", ensureAuthenticated, (0, import_express_async_handler3.default)(async (req, res) => {
  const user = getUser(req);
  const sessionId = req.params.id;
  const session = activeSessions[sessionId];
  if (!session) {
    res.status(404).json();
    return;
  }
  const gasUnitPrice = 100;
  const maxGasAmount = Math.floor(Number(session.requestedFunds) / gasUnitPrice);
  const payload = buildSaveSessionPayload(user.address, req.body.survivalTimeMs);
  const userTxn = await submitAdminTransactionWithPayload(payload, {
    gasUnitPrice,
    maxGasAmount
  });
  if (!userTxn.success) {
    res.status(500).json(userTxn.vm_status);
    return;
  }
  await updateUserStats(user.address, req.body.survivalTimeMs);
  res.status(204).json();
}));

// src/app.ts
var app = (0, import_express4.default)();
app.use(import_express4.default.json());
app.disable("x-powered-by");
app.use(
  (0, import_cors.default)({
    credentials: true,
    origin: (origin, callback) => {
      callback(null, "*");
    }
  })
);
app.use("/v1/auth", authCtrl);
app.use("/v1/user", userCtrl);
app.use("/v1/sessions", sessionCtrl);
function errorHandler(err, req, res, next) {
  console.log(err);
  if (err instanceof import_ts_sdk3.AptosApiError) {
    res.status(500).json({
      name: err.name,
      message: err.message,
      data: err.data
    });
    return;
  }
  res.status(500).json({
    name: err.name,
    message: err.message
  });
}
app.use(errorHandler);
var app_default = app;

// src/index.ts
app_default.listen(SERVER_PORT, () => {
  console.log(`Starfighter server listening on PORT ${SERVER_PORT}`);
});
var src_default = app_default;
