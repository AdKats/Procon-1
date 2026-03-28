# PRoCon v2.0 Changelog

## Overview

PRoCon v2.0 is a complete modernization of PRoCon Frostbite, migrating from .NET Framework 4.7/WinForms to .NET 8/Avalonia UI. This is the largest update in PRoCon's history — every layer of the application has been rewritten or significantly reworked while maintaining backward compatibility with existing game servers and workflows.

---

## Platform & Runtime

- **Migrated from .NET Framework 4.7 to .NET 8** — modern runtime with better performance, TLS 1.3, and cross-platform support
- **Cross-platform support** — runs natively on Windows, Linux, and macOS (no Mono required)
- **SDK-style project files** — replaced legacy `.csproj` format with modern SDK-style projects
- **Docker support** — headless console mode works in containers with multi-platform CI builds
- **Self-contained publishing** — can be distributed as a single deployment without requiring .NET installed

## UI — Complete Rewrite

- **Avalonia UI replaces WinForms** — modern, cross-platform UI framework
- **Dark and Light themes** — blue + orange gaming palette with Fluent design, switchable at runtime
- **16 tabbed panels**: Info/Dashboard, Chat, Players, Maps, Bans, Reserved Slots, Spectators, PunkBuster, Settings, Plugins, Accounts, Events, Console, Layer
- **Live server dashboard** — real-time player count graph, kill feed, team scores, server info
- **Landing page** — shows aggregate stats across all connected servers when no server is selected
- **Multi-server sidebar** — servers grouped by game type, sorted by name, with animated status indicators
- **Marquee scrolling** — long server names auto-scroll in the sidebar
- **Add Server dialog** — supports Direct RCON and PRoCon Layer connection modes
- **Settings dialog** — moved out of tab list into a dedicated modal window
- **Disconnected placeholder** — context-aware messaging when a server isn't connected
- **Ripple animation** — connection status indicators use outward ring animation

## RCON Console

- **Command history** — up/down arrows cycle through previous commands
- **Autocomplete** — tab-completion with full command signatures from game definitions
- **Game-aware commands** — only shows commands for the connected game type (BF4, BF3, BFH, etc.)
- **admin.help discovery** — queries the server for supported commands and merges with local definitions
- **Input validation** — validates command arguments before sending, with error messages
- **Error coloring** — `InvalidArguments` responses shown in red, warnings in yellow

## Network & Security

- **TLS 1.3 support** — modern encryption for game server connections
- **Removed TLS downgrade fallback** — eliminated `AllowTlsFallback` to prevent downgrade attacks
- **Fixed SslStream/NetworkStream double-dispose** — prevented crash in connection teardown
- **ProxyCheck.io v3 integration** — async IP reputation checking with 48-hour file cache, 1K free daily queries
- **Thread-safe daily counter** — fixed race condition in IP check service

## Layer System — SignalR Migration

- **SignalR WebSocket replaces TCP binary protocol** — Kestrel + SignalR Hub at `/layer` endpoint
- **LayerHostService** — new `ILayerInstance` implementation using ASP.NET Core
- **Automatic keepalive** — SignalR handles connection health (no manual `Poke()` needed)
- **Command routing** — layer commands route through FrostbiteConnection with sequence number tracking and 10s timeout
- **Force disconnect** — removing a layer account or changing permissions immediately disconnects affected clients
- **Background thread startup/shutdown** — avoids UI deadlock when enabling/disabling the layer

## HTTP Server — Removed

- **Built-in HTTP web server fully removed** — was 1,073 lines of code across 6 files
- **Replaced by SignalR** — all remote management now goes through the layer system
- **`OnHttpRequest` removed from plugin API** — plugins using this method must remove it
- **Config file backward compatibility** — old `procon.private.httpWebServer.enable` config lines are silently ignored

## Plugin System

