# Cost control & free workflow

**Last updated:** 2026-08-12  

Paid xAI **API** usage is convenient but **not free**. Consumer Grok Imagine (subscription you may already pay for) and tools like NotebookLM can produce stills at **$0 extra API cost**. This app now defaults to **economy models + import-first** so you are not forced to burn credits.

---

## Why $5 for 17 stills + 2 short clips is too much for volume work

Approximate **API list** prices (re-check [docs.x.ai pricing](https://docs.x.ai/developers/models)):

| Item | Economy | Premium (old app defaults) |
|------|---------|----------------------------|
| Still | ~$0.02 | ~$0.05 (`image-quality`) |
| Video | ~$0.05 / sec | ~$0.08 / sec (`video-1.5`) |

Examples:

- 17 economy stills ≈ **$0.34**
- 17 premium stills ≈ **$0.85**
- 2 × 6s premium video ≈ **$0.96**
- Retries, script chat tokens, higher tiers, longer clips → can land near **several dollars**

That is fine for **final hero shots**. It is a bad default for **every panel in every episode**.

---

## Recommended workflow (cheap / free)

1. **Story Input** → generate script (small chat cost; optional later: write scripts offline).
2. **Storyboard** → **Copy missing prompts** (or **Download missing prompts** `.txt`).
3. Open free **[grok.com/imagine](https://grok.com/imagine)** / Super Grok (or NotebookLM).
4. Generate **one image per numbered `PROMPT N`**, keep file order.
5. In the app: **Import many stills** → multi-select files in the **same order** (fills empty panels only, $0).
6. Or **Import** on a single panel. Use paid **Still $** / **Video $** only for gaps / hero shots.

**Paid: missing stills** only fills panels with **no** still, and asks for **cost confirmation** first.

---

## App defaults (after cost update)

| Setting | Default |
|---------|---------|
| Quality tier | **Economy** |
| Image model | `grok-imagine-image` (~$0.02) |
| Video model | `grok-imagine-video` (~$0.05/s) |
| Video length | **5s** |
| Video resolution | **480p** |
| Confirm before spend | **On** |
| Prefer free import guidance | **On** |

Change tier / confirmations under **Settings**.

---

## What this app is still good for (even if API is expensive)

- Project / series structure, script breakdown, character vault  
- Persistence, export ZIP, prompts for free tools  
- Selective paid generation when time > money  
- Future: assembly / CapCut package without regenerating art  

---

## Continuity (quality storyboard)

Independent text-to-image calls **always drift** — new village, new extras, new wardrobe.

1. **Continuity Bible** (`/continuity`): one location + establishing plate per set.  
2. **Character Vault**: mark background people as **Extra** with a frozen locked look.  
3. Paid **Still $** uses Imagine **edit** with up to 3 refs (plate + previous frame + portrait) when Settings → Continuity lock is on.  
4. Free Imagine: use **edit / reference** with the previous still for frames 2+ of a scene. Do not start a new image from text.  
5. Script Library: set **Location**, **Cast**, and **Extras lock** on every scene.  

## Practical rules for Timothy

1. **Never** “Generate missing stills” for a full 15–30 min episode without checking the estimate.  
2. Prefer **Import** for 80%+ of panels.  
3. Use **Premium** tier only for thumbnail / key frames.  
4. Keep video short (5s) and rare until assembly pipeline + budget allow.  
5. Rotate API keys if exposed; watch xAI console usage.  
6. Do not batch-generate stills until each scene has a locked set + extras lock.  

---

## Related

- [SECRETS.md](./SECRETS.md)  
- [planning/Grok-Capabilities-And-Limits.md](./planning/Grok-Capabilities-And-Limits.md)  
