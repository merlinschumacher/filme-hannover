

const template = document.createElement('template');
template.innerHTML = `<button class="filter-button"></button>`;

export default class FilterButtonElement extends HTMLElement {
  static get observedAttributes() {
    return ['data'];
  }

  private readonly data: string;

  constructor(data: string) {
    super();
    this.data = data;
  }

  connectedCallback() {
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));
    const button = shadow.querySelector('.filter-button');
    if (button) {
      button.textContent = this.data;
    }
  }
}

customElements.define('filter-button', FilterButtonElement);