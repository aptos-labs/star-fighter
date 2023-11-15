import { AptosApiError } from '@aptos-labs/ts-sdk';
import cors from 'cors';
import express, { NextFunction, Request, Response } from 'express';
import { authCtrl, userCtrl } from './controllers';
import { sessionCtrl } from './controllers/session';
import { leaderboardCtrl } from './controllers/leaderboard';

const app = express();
app.use(express.json());
app.disable('x-powered-by');
app.use(
  cors({
    credentials: true,
    origin: (origin, callback) => {
      callback(null, '*');
    },
  }),
);

app.use('/v1/auth', authCtrl);
app.use('/v1/user', userCtrl);
app.use('/v1/sessions', sessionCtrl);
app.use('/v1/leaderboard', leaderboardCtrl);

export function errorHandler(
  err: Error,
  req: Request,
  res: Response,
  next: NextFunction,
) {
  console.log(err);
  if (err instanceof AptosApiError) {
    res.status(500).json({
      name: err.name,
      message: err.message,
      data: err.data,
    });
    return;
  }

  res.status(500).json({
    name: err.name,
    message: err.message,
  });
}

app.use(errorHandler);

export default app;
