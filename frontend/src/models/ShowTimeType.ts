
export enum ShowTimeType {
  Regular,
  OriginalVersion,
  Subtitled
}

export function getShowTimeTypeString(showTimeType: ShowTimeType): string {
  switch (showTimeType) {
    case ShowTimeType.Regular:
      return '';
    case ShowTimeType.OriginalVersion:
      return 'OV';
    case ShowTimeType.Subtitled:
      return 'OmU';
  }
}
