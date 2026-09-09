# Motos

Aplicación de Motos: API .NET 8 (`ApiMotos`) + sitio Angular (`SiteMotos`), con deploy
en contenedores (GHCR + runner self-hosted) al estilo del repo `regula`.

## Estructura

```
motos/
├── src/ApiMotos/     # Backend .NET 8 (+ Dockerfile). Provisiona la BD al arrancar (DbBootstrap).
├── web/              # Frontend Angular (+ Dockerfile + nginx.conf).
├── docker-compose.yml        # Producción: imágenes de ghcr.io/mceschiuy/motos-*.
├── docker-compose.local.yml  # Local: build desde el código.
└── .github/workflows/deploy.yml  # CI: build+push a GHCR y deploy en runner [self-hosted, linux, lab].
```

## Desarrollo local

### Sin docker (como siempre)
Correr la API y el sitio directamente:

```bash
# Backend
cd src/ApiMotos && dotnet run

# Frontend (otra terminal)
cd web && npm install && npm start   # http://localhost:4210
```

Al arrancar en Development, `DbBootstrap` crea la base, aplica FKs, configuración y el
**seed de demo** (`src/ApiMotos/Scripts/Seed_Dominio_Motos.sql`: catálogo, stock, clientes y
envíos). Para vaciar el dominio y volver a sembrar:

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -d Motos -C -i src/ApiMotos/Scripts/Limpiar_Datos_Dominio.sql
```

### Con docker (stack completo)
Requiere un `.env` (copiar de `.env.example` y cambiar la contraseña):

```bash
cp .env.example .env      # editar DB_PASSWORD
./iniciar-motos.sh        # Front: http://localhost:8092
./detener-motos.sh
```

## Puertos

| Servicio | Host | Interno | Nota |
|----------|------|---------|------|
| frontend | 8092 | 80      | no choca con inac (8090) ni regula (8091) |
| api      | 5309 | 8080    | solo diagnóstico; el SPA va por `/api` del nginx |
| db       | —    | 1433    | sin publicar (solo red interna) |

## Deploy

Push a `main` dispara el workflow:
1. **build** (nube GitHub): construye y pushea `motos-backend` y `motos-frontend` a GHCR.
2. **deploy** (runner self-hosted): `docker compose -p motos pull && up -d`, health check.

Secret requerido en el repo de GitHub: **`DB_PASSWORD`**.
Subdominio sugerido: `motos.ceskia.tech → host:8092` (Cloudflare Zero Trust).
