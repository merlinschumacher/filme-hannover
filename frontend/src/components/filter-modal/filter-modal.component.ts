import html from './filter-modal.component.html?inline';
import css from './filter-modal.component.css?inline';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class FilterModal extends HTMLElement {

  constructor() {
    super();
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));
    shadow.adoptedStyleSheets = [style];
  }

  connectedCallback() {
  }

  disconnectedCallback() {
  }
}

customElements.define('filter-modal', FilterModal);

