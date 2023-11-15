import { ADMIN_ACCOUNT_ADDRESS } from "../constants";
import { aptosProvider } from "./adminOperations";
import { compareAscending, compareDescending } from "./compare";

interface AllUserStatsItem {
  addr: string;
  best_survival_time_ms: string;
  games_played: string;
}

export interface UserStats {
  bestSurvivalTimeMs: number;
  gamesPlayed: number;
}

export interface LeaderboardItem extends UserStats {
  address: string;
}

interface UsersStats {
  byAddress: { [address: string]: LeaderboardItem };
  byScoreDescending: LeaderboardItem[];
}

let cachedUsersStats: UsersStats | undefined;

async function fetchUsersStats() {
  const responseBody = await aptosProvider.view({
    function: `${ADMIN_ACCOUNT_ADDRESS}::star_fighter::get_all_user_stats`,
    type_arguments: [],
    arguments: [],
  });
  const rawUsersStats = responseBody[0] as AllUserStatsItem[];

  cachedUsersStats = {
    byAddress: {},
    byScoreDescending: [],
  };

  for (const userStats of rawUsersStats) {
    const entry: LeaderboardItem = {
      address: userStats.addr,
      bestSurvivalTimeMs: Number(userStats.best_survival_time_ms),
      gamesPlayed: Number(userStats.games_played),
    };
    cachedUsersStats.byScoreDescending.push(entry);
    cachedUsersStats.byAddress[entry.address] = entry;
  }

  ensureLeaderboardSorted();
  return cachedUsersStats!;
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
  })
}

export async function getUsersStats() {
  if (!cachedUsersStats) {
    cachedUsersStats = await fetchUsersStats();
  }
  return cachedUsersStats;
}

export interface UserStatsWithRank extends UserStats {
  rank?: number;
}

export async function getUserStats(address: string): Promise<UserStatsWithRank> {
  const usersStats = await getUsersStats();
  if (address in usersStats.byAddress) {
    const userStats = usersStats.byAddress[address];
    const rank = usersStats.byScoreDescending.indexOf(userStats) + 1;
    return { ...userStats, rank };
  }

  return {
    gamesPlayed: 0,
    bestSurvivalTimeMs: 0,
    rank: undefined,
  };
}

export async function updateUserStats(address: string, survivalTimeMs: number) {
  const userStats = await getUsersStats();
  let entry = userStats.byAddress[address];
  if (!entry) {
    entry = {
      address,
      bestSurvivalTimeMs: survivalTimeMs,
      gamesPlayed: 1,
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
