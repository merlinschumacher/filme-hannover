import html from './empty-day.component.tpl';
import css from './empty-day.component.css?inline';

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class EmptyDayElement extends HTMLElement {
  connectedCallback() {
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(html.content.cloneNode(true));
    shadow.adoptedStyleSheets = [styleSheet];
  }
}

customElements.define('empty-day', EmptyDayElement);
