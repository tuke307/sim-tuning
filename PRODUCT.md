# Product

<!-- impeccable:product-schema 1 -->

## Platform

adaptive

One product on two surfaces: an offline-first web PWA (Next.js) and a native iOS 18 app (SwiftUI).
Both share one token system; iOS keeps system controls, Dynamic Type and safe areas.

## Users

Hobby mechanics who tune East German two-stroke mopeds and motorcycles (Simson SR 2, KR 50,
KR 51 Schwalbe, S 50, S 51, S 70 and similar). They work in a garage or at a workbench, often in
the evening, with the phone lying on the bench or the tank, mixed or poor light and dirty hands.
Their job: understand and change their engine — port timings, compression, intake and exhaust
dimensions — and measure the effect on a dyno run.

## Product Purpose

SimTuning turns the classic two-stroke tuning workshop maths into a tool: engine geometry
(displacement, compression, port timing diagram, timing conversion degree ↔ mm), intake
resonance and carburettor sizing, expansion chamber design with a dimensioned drawing, and a dyno
that derives RPM curves from a recorded engine sound (spectrogram → RPM points → clusters) and
speed/coast-down from GPS runs. Success: a tuner can go from "my vehicle" to concrete
measurements they can take to the lathe or the file, and verify the result by measuring.

## Positioning

A pocket tuning workshop for DDR two-strokes: the formulas from the tuning literature applied to
the rider's own vehicle, plus a phone-only dyno — no roller dyno or extra hardware needed.

## Operating Context

- Used next to the engine: measuring ports with calipers, reading timings off a degree wheel,
  machining the cylinder head, building or modifying an exhaust.
- Dyno runs happen outside on a road: phone mounted on the vehicle, engine sound recorded during a
  full-throttle pull, GPS for acceleration and coast-down.
- Works offline (guest mode stores everything on the device); an account syncs across devices.
- Values are entered in workshop units (mm, mm³/cm³, °KW, U/min) and stored in base units.

## Capabilities and Constraints

- Modules: Fahrzeuge (vehicles), Motor, Einlass, Auslass, Dyno; profile/settings.
- German domain terms are kept (Motor, Einlass, Auslass, Überströmer, Vergaser, Auspuff, Krümmer,
  Diffusor, Gegenkonus, Endrohr, Steuerzeit, OT/UT); the UI is bilingual DE/EN.
- Starter vehicles from the original app are read-only templates; users duplicate them.
- Calculations are golden-tested ports of the v2 app; some v2 formulas keep known quirks.
- Audio never leaves the device.

## Brand Commitments

- Name: SimTuning, by tuke productions (Tony Meissner).
- The look must not resemble the sibling project LukiFitti (Apple-grouped grey surfaces with an
  ember/orange accent).
- No neon, gamer or "cyber racing" look: no glows, RGB gradients or HUD costumes.
- iOS stays native-feeling: system navigation and controls, Dynamic Type, safe areas.
- Diagrams and charts (timing diagram, exhaust drawing, RPM and power curves) are the hero
  content, not decoration around forms.
- One token system for web and iOS, changed together.

## Evidence on Hand

- Real demo recording of an acceleration run: `packages/common/src/__fixtures__/dyno-acceleration.wav`
  (also `apps/web/public/demo/`).
- Starter catalog of six real vehicles with engine data: `packages/common/src/catalog/index.ts`.
- Golden values from the original app: `packages/common/src/__fixtures__/v2-golden.json`.
- No user testimonials, download numbers, press or partner logos exist; do not invent any.

## Product Principles

1. Numbers you can take to the workbench: every result shows its unit and is readable at arm's
   length.
2. The drawing is the answer: show the geometry (diagram, chamber, curve), not just a table.
3. Respect the heritage: speak the language of the DDR two-stroke tuning community.
4. Works where the engine is: offline, one-handed, in poor light.

## Accessibility & Inclusion

Garage use: high contrast in both themes, touch targets ≥ 44 pt, readable numerals at a distance,
Dynamic Type on iOS, keyboard focus on web, colour never the only carrier of meaning in diagrams
(port lines also differ by label/pattern).
