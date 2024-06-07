#!/bin/bash
set -e

# Variables
DB_NAME=${POSTGRES_DB:-docprojdevplant}
DB_USER=${POSTGRES_USER:-postgres}
DB_PASSWORD=${POSTGRES_PASSWORD:-postgres}
DUMP_FILE=./testdb.tar

# Wait for PostgreSQL to start
until pg_isready -h localhost; do
  echo "Waiting for PostgreSQL to start..."
  sleep 1
done

# Restore the database
echo "Restoring database $DB_NAME from $DUMP_FILE"
pg_restore -U "$DB_USER" -d "$DB_NAME" "$DUMP_FILE"
