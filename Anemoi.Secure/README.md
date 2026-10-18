# Anemoi.Secure — Placeholder Module

This directory is currently a **placeholder** and contains no source code.

## Status

- ❌ Not part of `Anemoi.sln` — nothing here is compiled.
- ❌ Not built or deployed. The `anemoi_secure` service in `docker-compose.yml`
  is commented out (build context `./Anemoi.Secure`, `dockerfile: Dockerfile`).
- The dead `Certifications` config section pointing at this module was flagged for
  removal (see audit finding LOW-2 / SEC-013).

## Purpose (intended)

Security & encryption service (certificates / key handling). Refer to
`docs/architecture/TECHNICAL_DEBT_REGISTER.md` and `ArchitectureGuide.md` for
context on the security service roadmap.

## Do not delete this folder

Kept intentionally to avoid breaking tooling/scripts that reference the path
(e.g. the commented compose service). Remove it together with the compose
references when the module is either implemented or formally retired.
