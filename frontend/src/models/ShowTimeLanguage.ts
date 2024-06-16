
export enum ShowTimeLanguage {
  Danish,
  German,
  English,
  French,
  Spanish,
  Italian,
  Georgian,
  Russian,
  Turkish,
  Mayalam,
  Japanese,
  Miscellaneous,
  Other
}

export function getShowTimeLanguageString(showTimeLanguage: ShowTimeLanguage): string {
  switch (showTimeLanguage) {
    case ShowTimeLanguage.Danish:
      return 'Dänisch';
    case ShowTimeLanguage.German:
      return '';
    case ShowTimeLanguage.English:
      return 'Englisch';
    case ShowTimeLanguage.French:
      return 'Französisch';
    case ShowTimeLanguage.Spanish:
      return 'Spanisch';
    case ShowTimeLanguage.Italian:
      return 'Italienisch';
    case ShowTimeLanguage.Georgian:
      return 'Georgisch';
    case ShowTimeLanguage.Russian:
      return 'Russisch';
    case ShowTimeLanguage.Turkish:
      return 'Türkisch';
    case ShowTimeLanguage.Mayalam:
      return 'Malayalam';
    case ShowTimeLanguage.Japanese:
      return 'Japanisch';
    case ShowTimeLanguage.Miscellaneous:
      return 'Sonstige';
    case ShowTimeLanguage.Other:
      return 'Andere';
  }

}
