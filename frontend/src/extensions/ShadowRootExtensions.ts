// shadow-root-extensions.ts
declare global {
  interface ShadowRoot {
    safeQuerySelector(selector: string): HTMLElement;
  }
}

ShadowRoot.prototype.safeQuerySelector = function (selector: string): HTMLElement {
  const element = this.querySelector(selector);
  if (!element) {
    throw new Error(`Element with selector ${selector} not found`);
  }
  return element as HTMLElement;
};

export { };
