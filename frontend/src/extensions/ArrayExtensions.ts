declare global {
  interface Array<T> {
    toggleElement(element: T): void;
  }
}

Array.prototype.toggleElement = function (element: unknown): void {
  const index = this.indexOf(element);
  if (index === -1) {
    this.push(element);
  } else {
    this.splice(index, 1);
  }
};

export {};
