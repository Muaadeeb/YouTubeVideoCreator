# Video submission agenda (frame / scene pipeline)

**Purpose:** After a project has many stills (e.g. Project #3), use this checklist to **QC**, then **submit only approved frames** for video — without re-spending on bad art.

**Timing model:** Scene = **6–15s** window · Frames ≈ every **3–5s** inside that window.

**App tools:**
- **Script Library** (`/scripts`) — structure & timing  
- **Storyboard Gallery** (`/storyboards`) — import / free prompts / paid stills  
- **Frame QC Review** (`/review`) — approve/reject, notes, video queue  

---

## Phase 0 — Open the right project

| # | Action | Done |
|---|--------|------|
| 0.1 | **My Projects** → open Project #3 (or current working project) | ☐ |
| 0.2 | Confirm **Script Library** shows expected scene count & est. runtime | ☐ |
| 0.3 | **Normalize timing** if any scenes still look like old 30s slabs | ☐ |
| 0.4 | **Storyboard**: note missing stills vs filled frames | ☐ |

---

## Phase 1 — Inventory (no API spend)

| # | Action | Done |
|---|--------|------|
| 1.1 | Open **Frame QC Review** → read the summary bar (missing / pending / approved / rejected) | ☐ |
| 1.2 | List frames **without stills** → free path: Copy missing prompts → Imagine → Import many | ☐ |
| 1.3 | List frames **with stills but Pending** → these need QC before video | ☐ |
| 1.4 | Export ZIP backup of current project (safety) | ☐ |

---

## Phase 2 — Quality control pass (Frame QC Review)

Work **scene-by-scene** or **pending-only** filter.

### Per frame checklist

| Check | Pass? |
|-------|--------|
| Correct **scene beat** (matches title/description) | ☐ |
| **Character** consistent with vault / prior frames | ☐ |
| **Composition** readable at 16:9 YouTube | ☐ |
| **Text/UI** (system windows) legible if story-critical | ☐ |
| No obvious **artifact / wrong age / glitch** | ☐ |
| **Caption/VO** line fits a 3–5s beat | ☐ |
| **Motion prompt** makes sense for I2V (optional edit) | ☐ |

### Review actions in app

| Action | Meaning |
|--------|---------|
| **Approve** | Still is production-ready → auto **queue for video** |
| **Needs fix** | Keep still but do not queue; leave notes |
| **Reject** | Do not use for video; re-import or re-gen later |
| **Skip / Pending** | Come back later |

| # | Action | Done |
|---|--------|------|
| 2.1 | Filter: **Pending + has still** | ☐ |
| 2.2 | Walk every frame: Approve / Needs fix / Reject + notes | ☐ |
| 2.3 | Filter: **Rejected / Needs fix** → re-import free or selective paid Still $ | ☐ |
| 2.4 | Re-review fixed frames → Approve | ☐ |
| 2.5 | Optional: mark **scene** Approved when all its frames are Approved | ☐ |

**Exit criteria:** `Missing stills = 0` (or accepted) **and** `Pending review ≈ 0` for frames you care about for v1 video.

---

## Phase 3 — Build the video queue

| # | Action | Done |
|---|--------|------|
| 3.1 | In QC Review, confirm **Queued for video** only on **Approved** frames | ☐ |
| 3.2 | Prefer **one scene at a time** for first paid batch (cost control) | ☐ |
| 3.3 | Copy **motion prompt pack** for free tools if testing motion outside API | ☐ |
| 3.4 | Settings: **Economy** tier, short default video length (5s), confirm-before-spend ON | ☐ |

### What to submit for video (rules)

1. **Only Approved** frames with a still.  
2. **One frame → one short clip** (≈ frame interval 3–5s, or Settings default 5s).  
3. **Do not** auto-video Rejected / Needs fix / Pending.  
4. **Do not** re-video frames that already have a clip unless you intentionally re-queue.  
5. Batch by **scene** first (easier QC of motion continuity).

---

## Phase 4 — Generate / obtain video clips

### Path A — Free / Super Grok (preferred for volume)

| # | Action | Done |
|---|--------|------|
| 4A.1 | For each queued frame: use still + motion prompt in free Imagine video if available | ☐ |
| 4A.2 | Download clips; store offline; re-import when app supports video import (or keep CapCut) | ☐ |

### Path B — Paid API (selective)

| # | Action | Done |
|---|--------|------|
| 4B.1 | Storyboard or future “Video queue” batch: **only queued frames** | ☐ |
| 4B.2 | Confirm $ estimate before batch | ☐ |
| 4B.3 | After each scene batch: spot-check motion; un-queue failures | ☐ |

---

## Phase 5 — Assembly (post-clip)

| # | Action | Done |
|---|--------|------|
| 5.1 | Export ZIP (stills + script + video URLs/files) | ☐ |
| 5.2 | CapCut / FFmpeg: order clips by scene order → frame order | ☐ |
| 5.3 | VO / captions from Script Library dialogue | ☐ |
| 5.4 | Thumbnail from one **Approved** hero frame | ☐ |
| 5.5 | YouTube package: title, description, AI disclosure | ☐ |

---

## Suggested order for a large Project #3

1. **QC all stills** (Phase 2) — zero or low spend.  
2. **Approve a pilot scene** (3–5 frames) → video only that scene.  
3. If pilot looks good → next scenes in batches of 1–2.  
4. Never “video everything” until QC dashboard shows clean **Approved** counts.

---

## Status legend (for your notes)

| Tag | Meaning |
|-----|---------|
| ☐ | Not started |
| ◐ | In progress |
| ☑ | Done |

Update this file or use **Frame QC Review** counters as the live dashboard.
