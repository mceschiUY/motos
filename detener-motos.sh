#!/bin/bash
set -e

cd "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "🛑 Deteniendo Motos (los datos quedan: volumen de SQL Server)..."
docker compose -p motos -f docker-compose.local.yml down

echo "✅ Detenido."
