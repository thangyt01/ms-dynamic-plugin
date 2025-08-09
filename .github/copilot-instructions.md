# AI Agent Guide for `ms-dynamic-plugin`

This repo is a Dynamics 365 / Dataverse plugin project targeting .NET Framework 4.6.2, packaged as a NuGet for deployment with the Power Platform Plugin build targets.

## Big picture
- Core base: `PluginBase` and `LocalPluginContext` in `PluginBase.cs` wrap IPlugin execution with tracing, services, and guards.
- Plugins live in the `demo` namespace, each class inheriting `PluginBase` and overriding `ExecuteDataversePlugin`.
- Example plugins provided (split by responsibility):
  - `CreatePlugin.cs`: handles Account Create (PostOperation) and adds a Note.
  - `ReadPlugin.cs`: reads fields from Account after create and traces.
  - `UpdatePlugin.cs`: updates Account description after create.
  - `DeletePlugin.cs`: creates a Task to review deletion (non-destructive).
- There is also a consolidated example (`CrudPlugin.cs` or `Plugin1.cs`) showing full CRUD demo logic.

## Execution model & services
- Entry: `PluginBase.Execute(IServiceProvider)` constructs `LocalPluginContext`, then calls `ExecuteDataversePlugin`.
- Use `local.PluginUserService` for data ops (runs as Step user) and `local.InitiatingUserService` when acting as the caller.
- Access context via `local.PluginExecutionContext` (MessageName, PrimaryEntityName, Stage, Depth, etc.).
- Use `local.TracingService` or `local.Trace(...)` for perf-stamped logs. A depth guard (`ctx.Depth > 1`) prevents recursion.

## Build, pack, and outputs
- Target framework: `net462`.
- MSBuild imports PowerApps Plugin targets if installed:
  - `Microsoft.PowerApps.VisualStudio.Plugin.props/targets` via `PowerAppsTargetsPath`.
- NuGet packaging is configured in `demo.csproj` (PackageId: `demo`).
- Common workflows:
  - Build: dotnet build ./demo.csproj
  - Outputs: `bin/Debug/net462/` and `bin/Debug/net462/publish/`. A `.nupkg` is produced under `bin/Debug/`.

## Registration expectations
- Plugins are designed to be registered on specific pipeline steps:
  - Typical demo config: Message=Create, Primary Entity=account, Stage=PostOperation (40), Sync.
- Each plugin class self-checks: MessageName, PrimaryEntityName, Stage, and `Depth`.
- If you change the target entity or stage, update both the code checks and the registration.

## Project conventions
- Namespace `demo`; one class per file.
- Keep plugin logic minimal in `ExecuteDataversePlugin` and extract helpers as private methods when needed.
- Always include:
  - Recursion guard: `if (ctx.Depth > 1) return;`
  - Message/entity/stage checks; exit early when not matched.
  - Tracing around external/service operations.
- Use `ColumnSet` to retrieve specific fields; avoid `ColumnSet(true)`.
- Prefer non-destructive examples (e.g., create Task instead of deleting core records by default).

## Examples
- Update pattern (from `UpdatePlugin.cs`):
  - Validate: `Message=Create`, `Entity=account`, `Stage=40`.
  - `var update = new Entity("account", accountId) { ["description"] = "..." };`
  - `svc.Update(update);`
- Read pattern (from `ReadPlugin.cs`):
  - `var cols = new ColumnSet("name", "accountnumber", "telephone1");`
  - `var entity = svc.Retrieve("account", id, cols);`
  - Use `GetAttributeValue<T>("field")` safely.

## External dependencies
- NuGet: `Microsoft.CrmSdk.CoreAssemblies`, `Microsoft.PowerApps.MSBuild.Plugin`, and `Microsoft.NETFramework.ReferenceAssemblies`.
- No test framework configured; build-only validation by default.

## When editing or adding plugins
- Inherit from `PluginBase` and override `ExecuteDataversePlugin`.
- Add early exits for mismatched steps; keep logic idempotent.
- Use `PluginUserService` unless you intentionally need `InitiatingUserService`.
- After code changes, build to regenerate binaries and NuGet package.

## Open questions for maintainers
- Confirm intended registration matrix per plugin (messages/entities/stages) for environments beyond demos.
- Should `DeletePlugin` actually delete or remain as guidance creating a review Task?
- Any organization-specific fields or custom entities to standardize in examples?
