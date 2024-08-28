import html from './day-list.component.html?raw';
import css from './day-list.component.css?inline';
import { EventData } from "../../models/EventData";
import EventItem from '../event-item/event-item.component';
import { SlotSpanFactory } from '../component-helpers';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class DayListElement extends HTMLElement {

  static get observedAttributes() {
    return ['date'];
  }

  public EventData: EventData[] = [];

  constructor() {
    super();
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));
    shadow.adoptedStyleSheets = [style];
  }

  connectedCallback() {
    const header = this.shadowRoot?.querySelector('.header')!;
    if (header) {
      header.textContent = this.getAttribute('date') || '';
    }
  }

  static BuildElement(date: Date, events: EventData[]) {
    const isToday = new Date().toDateString() === date.toDateString();
    const isSundayOrHoliday = date.getDay() === 0;
    const dateString = date.toLocaleDateString([], { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
    const item = new DayListElement();
    item.setAttribute('date', dateString);
    item.EventData = events;
    if (isToday) {
      item.classList.add('today');
    }
    if (isSundayOrHoliday) {
      item.classList.add('sunday');
    }
    if (date.getDay() === 6) {
      item.classList.add('saturday');
    }
    let eventCumulativeDuration = 0;
    const eventElements: EventItem[] = [];
    events.forEach(element => {
      const eventItem = EventItem.BuildElement(element);
      eventItem.slot = 'body';
      const runtime = element.runtime ?? 120;
      eventCumulativeDuration += +runtime;
      eventElements.push(eventItem);
    });
    item.append(...eventElements);
    const eventHours = eventCumulativeDuration / 60;
    const footerText = `${events.length} Vorführungen, ca. ${eventHours.toFixed(0)} h`;
    const footerSpan = SlotSpanFactory(footerText, 'footer');
    footerSpan.slot = 'footer';
    item.appendChild(footerSpan);

    return item;
  }
}

customElements.define('day-list', DayListElement);
