import { NextFunction, Request, Response } from "express";
import { validationResult } from 'express-validator';

export function ensureValidRequest(req: Request, res: Response, next: NextFunction) {
  const errors = validationResult(req);
  if (!errors.isEmpty()) {
    res.status(400).json({
      name: 'ValidationError',
      errors: errors.array({ onlyFirstError: true }),
    });
    return;
  }
  next();
}
