// Desarrollo. La API se resuelve según CÓMO se abrió el front (plan Etapa F2, 2026-09-12):
//   · por localhost → https://localhost:7100/api (como siempre);
//   · por la IP de la máquina (el celular en la misma WiFi) → http://<ip>:5100/api, porque el
//     certificado de desarrollo no vale en el teléfono. La API tiene que escuchar en 0.0.0.0
//     (`dotnet run --launch-profile lan`) y el front servirse con `npm run start:lan`.
const host = typeof window !== 'undefined' ? window.location.hostname : 'localhost';
const esLocal = host === 'localhost' || host === '127.0.0.1' || host === '';

export const environment = {
  production: false,
  apiUrl: esLocal ? 'https://localhost:7100/api' : `http://${host}:5100/api`,
  sentinelUrl: esLocal ? 'http://localhost:5002' : `http://${host}:5002`
};
