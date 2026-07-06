# Backup, Restore, and Migration Rollback

Release 1 operations guide for the Anemoi HR Platform.

## 1. Databases

Each service maintains its own PostgreSQL database within a single PostgreSQL container:

| Service      | Database Name       |
|-------------|--------------------|
| Identity     | Identity           |
| Hr           | Hr                 |
| MasterData   | MasterData         |
| Notification | Notification       |
| Workspace    | Workspace          |

MongoDB is used for notification persistence and structured logs (`Notification` collection).

RabbitMQ holds transient messages only; it is not a persistent data store.

Redis is used as a cache and SignalR backplane; it is not a persistent data store.

## 2. Backup

### PostgreSQL Full Backup (All Databases)

Run from host while the postgres container is running:

```bash
docker exec postgres pg_dumpall -U postgres > anemoi_backup_$(date +%Y%m%d_%H%M%S).sql
```

### Single Database Backup

```bash
docker exec postgres pg_dump -U postgres -d Hr > anemoi_hr_backup_$(date +%Y%m%d_%H%M%S).sql
```

### MongoDB Backup

```bash
docker exec mongodb mongodump --username $MONGO_INITDB_ROOT_USERNAME --password $MONGO_INITDB_ROOT_PASSWORD --authenticationDatabase admin --out /data/backup
docker cp mongodb:/data/backup ./mongo_backup_$(date +%Y%m%d_%H%M%S)
```

### Recommended Backup Schedule

| Frequency | What                                  |
|-----------|--------------------------------------|
| Daily     | Full pg_dumpall (all PostgreSQL DBs) |
| Hourly    | WAL archiving (if enabled)           |
| Pre-deploy| Full backup before migration         |

### Pre-Deploy Backup Checklist

- [ ] Full `pg_dumpall` completed successfully
- [ ] MongoDB backup completed
- [ ] Backup file is accessible outside container
- [ ] Backup size is consistent with expectations
- [ ] Backup checksum verified (`sha256sum`)

## 3. Restore

### PostgreSQL Restore

```bash
# Drop and recreate databases (optional — adds safety)
docker exec postgres psql -U postgres -c "DROP DATABASE IF EXISTS Hr;"
docker exec postgres psql -U postgres -c "CREATE DATABASE Hr OWNER postgres;"

# Restore from dump
cat anemoi_backup_YYYYMMDD_HHMMSS.sql | docker exec -i postgres psql -U postgres
```

### Single Database Restore

```bash
cat anemoi_hr_backup_YYYYMMDD_HHMMSS.sql | docker exec -i postgres psql -U postgres -d Hr
```

### MongoDB Restore

```bash
docker cp ./mongo_backup_YYYYMMDD_HHMMSS mongodb:/data/restore
docker exec mongodb mongorestore --username $MONGO_INITDB_ROOT_USERNAME --password $MONGO_INITDB_ROOT_PASSWORD --authenticationDatabase admin /data/restore
```

### Post-Restore Verification Checklist

- [ ] All expected databases exist and contain tables
- [ ] Row counts match backup expectations
- [ ] All services restart successfully
- [ ] Key API endpoints respond correctly
- [ ] Workflow definitions are present (7 expected)
- [ ] Identity users and permissions are intact

## 4. Database Migrations

Migrations are managed by EF Core and run automatically at service startup via `MigrationDatabaseAsync<T>()` in each service's `Program.cs`.

### Migration Apply (Automatic)

Migrations apply automatically on container start. No manual action required.

Trigger: `docker compose up -d --build anemoi_centralize` (or any service container)

### Migration Rollback

EF Core migrations are forward-only by design. To rollback:

```bash
# List applied migrations
dotnet ef migrations list --project Anemoi.Hr/Anemoi.Hr.Infrastructure --startup-project Anemoi.Hr/Anemoi.Hr.Api

# Rollback to a specific migration
dotnet ef database update <TargetMigrationName> \
  --project Anemoi.Hr/Anemoi.Hr.Infrastructure \
  --startup-project Anemoi.Hr/Anemoi.Hr.Api
```

**Important:** Rollback may cause data loss for columns/tables created by the removed migrations. Restore from backup if necessary.

### Creating New Migrations

```bash
dotnet ef migrations add <MigrationName> \
  --project Anemoi.Hr/Anemoi.Hr.Infrastructure \
  --startup-project Anemoi.Hr/Anemoi.Hr.Api
```

## 5. Authoritative Data

The following data must be backed up before any restore/deploy operation:

| Data                   | Source      | Priority    |
|------------------------|-------------|-------------|
| Identity users/roles   | PostgreSQL  | Critical    |
| Permissions catalog    | PostgreSQL  | Critical    |
| Employee records       | PostgreSQL  | Critical    |
| Payroll runs/snapshot  | PostgreSQL  | Critical    |
| Leave requests/balances| PostgreSQL  | Critical    |
| Attendance records     | PostgreSQL  | High        |
| Workflow instances     | PostgreSQL  | High        |
| Notification records   | PostgreSQL + MongoDB | Medium |
| Seed data ID constants | Source code | Always preserved in repo |

## 6. Data Recoverability Notes

- **RabbitMQ:** Transient messages only. No backup needed. Messages are reprocessed by MassTransit after restart.
- **Redis:** Cache only. No backup needed. Data rebuilds on service restart.
- **SFTP (dev):** Local file data under `./local_env/sftp_data/`. Not production.
- **JWT keys:** Stored in Docker volume `jwt_keys`. Backup the volume or the key files.

## 7. Emergency Recovery Flow

1. Stop all application containers: `docker compose stop anemoi_centralize anemoi_identity anemoi_hr ...`
2. Restore PostgreSQL from latest backup (see Section 3)
3. Restore MongoDB from latest backup
4. Restart all containers: `docker compose up -d`
5. Run post-restore verification checklist
6. Monitor logs for errors
