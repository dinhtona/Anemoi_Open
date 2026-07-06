# Observability Baseline

Release 1 observability setup for the Anemoi HR Platform.

## 1. Logging

### Structured Logging

All services use **Serilog** for structured JSON logging.

Configuration is loaded from `serilogConfiguration.json` and enriched with:
- Log context (`Enrich.FromLogContext()`)
- Environment name
- Application name (set via `Serilog:Properties:Application`)

Sinks configured per service (typically Console + File for dev):
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### Correlation ID

A correlation request ID is included via Serilog's log context enrichment. Each HTTP request processed through the centralize gateway receives a correlation ID propagated through the request pipeline.

### Log Locations

| Environment | Log Destination     |
|-------------|--------------------|
| Docker dev  | Console (stdout)   |
| Local host  | Console + file     |

View container logs:
```bash
docker logs anemoi_centralize -f --tail 100
docker logs anemoi_hr -f --tail 100
docker logs anemoi_identity -f --tail 100
```

### Security: No Sensitive Data in Logs

- Authorization headers are NOT logged (see Production Hardening Sprint 1, Task 2)
- Cookie headers are NOT logged
- JWT tokens are NOT included in logs
- Password fields are excluded from request logging

HTTP logging in production uses minimal fields:
```
RequestPath | RequestMethod | ResponseStatusCode | Duration
```

Verbose HTTP logging (`HttpLoggingFields.All`) is gated to Development environment only.

## 2. Tracing a Failing Request

1. **Get the correlation/request ID** from response headers or logs
2. **Search logs across services:**
   ```bash
   # Centralize gateway logs
   docker logs anemoi_centralize --since 5m | grep "<correlation_id_or_endpoint>"

   # HR service logs
   docker logs anemoi_hr --since 5m | grep "<correlation_id_or_endpoint>"
   ```
3. **Check HTTP status codes** from the centralize HTTP logging
4. **Check MassTransit failures** via RabbitMQ management UI (`http://localhost:15672`)
5. **Check database exceptions** in the target service's logs

## 3. Health Checks

### Endpoints

| Service    | Liveness         | Readiness         |
|-----------|-------------------|--------------------|
| Centralize | `/health/live`   | `/health/ready`   |
| Hr         | `/health/live`   | `/health/ready`   |

### Liveness (`/health/live`)
- Returns 200 if the service process is running
- No dependency checks
- Lightweight, suitable for frequent polling

### Readiness (`/health/ready`)
- Returns 200 only if all dependencies are available
- Centralize: checks Redis connectivity and RabbitMQ port
- Hr: checks RabbitMQ port
- Suitable for load balancer / gateway routing decisions

### Example Usage

```bash
# Centralize
curl http://localhost:8080/health/live
curl http://localhost:8080/health/ready

# Hr
curl http://localhost:8082/health/live
curl http://localhost:8082/health/ready
```

## 4. Docker-Level Monitoring

### Container Status
```bash
docker compose ps
docker stats
```

### Infrastructure Service Health

| Service   | Health Check |
|-----------|-------------|
| PostgreSQL | `pg_isready` (docker-compose healthcheck) |
| RabbitMQ   | `rabbitmq-diagnostics check_port_connectivity` (docker-compose) |
| Redis      | `redis-cli ping` (docker-compose healthcheck) |

## 5. Metrics and Monitoring (Future)

Not yet implemented for Release 1. Planned:
- OpenTelemetry tracing across services
- Prometheus metrics endpoint
- Request duration histograms
- Database query performance tracking
- MassTransit message processing metrics

## 6. Operational Troubleshooting

### Common Issues

| Symptom                         | Check                                 |
|--------------------------------|---------------------------------------|
| All APIs return 502/503       | `docker ps` — are all services running? |
| Slow API responses             | RabbitMQ management UI — message queue depth |
| Authentication failures        | Identity service logs                 |
| Workflow submission fails      | WorkflowDefinitions table — definitions present? |
| Notification not delivered     | Notification service logs + MongoDB   |
| Leave accrual not updating     | HR MonthlyLeaveAccrualWorker logs     |

### Key Diagnostic Commands

```bash
# Service status
docker compose ps

# Recent logs across all services
docker compose logs --tail 50

# Database connectivity
docker exec postgres psql -U postgres -d Hr -c "SELECT 1;"

# RabbitMQ management
open http://localhost:15672

# Redis connectivity
docker exec redis redis-cli ping
```
