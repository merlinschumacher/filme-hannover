import html from './day-list-container.component.html?inline';
import css from './day-list-container.component.css?inline';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class DayListContainerElement extends HTMLElement {

  constructor() {
    super();
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));
    shadow.adoptedStyleSheets = [style];
    this.addEventListener('click', this.onClick);
  }

  onClick() {
    console.log('clicked!');
  }

  connectedCallback() {
  }

  disconnectedCallback() {
    this.removeEventListener('click', this.onClick);
  }
}

customElements.define('day-list-container', DayListContainerElement);
