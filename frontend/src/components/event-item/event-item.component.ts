
import html from './event-item.component.html?raw';
import css from './event-item.component.css?inline';
import { EventData } from '../../models/EventData';
import { getShowTimeTypeAttributeString } from '../../models/ShowTimeType';
import { getShowTimeLanguageString } from '../../models/ShowTimeLanguage';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class EventItem extends HTMLElement {
  static observedAttributes = ['href', 'color', 'time', 'title', 'suffix'];

  private shadow: ShadowRoot;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.adoptedStyleSheets = [style];
    this.shadow.appendChild(template.content.cloneNode(true));
  }

  attributeChangedCallback(name: string, oldValue: string, newValue: string) {
    if (oldValue === newValue) return;

    switch (name) {
      case 'title':
        this.shadow.updateElement('.title', el => el.textContent = newValue);
        break;
      case 'href':
        this.shadow.updateElement('.link', el => {
          el.setAttribute('href', newValue)
        });
        break;
      case 'color':
        this.shadow.updateElement('.dot', el => el.style.backgroundColor = newValue);
        break;
      case 'time': {
        const date = new Date(newValue);
        const dateString = date.toLocaleTimeString([], { timeStyle: 'short' });
        this.shadow.updateElement('.time', el => el.textContent = dateString);
        break;
      }
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
    const item = new EventItem();
    item.setAttribute('href', event.url.toString());
    item.setAttribute('color', event.color);
    item.setAttribute('time', event.startTime.toISOString());
    item.setAttribute('title', event.title);
    const typeString = getShowTimeTypeAttributeString(event.type);
    const languageString = getShowTimeLanguageString(event.language);
    const suffixString = EventItem.BuildSuffixString(typeString, languageString);
    item.setAttribute('suffix', suffixString);
    return item;
  }
}

customElements.define('event-item', EventItem);
