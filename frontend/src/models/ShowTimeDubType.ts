export enum ShowTimeDubType {
  Regular = 0,
  OriginalVersion = 1,
  Subtitled = 2,
  SubtitledEnglish = 3,
}

export function getShowTimeDubTypeAttributeString(
  showTimeDubType: ShowTimeDubType,
): string {
  switch (showTimeDubType) {
    case ShowTimeDubType.OriginalVersion:
      return 'OV';
    case ShowTimeDubType.Subtitled:
      return 'OmU';
    case ShowTimeDubType.SubtitledEnglish:
      return 'OmeU';
    default:
      return '';
  }
}

export function getShowTimeDubTypeLabelString(
  showTimeDubType: ShowTimeDubType,
): string {
  switch (showTimeDubType) {
    case ShowTimeDubType.OriginalVersion:
      return 'OV';
    case ShowTimeDubType.Subtitled:
      return 'OmU';
    case ShowTimeDubType.SubtitledEnglish:
      return 'OmeU';
    default:
      return 'Normal';
  }
}

export const allShowTimeDubTypes: ShowTimeDubType[] = [
  ShowTimeDubType.Regular,
  ShowTimeDubType.OriginalVersion,
  ShowTimeDubType.Subtitled,
  ShowTimeDubType.SubtitledEnglish,
];
