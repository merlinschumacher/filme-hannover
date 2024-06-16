
import html from './event-item.component.html?inline';
import css from './event-item.component.css?inline';
import { EventData } from '../../models/EventData';
import { SlotSpanFactory } from '../component-helpers';
import { getShowTimeTypeString } from '../../models/ShowTimeType';
import { getShowTimeLanguageString } from '../../models/ShowTimeLanguage';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class EventItem extends HTMLElement {

  static get observedAttributes() {
    return ['href', 'color'];
  }

  constructor() {
    super();
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.adoptedStyleSheets = [style];
    shadow.appendChild(template.content.cloneNode(true));
  }

  connectedCallback() {
    const dotElem = this.shadowRoot?.querySelector('.dot') as HTMLElement;
    const titleElem = this.shadowRoot?.querySelector('.title') as HTMLElement;
    if (titleElem && dotElem) {
      dotElem.style.backgroundColor = this.getAttribute('color') || 'black';
      titleElem.setAttribute('href', this.getAttribute('href') || '#');
    }
  }

  static BuildElement(event: EventData) {
    const item = new EventItem();

    const timeSpan = SlotSpanFactory(new Date(event.startTime).toLocaleTimeString([],{timeStyle: 'short'}), 'time');
    const titleSpan = SlotSpanFactory(event.displayName, 'title');
    const typeString = getShowTimeTypeString(event.type);
    const typeSpan = SlotSpanFactory(typeString, 'type');
    const languageString = getShowTimeLanguageString(event.language);
    const languageSpan = SlotSpanFactory(languageString, 'language');

    item.appendChild(timeSpan);
    item.appendChild(titleSpan);
    item.appendChild(typeSpan);
    item.appendChild(languageSpan);
    item.setAttribute('href', event.url.toString());
    item.classList.add(event.colorClass);
    return item;
  }
}

customElements.define('event-item', EventItem);
