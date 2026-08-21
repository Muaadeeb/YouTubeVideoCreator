# Setup & run

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- xAI API key (for script / image / video generation) — see [SECRETS.md](./SECRETS.md)

## Run

```powershell
cd AnimeStoryVideoCreatorApp\AnimeStoryVideoCreator\AnimeStoryVideoCreator
dotnet run
```

Open the URL from the console (typically `https://localhost:7065` or `http://localhost:5129`).

## First-use flow

1. **Settings** → confirm API key status (green if configured).
2. **New Project** → name, series, length, art style.
3. **Story Input** → paste idea, pick tone, **Generate Script Now**.
4. **Script Library** → review/edit scenes; set **Location**, **Cast**, and **Extras lock**.
5. **Continuity Bible** → lock each set + establishing plate. **Character Vault** → portraits; mark background people as Extra.
6. **Storyboard Gallery** → lock each scene, then generate stills (edit-from-refs when continuity is on). Optional image-to-video, **Export ZIP**.

## Projects storage

Host **SQLite** at `AnimeStoryVideoCreator/App_Data/asvc.db` plus stills/clips under `App_Data/assets/`.
The WASM UI talks to `/api/projects` on the Blazor host. Http vs https no longer splits your library.

First launch will **import** any older stories still in this browser’s localStorage. You can also use **My Projects → Import from this browser**.

## Related docs

- [planning/Current-Status-And-Action-Plan.md](./planning/Current-Status-And-Action-Plan.md)
- [planning/Grok-Capabilities-And-Limits.md](./planning/Grok-Capabilities-And-Limits.md)
- [planning/Design-AnimeStoryVideoCreator.md](./planning/Design-AnimeStoryVideoCreator.md)
