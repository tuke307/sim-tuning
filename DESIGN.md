# Design — „Werkstattzeichnung"

Every screen reads like a sheet from a Simson repair manual drawn on a drafting table at night.
The drawings (timing diagram, exhaust chamber, curves) carry the identity; forms stay quiet.
Mode: **Operate** — navigation and controls stay standard (web components, native iOS controls).
The world lends only palette, type, density and one signature move.

## Scene → theme

Garage / workbench, evening, mixed light, phone on the bench. **Dark is the primary theme**
(default for new users), light ("Lichtpause", a diazo blueprint copy on cream paper) is equally
complete. Both reach ≥ 4.5:1 for all text and meaningful lines.

## Colour tokens

| Token                                  | Role                                               | Dark                                        | Light                                    |
| -------------------------------------- | -------------------------------------------------- | ------------------------------------------- | ---------------------------------------- |
| `--background`                         | drafting table / paper                             | `#111614`                                   | `#ECE7DA`                                |
| `--card`                               | sheet                                              | `#19201D`                                   | `#F7F4EC`                                |
| `--raised` / `--secondary` / `--muted` | raised sheet, inputs                               | `#222B27`                                   | `#E3DDCD`                                |
| `--border`                             | hairline (1px, no shadows)                         | `#33403A`                                   | `#C9C2B0`                                |
| `--foreground`                         | ink                                                | `#E9E5D8` (14.5:1)                          | `#1B2333` (12.7:1)                       |
| `--muted-foreground`                   | secondary ink                                      | `#A3AB9F` (7.7:1)                           | `#545C6B` (5.5:1)                        |
| `--primary`                            | signal: selection, dimension lines, primary action | `#F2B92C` Signalgelb (10.2:1), fg `#1A1406` | `#1E4F9C` Pausblau (6.4:1), fg `#FFFFFF` |
| `--mod-motor`                          | Motor module, crank/piston                         | `#CDA977` Saharabraun (8.3:1)               | `#7A5A2C` (5.1:1)                        |
| `--mod-einlass`                        | Einlass module & intake port lines                 | `#5AA7D6` Lagunenblau (6.9:1)               | `#1B6797` (5.0:1)                        |
| `--mod-auslass`                        | Auslass module & exhaust port lines                | `#E66A52` Ibisrot (5.7:1)                   | `#A8361F` (5.3:1)                        |
| `--mod-ueberstroemer`                  | transfer port lines                                | `#6DBB8A` Billardgrün (7.9:1)               | `#2A6E46` (5.0:1)                        |
| `--mod-dyno`                           | Dyno module, curves                                | = signal `#F2B92C`                          | `#1E4F9C`                                |
| `--mod-vehicles`                       | Fahrzeuge module                                   | = ink                                       | = ink                                    |
| `--destructive`                        | delete / error                                     | `#F07A6A` (6.7:1)                           | `#A8361F`                                |
| `--success` / `--warning`              | states only                                        | `#6DBB8A` / `#F2B92C`                       | `#2A6E46` / `#7A5A2C`                    |
| `--grid`                               | 5 mm drawing grid (diagrams only)                  | ink @ 6 %                                   | ink @ 7 %                                |

Module colours are named after classic Simson enamel colours and are the module's section colour
everywhere (tab indicator, title-block rule, port lines in drawings). Never the only carrier of
meaning: diagram lines also differ by label and dash pattern (Einlass solid, Auslass long dash,
Überströmer dot-dash). Charts: series 1 signal, 2 Lagunenblau, 3 Ibisrot, 4 Billardgrün, 5 Saharabraun.

## Type (system fonts only)

- **Text & headings** — condensed technical sans from the system, like DIN 1451 lettering:
  `"Bahnschrift", "DIN Alternate", "SF Pro Display", system-ui, sans-serif` with
  `font-stretch: condensed` where supported (Bahnschrift on Windows, SF on Apple); body stays at
  normal width for readability: `system-ui, -apple-system, "Segoe UI", Roboto, sans-serif`.
