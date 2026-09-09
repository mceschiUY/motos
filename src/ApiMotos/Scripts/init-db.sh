#!/bin/bash

# Script de inicialización de base de datos para Docker
# Ejecuta Core_Schema.sql y todos los scripts de entidades PC_*.sql

set -e

DB_SERVER="db"
DB_NAME="AppDB"
DB_USER="sa"
DB_PASS="${SA_PASSWORD:-YourStrong!Passw0rd}"
SCRIPTS_DIR="/scripts"

# Detectar ruta de sqlcmd
if [ -f "/opt/mssql-tools18/bin/sqlcmd" ]; then
    SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
elif [ -f "/opt/mssql-tools/bin/sqlcmd" ]; then
    SQLCMD="/opt/mssql-tools/bin/sqlcmd"
else
    echo "ERROR: sqlcmd no encontrado"
    exit 1
fi

echo "=========================================="
echo "Inicializando base de datos..."
echo "Usando sqlcmd: $SQLCMD"
echo "=========================================="

# Función para ejecutar SQL
run_sql() {
    local sql="$1"
    $SQLCMD -S "$DB_SERVER" -U "$DB_USER" -P "$DB_PASS" -C -Q "$sql"
}

# Función para ejecutar archivo SQL
run_sql_file() {
    local file="$1"
    echo "Ejecutando: $(basename "$file")"
    $SQLCMD -S "$DB_SERVER" -U "$DB_USER" -P "$DB_PASS" -C -d "$DB_NAME" -i "$file"
}

# 1. Crear base de datos si no existe
echo "Verificando base de datos ${DB_NAME}..."
run_sql "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '${DB_NAME}') BEGIN CREATE DATABASE [${DB_NAME}]; PRINT 'Base de datos ${DB_NAME} creada'; END ELSE BEGIN PRINT 'Base de datos ${DB_NAME} ya existe'; END"

echo ""

# 2. Ejecutar Core_Schema.sql primero (tablas base del sistema)
if [ -f "${SCRIPTS_DIR}/Core_Schema.sql" ]; then
    echo "=========================================="
    echo "Ejecutando Core Schema..."
    echo "=========================================="
    run_sql_file "${SCRIPTS_DIR}/Core_Schema.sql"
    echo "Core Schema completado."
    echo ""
fi

# 3. Ejecutar todos los scripts de entidades (PC_*.sql)
echo "=========================================="
echo "Ejecutando scripts de entidades..."
echo "=========================================="

ENTITY_SCRIPTS=$(find "${SCRIPTS_DIR}" -maxdepth 1 -name "PC_*.sql" -type f 2>/dev/null | sort)

if [ -n "$ENTITY_SCRIPTS" ]; then
    for script in $ENTITY_SCRIPTS; do
        run_sql_file "$script"
    done
    echo "Scripts de entidades completados."
else
    echo "No se encontraron scripts de entidades (PC_*.sql)"
fi

echo ""

# 4. Foreign keys entre agregados (FK_*.sql): recién cuando existen todas las tablas.
#    En producción DbBootstrap NO corre (solo Development), así que se aplican acá.
echo "=========================================="
echo "Ejecutando foreign keys..."
echo "=========================================="
for script in $(find "${SCRIPTS_DIR}" -maxdepth 1 -name "FK_*.sql" -type f 2>/dev/null | sort); do
    run_sql_file "$script"
done

# 5. Configuración del sitio (Cfg_*.sql), idempotente.
echo "=========================================="
echo "Ejecutando configuracion del sitio..."
echo "=========================================="
for script in $(find "${SCRIPTS_DIR}" -maxdepth 1 -name "Cfg_*.sql" -type f 2>/dev/null | sort); do
    run_sql_file "$script"
done

# 6. Seed de dominio (Seed_*.sql): maestras + datos de demostración. Idempotente,
#    nunca borra. Para vaciar: Limpiar_Datos_Dominio.sql (a mano, a pedido).
echo "=========================================="
echo "Ejecutando seed de dominio..."
echo "=========================================="
for script in $(find "${SCRIPTS_DIR}" -maxdepth 1 -name "Seed_*.sql" -type f 2>/dev/null | sort); do
    run_sql_file "$script"
done

echo ""
echo "=========================================="
echo "Inicializacion completada exitosamente!"
echo "=========================================="
