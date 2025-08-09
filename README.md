# ms-dynamic-plugin

Dynamics 365 / Dataverse plugin project (.NET Framework 4.6.2) packaged via Power Platform Plugin MSBuild targets. Includes split CRUD demo plugins and a consolidated example.

## Project layout
- `PluginBase.cs` – Base wrapper (tracing, services, guards) and `LocalPluginContext`.
- `CreatePlugin.cs` – On Account Create (PostOperation), adds a Note (annotation).
- `ReadPlugin.cs` – On Account Create (PostOperation), reads fields and traces them.
- `UpdatePlugin.cs` – On Account Create (PostOperation), updates Account description.
- `DeletePlugin.cs` – On Account Create (PostOperation), creates a Task to review deletion (non-destructive).
- `demo.csproj` – Sdk-style project targeting `net462`, NuGet packaging configured.

## Prerequisites
- Windows
- .NET SDK 6+ (8.x recommended). The project uses `Microsoft.NETFramework.ReferenceAssemblies` so you don’t need full .NET Framework targeting packs installed.
- Optional: Power Platform Tools
  - Plugin Registration Tool (PRT) from XrmToolBox or SDK Tools
  - Alternatively, Power Platform Build Tools in Azure DevOps/GitHub Actions for CI/CD

## Build and pack
The PowerApps MSBuild targets package the plugin automatically on build.

- Debug build:
```bash
dotnet build ./demo.csproj
```
- Release build:
```bash
dotnet build -c Release ./demo.csproj
```
- Clean:
```bash
dotnet clean ./demo.csproj
```

Outputs:
- Assembly: `bin/Debug/net462/publish/demo.dll` (or `bin/Release/...`)
- NuGet package: `bin/Debug/demo.1.0.0.nupkg` (or `bin/Release/...`) – for pipeline distribution

VS Code tasks (optional):
- Build task is available as “build” and runs the same `dotnet build`.

## Register the plugin in Dataverse
Use Plugin Registration Tool (PRT):
1. Connect to your environment.
2. Register → Register New Assembly → browse to `bin/Debug/net462/publish/demo.dll` (or Release build).
3. Add Steps for the classes you want to test. Typical demo setup (synchronous):
   - Class: `demo.CreatePlugin` → Message: Create, Primary Entity: account, Stage: PostOperation (40)
   - Class: `demo.ReadPlugin` → Message: Create, Primary Entity: account, Stage: PostOperation (40)
   - Class: `demo.UpdatePlugin` → Message: Create, Primary Entity: account, Stage: PostOperation (40)
   - Class: `demo.DeletePlugin` → Message: Create, Primary Entity: account, Stage: PostOperation (40)
   - Optional: `demo.CrudPlugin` or `demo.Plugin1` (combined CRUD) on the same step if you want the end‑to‑end demo.

Note: In production you typically align each plugin with its natural message (e.g., UpdatePlugin on Update, DeletePlugin on Delete). The demo uses Create for easy testing.

## Try it
1. Ensure Plug‑in Trace Log is enabled (Settings → Administration → System Settings → Customization → Enable logging to Plug‑in Trace Log = All/Exception).
2. Create an Account record in the environment.
3. Open Plug‑in Trace Log to see output from the demo plugins.
   - CreatePlugin: should create a Note on the new account.
   - ReadPlugin: traces name/accountnumber/telephone1.
   - UpdatePlugin: sets a description on the account.
   - DeletePlugin: creates a Task to review deletion (non‑destructive).
   - CrudPlugin/Plugin1: will create its own demo account, update, then delete it.

## Conventions & tips
- Always guard recursion: `if (ctx.Depth > 1) return;`
- Verify pipeline context before acting: Message, PrimaryEntity, Stage.
- Use `PluginUserService` unless you explicitly need the initiating user.
- Prefer `ColumnSet("f1", "f2")` over `ColumnSet(true)`.
- Use `GetAttributeValue<T>("field")` for safe reads.

## Extending
- Create a new class in namespace `demo` inheriting `PluginBase` and override `ExecuteDataversePlugin`.
- Add early exits for mismatched message/entity/stage.
- Trace external calls and handle exceptions with best‑effort cleanup.
- Rebuild, then update the assembly/step registration in PRT.

## Troubleshooting
- Build errors: ensure .NET SDK is installed and you’re on Windows.
- Missing PowerApps targets: Visual Studio PowerApps Plugin targets are referenced in `demo.csproj`. The project still builds and produces a NuGet; if targets are not present locally, packaging may fall back to defaults.
- No traces: ensure Plug‑in Trace Log is enabled.

## License
Sample/demo code; adapt for your organization’s standards.
