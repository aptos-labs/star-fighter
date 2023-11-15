import { JWT_SECRET } from "../constants";
import { expressjwt } from 'express-jwt';

export const ensureAuthenticated = expressjwt({
  secret: JWT_SECRET,
  algorithms: ['HS256'],
});
