# Production Deployment Guide

**Version:** 1.0
**Date:** 2026-06-21

---

## 1. Security Hardening

### 1.1 Docker Socket Access — CRITICAL

**Current state:** `anemoi_centralize` runs as `root` and mounts `/var/run/docker.sock`.

**Production requirement:**
```yaml
# docker-compose.yml — Centralize service
services:
  anemoi_centralize:
    # Remove: user: root
    # Remove: volumes: - /var/run/docker.sock:/var/run/docker.sock
    # If docker.sock is needed for dev environment management, restrict to read-only:
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock:ro
```

**Alternative:** Remove Docker socket entirely in production. The dev environment management (mail/SFTP/mock API toggling) is a development feature and should not be exposed in production.

### 1.2 JWT Key Paths

**Current state:** JWT key material is no longer committed in source. Development uses a generated RSA key pair outside the repository, and local Docker overrides the paths through environment variables.

**Production requirement:**
```bash
JwtSetting__PrivateKeyPath="/secure/path/to/deployment-private.key"
JwtSetting__PublicKeyPath="/secure/path/to/deployment-public.crt"
```

The signing service and every verifier must point to the same key pair.

### 1.3 PostgreSQL Credentials

**Current state:** Development passwords in `docker-compose.yml` and `appsettings.json`.

**Production requirement:**
- Use strong, randomly generated passwords
- Inject via environment variables (already supported via `POSTGRES_PASSWORD` etc.)
- Restrict network access to application services only
- Use PostgreSQL SSL/TLS connections

### 1.4 RabbitMQ Credentials

Same as PostgreSQL — use environment variables for `MassTransitSetting__UserName` and `MassTransitSetting__Password`.

---

## 2. Docker Compose Production Override

```yaml
# docker-compose.prod.yml
version: "3.8"

services:
  anemoi_centralize:
    restart: unless-stopped
    user: "1654:1654"  # Non-root user
    # Remove: user: root
    # Remove: volumes: - /var/run/docker.sock:/var/run/docker.sock
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
    deploy:
      resources:
        limits:
          memory: 1G
          cpus: "1.0"

  anemoi_identity:
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5031/health"]
      interval: 30s
      timeout: 10s
      retries: 3
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: "0.5"

  anemoi_hr:
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
    deploy:
      resources:
        limits:
          memory: 1G
          cpus: "1.0"

  anemoi_notification:
    restart: unless-stopped
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: "0.5"

  anemoi_masterdata:
    restart: unless-stopped
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: "0.5"

  anemoi_workspace:
    restart: unless-stopped
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: "0.5"

  postgres:
    restart: unless-stopped
    deploy:
      resources:
        limits:
          memory: 2G
          cpus: "2.0"

  rabbitmq:
    restart: unless-stopped
    deploy:
      resources:
        limits:
          memory: 1G
          cpus: "1.0"

  redis:
    restart: unless-stopped
    deploy:
      resources:
        limits:
          memory: 256M
          cpus: "0.25"
```

Run with:
```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

---

## 3. Environment Variables Required

| Variable | Description | Required | Default |
|----------|-------------|----------|---------|
| `POSTGRES_PASSWORD` | PostgreSQL password | Yes | — |
| `POSTGRES_USER` | PostgreSQL user | No | `postgres` |
| `RABBITMQ_DEFAULT_USER` | RabbitMQ user | No | `guest` |
| `RABBITMQ_DEFAULT_PASS` | RabbitMQ password | No | `guest` |
| `JwtSetting__PrivateKeyPath` | JWT signing private key path | Yes | — |
| `JwtSetting__PublicKeyPath` | JWT verification public key path | Yes | — |
| `JwtSetting__Issuer` | JWT issuer | No | `anemoi-identity` |
| `JwtSetting__Audience` | JWT audience | No | `anemoi-services` |
| `MassTransitSetting__Host` | RabbitMQ host | No | `rabbitmq` |
| `MassTransitSetting__UserName` | RabbitMQ user | No | `guest` |
| `MassTransitSetting__Password` | RabbitMQ password | No | `guest` |
| `RedisSetting__Endpoints` | Redis endpoint | No | `redis:6379` |
| `RedisSetting__Password` | Redis password | No | — |
| `S3Setting__AccessKey` | S3 access key | No | — |
| `S3Setting__SecretKey` | S3 secret key | No | — |
| `S3Setting__BucketName` | S3 bucket name | No | — |
| `S3Setting__Region` | S3 region | No | — |

---

## 4. Health Check Endpoints

Each API service exposes a health check endpoint that can be used by Docker or orchestration:

| Service | Endpoint | Expected Response |
|---------|----------|-------------------|
| Centralize | `GET /health` | 200 OK |
| HR | `GET /health` | 200 OK |
| Identity | `GET /health` | 200 OK (gRPC endpoint) |

These endpoints are provided by ASP.NET Core health checks middleware. Ensure they are configured in `Program.cs`.

---

## 5. Backup & Restore

### PostgreSQL Backup

```bash
#!/bin/bash
# backup.sh
BACKUP_DIR="/backups/$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"

