export enum ShowTimeType {
  Regular = 0,
  OriginalVersion = 1,
  Subtitled = 2,
}

export function getShowTimeTypeAttributeString(
  showTimeType: ShowTimeType,
): string {
  switch (showTimeType) {
    case ShowTimeType.OriginalVersion:
      return "OV";
    case ShowTimeType.Subtitled:
      return "OmU";
    default:
      return "";
  }
}

export function getShowTimeTypeLabelString(showTimeType: ShowTimeType): string {
  switch (showTimeType) {
    case ShowTimeType.Regular:
      return "Normal";
    case ShowTimeType.OriginalVersion:
      return "OV";
    case ShowTimeType.Subtitled:
      return "OmU";
  }
}

export function getAllShowTimeTypes() {
  return [
    ShowTimeType.Regular,
    ShowTimeType.OriginalVersion,
    ShowTimeType.Subtitled,
  ];
}

export function getShowTimeTypeByNumber(value: number): ShowTimeType {
  switch (value) {
    case 1:
      return ShowTimeType.OriginalVersion;
    case 2:
      return ShowTimeType.Subtitled;
    default:
      return ShowTimeType.Regular;
  }
}
