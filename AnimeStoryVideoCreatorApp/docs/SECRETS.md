# Secrets & xAI setup

The Anime Story Video Creator keeps **all xAI API keys on the server host**. The Blazor WASM client never receives the key.

## 1. Get an API key

1. Create an account at [accounts.x.ai](https://accounts.x.ai) and add credits.
2. Create a key in the [xAI console](https://console.x.ai).

## 2. Configure the key (pick one)

### Option A — User secrets (recommended for local dev)

From the **server project** directory:

```powershell
cd AnimeStoryVideoCreatorApp\AnimeStoryVideoCreator\AnimeStoryVideoCreator
dotnet user-secrets init
dotnet user-secrets set "Xai:ApiKey" "xai-your-key-here"
```

### Option B — Environment variable

```powershell
$env:XAI_API_KEY = "xai-your-key-here"
```

`XAI_API_KEY` overrides `Xai:ApiKey` from configuration when the host starts.

### Option C — appsettings (not recommended)

You can put the key in `appsettings.Development.json` under `Xai:ApiKey`, but **do not commit real keys**. Prefer user-secrets or env vars.

## 3. Verify

1. Run the host project.
2. Open **Settings** in the app, or:

```powershell
curl http://localhost:5129/api/health
```

You should see `"hasApiKey": true` when configured.

## 4. Models (defaults)

Configured under `Xai` in `appsettings.json`:

| Setting | Default |
|---------|---------|
| ChatModel | `grok-4.5` |
| ImageModel | `grok-imagine-image-quality` |
| VideoModel | `grok-imagine-video-1.5` |
| BaseUrl | `https://api.x.ai/v1/` |

## 5. Never do this

- Put keys in the Client project, `wwwroot`, or any WASM-visible file.
- Commit secrets to git.
- Share keys in screenshots of Settings (Settings only shows whether a key is present, not the value).