for DB in Hr Identity Notification MasterData Workspace; do
    docker exec postgres pg_dump -U postgres "$DB" > "$BACKUP_DIR/${DB}.sql"
    gzip "$BACKUP_DIR/${DB}.sql"
done

# Keep only 7 days of backups
find /backups -type d -mtime +7 -exec rm -rf {} \;
```

### PostgreSQL Restore

```bash
# restore.sh
gunzip -c backups/20260101_120000/Hr.sql.gz | docker exec -i postgres psql -U postgres Hr
```

### RabbitMQ Queue Durability

Ensure queues are configured as `durable` in MassTransit configuration (already the default for most setup). No additional backup needed for transient messages.

---

## 6. Monitoring

### Recommended Metrics

| Metric | Source | Alert Threshold |
|--------|--------|-----------------|
| CPU usage per service | Docker stats | > 80% for 5 min |
| Memory usage per service | Docker stats | > 90% of limit |
| PostgreSQL connections | `pg_stat_activity` | > 100 |
| RabbitMQ queue depth | RabbitMQ management API | > 1000 |
| Notification queue lag | RabbitMQ management API | > 100 |
| 5xx response rate | Centralize logs | > 1% for 5 min |

### Logging

Serilog is configured with JSON formatting. Collect logs via:
- Docker `docker logs` for development
- ELK stack or Grafana Loki for production
- Configure `serilogConfiguration.json` for log shipping

---

## 7. Database Migration Strategy

EF Core migrations run automatically on service startup via `app.MigrateDatabaseAsync()`. This is safe for most schema changes but can cause issues with:

- **Breaking changes**: Renaming columns, changing types
- **Long-running migrations**: Large tables during peak hours

**Recommendation:** Set `ASPNETCORE_ENVIRONMENT=Staging` first, verify migrations, then switch to `Production`.

---

## 8. Seed Data

Seed data runs only in `Development` environment:
- `HrDevSeedData.SeedAsync()` — HR dev data (6 employees, departments, positions)
- `SeedData.RegisterAdministratorAsync()` — Admin user
- `SeedData.RegisterDevTestUsersAsync()` — Dev test users

**Production requires:**
1. Administrator registration via `POST /api/identity/Identity/register` or external identity provider
2. Employee data import via standard HR APIs
3. No seed data runs automatically in Production

---

## 9. Network Security

### Current Architecture
- All services on a single Docker network (`anemoi_network`)
- Caddy acts as reverse proxy (port 443)
- Centralize is the only externally exposed service

### Production Recommendations
1. Use separate networks for data tier (postgres, redis, rabbitmq) vs application tier
2. Use Caddy/WAF in front of Centralize for rate limiting, IP filtering
3. Disable CORS in production unless API needs to be called from web clients
4. Use HTTPS with valid certificates via Let's Encrypt (Caddy auto-provisioning)
5. Restrict SSH access to production hosts

---

## 10. Frontend Deployment

The Next.js application in `cody-web-app/` is not containerized. Deploy via:
```bash
cd cody-web-app
npm run build
npm start  # or deploy to Vercel/Netlify
```

Or build a Docker image for the frontend:
```dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY cody-web-app/ .
RUN npm ci && npm run build

FROM node:20-alpine AS run
WORKDIR /app
COPY --from=build /app/.next ./.next
COPY --from=build /app/public ./public
COPY --from=build /app/package.json .
ENV NODE_ENV=production
EXPOSE 3000
CMD ["npm", "start"]
```
