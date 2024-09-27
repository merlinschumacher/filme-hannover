import html from './loader.component.tpl';
import css from './loader.component.css?inline';

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class Loader extends HTMLElement {
  private shadow: ShadowRoot;
  static get observedAttributes() {
    return ['visible'];
  }
  private loaderEl: HTMLElement;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [styleSheet];
    this.loaderEl = this.shadow.safeQuerySelector('#loader');
    this.setAttribute('visible', 'true');
  }
  attributeChangedCallback(name: string, _: string, newValue: string) {
    switch (name) {
      case 'visible': {
        if (newValue) {
          this.loaderEl.classList.remove('hidden');
        } else {
          this.loaderEl.classList.add('hidden');
        }
        break;
      }
    }
  }
}

customElements.define('loading-spinner', Loader);
