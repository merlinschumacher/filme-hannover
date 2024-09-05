export default class ViewPortService {
  public viewPortChanged?: () => void;

  private readonly daySizeMap = new Map<number, number>([
    [400, 1],
    [600, 2],
    [800, 3],
    [1000, 4],
    [1200, 5],
  ]);

  public getVisibleDays(): number {
    const width = window.innerWidth;
    this.daySizeMap.forEach((value, key) => {
      if (width <= key) {
        return value;
      }
    });
    return 4;
  }
}
