# PRoCon v2.0 — Plugin Refactoring Guide

This guide helps you migrate existing PRoCon v1.x plugins to work on .NET 8 / PRoCon v2.0. Use this as a reference when refactoring plugins that fail to compile.

---

## Quick Checklist

For each plugin, apply these changes in order:

1. Remove `using PRoCon.Core.HttpServer;` and any `OnHttpRequest` override
2. Replace `using MySql.Data.MySqlClient;` with `using MySqlConnector;`
3. Remove `using System.Windows.Forms;` and replace `MessageBox.Show()` calls
4. Remove `using System.Security.Permissions;` and any CAS/sandbox code
5. Remove `using System.Runtime.Remoting;` and any AppDomain code
6. Replace Win32 API calls (`Registry`, `FILETIME`, `DllImport` for kernel32)
7. Test compile by connecting to a server or running the Roslyn compiler manually

---

## Common Migration Patterns

### 1. HttpServer Removal

**Before (v1.x):**
```csharp
using PRoCon.Core.HttpServer;

public override HttpWebServerResponseData OnHttpRequest(HttpWebServerRequestData data) {
    return new HttpWebServerResponseData("Hello World!");
}
```

**After (v2.0):** Delete the entire method and the `using` directive. The HTTP server no longer exists. If you need remote access, use the SignalR layer system.

Also remove `"OnHttpRequest"` from your `RegisterEvents()` call.

---

### 2. MySQL — MySql.Data to MySqlConnector

**Before (v1.x):**
```csharp
using MySql.Data.MySqlClient;
```

**After (v2.0):**
```csharp
using MySqlConnector;
```

The API is almost identical. Key differences:

| MySql.Data | MySqlConnector | Notes |
|------------|---------------|-------|
| `MySqlConnection` | `MySqlConnection` | Same |
| `MySqlCommand` | `MySqlCommand` | Same |
| `MySqlDataReader` | `MySqlDataReader` | Same |
| `MySqlParameter` | `MySqlParameter` | Same |
| `MySqlException` | `MySqlException` | Same |
| `MySqlDbType` | `MySqlDbType` | Same |
| `AllowUserVariables=true` | `AllowUserVariables=true` | Same |
| `UseAffectedRows=true` | `UseAffectedRows=true` | Same (default differs!) |

**Watch out for:**
- `MySqlConnector` defaults `UseAffectedRows=false` (returns matched rows). If your plugin relies on `cmd.ExecuteNonQuery()` returning affected rows, add `UseAffectedRows=true` to your connection string.
- Async methods are truly async in MySqlConnector (not faked). If you use `OpenAsync()`, `ExecuteReaderAsync()`, etc., they will actually run asynchronously.

---

### 3. System.Windows.Forms Removal

**Before (v1.x):**
```csharp
using System.Windows.Forms;

MessageBox.Show("Error occurred", "Plugin Error", MessageBoxButtons.OK);
Application.DoEvents();
```

**After (v2.0):**
```csharp
// MessageBox — replace with plugin console output
ExecuteCommand("procon.protected.pluginconsole.write", "^1[MyPlugin] Error occurred");

// Application.DoEvents() — remove entirely (not needed, was always a code smell)
// If you need async work, use System.Threading.Tasks
```

**For TypeDescriptor usage (MULTIbalancer pattern):**
```csharp
// Before:
using System.ComponentModel;
TypeDescriptor.GetConverter(typeof(MyEnum)).ConvertFromString(value);

// After:
Enum.Parse<MyEnum>(value);
// or for non-generic:
(MyEnum)Enum.Parse(typeof(MyEnum), value);
```

---

### 4. Win32 Registry Access

**Before (v1.x):**
```csharp
using Microsoft.Win32;

RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\MyPlugin");
string value = key?.GetValue("Setting")?.ToString();
```

**After (v2.0):** Use file-based configuration instead. Plugins run cross-platform (Linux, macOS, Windows), so the Windows Registry is not available.

```csharp
using System.IO;

string configPath = Path.Combine("Plugins", "MyPlugin.json");
if (File.Exists(configPath))
{
    string json = File.ReadAllText(configPath);
    // Parse with Newtonsoft.Json (available to plugins)
}
```

---

### 5. FILETIME and Kernel32 DllImport

**Before (v1.x):**
```csharp
using System.Runtime.InteropServices;

[DllImport("kernel32.dll")]
static extern bool GetProcessTimes(IntPtr hProcess, out FILETIME creation, ...);

struct FILETIME { public uint dwLowDateTime; public uint dwHighDateTime; }
```

