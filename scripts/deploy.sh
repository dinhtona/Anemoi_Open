#!/usr/bin/env bash
# ==============================================================================
# Anemoi_Open — Production Deployment Script
# ==============================================================================
# Usage:
#   bash scripts/deploy.sh                          # deploy latest tag
#   bash scripts/deploy.sh --tag v1.2.3             # deploy specific version
#   bash scripts/deploy.sh --tag latest --no-pull   # skip image pull (offline)
#   bash scripts/deploy.sh --health-only            # check container health only
#   bash scripts/deploy.sh --clean                  # prune old images after deploy
#
# Prerequisites on the target Ubuntu VM:
#   - Docker Engine 24+ with compose plugin
#   - openssl (for JWT key generation)
#   - Enough disk space for pulled images (~5 GB)
# ==============================================================================

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(realpath "$SCRIPT_DIR/..")"
cd "$PROJECT_ROOT"

# ── Defaults ─────────────────────────────────────────────────────────────────

ENV_FILE=".env.production"
COMPOSE_FILE="docker-compose.prod.yml"
CLI_IMAGE_TAG=""
DO_PULL=true
DO_HEALTH_ONLY=false
DO_CLEAN=false
HEALTH_TIMEOUT=180

# ── Color Output ─────────────────────────────────────────────────────────────

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log()  { echo -e "${GREEN}[INFO]${NC}  $(date +'%H:%M:%S') $*"; }
warn() { echo -e "${YELLOW}[WARN]${NC}  $(date +'%H:%M:%S') $*"; }
err()  { echo -e "${RED}[ERROR]${NC} $(date +'%H:%M:%S') $*"; }
step() { echo -e "\n${BLUE}═══ $* ═══${NC}"; }

# ── Argument Parsing ─────────────────────────────────────────────────────────

while [[ $# -gt 0 ]]; do
    case "$1" in
        --tag)
            CLI_IMAGE_TAG="$2"
            shift 2
            ;;
        --no-pull)
            DO_PULL=false
            shift
            ;;
        --health-only)
            DO_HEALTH_ONLY=true
            shift
            ;;
        --clean)
            DO_CLEAN=true
            shift
            ;;
        --timeout)
            HEALTH_TIMEOUT="$2"
            shift 2
            ;;
        --env-file)
            ENV_FILE="$2"
            shift 2
            ;;
        --help|-h)
            echo "Usage: bash scripts/deploy.sh [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --tag TAG        Image tag to deploy (default: latest)"
            echo "  --no-pull        Skip pulling images (use cached)"
            echo "  --health-only    Only check container health, skip deploy"
            echo "  --clean          Prune old images after successful deploy"
            echo "  --timeout SEC    Health check timeout in seconds (default: 180)"
            echo "  --env-file FILE  Path to production env file (default: .env.production)"
            echo "  --help           Show this help"
            exit 0
            ;;
        *)
            err "Unknown option: $1"
            exit 1
            ;;
    esac
done

# ── Pre-flight Checks ────────────────────────────────────────────────────────

step "Pre-flight Validation"

if ! command -v docker &>/dev/null; then
    err "Docker is not installed. Install: https://docs.docker.com/engine/install/ubuntu/"
    exit 1
fi

if ! docker compose version &>/dev/null; then
    err "Docker Compose plugin is not available. Install: sudo apt-get install docker-compose-plugin"
    exit 1
fi

if ! command -v openssl &>/dev/null; then
    err "OpenSSL is not installed. Install: sudo apt-get install openssl"
    exit 1
fi

if [[ ! -f "$COMPOSE_FILE" ]]; then
    err "Compose file not found: $COMPOSE_FILE"
    exit 1
fi

if [[ ! -f "$ENV_FILE" ]]; then
    err "Missing $ENV_FILE"
    log "Create it from template: cp .env.production.example $ENV_FILE"
    log "Then edit $ENV_FILE with real values before deploying."
    exit 1
fi

# Source env vars
set -a
source "$ENV_FILE"
set +a

# CLI --tag always wins over env IMAGE_TAG
IMAGE_TAG="${CLI_IMAGE_TAG:-${IMAGE_TAG:-latest}}"
export IMAGE_TAG

# Validate required vars (not in health-only mode)
if ! $DO_HEALTH_ONLY; then
    if [[ "${APP_DOMAIN:-}" == "hr.example.com" || -z "${APP_DOMAIN:-}" ]]; then
        err "APP_DOMAIN is not set or still has placeholder value 'hr.example.com'"
        log "Edit $ENV_FILE with your real domain."
        exit 1
    fi

    if echo "${POSTGRES_PASSWORD}${RABBITMQ_DEFAULT_PASS}${REDIS_PASSWORD}" | grep -q "CHANGE_ME"; then
        err "One or more passwords still contain placeholder 'CHANGE_ME'"
        log "Edit $ENV_FILE with strong, unique passwords."
        exit 1
    fi

    CADDYFILE="${CADDYFILE_HOST_PATH:-./deploy/Caddyfile}"
    if [[ ! -f "$CADDYFILE" ]]; then
        err "Caddyfile not found at: $CADDYFILE"
        exit 1
    fi
fi