- **Roslyn 4.8 compiler** — plugins compiled with `Microsoft.CodeAnalysis.CSharp` instead of legacy CodeDom
- **AssemblyLoadContext** — collectible load contexts replace AppDomain sandboxing (not available in .NET 8)
- **Type identity fix** — shared assemblies (PRoCon.Core, Newtonsoft.Json, MySqlConnector) always load from the host context, preventing `InvalidCastException` on `IPRoConPluginInterface`
- **Expanded compilation references** — 40+ .NET 8 assembly references including System.IO, System.Threading, System.Net.*, System.Xml, System.Web.HttpUtility, etc.
- **Automatic namespace migration** — `using MySql.Data.MySqlClient;` auto-rewritten to `using MySqlConnector;`, `using PRoCon.Core.HttpServer;` auto-stripped
- **Failed compilation cleanup** — corrupt DLLs from failed compiles are deleted instead of leaving bad files that cause `BadImageFormatException`
- **Embedded default plugins** — extracted from assembly resources at runtime, no separate download needed
- **`#include` directive support** — shared `.inc` files correctly resolved across game-type directories
- **Plugin trust warning** — prominent banner: "Plugins run with full trust on .NET 8"
- **Retry with backoff** — plugin panel retries wiring to PluginsManager with exponential backoff (1s, 2s, 4s, 8s, 15s)
- **Error logging** — `PreparePluginsDirectory` and compilation errors are now logged instead of silently swallowed

### Plugin Compatibility Results (BF4)

| Status | Plugins |
|--------|---------|
| **Works** | CBasicInGameInfo, CInGameAdmin, CSpambot, CBattlelogCache, CAdaptiveTicketCount, TrueBalancer, CLatencyManager, LanguageEnforcer |
| **Needs Refactoring** | CAdminIn_and_SpawnMsg, xVotemap, MULTIbalancer (System.Windows.Forms), CUltimateMapManager (Win32 Registry/FILETIME), AdKats, InsaneLimits, CChatGUIDStatsLogger, ProconRulz, etc. |

See `docs/PLUGIN-REFACTORING-GUIDE.md` for migration instructions.

## Account Management

- **Redesigned Accounts panel** — grouped privileges with quick presets
- **Layer panel** — shows connected layer clients with connection info

## Performance

- **Cached FindControl calls** — 40+ UI controls populated once instead of per-access lookup
- **Static color brushes** — `StatusColor` uses 3 static readonly instances instead of allocating per-access
- **Batch collection operations** — console line trimming and player list updates use efficient bulk operations
- **Removed redundant KillFeed reassignment** — `ItemsSource` set once, not on every kill event
- **ConsoleFileLogger with rotation** — capped at configurable size
- **ChatBuffer capped at 500 lines** — prevents unbounded memory growth

## Ban List

- **Auto-refresh fixed** — packet cache invalidation via `Invalidate(Regex)` after ban mutations
- **Cache-aware** — `CacheManager` supports pattern-based invalidation for stale data

## Infrastructure

- **GitHub Actions CI** — PR builds verify compilation, tag pushes create releases with ZIP archives and checksums
- **Version stamping** — extracted from git tags during CI, stamped into `VersionInfo.cs`
- **Localization extraction** — `.loc` files extracted from embedded resources on first run
- **Per-install mutex** — uses FNV-1a hash of BaseDirectory for process isolation

---

## Known Limitations

- Self-contained Linux publish may have CoreCLR host path issues — use `dotnet run` as workaround
- TextChatModeration panel not yet implemented
- Player right-click context menus on team lists not yet implemented
- Battlemap/MapViewer deferred (very complex, 9/10 effort)
- `procon.protected` command handling not yet implemented
- ServerEventBridge for proper event unsubscription still pending (lambda-based subscriptions may leak)

---

# Breaking Changes

## For Server Administrators

1. **Requires .NET 8 Runtime** — .NET Framework 4.7 is no longer sufficient. Install the [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) or use the self-contained build.

