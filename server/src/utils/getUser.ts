import { Request } from "express";
import { Request as JWTRequest } from "express-jwt";

export interface AuthenticatedUser {
  address: string;
  publicKey: string;
}

export function getUser(req: Request) {
  const jwtRequest = req as JWTRequest;
  if (jwtRequest.auth === undefined) {
    throw new Error('User not authenticated, are you using `ensureAuthenticated`');
  }
  return jwtRequest.auth as AuthenticatedUser;
}
