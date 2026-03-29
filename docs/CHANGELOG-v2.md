# PRoCon v2.0 Changelog

PRoCon v2.0 is a complete modernization from .NET Framework 4.7/WinForms to .NET 8/Avalonia UI. Every layer of the application has been rewritten while maintaining backward compatibility with existing Frostbite game servers.

---

## Platform & Runtime

- .NET Framework 4.7 replaced by **.NET 8** — cross-platform (Windows, Linux, macOS)
- **Single-file self-contained executables** — one ~77MB file per platform, no .NET install needed
- **SDK-style project files** — modern `.csproj` format
- **Docker/Kubernetes support** — headless console mode, auto-detects container environment (`/config/` data directory)
- **Centralized data directory** — platform-aware (`%APPDATA%`, `~/.config/procon/`, `/config/`), overridable via `PROCON_DATA_DIR` or `--datadir`
- **JSON configuration** — `procon.json` replaces `procon.cfg` (legacy format still loaded for backward compat)
- **Code formatting** — `.editorconfig` + `dotnet format` pre-commit hook

## UI

- **Avalonia UI replaces WinForms** — modern cross-platform UI framework
- **Dark and Light themes** — blue + orange gaming palette, switchable at runtime
- **16 tabbed panels**: Dashboard, Chat, Players, Maps, Bans, Reserved Slots, Spectators, TextChat Moderation, PunkBuster, Settings, Plugins, Accounts, Events, Console, Layer
- **Live server dashboard** — real-time player count graph, kill feed, team scores
- **Multi-server sidebar** — servers grouped by game type with animated status indicators
- **RCON console** — command history, tab-completion with game-aware signatures, input validation
- **Player context menus** — right-click Kill, Kick, Move, Ban, Copy Name on team lists
- **Plugin Output console** — timestamped compilation/load messages with 500-line scrollback
- **Add Server dialog** — Direct RCON and PRoCon Layer connection modes

## Network & Security

- **TLS 1.3 support** — modern encryption for game server connections
- TLS downgrade fallback removed (prevented downgrade attacks)
- SslStream/NetworkStream double-dispose fixed
- **ProxyCheck.io v3 integration** — async IP reputation checking (VPN/proxy/Tor detection)
  - SQLite cache with Dapper ORM, WAL mode, 48-hour TTL
  - Memory cache layer for hot lookups
  - Rate limiting (5 concurrent) + daily query budgeting (1K free, 100K with API key)
  - Exposed to plugins via `procon.protected.ipcheck` command + `OnIPChecked` event

## Layer System

- **SignalR WebSocket replaces TCP binary protocol** — Kestrel + SignalR Hub at `/layer`
- JWT authentication replaces plaintext login
- Automatic keepalive (SignalR handles connection health)
- Command routing with sequence number tracking and 10s timeout

## HTTP Server

- **Fully removed** — 1,073 lines of code across 6 files deleted
- Replaced by SignalR layer system
- `OnHttpRequest` removed from plugin API
- Old config lines silently ignored

## Auto-Updater

- **Fully removed** — was downloading v1 files from defunct Myrcon servers
- Download updates from GitHub Releases instead

## Plugin System

- **Roslyn 4.8 compiler** — replaces CodeDom, supports C# latest features
- **AssemblyLoadContext isolation** — collectible contexts with shared assembly blocklist (prevents `InvalidCastException` on `IPRoConPluginInterface`)
- **Plugin isolation** — one plugin failing to compile/load does not block others
- **Multi-file plugins** — two layouts:
  - Flat: `AdKats.cs` + `AdKats.Commands.cs` (v1 compatible)
  - Subfolder: `AdKats.cs` + `AdKats/Commands.cs` (new, recursive scan)
- **`#include` directive** — shared `.inc` files across game types
- **Plugin Output console** — UI panel showing compilation/load/runtime messages
- **IP check API** — `procon.protected.ipcheck <ip>` → `OnIPChecked` event callback
- **No embedded default plugins** — users install plugins manually into `Plugins/<GameType>/`
- **Pre-created plugin directories** — `Plugins/BF3`, `BF4`, `BFBC2`, `BFHL`, `MOH`, `MOHW` exist on first launch
- **Available libraries**: PRoCon.Core, Newtonsoft.Json, MySqlConnector, Dapper, Flurl.Http, Microsoft.Data.Sqlite
- 40+ .NET 8 assembly references for plugin compilation
- Failed compilation cleanup (corrupt DLLs deleted, not left for `BadImageFormatException`)
- Plugin SDK template in `pluginsdk/` with database, HTTP, and IP check examples

## Performance

- Cached FindControl calls (40+ UI controls populated once)
- Static color brushes (no per-access allocation)
- Batch collection operations for console trimming and player lists
- ChatBuffer capped at 500 lines
- ConsoleFileLogger with rotation

## Infrastructure

- **GitHub Actions CI** — PR builds verify compilation, tag pushes create releases with checksums
- Version stamping from git tags into `VersionInfo.cs`
- Localization files extracted from embedded resources on first run
- Per-install mutex for process isolation
- `.editorconfig` + pre-commit formatting hook (`dotnet format`)
- v1.x code preserved on `v1-legacy` branch

---

## Known Limitations

- Battlemap/MapViewer not yet implemented (deferred — very complex)
- ServerEventBridge for proper event unsubscription pending (lambda subscriptions may leak)
- Some v1 plugins need manual refactoring for cross-platform compatibility (System.Windows.Forms, Win32 Registry)

---

## Breaking Changes

### For Server Administrators

1. **Self-contained build or .NET 8 Runtime required** — .NET Framework 4.7 no longer supported
2. **HTTP web server removed** — use SignalR layer for remote management
3. **Layer protocol changed** — v1.x and v2.0 cannot cross-connect (upgrade all instances together)
4. **Plugin sandbox removed** — plugins run with full trust (only install trusted plugins)
5. **Auto-updater removed** — download updates from GitHub Releases
6. **Default plugins not bundled** — install plugins manually into `Plugins/<GameType>/`
7. **Config format** — new installs create `procon.json`; existing `procon.cfg` files still work
8. **Data directory moved** — data no longer stored next to exe (see Data Directory section in README)

### For Plugin Developers

1. **Target .NET 8** — APIs removed from .NET 8 (`System.Windows.Forms`, `System.Security.Permissions`, `System.Runtime.Remoting`) are not available
2. **`OnHttpRequest` removed** — delete HTTP request handling from plugins
3. **`MySql.Data` → `MySqlConnector`** — `using MySqlConnector;` (auto-rewritten during compilation)
4. **AppDomain → AssemblyLoadContext** — no cross-domain calls, no `MarshalByRefObject` remoting
5. **New libraries available** — Dapper, Flurl.Http, Microsoft.Data.Sqlite, plus ProxyCheck.io via `procon.protected.ipcheck`
6. **Subfolder layout** — organize large plugins in `ClassName/` subdirectories
7. **Plugin SDK** — see `pluginsdk/` for templates and `docs/PLUGIN-REFACTORING-GUIDE.md` for migration

### For Layer Client Developers

1. **SignalR protocol** — connect to `http://<host>:<port>/layer` with a SignalR client
2. **JWT authentication** — replaces plaintext login sequence