- **Numbers & units** — `ui-monospace, "SF Mono", "Cascadia Mono", "Roboto Mono", Menlo, monospace`,
  `font-variant-numeric: tabular-nums slashed-zero`. Values never jump when they change.
- Units and labels of results in small caps / uppercase tracking `0.06em`, size one step smaller.
- Scale (rem): 0.75 caption · 0.8125 label · 0.9375 body · 1.125 section · 1.5 / 2 title.
  Headings uppercase, condensed, tracking `0.04em`. Body ≥ 16 px on mobile inputs.
- iOS: SF Pro with `.fontWidth(.condensed)` for headings, `.monospacedDigit()` / SF Mono for values,
  Dynamic Type everywhere.

## Shape & material

- Hairline borders, **no drop shadows**, radius 4px (inputs, cards, buttons), 2px for tags.
  Sheets look cut from paper, not floating.
- Diagram areas sit on the 5 mm grid (`.drawing-grid`), forms never do.
- Focus: 2px signal-colour outline, 2px offset.

## Signature move

1. **Schriftfeld (title block)** opens every module: a ruled box at the top with
   `Blatt n · Modulname` (sheet number per module: 1 Fahrzeuge, 2 Motor, 3 Einlass, 4 Auslass,
   5 Dyno), the vehicle selector, engine name, unit system and date; its top rule is the module
   colour. Collapses to one line below 480 px.
2. **Positionsnummern (callouts)**: results are numbered circled callouts ①②③ (monospace numeral in
   a 1px circle) placed on the drawing at the element they describe and repeated as the first column
   of a **Stückliste** (parts-list table: Pos · Benennung · Wert · Einheit). Hover/focus on either
   side highlights both.
3. **Maßlinien (dimension lines)**: computed lengths/angles are drawn as dimension lines with arrow
   heads in signal colour directly in the drawings.

Borrowed disciplines: hairline seams + isolated destructive actions (developer console), fixed digit
cells (split-flap board), one section hue per module (tabbed manual).

## Layout

Standard shell: header with brand + profile, bottom tab bar on phones, top/side navigation on
desktop. Tabs show the module colour as a 2px indicator. Content max-width 64rem. Module page =
Schriftfeld → sub-tabs → drawing (when the module has one) → inputs → Stückliste.

```
compact (375)                    regular (≥ 1024)
┌───────────────────────────┐    ┌──────────────────────────────────────────────┐
│ SIMTUNING          (◯)    │    │ SIMTUNING  Fahrz Motor Einl Ausl Dyno  (◯)   │
├───────────────────────────┤    ├──────────────────────────────────────────────┤
│ BLATT 4 · AUSLASS  S51 ▾  │    │ BLATT 4 · AUSLASS │ S51 B1-4 ▾ │ M 541 │ mm │
│ [Theorie][Anwendung]      │    │ [Theorie] [Anwendung]                        │
│ ┌ grid ────────────────┐  │    │ ┌ grid drawing ───────────┐ ┌ inputs ──────┐ │
│ │ chamber ①②③ ⟷ dims   │  │    │ │ chamber ①②③④⑤ + dims    │ │              │ │
│ └──────────────────────┘  │    │ └─────────────────────────┘ └──────────────┘ │
│ inputs                    │    │ STÜCKLISTE Pos Benennung Wert Einheit        │
│ STÜCKLISTE                │    └──────────────────────────────────────────────┘
├───────────────────────────┤
│ Fz  Mot  Ein  Aus  Dyno   │
└───────────────────────────┘
```

## Motion

One moment: when a result appears, its dimension line draws in (stroke-dashoffset, 200 ms,
ease-out). Everything else is instant. `prefers-reduced-motion` → no animation.

## Don'ts

No glows, neon, gradients, glassmorphism or blur; no LukiFitti grouped-grey/ember look; no
decorative gauges or HUD bezels; no ruled-paper costume behind forms; no emoji.
