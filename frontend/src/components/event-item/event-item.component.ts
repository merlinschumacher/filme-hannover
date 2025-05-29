import html from './event-item.component.tpl';
import css from './event-item.component.css?inline';
import { EventData } from '../../models/EventData';
import { getShowTimeDubTypeAttributeString as getShowTimeDubTypeAttributeString } from '../../models/ShowTimeDubType';
import { getShowTimeLanguageString } from '../../models/ShowTimeLanguage';
const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class EventItemElement extends HTMLElement {
  public static observedAttributes = [
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
    this.shadow.adoptedStyleSheets = [styleSheet];
    this.shadow.appendChild(html.content.cloneNode(true));
  }

  attributeChangedCallback(name: string, oldValue: string, newValue: string) {
    if (oldValue === newValue) return;

    switch (name) {
      case 'title': {
        const titleEl = this.shadow.safeQuerySelector('.title');
        if (titleEl.textContent !== newValue) {
          titleEl.textContent = newValue;
        }
        break;
      }
      case 'href': {
        const linkEl = this.shadow.safeQuerySelector('.link');
        if (!newValue || newValue === '') {
          if (!linkEl.classList.contains('disabled')) {
            linkEl.classList.add('disabled');
          }
          linkEl.removeAttribute('href');
        } else {
          if (linkEl.classList.contains('disabled')) {
            linkEl.classList.remove('disabled');
          }
          if (linkEl.getAttribute('href') !== newValue) {
            linkEl.setAttribute('href', newValue);
          }
        }
        break;
      }
      case 'color': {
        const iconEl = this.shadow.safeQuerySelector('.icon');
        if (iconEl.style.backgroundColor !== newValue) {
          iconEl.style.backgroundColor = newValue;
        }
        break;
      }
      case 'icon': {
        const iconEl = this.shadow.safeQuerySelector('.icon');
        const className = 'cinema-icon-' + newValue;
        if (!iconEl.classList.contains(className)) {
          iconEl.classList.add(className);
        }
        break;
      }
      case 'time': {
        const date = new Date(newValue);
        const dateString = date.toLocaleTimeString([], { timeStyle: 'short' });
        const timeEl = this.shadow.safeQuerySelector('.time');
        if (timeEl.textContent !== dateString) {
          timeEl.textContent = dateString;
        }
        break;
      }
      case 'suffix': {
        const suffixEl = this.shadow.safeQuerySelector('.suffix');
        if (suffixEl.textContent !== newValue) {
          suffixEl.textContent = newValue;
        }
        break;
      }
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
    const item = new EventItemElement();
    item.setAttribute('href', event.url.toString());
    item.setAttribute('color', event.color);
    item.setAttribute('icon', event.iconClass);
    item.setAttribute('time', event.startTime.toISOString());
    item.setAttribute('title', event.title);
    const typeString = getShowTimeDubTypeAttributeString(event.dubType);
    const languageString = getShowTimeLanguageString(event.language);
    const suffixString = EventItemElement.BuildSuffixString(
      typeString,
      languageString,
    );
    item.setAttribute('suffix', suffixString);
    return item;
  }
}

customElements.define('event-item', EventItemElement);
