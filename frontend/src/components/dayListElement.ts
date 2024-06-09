const template = document.createElement('template');

template.innerHTML = `
  <div class="day-list-element">
    <div class="day-list-element__header"></div>
    <div class="day-list-element__body"></div>
    <div class="day-list-element__footer"></div>
  </div>
`;

export default class DayListElement extends HTMLElement {

  static get observedAttributes() {
    return ['data'];
  }

  constructor() {
    super();
  }

  connectedCallback() {
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));

  }
};

customElements.define('day-list', DayListElement);