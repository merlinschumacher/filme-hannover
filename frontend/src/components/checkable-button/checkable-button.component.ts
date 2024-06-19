import html from './checkable-button.component.html?inline';
import css from './checkable-button.component.css?inline';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class CheckableButtonElement extends HTMLElement {

  static get observedAttributes() {
    return ['label', 'value', 'color'];
  }

  constructor() {
    super();
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));
    shadow.adoptedStyleSheets = [style];
  }

  connectedCallback() {
    const input = this.shadowRoot?.querySelector('label') as HTMLElement;
      input.textContent = this.getAttribute('label') || '';
      input.style.borderColor = this.getAttribute('color') || '';
  }

}

customElements.define('checkable-button', CheckableButtonElement);

