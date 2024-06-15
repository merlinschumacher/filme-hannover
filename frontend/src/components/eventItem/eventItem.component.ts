
import html from './eventItem.component.html?inline';
import css from './eventItem.component.css?inline';

const template = document.createElement('template');
template.innerHTML = `<style>${css}</style>${html}`;

export default class EventItem extends HTMLElement {

  static get observedAttributes() {
    return ['href', 'color'];
  }

  constructor() {
    super();
    this.attachShadow({ mode: 'open' });
    this.shadowRoot?.appendChild(template.content.cloneNode(true));
  }

  connectedCallback() {
    const dotElem = this.shadowRoot?.querySelector('.dot') as HTMLElement;
    const titleElem = this.shadowRoot?.querySelector('.title') as HTMLElement;
    if (titleElem && dotElem) {
      dotElem.style.backgroundColor = this.getAttribute('color') || 'black';
      titleElem.setAttribute('href', this.getAttribute('href') || '#');
    }
  }
}

customElements.define('event-item', EventItem);
