export enum ShowTimeDubType {
  Regular = 0,
  OriginalVersion = 1,
  Subtitled = 2,
}

export function getShowTimeDubTypeAttributeString(
  showTimeDubType: ShowTimeDubType,
): string {
  switch (showTimeDubType) {
    case ShowTimeDubType.OriginalVersion:
      return "OV";
    case ShowTimeDubType.Subtitled:
      return "OmU";
    default:
      return "";
  }
}

export function getShowTimeDubTypeLabelString(
  showTimeDubType: ShowTimeDubType,
): string {
  switch (showTimeDubType) {
    case ShowTimeDubType.Regular:
      return "Normal";
    case ShowTimeDubType.OriginalVersion:
      return "OV";
    case ShowTimeDubType.Subtitled:
      return "OmU";
  }
}

export function getAllShowTimeDubTypes() {
  return [
    ShowTimeDubType.Regular,
    ShowTimeDubType.OriginalVersion,
    ShowTimeDubType.Subtitled,
  ];
}

export function getShowTimeDubTypeByNumber(value: number): ShowTimeDubType {
  switch (value) {
    case 1:
      return ShowTimeDubType.OriginalVersion;
    case 2:
      return ShowTimeDubType.Subtitled;
    default:
      return ShowTimeDubType.Regular;
  }
}
