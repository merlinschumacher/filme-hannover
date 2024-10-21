import html from './checkable-button.component.tpl';
import css from './checkable-button.component.css?inline';
import Checkbox from '@material-symbols/svg-400/outlined/circle.svg?raw';
import CheckboxChecked from '@material-symbols/svg-400/outlined/check_circle.svg?raw';

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class CheckableButtonElement extends HTMLElement {
  static get observedAttributes(): string[] {
    return ['label', 'value', 'color', 'checked'];
  }

  private shadow: ShadowRoot;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [styleSheet];
  }

  connectedCallback() {
    this.setCheckedState(this.hasAttribute('checked'));
    this.addEventListener('click', this.handleClick);
  }

  disconnectedCallback() {
    this.removeEventListener('click', this.handleClick);
  }

  attributeChangedCallback(
    name: string,
    oldValue: string | null,
    newValue: string | null,
  ) {
    if (oldValue === newValue) return;

    switch (name) {
      case 'label':
        this.shadow.updateElement(
          '.label',
          (el) => (el.textContent = newValue),
        );
        break;
      case 'value':
        this.shadow.updateElement('input', (el: HTMLElement) => {
          if (el instanceof HTMLInputElement) {
            el.value = newValue ?? '';
          }
        });
        break;
      case 'color': {
        const colorStyle = new CSSStyleSheet();
        const color = newValue ?? '#000000';
        colorStyle.insertRule(`.border { background-color: ${color}; }`);
        this.shadow.adoptedStyleSheets = [styleSheet, colorStyle];
        break;
      }
      case 'checked': {
        const checked = newValue !== null;
        this.setCheckedState(checked);
        break;
      }
    }
  }

  getValue(): number {
    return parseInt(this.getAttribute('value') ?? '-1');
  }

  setChecked(checked: boolean) {
    if (checked) {
      this.setAttribute('checked', '');
    } else {
      this.removeAttribute('checked');
    }
  }

  private handleClick = (ev: MouseEvent) => {
    ev.preventDefault();
    if (ev.target instanceof CheckableButtonElement) {
      ev.target.toggleAttribute('checked');
    }
  };

  private setCheckedState(checked: boolean) {
    const icon = checked ? CheckboxChecked : Checkbox;
    this.shadow.updateElement('.icon', (el) => (el.innerHTML = icon));
    this.shadow.updateElement('input', (el: HTMLElement) => {
      if (el instanceof HTMLInputElement) {
        el.checked = checked;
      }
    });
  }
}

customElements.define('checkable-button', CheckableButtonElement);
