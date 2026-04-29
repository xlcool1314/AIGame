# Mist Mine Placeholder Art Guide

This folder contains lightweight placeholder art used by the current Godot UI.
Final art can replace these files without changing scene layout if the same paths
and approximate aspect ratios are kept.

## Directory Rules

- `art/placeholders/ui/`
  - Main menu and broad UI illustration placeholders.
  - Recommended aspect ratio: `16:9` or `2:1`.
- `art/placeholders/characters/`
  - Player character portraits.
  - Recommended aspect ratio: `1:1`.
  - Used by `data/characters.json` through `artPath`.
- `art/placeholders/enemies/`
  - Enemy portraits.
  - Recommended aspect ratio: `1:1`.
  - Used by `data/enemies.json` through `artPath`.

## Replacement Contract

- Keep the same file path, or update the related `artPath` in JSON.
- Character and enemy portraits should read clearly at `160x160`.
- Leave transparent or simple backgrounds in portraits; UI panels provide framing.
- Avoid important detail at the extreme edges. The UI uses contained scaling.
- Preferred final format: PNG or SVG. PNG should be at least `512x512` for portraits.

## Style Direction

- Thick dark outline, rounded forms, readable silhouette.
- Foggy green mine palette with orange/yellow equipment accents.
- UI panels use dark blue-green backgrounds, warm highlights, and compact spacing.
