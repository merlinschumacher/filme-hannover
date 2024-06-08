
const template = document.createElement('template');

template.innerHTML = `
  <div class="event-list-element">
    <span class="event-list-element__dot"></span>
    <span class="event-list-element__date"></span>
    <span class="event-list-element__title"></span> 
    <span class="event-list-element__type"></span>
    <span class="event-list-element__language"></span>
  </div>
`;

export default class EventListElement extends HTMLElement {

  static get observedAttributes() {
    return ['date', 'title', 'type', 'language'];
  }

  private readonly shadow: ShadowRoot;
  private date: Date;
  private eventTitle: string;
  private type: string;
  private language: string;
  private color: string;
  private url: URL;
  constructor(date: Date, eventTitle: string, type: string, language: string, color: string, url: URL) {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(template.content.cloneNode(true));
    this.date = date;
    this.eventTitle = eventTitle;
    this.type = type;
    this.language = language;
    this.color = color;
    this.url = url;
  }

  connectedCallback() {
    const dateElem = this.shadow.querySelector('.event-list-element__date');
    const titleElem = this.shadow.querySelector('.event-list-element__title');
    const typeElem = this.shadow.querySelector('.event-list-element__type');
    const languageElem = this.shadow.querySelector('.event-list-element__language');

    if (dateElem && titleElem && typeElem && languageElem) {
      dateElem.textContent = this.date.toString(); 
      titleElem.textContent = this.eventTitle;
      typeElem.textContent = this.type; 
      languageElem.textContent = this.language; 
    }
  }
}

customElements.define('event-list-element', EventListElement);