2. **HTTP Web Server Removed** — If you used the built-in HTTP server for remote management, it no longer exists. Use the SignalR-based layer system instead. Old config lines are silently ignored.

3. **Layer Protocol Changed** — The layer now uses SignalR WebSocket instead of the custom TCP binary protocol. **v1.x PRoCon instances cannot connect as layer clients to a v2.0 layer, and vice versa.** All instances in a layer setup must be upgraded together.

4. **Plugin Sandbox Removed** — .NET 8 does not support AppDomain sandboxing. Plugins now run with full trust. Only install plugins you trust.

5. **Windows Forms Removed** — The UI is now Avalonia-based. Plugins that referenced System.Windows.Forms must be refactored.

## For Plugin Developers

1. **Target .NET 8** — Plugins are compiled against .NET 8 runtime assemblies. APIs removed from .NET 8 (e.g., `System.Security.Permissions`, `System.Runtime.Remoting`, `System.Windows.Forms`) are not available.

2. **`OnHttpRequest` Removed** — The method and its types (`HttpWebServerRequestData`, `HttpWebServerResponseData`) no longer exist. Remove all HTTP request handling from your plugins.

3. **`MySql.Data` replaced by `MySqlConnector`** — The `using MySql.Data.MySqlClient;` is auto-rewritten to `using MySqlConnector;`. The API is largely compatible but some edge cases may differ.

4. **AssemblyLoadContext replaces AppDomain** — `AppDomain.CreateDomain()`, `MarshalByRefObject` remoting, and cross-domain calls are not available. Plugins load into a collectible `AssemblyLoadContext`.

5. **`System.Web` namespace** — Only `System.Web.HttpUtility` is available. The full `System.Web` assembly from .NET Framework is not present.

6. **Roslyn Compilation** — Plugins are compiled with Roslyn (`Microsoft.CodeAnalysis.CSharp 4.8.0`) instead of CodeDom. All C# language features up to the latest version are supported.

7. **See the SDK Plugin Template** — `src/Resources/DefaultPlugins/SdkTemplatePlugin.cs` provides a starting point for new .NET 8 compatible plugins.

## For Layer Client Developers

1. **SignalR Protocol** — Connect to `http://<host>:<port>/layer` using a SignalR client instead of raw TCP sockets with the Frostbite binary protocol.

2. **Authentication** — JWT-based authentication replaces the plaintext login sequence.

---

# Discord Announcement

```
## PRoCon v2.0 — Coming Soon

The biggest update in PRoCon history is in development. Every part of the application is being modernized.

**What's Coming:**
- .NET 8 runtime — Windows, Linux, and macOS natively (no Mono!)
- Brand new Avalonia UI with dark/light themes
- Live server dashboard with player graphs and kill feed
- SignalR layer system replacing the old TCP protocol
- RCON console with autocomplete, command history, and validation
- Async IP reputation checking via ProxyCheck.io
- Docker support for headless deployments

**Breaking Changes to Prepare For:**
- .NET 8 Runtime will be required (or use self-contained build)
- Built-in HTTP web server is being removed (replaced by SignalR layer)
- Layer protocol is changing — v1.x and v2.0 instances will not cross-connect
- Plugin sandbox is being removed — plugins will run with full trust
- MySQL driver changing from MySql.Data to MySqlConnector
- System.Windows.Forms no longer available — plugins using MessageBox, Application, etc. will need updates

**For Plugin Developers:**
- Plugins still compile from .cs source files, now using Roslyn
- A new SDK template plugin is included as a starting point
- Most plugins that don't use WinForms or Windows-only APIs will work as-is
- Full .NET 8 API surface available (C# latest features supported)
- See the plugin refactoring guide for migration steps

**What's Still in Progress:**
- TextChatModeration panel
- Player context menus
- Refactoring remaining default plugins for cross-platform compatibility
- Battlemap viewer (deferred to a future release)

Stay tuned for the release. Back up your `Configs/` directory before upgrading — config files will be backward compatible.
```
