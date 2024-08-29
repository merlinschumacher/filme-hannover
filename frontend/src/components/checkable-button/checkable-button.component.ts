import html from './checkable-button.component.html?raw';
import css from './checkable-button.component.css?inline';
import Checkbox from '@material-symbols/svg-400/outlined/circle.svg?raw'
import CheckboxChecked from '@material-symbols/svg-400/outlined/check_circle.svg?raw'

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class CheckableButtonElement extends HTMLElement {

  static get observedAttributes(): string[] {
    return ['label', 'value', 'color', 'checked'];
  }

  private shadow: ShadowRoot;

  public value = '';
  public label = '';
  public checked = false;
  public color = '#000000';

  private handleClick(ev: MouseEvent) {
    ev.preventDefault();
    if (ev.target instanceof CheckableButtonElement) {
      ev.target.toggleAttribute('checked');
    }
  }

  private updateStyle() {
    const icon = this.checked ? CheckboxChecked : Checkbox;
    const iconEl = this.shadow.safeQuerySelector('.icon');
    iconEl.innerHTML = icon;
    const textEl = this.shadow.safeQuerySelector('.text');
    textEl.textContent = this.label;
    const input = this.shadow.safeQuerySelector('input') as HTMLInputElement;
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
        if (!newValue) {
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
    this.addEventListener('click', (ev) => { this.handleClick(ev) });
  }

}

customElements.define('checkable-button', CheckableButtonElement);

