# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| v2.0.x (master) | In development - not yet released |
| v1.x (v1-legacy) | No longer maintained |

## Reporting a Vulnerability

If you discover a security vulnerability in PRoCon, please report it responsibly.

Please [open a GitHub issue](https://github.com/AdKats/Procon-1/issues/new) with the label `security`. If the vulnerability is sensitive, use [GitHub's private vulnerability reporting](https://github.com/AdKats/Procon-1/security/advisories/new).

Include:
- A description of the vulnerability
- Steps to reproduce
- Affected versions
- Any potential impact

## Scope

The following are in scope for security reports:

- RCON credential handling and storage
- Config file encryption
- Layer system authentication (JWT, SignalR)
- Plugin sandboxing and execution
- Network protocol implementation
- IP check service data handling

## Out of Scope

- Vulnerabilities in the Frostbite RCON protocol itself (report to EA/DICE)
- Vulnerabilities in game servers
- Social engineering attacks
- Denial of service against game servers
