Anime Story Video Creator – Complete Project Reference & TODO List
Project Goal
A desktop-optimized web application that lets Timothy create full original anime / manhwa / comic-strip style YouTube story videos (isekai, system, overpowered MC, village building, etc.) using only Grok Imagine for scripts, images, storyboard panels, camera-controlled video clips, and final rendering.
Core Philosophy

Fast automation path with “Approve All Defaults” at every major step
Full manual control and editing available when desired
Extremely strong character consistency across episodes and series
CapCut as a reliable safety net for final polish
Clean, modern, dark professional video-editor theme with neon cyan and purple accents
Optimized for laptop/desktop use (wide, spacious layouts)

Finalized Sidebar Menu (Exact Order)

New Project
My Projects
Character Vault
Series Settings
Script Library
Storyboard Gallery
Analytics
Settings

Locked-in Core Workflow (11 Steps + Additions)

New Project
Project Name, YouTube Series Name, Target Video Length (Shorts, 5-10 min, 15-30 min, 30-60+ min, custom), Art Style preset.

Story Input Screen (Minimal)
Large central text box with exact placeholder: “Write or paste your full story idea here… (Grok will turn this into a complete script with scenes)”
Project Summary panel (name, series, length, art style)
Tone selector (Dark, Epic, Humorous, Tense, Custom)

Outline Editor
100% story-based (no camera or visuals yet)
Table view + clickable expandable cards for editing
Approve & Expand to Full Script or Accept Defaults

Character Vault (High Priority)
Global searchable table (supports hundreds of characters)
Rich Character Detail Page with sections: Basic Info, Personality & Voice, History, Abilities & Powers, Pathfinder 2e Game Mechanics (toggleable), Relationships, Images & References, Usage & Series Assignments
Image workflow: Generate multiple samples → Approve selected → One-click “Character Reference Pack” (main portrait + 3–5 expressions + full body)

Script Editor
Multi-column layout (left: scene list, center: editable narration/dialogue, right: AI helpers & context)
Click-to-expand editing with AI helpers

Visual Storyboard
Two-column layout:
Left: Grid of rectangular comic-style panels (key moment image + caption)
Right: Details panel (Camera hints, Dialog/Narration, Characters Included, Timing, etc.)

Bottom timeline scrubber to play as slideshow

Camera Controls / Scene Timeline
Auto-generated smart defaults + full manual editing (pan, zoom, dolly, tilt, focus, motion paths)
“Approve All Defaults” button

Voiceover & Audio Editor
Scene list + preview player
Voice style presets (per scene or global for series)
Background music & SFX suggestions

Video Generation
Progress list with live previews
Regenerate individual scenes or all
Approve/Fail Fast track
“Approve All” button

Final Preview & Export
Full video player
Side-by-side storyboard vs final video comparison
Export options: Full MP4, YouTube-ready package, Layered assets, CapCut Project Package (safety net)

Thumbnail Generator
Multiple anime-style thumbnails with editable text overlays (integrated before final export)


Additional MVP Features (Locked In)

Series Settings Hub: Global defaults for voice style, color palette, recurring music themes, end-screen templates, series branding.
Version History: Lightweight compare + rollback for scripts, storyboards, and full projects.
One-Click “Full Auto” Mode with smart quality gates (flags character inconsistency, pacing issues, etc.).
Direct YouTube Upload button (after final preview).
Analytics Dashboard: Basic YouTube stats (views, watch time, top-performing story types).

Visual Design Standards (Locked In)

Dark modern professional video-editor theme
Background: Deep charcoal #0F1117
Accents: Neon cyan #00F5FF and electric purple #A855F7
Spacious, comfortable for long editing sessions
All screens optimized for laptop/desktop (wide layouts)

Key Technical Decisions

Built with Blazor Web App (.NET 10) + Interactive Auto render mode
All image/video/camera generation uses Grok Imagine API only
Character consistency is critical — Reference Pack system required
CapCut safety net export is mandatory
Server-side calls preferred for Grok Imagine API (to protect key)

This document is the single source of truth.
All code, components, and future decisions must align with the items listed above.
Last Updated: April 5, 2026