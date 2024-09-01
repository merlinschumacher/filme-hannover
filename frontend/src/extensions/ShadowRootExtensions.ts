declare global {
  interface ShadowRoot {
    updateElement(selector: string, updater: (el: HTMLElement) => void): void;
    safeQuerySelector(selector: string): HTMLElement;
  }
}

ShadowRoot.prototype.updateElement = function (selector: string, updater: (el: HTMLElement) => void): void {
  const element = this.safeQuerySelector(selector);
  updater(element);
}

ShadowRoot.prototype.safeQuerySelector = function (selector: string): HTMLElement {
  const element = this.querySelector(selector);
  if (!element) {
    throw new Error(`Element with selector ${selector} not found`);
  }
  return element as HTMLElement;
};

export { };

