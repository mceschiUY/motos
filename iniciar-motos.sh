#!/bin/bash
set -e

cd "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [ ! -f .env ]; then
  echo "❌ Falta el archivo .env (contraseña de SQL Server). Copiá .env.example a .env y editá la contraseña."
  exit 1
fi

echo "🚀 Levantando Motos (SQL Server + API + Front)..."
docker compose -p motos -f docker-compose.local.yml up -d --build

echo "✅ Listo — Front: http://localhost:8092"
