export enum ShowTimeLanguage {
  German,
  English,
  Danish,
  French,
  Spanish,
  Italian,
  Russian,
  Turkish,
  Japanese,
  Korean,
  Hindi,
  Other,
  Unknown,
}

export function getShowTimeLanguageString(
  showTimeLanguage: ShowTimeLanguage,
): string {
  switch (showTimeLanguage) {
    case ShowTimeLanguage.English:
      return "Englisch";
    case ShowTimeLanguage.Danish:
      return "Dänisch";
    case ShowTimeLanguage.French:
      return "Französisch";
    case ShowTimeLanguage.Spanish:
      return "Spanisch";
    case ShowTimeLanguage.Italian:
      return "Italienisch";
    case ShowTimeLanguage.Russian:
      return "Russisch";
    case ShowTimeLanguage.Turkish:
      return "Türkisch";
    case ShowTimeLanguage.Japanese:
      return "Japanisch";
    case ShowTimeLanguage.Korean:
      return "Koreanisch";
    case ShowTimeLanguage.Hindi:
      return "Hindi";
    case ShowTimeLanguage.Other:
      return "Andere";
  }
  return "";
}
