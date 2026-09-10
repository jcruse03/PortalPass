# Changelog

## 0.1.1 - 2026-09-10

- Preserve the original dedicated host and port when Valheim resolves a direct
  connection to a PlayFab backend before the password handshake.
- Add safe diagnostics for unavailable and unmatched endpoints without logging
  passwords.

## 0.1.0 - 2026-09-10

- Initial client-only Valheim 1.0 release.
- Add exact endpoint and host-wide password matching with deterministic precedence.
- Add IPv4, hostname, and bracketed/bare IPv6 support without DNS guessing.
- Keep secrets outside exported r2modman profiles and fall back to vanilla on missing or invalid files.
