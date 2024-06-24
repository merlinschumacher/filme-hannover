import html from './checkable-button.component.html?inline';
import css from './checkable-button.component.css?inline';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class CheckableButtonElement extends HTMLElement {

  static get observedAttributes(): string[] {
    return ['label', 'value', 'color', 'checked'];
  }

  private shadow: ShadowRoot;

  public value: string = '';
  public label: string = '';
  public checked: boolean = false;
  public color: string = '#000000';

  private handleClick(e: MouseEvent) {
    if (e.target instanceof CheckableButtonElement) {
      this.toggleAttribute('checked');
      e.preventDefault();
    }
  }

  private updateStyle() {
    const labelEl = this.shadow?.querySelector('label') as HTMLLabelElement;
    labelEl.textContent = this.label;
    labelEl.style.borderColor = this.color;
    const input = this.shadow?.querySelector('input') as HTMLInputElement;
    input.value = this.value;
    if (this.checked) {
      input.setAttribute('checked', '');
    } else {
      input.removeAttribute('checked');
    }
  }

  attributeChangedCallback(name: string, oldValue: string, newValue: string) {
    if (oldValue === newValue) {
      return;
    }
    switch (name) {
      case 'label':
        this.label = newValue;
        break;
      case 'value':
        this.value = newValue;
        break;
      case 'color':
        this.color = newValue;
        break;
      case 'checked':
        if (newValue === null) {
          this.checked = false;
        } else {
          this.checked = true;
        }

        break;
    }
    this.updateStyle();
  }

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(template.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [style];
  }

  connectedCallback() {
    this.updateStyle();
    this.addEventListener('click', this.handleClick);
  }

  disconnectedCallback() {
    this.removeEventListener('click', this.handleClick);
  }

}

customElements.define('checkable-button', CheckableButtonElement);

