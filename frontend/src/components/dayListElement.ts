const template = document.createElement('template');

template.innerHTML = `
  <div class="day-list-element">
    <div class="day-list-element__header"><slot name="header"></slot></div>
    <div class="day-list-element__body"><slot name="body"></slot></div>
    <div class="day-list-element__footer"><slot name="footer"></slot></div>
  </div>
`;

export default class DayListElement extends HTMLElement {

  static get observedAttributes() {
    return ['data'];
  }

  constructor() {
    super();
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));
  }

};

customElements.define('day-list', DayListElement);