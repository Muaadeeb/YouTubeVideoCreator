# Current Feature Status & Action Plan

**Status**: Living / Active  
**Project**: AnimeStoryVideoCreatorApp  
**Last audited**: 2026-08-20 (SQLite host store + continuity; uncommitted factory now being checked in)  
**Purpose**: Single agenda document — what exists today, what does not, and remaining work.

**Related docs**
- [SETUP.md](../SETUP.md) — run the app  
- [SECRETS.md](../SECRETS.md) — xAI API key  
- [Grok-Capabilities-And-Limits.md](./Grok-Capabilities-And-Limits.md)  
- [Design-AnimeStoryVideoCreator.md](./Design-AnimeStoryVideoCreator.md)

---

## Progress snapshot (after P0–P2)

| Area | Status | Notes |
|------|--------|--------|
| Shell / theme / nav | ✅ | Dark theme CSS, working nav routes |
| New Project | ✅ | Modal + localStorage persist |
| My Projects | ✅ | List / open / delete |
| Story Input | ✅ | Tone, idea, save, generate script |
| Domain models | ✅ | Project graph + assets |
| Persistence | ✅ | Host SQLite + asset files (`/api/projects`, IRepository) |
| xAI script proxy | ✅ | `/api/generate-script` |
| Storyboard stills | ✅ | `/api/generate-image` + gallery; continuity edit when refs present |
| Continuity bible | ✅ | Locations + plates + named extras; `/continuity` |
| Image-to-video | ✅ | `/api/generate-video` + poll |
| Character Vault | ✅ | CRUD + portrait gen (per project) |
| Export ZIP | ✅ | Script + images + video URLs |
| Settings / health | ✅ | Key presence check, clear data |
| CapCut / YouTube upload | ⬜ | Deferred |
| Full multi-clip stitch | ⬜ | Deferred (ZIP + per-panel video for now) |
| Tests / CI | ⬜ | Deferred |

---

## Status legend

| Tag | Meaning |
|-----|---------|
| ✅ **Done** | Implemented in codebase |
| 🟡 **Partial** | Usable but limited |
| ⬜ **Not started** | Still open |
| 🚫 **Deferred** | Intentionally later |

---

## Feature inventory

### App shell
| Feature | Status |
|---------|--------|
| Dark layout + sidebar | ✅ |
| New Project | ✅ |
| My Projects | ✅ |
| Story Input | ✅ |
| Script Library | ✅ |
| Storyboard Gallery | ✅ |
| Character Vault | ✅ |
| Settings | ✅ |
| Analytics / Series Settings | 🚫 Removed from nav |

### Domain & persistence
| Feature | Status |
|---------|--------|
| Full models (Project/Scene/Panel/Character/Asset) | ✅ |
| SQLite project store (IRepository + files) | ✅ |
| Import from browser localStorage | ✅ |
| SchemaVersion on Project | ✅ |

### AI pipeline
| Feature | Status |
|---------|--------|
| Server-only API key | ✅ |
| Script generation (Grok chat) | ✅ |
| Panel image generation | ✅ |
| Image-to-video | ✅ |
| Cost estimate UI | ⬜ |
| Multi-image reference edit API | ⬜ (portraits are single-image) |

### Export
| Feature | Status |
|---------|--------|
| ZIP (script.md, project.json, images, video URL manifest) | ✅ |
| CapCut package | ⬜ |
| Assembled long MP4 | ⬜ |

---

## Remaining backlog (post P0–P2)

| ID | Action | Priority |
|----|--------|----------|
| R1 | Manual QA with real `XAI_API_KEY` (script → still → video) | P0 |
| R2 | Cost estimates before batch generate | P2 |
| R3 | Multi-ref character pack (edit API, up to 3 images) | P2 |
| R4 | Client or server clip assembly (FFmpeg) | P2 |
| R5 | CapCut / YouTube package polish | P2 |
| R6 | Unit/bUnit tests + CI | P2 |
| R7 | Active page title in header | P3 |

---

## How to run (quick)

```powershell
cd AnimeStoryVideoCreatorApp\AnimeStoryVideoCreator\AnimeStoryVideoCreator
dotnet user-secrets set "Xai:ApiKey" "xai-..."
dotnet run
```

See [SETUP.md](../SETUP.md) and [SECRETS.md](../SECRETS.md).

---

## Changelog

| Date | Update |
|------|--------|
| 2026-08-11 | Initial audit |
| 2026-08-11 | P0–P2 implemented: models, persistence, Grok proxy (script/image/video), pages, export, docs |
| 2026-08-12 | Continuity bible: location plates, named extras, image-edit refs (up to 3), locked prompts |
| 2026-08-12 | Host SQLite + IRepository + `/api/projects`; stills as files; import from browser localStorage |
