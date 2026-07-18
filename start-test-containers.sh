#!/usr/bin/env bash
set -euo pipefail

PASSWORD="BamTest1!"
TIMEOUT=60

# Start a container if not already running.
# Usage: ensure_container <name> <image> <host_port> <env_args...>
ensure_container() {
    local name="$1" image="$2" port="$3"
    shift 3
    local env_args=("$@")

    if podman inspect --format '{{.State.Running}}' "$name" 2>/dev/null | grep -q true; then
        echo "[$name] already running"
        return
    fi

    echo "[$name] starting..."
    podman rm -f "$name" 2>/dev/null || true

    local env_flags=()
    for e in "${env_args[@]}"; do
        env_flags+=(-e "$e")
    done

    podman run -d --name "$name" -p "$port" "${env_flags[@]}" "$image"
    wait_for_port "$name" "${port%%:*}"
    echo "[$name] ready"
}

# Wait for a TCP port to accept connections.
wait_for_port() {
    local name="$1" port="$2"
    local elapsed=0

    echo "[$name] waiting for port $port..."
    while ! (echo > /dev/tcp/localhost/"$port") 2>/dev/null; do
        sleep 2
        elapsed=$((elapsed + 2))
        if [ "$elapsed" -ge "$TIMEOUT" ]; then
            echo "[$name] timed out waiting for port $port" >&2
            exit 1
        fi
    done
}

# pgvector/pgvector is the official postgres image plus the pgvector extension,
# required by the vector column integration tests (PostgresVectorShould)
ensure_container "bam-data-test-postgres" "pgvector/pgvector:pg16" "5432:5432" \
    "POSTGRES_PASSWORD=$PASSWORD" "POSTGRES_DB=bamtest"

ensure_container "bam-data-test-mssql" "mcr.microsoft.com/mssql/server:2022-latest" "1433:1433" \
    "ACCEPT_EULA=Y" "MSSQL_SA_PASSWORD=$PASSWORD"

ensure_container "bam-data-test-mysql" "mysql:8.0" "3306:3306" \
    "MYSQL_ROOT_PASSWORD=$PASSWORD" "MYSQL_DATABASE=bamtest"

ensure_container "bam-data-test-oracle" "gvenzl/oracle-xe:21-slim" "1521:1521" \
    "ORACLE_PASSWORD=$PASSWORD"

ensure_container "bam-data-test-firebird" "jacobalberty/firebird:v4.0" "3050:3050" \
    "ISC_PASSWORD=$PASSWORD" "FIREBIRD_DATABASE=bamtest.fdb"

echo ""
echo "All test containers running."
