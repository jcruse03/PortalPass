# AGENTS.md

## Project

PortalPass is a client-only Valheim 1.0 BepInEx/Harmony mod. It supplies passwords
from a user-owned file during Valheim's normal handshake, including joins initiated
by Cross Server Portals Continued.

## Routine commands

- `mise run test` — pure endpoint/parser tests
- `mise run build` — release build against `.game/`
- `mise run package` — Thunderstore-ready ZIP
- `mise run check` — tests plus package build

Never log or commit real passwords. Keep the secret file outside r2modman profiles.
Exact `host:port` rules must take precedence over host-only rules. Do not add DNS
resolution: hostname and IP rules are intentionally distinct. This project never
deploys to a client or server as part of build/package tasks.
