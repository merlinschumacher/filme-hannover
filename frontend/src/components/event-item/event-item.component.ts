
import html from './event-item.component.html?raw';
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
    return ['href', 'dotColor'];
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
      dotElem.style.backgroundColor = this.getAttribute('dotColor') || 'black';
    }
  }

  private static BuildSuffixString(type: string, language: string) {
    if (!type && !language) {
      return ''
    }
    let suffix = type;
    if (suffix && language) {
      suffix += ',\xa0';
    };
    if (language) {
      suffix += language;
    }
    return '(' + suffix + ')';
  }

  static BuildElement(event: EventData) {
    const timeSpan = SlotSpanFactory(event.startTime.toLocaleTimeString([], { timeStyle: 'short' }), 'time');
    const titleSpan = SlotSpanFactory(event.displayName, 'title');
    const typeString = getShowTimeTypeString(event.type);
    const languageString = getShowTimeLanguageString(event.language);
    const suffixString = EventItem.BuildSuffixString(typeString, languageString);

    const suffixSpan = SlotSpanFactory(suffixString, 'suffix');

    const item = new EventItem();
    item.setAttribute('href', event.url.toString());
    item.setAttribute('dotColor', event.color);
    item.appendChild(timeSpan);
    item.appendChild(titleSpan);
    item.appendChild(suffixSpan);
    return item;
  }
}

customElements.define('event-item', EventItem);
