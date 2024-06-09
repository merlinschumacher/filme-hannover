
import { EventData } from "../interfaces";

const template = document.createElement('template');

template.innerHTML = `
  <div class="day-list-element">
    <div class="day-list-element__header"></div>
    <slot></slot>
    <div class="day-list-element__footer"></div>
  </div>
`;

export default class EventListElement extends HTMLElement {

  static get observedAttributes() {
    return ['data'];
  }

  public data: EventData | undefined;


  constructor() {
    super();
  }

  connectedCallback() {
    console.log('connectedCallback');
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));
    const dotElem = shadow.querySelector('.event-list-element__dot');
    const dateElem = shadow.querySelector('.event-list-element__date');
    const titleElem = shadow.querySelector('.event-list-element__title');
    const typeElem = shadow.querySelector('.event-list-element__type');
    const languageElem = shadow.querySelector('.event-list-element__language');

    if (this.data && dateElem && titleElem && typeElem && languageElem) {
      dotElem?.classList.add(this.data.colorClass);
      dateElem.textContent = new Date(this.data.startTime).toLocaleTimeString('de-DE', { hour: '2-digit', minute: '2-digit'}); 
      titleElem.textContent = this.data.displayName;
      titleElem.setAttribute('href', this.data.url.toString());
      typeElem.textContent = this.data.type.toString(); 
      languageElem.textContent = this.data.language.toString(); 
    }
  }
}

customElements.define('event-list-element', EventListElement);