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

Quit the browser completely with `Cmd+Q`, reopen it, then visit
`https://localhost:3000`. Reloading an existing Chromium tab is sometimes not
enough because the browser can retain the previous TLS error state.

## Stable Public URL

Create one Cloudflare Tunnel and configure its published application:

```text
Public hostname: app.example.com
Service:         http://caddy:80
```

Set its token in `.env`:

```dotenv
CLOUDFLARE_TUNNEL_TOKEN=your-token
PUBLIC_APP_ORIGIN=https://app.example.com
NEXT_PUBLIC_GOOGLE_CLIENT_ID=your-client-id.apps.googleusercontent.com
```

Start the optional tunnel profile:

```bash
docker compose --profile public up -d cloudflared
```

Cloudflare terminates public HTTPS at its edge. The browser still uses one
origin for the frontend, API, and SignalR hub, and no inbound router ports are
required.

Compose binds published development ports to `127.0.0.1` by default. The
Cloudflare connector still reaches Caddy over the internal Docker network. Set
`BIND_ADDRESS=0.0.0.0` in `.env` only when devices on the local network must
connect directly to development ports such as SFTP.

The smtp4dev web interface is available through the same entrypoint:

```text
https://app.example.com/mail/
```

Protect `/mail/*` with a Cloudflare Access policy before sharing the public URL.

Register the public origin in the Google OAuth web client:

```text
https://app.example.com
```

The same Google client ID must also be present in `cody-web-app/.env.local`
when building the frontend. The Identity service uses the root `.env` value to
verify the Google token audience.

SFTP remains a local-only developer service by default:

```bash
sftp -P 2222 user1@localhost
```

Publishing SFTP through Cloudflare requires a separate TCP Access setup on
each client machine. It is intentionally not part of the default Compose flow.