**After (v2.0):** Use .NET APIs instead of P/Invoke:

```csharp
using System.Diagnostics;

// Process times
var process = Process.GetCurrentProcess();
TimeSpan cpuTime = process.TotalProcessorTime;
DateTime startTime = process.StartTime;

// High-resolution timing
long timestamp = Stopwatch.GetTimestamp();
```

---

### 6. SecurityManager / SecurityPermission

**Before (v1.x):**
```csharp
using System.Security;
using System.Security.Permissions;

SecurityManager.IsGranted(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
```

**After (v2.0):** Code Access Security (CAS) does not exist in .NET 8. Delete all CAS checks. Plugins run with full trust.

```csharp
// Just delete the security check — all permissions are granted
```

---

### 7. System.Web Namespace

**Before (v1.x):**
```csharp
using System.Web;

string encoded = HttpUtility.UrlEncode(value);
NameValueCollection query = HttpUtility.ParseQueryString(url);
```

**After (v2.0):** `HttpUtility` is still available but the `using` changes:

```csharp
using System.Web;  // This still works — maps to System.Web.HttpUtility.dll

string encoded = HttpUtility.UrlEncode(value);  // Same API
```

If you used `HttpWebRequest`:
```csharp
// Before:
using System.Net;
HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

// After (preferred):
using System.Net.Http;
var client = new HttpClient();
string response = await client.GetStringAsync(url);

// Or synchronous (if you must):
string response = client.GetStringAsync(url).Result;
```

---

### 8. XmlDocument (System.Xml)

If your plugin uses `XmlDocument`, `XmlNode`, `XmlNodeList` — these still work, the assembly reference is included. No changes needed for the code itself. If you got compile errors before, those are fixed in the latest build.

---

## Available .NET 8 Assembly References

Plugins have access to these assemblies at compile time:

**Core Runtime:** System.Runtime, System.Private.CoreLib, System.Collections, System.Collections.Specialized, System.Linq, System.IO, System.IO.FileSystem, System.Console, System.Globalization, System.Reflection, System.ObjectModel, Microsoft.CSharp, netstandard

**Threading:** System.Threading, System.Threading.Thread, System.Threading.Timer

**Networking:** System.Net.Http, System.Net.Primitives, System.Net.Sockets, System.Net.WebClient, System.Net.Requests, System.Net.WebHeaderCollection, System.Private.Uri

**Data:** System.Data.Common, System.Xml, System.Private.Xml, System.Xml.XDocument, System.Xml.XmlSerializer

**Text:** System.Text.RegularExpressions, System.Text.Encoding.Extensions, System.Web.HttpUtility

**Components:** System.ComponentModel, System.ComponentModel.Primitives, System.ComponentModel.TypeConverter

**Security:** System.Security.Cryptography

**Serialization:** System.Runtime.Serialization.Primitives, System.Runtime.Serialization.Xml, System.Runtime.InteropServices

**Diagnostics:** System.Diagnostics.Debug, System.Diagnostics.Process

**PRoCon:** PRoCon.Core.dll, MySqlConnector.dll, Newtonsoft.Json.dll

---

## Plugin File Structure

```
Plugins/
  BF4/
    MyPlugin.cs              ← Main plugin file (class name = file name)
    MyPlugin.Additional.cs   ← Optional partial class files
    PluginCache.xml          ← Auto-generated compile cache
  BF3/
    ...
  BFHL/
    ...
```

- The main `.cs` file must have the same name as the class inside it
- Files with dots in the name (e.g., `MyPlugin.Additional.cs`) are treated as partial classes
- `#include "file.inc"` directives work — relative to `Plugins/<GameType>/`
- `#include "../file.inc"` goes up to `Plugins/` for shared includes
- `%GameType%` in include paths is replaced with the current game type

---

## Testing Your Plugin

1. Place the `.cs` file in `Plugins/<GameType>/`
2. Delete `PluginCache.xml` in that directory to force recompilation
3. Connect to a game server — plugins compile automatically
4. Check the Plugin Console tab for compilation errors
5. If the plugin compiles but doesn't load, check for `BadImageFormatException` (delete the `.dll` and retry)

---

## SDK Template

See the `pluginsdk/` directory for a complete working example with:
- Plugin metadata methods
- Variable system with categories and enums
- Player join/leave/kill event handling
- Chat command handling
- Round and server info events
- Logging helper
