import { ADMIN_ACCOUNT_ADDRESS } from "../constants";
import { aptosProvider } from "./adminOperations";
import { compareAscending, compareDescending } from "./compare";
import { kv } from "@vercel/kv"

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

// let cachedUsersStats: UsersStats | undefined;

async function fetchUsersStats() {
  console.log('Fetching user stats. This should happen once per session');
  const responseBody = await aptosProvider.view({
    function: `${ADMIN_ACCOUNT_ADDRESS}::star_fighter::get_all_user_stats`,
    type_arguments: [],
    arguments: [],
  });
  const rawUsersStats = responseBody[0] as AllUserStatsItem[];

  const cachedUsersStats: UsersStats = {
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

  ensureLeaderboardSorted(cachedUsersStats);
  return cachedUsersStats!;
}

function ensureLeaderboardSorted(usersStats: UsersStats) {
  usersStats.byScoreDescending.sort((lhs, rhs) => {
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
  let cachedUsersStats = await kv.get<UsersStats>('cachedUsersStats');
  if (!cachedUsersStats) {
    const fetchedUsersStats = await fetchUsersStats();
    kv.set('cachedUsersStats', fetchedUsersStats);
    return fetchedUsersStats;
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
  const usersStats = await getUsersStats();
  let entry = usersStats.byAddress[address];
  if (!entry) {
    entry = {
      address,
      bestSurvivalTimeMs: survivalTimeMs,
      gamesPlayed: 1,
    };
    usersStats.byAddress[address] = entry;
    usersStats.byScoreDescending.push(entry);
  } else {
    entry.gamesPlayed += 1;
    if (survivalTimeMs > entry.bestSurvivalTimeMs) {
      entry.bestSurvivalTimeMs = survivalTimeMs;
    }
  }
  ensureLeaderboardSorted(usersStats);
  kv.set('cachedUsersStats', usersStats);
}
