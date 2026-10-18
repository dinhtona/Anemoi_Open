# Anemoi.Orchestration — Placeholder Module

This directory is currently a **placeholder** and contains no source code
(`Anemoi.Orchestration.Machine/` is an empty scaffold).

## Status

- ❌ Not part of `Anemoi.sln` — nothing here is compiled.
- ❌ Not built or deployed. The `anemoi_orchestrator` service in `docker-compose.yml`
  is commented out (build context `./Anemoi.Orchestrator`, `dockerfile: Dockerfile`).
- Per the production-baseline documentation, the Orchestrator service is one of the
  modules intentionally disabled in the local Docker stack.

## Purpose (intended)

Saga orchestration service for long-running, cross-module workflows
(MassTransit + RabbitMQ). Refer to `docs/architecture/TECHNICAL_DEBT_REGISTER.md`
and ADR documents for the orchestration roadmap.

## Do not delete this folder

Kept intentionally to avoid breaking tooling/scripts that reference the path
(e.g. the commented compose service). Remove it together with the compose
references when the module is either implemented or formally retired.
