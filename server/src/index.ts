import 'dotenv/config';
import app from './app';
import { SERVER_PORT } from './constants';

app.listen(SERVER_PORT, () => {
  console.log(`Starfighter server listening on PORT ${SERVER_PORT}`);
});

export default app;
