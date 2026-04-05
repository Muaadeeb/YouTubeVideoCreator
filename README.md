# YouTubeVideoCreator
Used to create Projects and Series for YouTube
Anime Story Video Creator – Finalized Application Specification
Project Owner: Timothy
Purpose: A web-based tool that allows Timothy to efficiently create high-quality original anime / comic-strip / manhwa-style YouTube story videos using only Grok Imagine for all generation tasks.
Core Vision

Solo creator tool focused on original stories (isekai, system apocalypse, village building, overpowered MC, etc.).
Heavy emphasis on dramatic narration, emotional zooms, smooth panel transitions, and cinematic camera moves — matching the reference videos provided.
Balance between fast one-click automation and deep manual control.
All visual generation (images, characters, storyboard panels, video clips, camera motions) powered exclusively by Grok Imagine.
Strong, consistent character designs across entire series.
CapCut as a reliable safety net for final polishing.

Finalized Workflow (Locked In)

New Project — Project name, YouTube Series Name, Target Video Length (Shorts / 5-10 min / 15-30 min / 30-60+ min / custom), Art Style preset.
Story Input Screen — Minimal: large central text box with exact placeholder “Write or paste your full story idea here… (Grok will turn this into a complete script with scenes)”, Project Summary panel, Tone selector (Dark, Epic, Humorous, Tense, Custom).
Outline Editor — 100% story-based. Table view + clickable expandable cards. Editable suggestions per scene. Approve → expand to full script or Accept Defaults.
Character Vault — Top-level sidebar item. Global searchable table (supports hundreds of characters). Click row → full Character Detail Page with:
Basic Info, Personality & Voice, History & Backstory, Abilities & Powers, Pathfinder 2e Game Mechanics (toggleable), Relationships, Images & Visual References, Usage & Series Assignments.
Image workflow: Generate multiple samples → Approve → One-click “Character Reference Pack” (main portrait + 3–5 expressions + full body).

Script Editor — Multi-column layout (left: scene list, center: editable narration/dialogue, right: context/AI helpers). Click-to-expand editing.
Visual Storyboard — Two-column layout:
Left: Grid of rectangular comic-style panels (key moment image + script caption).
Right: Details panel (Camera Details, Dialog/Narration, Characters Included, Timing, etc.).
Bottom: Timeline scrubber to play as slideshow.

Camera Controls / Scene Timeline — Auto-generated smart defaults + full manual editing (pan, zoom, dolly, tilt, focus, motion paths). “Approve All Defaults” button available.
Voiceover & Audio Editor — Scene list + preview player, voice style presets (per scene or global), background music & SFX suggestions.
Video Generation — Progress list, live preview of clips, regenerate individual or all, Approve/Fail Fast track, “Approve All” button.
Final Preview & Export — Full video player, side-by-side storyboard comparison, export options:
Full MP4 & Download
YouTube-ready package (title, description, thumbnail, tags)
Layered assets package
CapCut Project Package (safety net)
Direct YouTube Upload

Thumbnail Generator — Integrated before final export.

Visual Design (Locked In)

Theme: Clean, modern, dark professional video-editor style.
Background: Deep charcoal (#0F1117)
Accents: Neon cyan (#00F5FF) and electric purple (#A855F7)
Optimized for laptop/desktop use (wide layouts, spacious, easy on the eyes for long sessions).

Sidebar Navigation (Exact Order)

New Project
My Projects
Character Vault
Series Settings
Script Library
Storyboard Gallery
Analytics
Settings

Key Technical & Feature Decisions

All generation uses Grok Imagine (your Premium+ subscription).
Character consistency is super important — Reference Pack system required.
Support both fast automation and full manual editing at every major step.
Export must always include CapCut safety net package.
Version history for scripts, storyboards, and projects.
Quality gates in Full Auto mode.
Pathfinder 2e mechanics optional per character.

Last Updated: April 2026
This document is the single source of truth for all design decisions. All code must align with this spec.

Timothy, this is the complete backup of everything we decided together.
Next Action:

Copy the entire content above into a new file in your repo called APP-SPEC.md.
Commit it.

Once you confirm it’s saved, reply with:
“APP-SPEC.md saved”
Then we will immediately continue building the actual code — starting with the updated

