---
name: "Lycans Mod Build Guidelines"
description: "Use when writing Lycans BepInEx mod C# code, project references, or MSBuild build and deployment configuration. Covers shared game assembly paths and opt-in plugin deployment."
applyTo:
  - "**/*.cs"
  - "**/*.csproj"
  - "**/*.props"
  - "**/*.targets"
---
# Lycans Mod Build Guidelines

- Treat the root `Directory.Build.props` and `Directory.Build.targets` files as shared infrastructure for every mod in this workspace. Keep changes compatible with both `LycansBotMod` and `LycansStatsMod`.
- Resolve game, BepInEx, and plugin dependencies through `LycansGameDir`, `LycansManagedDir`, `LycansBepInExCoreDir`, and `LycansPluginsDir`. Do not add hard-coded absolute assembly paths to individual project files.
- Preserve the validation target that fails early when required game assemblies are unavailable. Add new game-side dependencies through the shared properties or project references rather than bypassing validation.
- Keep deployment opt-in. `DeployLycansPlugin` must run only when `DeployToLycans=true`; ordinary builds must not write into the game installation.
- Do not run a build with `DeployToLycans=true` unless the user explicitly requests deployment.
- When changing build or deployment behavior, validate the affected project with a rebuild. Use deployment only when intentionally testing in the game:

```powershell
dotnet build <project>.csproj -t:Rebuild
dotnet build <project>.csproj -t:Rebuild -p:DeployToLycans=true
```