import type { TranslationDictionary } from '../../types';

/** `einlass` module strings (DE). */
export const deEinlass: TranslationDictionary['einlass'] = {
  title: 'Einlass',
  subtitle: 'Ansaugkanal und Vergaser.',
  tabs: {
    kanal: 'Kanal',
    vergaser: 'Vergaser',
  },
  kanal: {
    section: 'Einlass-Resonanzlänge',
    flaecheA: 'Einlass-Fläche',
    steuerzeitSZ: 'Einlass-Steuerwinkel',
    resonanzU: 'Resonanzdrehzahl',
    kurbelgehaeuseV: 'Kurbelgehäusevolumen',
    resonanzL: 'Einlass-Resonanzlänge',
    hint: 'Alle Eingaben ausfüllen, um die Resonanzlänge zu berechnen.',
  },
  vergaser: {
    sectionDurchmesser: 'Vergasergröße',
    sectionHauptduese: 'Hauptdüsendurchmesser',
    hubraumV: 'Hubraum-Volumen',
    resonanzU: 'Resonanzdrehzahl',
    vergaserFaktor: 'Widerstandsfaktor (Vergaser)',
    hauptdueseFaktor: 'Widerstandsfaktor (Hauptdüse)',
    faktorHint: 'Standard {{value}}.',
    durchmesserD: 'Ø Vergaser',
    hauptdueseD: 'Ø Hauptdüse',
    hint: 'Hubraum und Resonanzdrehzahl eingeben, um den Vergaser auszulegen.',
  },
  saveHint: 'Speichert die Eingaben (und den Vergaser-Ø) im ausgewählten Fahrzeug.',
};
