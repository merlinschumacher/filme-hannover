import html from './selection-list-item.component.html?raw';
import css from './selection-list-item.component.css?inline';
import Checkbox from '@material-symbols/svg-400/outlined/circle.svg?raw'
import CheckboxChecked from '@material-symbols/svg-400/outlined/check_circle.svg?raw'

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class SelectionListItemElement extends HTMLElement {

  static get observedAttributes(): string[] {
    return ['label', 'value', 'color', 'checked'];
  }

  private shadow: ShadowRoot;

  public value: string = '';
  public label: string = '';
  public checked: boolean = false;
  public color: string = '#000000';

  private handleClick(e: MouseEvent) {
    if (e.target instanceof SelectionListItemElement) {
      this.toggleAttribute('checked');
      e.preventDefault();
    }
  }

  private updateStyle() {
    const icon = this.checked ? CheckboxChecked : Checkbox;
    const iconEl = this.shadow?.querySelector('.icon') as HTMLElement;
    iconEl.innerHTML = icon;
    const textEl = this.shadow?.querySelector('.text') as HTMLLabelElement;
    textEl.textContent = this.label;
    const input = this.shadow?.querySelector('input') as HTMLInputElement;
    input.value = this.value;
    const colorStyle = new CSSStyleSheet();
    colorStyle.insertRule(`:host { --color: ${this.color}; }`);
    this.shadow.adoptedStyleSheets = [style, colorStyle];


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

customElements.define('selection-list-item', SelectionListItemElement);

