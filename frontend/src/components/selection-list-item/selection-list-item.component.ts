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

  private handleClick = (ev: MouseEvent) => {
    ev.preventDefault();
    if (ev.target instanceof SelectionListItemElement) {
      ev.target.toggleAttribute('checked');
    }
  }

  attributeChangedCallback(name: string, oldValue: string | null, newValue: string | null) {
    if (oldValue === newValue) {
      return;
    }
    switch (name) {
      case 'label':
        this.shadow.updateElement('.text', el => el.textContent = newValue);
        break;
      case 'value': {
        this.shadow.updateElement('input', (el: HTMLElement) => {
          if (el instanceof HTMLInputElement) { el.value = newValue ?? ''; }
        });
        break;
      }
      case 'color': {
        const colorStyle = new CSSStyleSheet();
        const color = newValue ?? '#000000';
        colorStyle.insertRule(`:host { --color: ${color}; }`);
        this.shadow.adoptedStyleSheets = [style, colorStyle];
        break;
      }
      case 'checked': {
        const checked = newValue !== null;
        this.setCheckedState(checked);
        break;
      }
    }
  }

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(template.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [style];
  }

  private setCheckedState(checked: boolean) {
    const icon = checked ? CheckboxChecked : Checkbox;
    this.shadow.updateElement('.icon', el => el.innerHTML = icon);
    this.shadow.updateElement('input', (el: HTMLElement) => {
      if (el instanceof HTMLInputElement) { el.checked = checked; }
    });
  }

  connectedCallback() {
    this.setCheckedState(this.hasAttribute('checked'));
    this.addEventListener('click', this.handleClick);
  }

  disconnectedCallback() {
    this.removeEventListener('click', this.handleClick);
  }

}

customElements.define('selection-list-item', SelectionListItemElement);