# Validate compose syntax (no secrets in output)
COMPOSE_OPTS=(-f "$COMPOSE_FILE" --env-file "$ENV_FILE")
if ! docker compose "${COMPOSE_OPTS[@]}" config --quiet 2>&1; then
    err "Compose configuration is invalid. Run:"
    err "  docker compose -f $COMPOSE_FILE --env-file $ENV_FILE config"
    # re-run without --quiet to show the error
    docker compose "${COMPOSE_OPTS[@]}" config >/dev/null
    exit 1
fi

log "Docker version: $(docker --version)"
log "Compose file:   $COMPOSE_FILE"
log "Env file:       $ENV_FILE"
log "Image tag:      $IMAGE_TAG"
log "Compose config: valid"

# ── Health-only Mode ─────────────────────────────────────────────────────────

if $DO_HEALTH_ONLY; then
    step "Health Check"

    RUNNING=$(docker compose "${COMPOSE_OPTS[@]}" ps --format json 2>/dev/null || true)
    if [[ -z "$RUNNING" ]]; then
        err "No containers running. Deploy first."
        exit 1
    fi

    docker compose "${COMPOSE_OPTS[@]}" ps --format "table {{.Name}}\t{{.Image}}\t{{.Status}}"

    # Check if any required service is unhealthy
    UNHEALTHY=$(docker compose "${COMPOSE_OPTS[@]}" ps --format json 2>/dev/null | \
        python3 -c "import sys,json; [print(l.get('Name','')) for l in [json.loads(l) for l in sys.stdin] if 'unhealthy' in l.get('Status','').lower()]" 2>/dev/null || true)

    if [[ -n "$UNHEALTHY" ]]; then
        err "Unhealthy services found. Exiting with non-zero."
        exit 1
    fi

    log "All services healthy."
    exit 0
fi

# ── Directory & Key Setup ────────────────────────────────────────────────────

step "Ensuring Directory Structure"

mkdir -p /opt/anemoi/uploads
mkdir -p "${JWT_KEYS_HOST_PATH:-/opt/anemoi/jwt}"

# ── JWT Key Generation ──────────────────────────────────────────────────────

JWT_KEYS_DIR="${JWT_KEYS_HOST_PATH:-/opt/anemoi/jwt}"
JWT_PRIV="${JWT_KEYS_DIR}/production-private.key"
JWT_PUB="${JWT_KEYS_DIR}/production-public.key"

if [[ ! -f "$JWT_PRIV" || ! -f "$JWT_PUB" ]]; then
    log "Generating JWT key pair (RSA 4096)…"
    mkdir -p "$JWT_KEYS_DIR"

    openssl genpkey \
        -algorithm RSA \
        -out "$JWT_PRIV" \
        -pkeyopt rsa_keygen_bits:4096 2>/dev/null

    openssl pkey \
        -in "$JWT_PRIV" \
        -pubout \
        -out "$JWT_PUB" 2>/dev/null

    chmod 600 "$JWT_PRIV"
    chmod 644 "$JWT_PUB"

    log "JWT keys created: $JWT_PRIV, $JWT_PUB"
else
    log "JWT keys already exist at $JWT_KEYS_DIR"
fi

# ── Pull Images ──────────────────────────────────────────────────────────────

step "Pulling Images (tag: $IMAGE_TAG)"

INFRA_IMAGES=(
    "masstransit/rabbitmq:3.13"
    "postgres:16-alpine"
    "redis:7-alpine"
    "caddy:2-alpine"
)

if $DO_PULL; then
    for img in "${INFRA_IMAGES[@]}"; do
        log "Pulling $img …"
        docker pull "$img" || {
            err "Failed to pull $img — aborting."
            exit 1
        }
    done
else
    warn "Skipping infrastructure image pull (--no-pull)"
fi

# Application images from GHCR
if $DO_PULL; then
    log "Pulling application images …"
    docker compose "${COMPOSE_OPTS[@]}" pull || {
        err "Failed to pull application images — aborting."
        log "Check GHCR credentials and image tag: $IMAGE_TAG"
        exit 1
    }
    log "All images pulled successfully."
else
    warn "Skipping image pull (--no-pull)"
fi

# ── Start / Update Services ──────────────────────────────────────────────────

step "Starting Services"

# Use up -d directly; only recreate services with changed image/config.
# This avoids unnecessary restart of PostgreSQL, Redis, RabbitMQ.
docker compose "${COMPOSE_OPTS[@]}" up -d \
    --remove-orphans \
    --wait \
    --wait-timeout "$HEALTH_TIMEOUT" || {
    err "Some services failed to become healthy within ${HEALTH_TIMEOUT}s"
    log "Check individual container status with:"
    log "  docker compose -f $COMPOSE_FILE --env-file $ENV_FILE ps"
    log "  docker compose -f $COMPOSE_FILE --env-file $ENV_FILE logs <service>"
    exit 1
}

log "All containers started and healthy."

# ── Post-deploy Summary ──────────────────────────────────────────────────────

step "Deployment Summary"

echo ""
docker compose "${COMPOSE_OPTS[@]}" ps --format "table {{.Name}}\t{{.Image}}\t{{.Status}}"
echo ""

log "Application URL: https://${APP_DOMAIN}"
log "All traffic enters through Caddy (auto-HTTPS). No internal ports are exposed."

# ── Clean Old Images (optional) ──────────────────────────────────────────────

if $DO_CLEAN; then
    step "Pruning Old Images"
    docker image prune -a --force --filter "until=24h" 2>/dev/null || true
    log "Old images pruned."
fi

step "Deployment Complete"
