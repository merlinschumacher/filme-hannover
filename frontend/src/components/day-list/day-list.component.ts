import html from './day-list.component.tpl';
import css from './day-list.component.css?inline';
import { EventData } from '../../models/EventData';
import EventItemElement from '../event-item/event-item.component';

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class DayListElement extends HTMLElement {
  static get observedAttributes() {
    return ['date', 'duration', 'eventcount'];
  }

  public EventData: EventData[] = [];
  private shadow: ShadowRoot;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [styleSheet];
  }

  attributeChangedCallback(name: string, oldValue: string, newValue: string) {
    if (oldValue === newValue) return;

    switch (name) {
      case 'date': {
        const date = new Date(newValue);
        const headerClass = this.getHeaderClass(date);
        if (headerClass) this.classList.add(headerClass);
        const dateString = date.toLocaleDateString([], {
          weekday: 'long',
          year: 'numeric',
          month: 'long',
          day: 'numeric',
        });
        // Only update if changed
        const headerEl = this.shadow.safeQuerySelector('.header');
        if (headerEl.textContent !== dateString) {
          headerEl.textContent = dateString;
        }
        break;
      }
      case 'duration':
      case 'eventcount': {
        this.updateFooterText();
        break;
      }
    }
  }

  private updateFooterText() {
    const eventCount = this.getAttribute('eventcount') ?? 0;
    const duration = this.getAttribute('duration') ?? 0;
    const eventHours = +duration / 60;
    const footerText = `${eventCount.toString()} Vorführungen, ca. ${eventHours.toFixed(0)} h`;
    const footerEl = this.shadow.safeQuerySelector('.footer');
    if (footerEl.textContent !== footerText) {
      footerEl.textContent = footerText;
    }
  }

  private getHeaderClass(date: Date): string {
    const isToday = new Date().toDateString() === date.toDateString();
    const isSundayOrHoliday = date.getDay() === 0;
    if (isToday) {
      return 'today';
    }
    if (isSundayOrHoliday) {
      return 'sunday';
    }
    if (date.getDay() === 6) {
      return 'saturday';
    }
    return '';
  }

  static BuildElement(date: Date, events: EventData[]) {
    let eventCumulativeDuration = 0;
    const fragment = document.createDocumentFragment();
    events.forEach((event) => {
      const eventItem = EventItemElement.BuildElement(event);
      eventItem.slot = 'body';
      eventCumulativeDuration += +event.runtime;
      fragment.appendChild(eventItem);
    });

    const item = new DayListElement();
    item.setAttribute('date', date.toDateString());
    item.setAttribute('eventcount', events.length.toString());
    item.setAttribute('duration', eventCumulativeDuration.toString());
    item.replaceChildren(fragment);
    return item;
  }
}

customElements.define('day-list', DayListElement);
