# Local HTTPS and Cloudflare Tunnel

## Local HTTPS

Start the stack:

```bash
cd cody-web-app
./scripts/build-docker-runtime.sh
cd ..
docker compose up -d --build
```

Open `https://localhost` or `https://localhost:3000`. Both URLs enter through
Caddy. The port `3000` alias is available for OAuth test callbacks that were
already registered with that port. Caddy routes `/api/*` and `/hubs/*` to
`anemoi_centralize`; all other requests go to `anemoi_web`.

The frontend is built on the host before Docker packages its standalone
runtime. This avoids running the memory-intensive Next.js build inside Docker.
The script builds the container artifact with
`NEXT_PUBLIC_API_BASE_URL=http://anemoi_centralize:8080` by default so
server-side requests stay inside the Compose network. When frontend source
files change, rerun `./scripts/build-docker-runtime.sh`.

Caddy generates a local root CA inside the `caddy_data` Docker volume. On
macOS, trust that root CA once to remove the browser warning:

```bash
mkdir -p local_env
docker compose cp caddy:/data/caddy/pki/authorities/local/root.crt local_env/caddy-root.crt
security add-trusted-cert -d -r trustRoot \
  -k "$HOME/Library/Keychains/login.keychain-db" \
  local_env/caddy-root.crt
```

## Stable Public URL

Create one Cloudflare Tunnel and configure its published application:

```text
Public hostname: app.example.com
Service:         http://caddy:80
```

Set its token in `.env`:

```dotenv
CLOUDFLARE_TUNNEL_TOKEN=your-token
```

Start the optional tunnel profile:

```bash
docker compose --profile public up -d cloudflared
```

Cloudflare terminates public HTTPS at its edge. The browser still uses one
origin for the frontend, API, and SignalR hub, and no inbound router ports are
required.
