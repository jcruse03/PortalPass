# PortalPass

PortalPass is a client-only Valheim 1.0 mod that automatically supplies server
passwords from a private local file. It works at Valheim's normal password
handshake, so direct joins and connections initiated by Cross Server Portals use
the same rules.

## Why

Repeated password entry is especially awkward on Steam Deck and during
cross-server portal travel. PortalPass makes those joins seamless without putting
credentials in a shared r2modman profile.

## Requirements

- Valheim 1.0
- BepInExPack Valheim 5.4.2350 or newer compatible release
- PortalPass installed on the client only

## Install

Install the release ZIP as a local mod in r2modman, or copy `PortalPass.dll` into
the client's `BepInEx/plugins/PortalPass/` directory. Start the game once to create
`com.jcruse03.portalpass.cfg`.

## Secret file

The default path is deliberately outside r2modman profiles:

- Linux / Steam Deck: `~/.config/portalpass/passwords.env`
- Windows: `%LOCALAPPDATA%\PortalPass\passwords.env`

Create the directory and file yourself. On Linux, protect it with:

```bash
chmod 600 ~/.config/portalpass/passwords.env
```

Format:

```dotenv
# Host-wide default
valheim.example.net = common-password

# Exact endpoint wins over the host-wide default
valheim.example.net:2471 = test-server-password

# IP addresses work the same way
203.0.113.20 = another-password
203.0.113.20:2459 = specific-password

# IPv6 exact endpoints use brackets
2001:db8::1 = ipv6-default
[2001:db8::1]:2459 = ipv6-specific
```

Passwords may be unquoted or wrapped in matching single or double quotes. The
first `=` separates the endpoint, so `=` is allowed inside a password.

To use another location, edit `SecretFilePath` in
`BepInEx/config/com.jcruse03.portalpass.cfg`. A `~/...` path is supported.

## Matching and safety

1. Exact normalized `host:port`
2. Normalized host/IP without a port
3. No match: the normal Valheim password dialog remains active

Hostnames are case-insensitive and ignore a trailing dot. IP addresses use their
canonical textual form. PortalPass does not resolve DNS, so a hostname and its IP
address are separate rules. Duplicate normalized endpoints or any malformed line
invalidate the file for that attempt; PortalPass then leaves the vanilla prompt
alone. Passwords are never logged or learned from manual entry.

The file is read fresh at every password handshake, so edits do not require a game
restart.

Valheim may resolve a dedicated IP or hostname to a PlayFab backend before the
handshake. PortalPass preserves the original dedicated endpoint for matching, so
the file continues to use the human-readable host or IP from the join request or
cross-server portal tag.

## Development

The local `.game` path must point at a Valheim installation with BepInEx. Then:

```bash
mise trust
mise run check
```

This builds against the local Valheim 1.0 assemblies, runs pure parser tests, and
creates `artifacts/PortalPass-v0.1.1.zip`.

## Status

Parser and build verification are automated. Treat the first live client join and
cross-server portal traversal as a canary before broad profile distribution.
