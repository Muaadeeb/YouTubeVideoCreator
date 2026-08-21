# Grok Capabilities & Limits (Living Document)

**Status**: Living / Active  
**Project**: AnimeStoryVideoCreatorApp  
**Owner**: Timothy  
**Last verified**: 2026-08-11  
**Primary sources**: [xAI Imagine docs](https://docs.x.ai/developers/model-capabilities/imagine), [Models & pricing](https://docs.x.ai/developers/models), [Acceptable Use Policy](https://x.ai/legal/acceptable-use-policy) (effective 2026-06-26), [Imagine Image 2.0 announcement](https://x.ai/news/grok-imagine-image-2)

---

## Purpose of this document

This is the **single living reference** for what Grok / xAI can and cannot do for this app.

Earlier planning (app spec, design doc) was written when image + video features were thinner or assumed slideshow/TTS workarounds. **Grok Imagine has advanced substantially** (native image edit, multi-ref, text-to-video, image-to-video, video edit/extend, audio in video). This document captures the current ceiling so product and engineering decisions stay honest.

**How we maintain it**
- When xAI ships a model or policy change, update the tables below and add a row to [Changelog](#changelog).
- Prefer **official docs + AUP** over social posts or community claims. Flag anything unofficial as *observed / reported*.
- Consumer Grok (grok.com / X app) and the **Imagine API** can differ slightly in UI, geo-blocks, and defaults. This app uses the **API** path unless noted.

---

## At a glance (for this app)

| Capability | Available now via API? | Fits our YouTube story pipeline? |
|------------|------------------------|----------------------------------|
| Script / outline / dialogue (chat models) | Yes | Core |
| Text → still image (storyboard panels) | Yes | Core |
| Image edit + multi-reference (character consistency) | Yes | Core (Character Vault / Reference Packs) |
| Text → short video clip | Yes | Core (scene clips) |
| Image → video (animate a panel) | Yes | Core (camera moves on approved panels) |
| Video edit / extend | Yes | Post-MVP polish |
| Native audio on generated video | Yes (default on video models) | Core / optional |
| Full multi-minute feature film in one shot | **No** | Assemble many short clips + edit |
| Guaranteed frame-perfect long continuity | **No** | Use refs + editing + human QC |
| Explicit CSAM / real-person nonconsensual nudes | **Hard no** (AUP + law) | Never |

---

## 1. What Grok **can** do

### 1.1 Text / chat (scripts, outlines, structure)

| Item | Detail |
|------|--------|
| Models (examples) | `grok-4.5`, `grok-4.3`, `grok-4.20-*` (see [models](https://docs.x.ai/developers/models)) |
| Strengths | Long-form creative writing, scene breakdowns, tone control, structured JSON / tool calling, iteration |
| Context | Up to ~500k–1M tokens depending on model |
| Tools | Web search, X search (optional); image generation tool inside chat |
| Knowledge cutoff | e.g. Grok 4.5: **2026-02-01** (no live knowledge without search tools) |

**App uses**: Story idea → outline → full script → image prompts → captions / SRT text.

### 1.2 Image generation (Grok Imagine)

| Item | Detail |
|------|--------|
| Models | `grok-imagine-image` ($0.02/img), `grok-imagine-image-2.0` ($0.04/img), `grok-imagine-image-quality` ($0.05/img) |
| Modes | Text-to-image; natural-language **image editing**; multi-image editing |
| Batch | Up to **10 images per request** |
| Resolution | **1K and 2K** |
| Aspect ratios | `1:1`, `16:9`, `9:16`, `4:3`, `3:4`, `3:2`, `2:3`, `2:1`, `1:2`, phone ultra-wides (`19.5:9`, `9:19.5`, `20:9`, `9:20`), `auto` |
| Output | Temporary URL and/or **base64** |
| Multi-ref edit | Official API docs: **up to 3** source images per edit (combine subjects, style transfer, scene compose). Consumer Image 2.0 product marketing also mentions higher multi-ref counts in product UI — **re-check API limits** when wiring Character Reference Packs. |
| Image 2.0 highlights | Stronger instruction following, typography/layout, identity preservation across edits, smart resize / aspect recompose (product features; map to API params as documented) |
| Moderation flag | Responses can include `respect_moderation` — treat failures as blocked, not “empty file” |

**App uses**: Panel art, character portraits/expressions, thumbnails, style-locked sets.

### 1.3 Video generation (Grok Imagine Video)

| Item | Detail |
|------|--------|
| Models | `grok-imagine-video` (~$0.05/sec), `grok-imagine-video-1.5` (~$0.08/sec) |
| Modes | **Text-to-video**, **image-to-video**, **reference-to-video**, **video editing**, **video extension** |
| Duration | **1–15 seconds** per generation (editing keeps source duration; edit path caps noted in docs ~8.7s for some edit flows) |
| Resolution | **480p**, **720p**, **1080p** (1080p on `grok-imagine-video-1.5` for T2V/I2V; reference-to-video capped at 720p per docs) |
| Aspect ratios | `1:1`, `16:9`, `9:16`, `4:3`, `3:4`, `3:2`, `2:3` (I2V defaults to source image aspect unless overridden) |
| Audio | Generated videos include an **audio track by default**; preset voices on reference-to-video (e.g. `voice_id`); custom voice file refs = partners-only |
| Async API | Start job → poll `request_id` until `done` / `failed` / `expired` (can take minutes) |
| Output | Temporary hosted video URL — **download promptly** |

**App uses**: Animate approved storyboard panels with camera moves (pan/zoom/dolly feel), short action beats, trailer-style clips. Stitch many clips client-side or later with FFmpeg for full YouTube runtime.

### 1.4 Audio (beyond embedded video audio)

| Item | Detail |
|------|--------|
| TTS | Text-to-speech available (priced per characters) |
| STT | Speech-to-text batch/streaming |
| Voice / speech-to-speech | Separate voice models (per-minute pricing) |

**App uses**: Narration tracks, optional character VO; complements or replaces browser `SpeechSynthesis` from the old MVP plan.

### 1.5 What this means vs the original design doc

The design doc planned:
- Server proxy to xAI for script + still images  
- Video MVP = **canvas slideshow + browser TTS + MediaRecorder**

**Still correct for architecture (server holds the API key).**  
**Outdated for capability ceiling**: native **image-to-video**, **text-to-video**, and **video edit/extend** are production API features now. Prefer Imagine video for cinematic motion; keep client slideshow only as offline/low-cost fallback.

---

## 2. What Grok **cannot** do (or cannot do reliably)

### 2.1 Hard product / technical limits

| Limit | Reality |
|-------|---------|
| Single long-form YouTube video (5–60 min) in one generation | **Cannot.** Max clip length is short (≤15s). Full videos = many generations + assembly. |
| Perfect multi-scene identity forever without refs | **Not guaranteed.** Use Character Reference Packs, multi-image edit, and reference-to-video; still needs human QC. |
| Deterministic camera path scripting (exact keyframes like CapCut) | **No pro NLE control.** Prompt for motion; refine with re-gens and edits. |
| Unlimited free generation | **Paid API** with per-image and per-second pricing; rate limits apply. |
| Offline generation | **Requires** xAI API (network + key). Offline only after assets are cached. |
| Real-time interactive 3D / game engine | **No.** 2D images + short videos. |
| Turning off safety systems | **Forbidden** (AUP: no jailbreaks, no bypassing safeguards). |
| Using outputs to train competing models | **Forbidden** (AUP). |
| Guaranteed permanent asset hosting | Video/image URLs are **temporary** — app must persist downloads (localStorage / files). |

### 2.2 Legal / AUP hard stops (never implement)

From the [xAI Acceptable Use Policy](https://x.ai/legal/acceptable-use-policy) (and law):

| Category | Rule |
|----------|------|
| **Children** | **Zero tolerance.** Sexualizing or exploiting children is banned. xAI reports suspected CSAM to NCMEC. **Never** generate sexual/pornographic content involving minors (including fictional “aged-down” or “looks underage” characters). For anime style: keep characters **clearly adult** when mature themes are present. |
| **Real people – nonconsensual intimate** | Ban on undressing/nudifying **real persons**, or altering a real person’s likeness into intimate/sexual context. |
| **Real people – pornographic likeness** | Ban on depicting likenesses of persons in a **pornographic** manner. |
| **Deepfake abuse / impersonation** | Deceptive impersonation, defamation, privacy/publicity violations prohibited. |
| **Crime / severe harm** | No using the service for critical harm to life, terrorism, weapons of mass destruction, fraud, etc. |
| **Circumvention** | Jailbreaks, prompt injection to defeat safety, stripping watermarks/provenance, etc. prohibited. |
| **Jurisdiction** | Content illegal in the relevant jurisdiction is prohibited; geo-restrictions may apply. |

Violations can mean account suspension/termination. This app must not offer features whose primary purpose is violative use.

---

## 3. How explicit can we get? (violence, gore, nudity, etc.)

> **Important**: Filters and product defaults change. The AUP is the legal contract; public statements and automatic moderation are extra layers. Always re-validate against [AUP](https://x.ai/legal/acceptable-use-policy) and live API behavior before shipping a “mature content” toggle.

### 3.1 Official policy floor (AUP)

The AUP is **short and harm-focused**. It does **not** publish a full MPAA-style grid. Explicitly banned:

- Child sexual exploitation (any form)
- Nonconsensual intimate imagery of **real** people
- Pornographic depictions of **real** persons’ likenesses
- Illegal content in the applicable jurisdiction
- Critical real-world harm categories listed in the AUP

It does **not** list a blanket ban on all fictional adult violence, all fictional gore, or all fictional mature themes. That does **not** mean “anything goes” — automated moderation still blocks many prompts/outputs (`respect_moderation: false`, errors with moderation-related codes).

### 3.2 Public “R-rated movie” framing (product intent, not a legal guarantee)

xAI leadership / Grok product messaging has described Imagine content allowance roughly as:

> **If it would be allowed in an R-rated movie, it should be allowed in Grok Imagine** (with regional law overrides).

Related public framing (NSFW-related):

- Upper-body / R-rated-level **nudity of imaginary adult humans** (not real people) has been described as intended when NSFW-style settings apply  
- Standards **vary by country/state**; geo-blocks exist  
- Consumer UI (X vs grok.com) has differed in enforcement over time

Treat this as **directional product intent**, not a promise that every R-rated prompt will succeed via API every time.

### 3.3 Practical matrix for **this app** (fictional anime / manhwa / original characters)

Use this for product copy, prompt templates, and QC. “Usually OK” means *commonly accepted for adult-fiction creative tools under R-rated framing*; still subject to model refusal.

| Theme | Fictional original adult characters | Real people / celebrities | Notes for story videos |
|-------|-------------------------------------|---------------------------|-------------------------|
| Mild violence, action fights, blood spatters | Usually OK | Usually OK if not glorifying real crime against IRL victims | Dark/epic tones in app fit |
| Graphic combat, wounds (cinematic, not torture porn) | Often OK under R-rated framing; may still moderate | Same | Prefer “cinematic action” wording over snuff-style prompts |
| Extreme gore / prolonged torture / real-world atrocity glorification | Risk of block; avoid as a product feature | Avoid | Not needed for typical isekai/OP-MC content |
| Romance, kissing, sensual tension | Usually OK | Careful with real celebs | Fine for story beats |
| Partial / R-rated nudity (imaginary adults) | Often allowed under stated R-rated/NSFW framing | **Banned** if real likeness / nonconsensual intimate | **Only original OC adults**; age-clear designs |
| Explicit sex / hardcore pornography | Unreliable / often blocked; AUP forbids pornographic **real-person** likenesses | **Banned** | Do not build “porn mode” into the app; stay story-first |
| Horror atmosphere, monsters, dark fantasy | Usually OK | OK | Matches “Dark / Tense” tones |
| Sexual content involving minors (any style) | **Never** | **Never** | Hard stop in prompts + UI + validation |
| Undress real photos / deepfake nudes | N/A | **Never** | Do not implement image-edit flows for this |

### 3.4 Violence & gore — how far for YouTube anime stories?

**Reasonable target for this product**: **TV-MA / R-rated action anime** level.

- **Supported intent**: Battles, injuries, blood, dramatic deaths, dark system-apocalypse vibes, villain cruelty *as story*, not as snuff gallery.  
- **Prompting tip**: Cinematic language (“dramatic wound, stylized anime blood, movie still”) works better than medical/snuff detail.  
- **Expect variability**: Some extreme gore prompts will hit moderation even if a human R-rated film might show similar. Build **retry / rephrase / manual upload** paths.  
- **YouTube layer**: Even if Grok generates it, YouTube may demonetize or age-restrict graphic content. Export packaging should warn creators.

### 3.5 Nudity & sexual content — product stance for AnimeStoryVideoCreatorApp

Recommended **app-level policy** (stricter than the absolute legal floor, safer for long-term account health + YouTube):

| Level | App support |
|-------|-------------|
| Suggestive outfits, fanservice, implied intimacy | Optional (creator-controlled tone) |
| R-rated partial nudity on **original adult** characters | Optional mature flag; never default for all projects |
| Explicit sexual acts / pornographic focus | **Out of scope** for v1 product features |
| Any sexualization of minors / ambiguous-age designs | **Hard block** in validation |
| Real-person undress / celebrity porn | **Hard block** |

Character Vault should store **age presentation = adult** for any mature-tagged project, and image prompts should reinforce “adult character, 25+ appearance” (or similar clear adult framing) when mature content is enabled.

### 3.6 Consumer Grok vs API (for developers)

| | Consumer Grok (web/app/X) | Imagine API (this app) |
|--|---------------------------|-------------------------|
| UI modes (e.g. “Spicy”) | May exist / change | You send prompts; server-side moderation still applies |
| Geo-blocks | Common for sensitive image edits | Still subject to policy + law; don’t rely on “no UI = no filter” |
| Moderation signal | Blur / “Moderated” UI | `respect_moderation`, failed jobs, `invalid_argument` with moderation message |
| Key risk | Account / platform enforcement | API key billing + team enforcement |

---

## 4. Models & rough pricing (snapshot)

Re-check [docs.x.ai/developers/models](https://docs.x.ai/developers/models) before cost estimates ship in UI.

| Model | Role | Snapshot price |
|-------|------|----------------|
| `grok-4.5` / `grok-4.3` | Scripts, structure, prompt writing | Per million tokens (see models page) |
| `grok-imagine-image` | Fast stills | ~$0.02 / image |
| `grok-imagine-image-2.0` | Newer quality stills | ~$0.04 / image |
| `grok-imagine-image-quality` | Highest still quality | ~$0.05 / image |
| `grok-imagine-video` | Video | ~$0.05 / sec |
| `grok-imagine-video-1.5` | Higher-end video (1080p paths) | ~$0.08 / sec |

**Cost intuition for a 10-minute video (illustrative only)**  
If you generate ~40 panels + ~40 × 8s clips: images alone might be a few dollars; video seconds dominate (e.g. 320s × $0.05–$0.08 ≈ $16–$25) before retries. UI should show estimates and allow regenerate-one.

---

## 5. Implications for AnimeStoryVideoCreatorApp architecture

| Old assumption | Updated direction |
|----------------|-------------------|
| Stills only from Imagine; video = slideshow | Prefer **panel still → image-to-video** for motion; slideshow as cheap fallback |
| Character consistency = text description only | Use **multi-image edit + reference packs** (portrait + expressions + full body) |
| CapCut package as only “real video” path | CapCut remains safety net; **native short clips** are first-class |
| Single generate-all button | Need **async job queue**, progress, cost gates, per-panel regenerate |
| No mature-content thinking | Add **project maturity flag**, age-clear character rules, AUP-aware refusals |

Server remains the **only** place that holds `Xai:ApiKey` / env secrets. Client never sees the key.

---

## 6. Verification checklist (run when updating this doc)

1. Open [Imagine overview](https://docs.x.ai/developers/model-capabilities/imagine) — confirm modes, durations, resolutions.  
2. Open [Models & pricing](https://docs.x.ai/developers/models) — update table.  
3. Open [AUP](https://x.ai/legal/acceptable-use-policy) — note effective date; diff banned categories.  
4. Smoke-test API: one safe image, one image-to-video, one deliberately edge-case mature prompt (fictional adult only) — record pass/fail in Changelog.  
5. Note any geo-policy news that affects creators in target countries.

---

## 7. Changelog

| Date | Change | Source |
|------|--------|--------|
| 2026-08-11 | Initial living document: API capabilities (image 1K/2K, multi-ref edit, video ≤15s, 480p–1080p, edit/extend), AUP hard stops, R-rated framing, practical content matrix for the app | docs.x.ai, x.ai AUP 2026-06-26, Image 2.0 news 2026-08-07 |
| *(next)* | | |

---

## 8. Related project docs

- [Root product README / app spec](../../../README.md) — product vision & workflow  
- [Design-AnimeStoryVideoCreator.md](./Design-AnimeStoryVideoCreator.md) — implementation design (partially superseded on video approach)  
- [docs/README.md](../README.md) — docs index  

---

*This document is intentionally conservative where law and AUP are clear, and honest about gray areas (fictional R-rated violence/nudity) where policy + moderation can disagree. When in doubt for a product feature: default to safer, require adult original characters, and keep real-person intimate flows unimplemented.*
