import html from './checkable-button.component.html?raw';
import css from './checkable-button.component.css?inline';
import Checkbox from '@material-symbols/svg-400/outlined/circle.svg?raw';
import CheckboxChecked from '@material-symbols/svg-400/outlined/check_circle.svg?raw';

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
  public color = '#000000';

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(template.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [style];
  }

  connectedCallback() {
    this.addEventListener('click', this.handleClick);
  }

  disconnectedCallback() {
    this.removeEventListener('click', this.handleClick);
  }

  attributeChangedCallback(name: string, oldValue: string, newValue: string) {
    if (oldValue === newValue) return;

    switch (name) {
      case 'label':
        this.shadow.updateElement('.label', el => el.textContent = this.label);
        break;
      case 'value':
        this.value = newValue;
        this.shadow.updateElement('input', (el: HTMLElement) => {
          if (el instanceof HTMLInputElement) { el.value = this.value; }
        });
        break;
      case 'color': {
        const colorStyle = new CSSStyleSheet();
        colorStyle.insertRule(`:host { --color: ${this.color}; }`);
        this.shadow.adoptedStyleSheets = [style, colorStyle];
        break;
      }
      case 'checked': {
        const checked = Boolean(newValue);
        const icon = checked ? CheckboxChecked : Checkbox;
        this.shadow.updateElement('.icon', el => el.innerHTML = icon);
        this.shadow.updateElement('input', (el: HTMLElement) => {
          if (el instanceof HTMLInputElement) { el.checked = checked; }
        });
        break;
      }
    }
  }

  private handleClick = (ev: MouseEvent) => {
    ev.preventDefault();
    if (ev.target instanceof CheckableButtonElement) {
      ev.target.toggleAttribute('checked');
    }
  }

}

customElements.define('checkable-button', CheckableButtonElement);
