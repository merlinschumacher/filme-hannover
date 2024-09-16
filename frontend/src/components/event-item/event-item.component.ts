import html from './event-item.component.tpl';
import css from './event-item.component.css?inline';
import cssicons from './event-item.component.icons.css?inline';
import { EventData } from '../../models/EventData';
import { getShowTimeDubTypeAttributeString as getShowTimeDubTypeAttributeString } from '../../models/ShowTimeDubType';
import { getShowTimeLanguageString } from '../../models/ShowTimeLanguage';

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);
const iconStyleSheet = new CSSStyleSheet();
iconStyleSheet.replaceSync(cssicons);

export default class EventItem extends HTMLElement {
  static observedAttributes = [
    'href',
    'color',
    'icon',
    'time',
    'title',
    'suffix',
  ];

  private shadow: ShadowRoot;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.adoptedStyleSheets = [styleSheet, iconStyleSheet];
    this.shadow.appendChild(html.content.cloneNode(true));
  }

  attributeChangedCallback(name: string, oldValue: string, newValue: string) {
    if (oldValue === newValue) return;

    switch (name) {
      case 'title':
        this.shadow.updateElement(
          '.title',
          (el) => (el.textContent = newValue),
        );
        break;
      case 'href':
        if (!newValue || newValue === '') {
          this.shadow.updateElement('.link', (el) => {
            el.classList.add('disabled');
            el.removeAttribute('href');
          });
        } else {
          this.shadow.updateElement('.link', (el) => {
            el.classList.remove('disabled');
            el.setAttribute('href', newValue);
          });
        }
        break;
      case 'color':
        this.shadow.updateElement(
          '.icon',
          (el) => (el.style.backgroundColor = newValue),
        );
        break;
      case 'icon': {
        this.shadow.updateElement('.icon', (el) => {
          el.classList.add('cinema-icon-' + newValue);
        });
        break;
      }
      case 'time': {
        const date = new Date(newValue);
        const dateString = date.toLocaleTimeString([], { timeStyle: 'short' });
        this.shadow.updateElement(
          '.time',
          (el) => (el.textContent = dateString),
        );
        break;
      }
      case 'suffix':
        this.shadow.updateElement(
          '.suffix',
          (el) => (el.textContent = newValue),
        );
        break;
    }
  }

  private static BuildSuffixString(dubType: string, language: string) {
    if (!dubType && !language) {
      return '';
    }
    let suffix = dubType;
    if (suffix && language) {
      suffix += ',\xa0';
    }
    if (language) {
      suffix += language;
    }
    return '(' + suffix + ')';
  }

  static BuildElement(event: EventData) {
    const item = new EventItem();
    item.setAttribute('href', event.url.toString());
    item.setAttribute('color', event.color);
    item.setAttribute('icon', event.iconClass);
    item.setAttribute('time', event.startTime.toISOString());
    item.setAttribute('title', event.title);
    const typeString = getShowTimeDubTypeAttributeString(event.dubType);
    const languageString = getShowTimeLanguageString(event.language);
    const suffixString = EventItem.BuildSuffixString(
      typeString,
      languageString,
    );
    item.setAttribute('suffix', suffixString);
    return item;
  }
}

customElements.define('event-item', EventItem);
