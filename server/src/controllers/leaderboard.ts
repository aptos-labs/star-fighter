import express from 'express';
import expressAsyncHandle from 'express-async-handler';
import { ensureAuthenticated } from '../middlewares';
import { getUsersStats } from '../utils/usersStats';

export const leaderboardCtrl = express.Router();

export type GetLeaderboardRequestQueryParams = {
  from: number;
  to: number;
}

leaderboardCtrl.get('', ensureAuthenticated, expressAsyncHandle(async (req, res) => {
  const { from, to } = req.query as any as GetLeaderboardRequestQueryParams;
  if (to < from) {
    res.status(400).json();
    return;
  }
  if (to - from > 20) {
    res.status(400).json();
    return;
  }
  const usersStats = await getUsersStats();
  const totalCount = usersStats.byScoreDescending.length;
  const rows = usersStats.byScoreDescending.slice(from, to);
  res.status(200).json({ totalCount, rows });
}));
