/** `einlass` module strings (EN). Owned by the einlass module; keep keys in sync with de/einlass.ts. */
export const enEinlass = {
  title: 'Intake',
  subtitle: 'Intake tract and carburettor.',
  tabs: {
    kanal: 'Intake tract',
    vergaser: 'Carburettor',
  },
  kanal: {
    section: 'Intake resonance length',
    flaecheA: 'Intake port area',
    steuerzeitSZ: 'Intake timing',
    resonanzU: 'Resonance rpm',
    kurbelgehaeuseV: 'Crankcase volume',
    durchmesserD: 'Intake pipe diameter',
    resonanzL: 'Intake resonance length',
    hint: 'Fill in all inputs to calculate the resonance length.',
  },
  vergaser: {
    sectionDurchmesser: 'Carburettor size',
    sectionHauptduese: 'Main jet',
    hubraumV: 'Displacement',
    resonanzU: 'Resonance rpm',
    vergaserFaktor: 'Resistance factor (bore)',
    hauptdueseFaktor: 'Resistance factor (main jet)',
    faktorHint: 'Default {{value}}.',
    durchmesserD: 'Carburettor bore',
    hauptdueseD: 'Main jet diameter',
    hint: 'Enter displacement and resonance rpm to size the carburettor.',
  },
  saveHint: 'Saves the inputs (and the carburettor bore) to the selected vehicle.',
};
