
import html from './event-item.component.html?inline';
import css from './event-item.component.css?inline';
import { EventData } from '../../models/EventData';
import { SlotSpanFactory } from '../component-helpers';
import { getShowTimeTypeString } from '../../models/ShowTimeType';
import { getShowTimeLanguageString } from '../../models/ShowTimeLanguage';
import { db } from '../../models/CinemaDb';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

db.getAllCinemas().then(cinemas => {
  cinemas.forEach(cinema => {
    style.insertRule(
      `.cinema-${cinema.id} { background-color: ${cinema.color}; }`
    );
  });
});

export default class EventItem extends HTMLElement {

  static get observedAttributes() {
    return ['href', 'dotClass'];
  }

  constructor() {
    super();
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.adoptedStyleSheets = [style];
    shadow.appendChild(template.content.cloneNode(true));
  }

  connectedCallback() {
    const titleElem = this.shadowRoot?.querySelector('.title') as HTMLElement;
    const dotElem = this.shadowRoot?.querySelector('.dot') as HTMLElement;
    if (titleElem && dotElem) {
      titleElem.setAttribute('href', this.getAttribute('href') || '#');
      dotElem.classList.add(this.getAttribute('dotClass') || '');
    }
  }

  static BuildElement(event: EventData) {
    const timeSpan = SlotSpanFactory(new Date(event.startTime).toLocaleTimeString([], { timeStyle: 'short' }), 'time');
    const titleSpan = SlotSpanFactory(event.displayName, 'title');
    const typeString = getShowTimeTypeString(event.type);
    const typeSpan = SlotSpanFactory(typeString, 'type');
    const languageString = getShowTimeLanguageString(event.language);
    const languageSpan = SlotSpanFactory(languageString, 'language');
    const item = new EventItem();
    item.setAttribute('href', event.url.toString());
    item.setAttribute('dotClass', event.colorClass);
    item.appendChild(timeSpan);
    item.appendChild(titleSpan);
    item.appendChild(typeSpan);
    item.appendChild(languageSpan);
    return item;
  }
}

customElements.define('event-item', EventItem);
