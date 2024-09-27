export enum ShowTimeLanguage {
  German = 0,
  English = 1,
  Danish = 2,
  French = 3,
  Spanish = 4,
  Italian = 5,
  Russian = 6,
  Turkish = 7,
  Japanese = 8,
  Korean = 9,
  Hindi = 10,
  Polish = 11,
  Other = 254,
  Unknown = -1,
}

export function getShowTimeLanguageString(
  showTimeLanguage: ShowTimeLanguage,
): string {
  switch (showTimeLanguage) {
    case ShowTimeLanguage.English:
      return 'Englisch';
    case ShowTimeLanguage.Danish:
      return 'Dänisch';
    case ShowTimeLanguage.French:
      return 'Französisch';
    case ShowTimeLanguage.Spanish:
      return 'Spanisch';
    case ShowTimeLanguage.Italian:
      return 'Italienisch';
    case ShowTimeLanguage.Russian:
      return 'Russisch';
    case ShowTimeLanguage.Turkish:
      return 'Türkisch';
    case ShowTimeLanguage.Japanese:
      return 'Japanisch';
    case ShowTimeLanguage.Korean:
      return 'Koreanisch';
    case ShowTimeLanguage.Hindi:
      return 'Hindi';
    case ShowTimeLanguage.Polish:
      return 'Polnisch';
    case ShowTimeLanguage.Other:
      return 'Andere';
  }
  return '';
}
