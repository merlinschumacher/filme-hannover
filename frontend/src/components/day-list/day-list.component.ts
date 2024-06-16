import html from './day-list.component.html?inline';
import css from './day-list.component.css?inline';
import { EventData } from "../../models/EventData";
import EventItem from '../event-item/event-item.component';

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
    const header = this.shadowRoot?.querySelector('.header') as HTMLElement;
    if (header) {
      header.textContent = this.getAttribute('date') || '';
    }
   this.EventData.forEach(element => {
      const eventItem = EventItem.BuildElement(element);
      this.shadowRoot?.querySelector('.body')?.appendChild(eventItem);
    });
  }
}

customElements.define('day-list', DayListElement);
