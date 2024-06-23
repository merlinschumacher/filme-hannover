import html from './checkable-button.component.html?inline';
import css from './checkable-button.component.css?inline';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class CheckableButtonElement extends HTMLElement {

  static get observedAttributes() {
    return ['label', 'value', 'color', 'checked'];
  }

  public value: string = '';
  public label: string = '';
  public checked: boolean = false;
  public color: string = '#000000';

  private updateStyle() {
    const labelEl = this.shadowRoot?.querySelector('label') as HTMLLabelElement;
    labelEl.textContent = this.label;
    labelEl.style.borderColor = this.color;
    const input = this.shadowRoot?.querySelector('input') as HTMLInputElement;
    input.setAttribute('value', this.value);
    if (this.checked) {
      input.setAttribute('checked', '');
    } else {
      input.removeAttribute('checked');
    }
  }

  attributeChangedCallback(name: string, oldValue: string, newValue: string) {
    switch (name) {
      case 'label':
          this.label = newValue || '';
        break;
      case 'value':
        this.value = newValue || '';
        break;
      case 'color':
        this.color = newValue || '';
        break;
      case 'checked':
        this.checked = newValue !== null;
        break;
    }
    this.updateStyle();
  }

  constructor() {
    super();
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));
    shadow.adoptedStyleSheets = [style];
  }

  connectedCallback() {
    this.updateStyle();
    const input = this.shadowRoot?.querySelector('input') as HTMLInputElement;
    if (this.checked) {
      input.setAttribute('checked', '');
    }
    input.addEventListener('change', (e) => {
      if (e.target instanceof HTMLInputElement && !e.target.checked) {
        this.removeAttribute('checked');
      } else {
        this.setAttribute('checked', '');
      }
    });
  }

}

customElements.define('checkable-button', CheckableButtonElement);

