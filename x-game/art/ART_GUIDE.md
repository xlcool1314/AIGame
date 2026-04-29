# Mist Mine Art Integration Guide

The current UI is structured so final art can be added without rewriting scenes.
The reference direction is a dark stone mine interface: rough black panels,
bone/iron dividers, purple crystals and status accents, and compact information
blocks.

## Runtime Entry Points

- `data/art_manifest.json`
  - Central place for UI backgrounds, frames, icons, and mine tile art.
  - Update this file when replacing UI assets with final art.
- `scripts/UiArt.cs`
  - Loads the manifest and applies scene backdrops.
  - Use this for future UI art lookups instead of hardcoding file paths.
- `scripts/MistTheme.cs`
  - Shared black-stone panel, button, card, and label styling.
  - Add new theme variants here before styling individual scenes manually.

## Final Art Folders

- `art/ui/backgrounds/`
  - Full-screen scene backdrops.
  - Recommended: `1920x1080`, dark mine composition, low contrast behind UI.
  - Suggested keys: `main_menu`, `character_select`, `run`, `battle`, `map`, `library`.
- `art/ui/frames/`
  - 9-slice or frame textures for panels, buttons, cards, route nodes, mine tiles.
  - Keep borders readable at small sizes.
- `art/ui/icons/`
  - Small status and room icons.
  - Recommended: transparent PNG/SVG, square, readable at `32x32`.
  - Suggested icons: `hp`, `shards`, `deck`, `oil`, `fog`, `score`, `objective`,
    `battle`, `elite`, `boss`, `mine`, `event`, `rest`, `shop`, `unknown`.
- `art/ui/cards/`
  - Card frame layers or full card templates.
  - Separate attack/skill/rare variants if needed.
- `art/ui/nodes/`
  - Route-map node and connection art.
  - Use separate variants for current, available, future, lost, boss, shop, rest.
- `art/characters/`
  - Final playable character portraits.
  - Update `data/characters.json` `artPath`, or keep matching placeholder paths.
- `art/enemies/`
  - Final enemy portraits.
  - Update `data/enemies.json` `artPath`, or keep matching placeholder paths.
- `art/tiles/`
  - Mine grid tiles: hidden, empty, trap, monster, treasure, ore, entrance, exit.
- `art/effects/`
  - Combat flashes, aim-line tips, card-use effects, fog overlays.

## Placeholder Folders

- `art/placeholders/ui/`
- `art/placeholders/characters/`
- `art/placeholders/enemies/`

These are lightweight stand-ins used while final art is missing. Final assets can
either replace placeholder files directly, or live in the final folders above and
be referenced from JSON/manifest.

## Replacement Contract

- Prefer updating `data/art_manifest.json` for UI art.
- Prefer updating `data/characters.json` and `data/enemies.json` for portraits.
- Character and enemy portraits should read clearly at `160x160`.
- Card art and tile art should remain legible at small sizes.
- Avoid important detail at the extreme edges; UI uses framed contained scaling.
- Preferred final format: PNG for painted art, SVG for simple scalable icons.
- PNG portrait minimum: `512x512`; background minimum: `1920x1080`.

## Visual Rules

- Background: dark rock, low saturation, high readability.
- Panels: black stone slabs, chipped corners, thin iron/bone borders.
- Primary accent: purple crystal/ember.
- Danger accent: muted red.
- Reward accent: crystal purple or aged gold.
- Text: bone-white main text, gray-brown secondary text.
- Icons should carry meaning before text: heart, crystal, card, lamp, skull,
  key, shop cart, camp/rest, event mark.
