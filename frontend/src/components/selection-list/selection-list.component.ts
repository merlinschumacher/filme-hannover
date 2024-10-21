import html from './selection-list.component.tpl';
import css from './selection-list.component.css?inline';
import SelectionListItemElement from '../selection-list-item/selection-list-item.component';

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class SelectionListElement extends HTMLElement {
  private selectedItemIds: number[] = [];
  public onSelectionChanged?: (movies: number[]) => void;

  private shadow: ShadowRoot;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [styleSheet];
  }

  connectedCallback() {
    const searchInput = this.shadow.safeQuerySelector(
      'input',
    ) as HTMLInputElement;
    searchInput.addEventListener('input', this.inputHandler);
    this.addEventListener('click', this.clickHandler);
  }

  disconnectedCallback() {
    const searchInput = this.shadow.safeQuerySelector(
      'input',
    ) as HTMLInputElement;
    searchInput.removeEventListener('input', this.inputHandler);
    this.removeEventListener('click', this.clickHandler);
  }

  public setSelections(ids: number[]) {
    this.selectedItemIds = ids;
  }

  private inputHandler = (event: Event) => {
    const searchTerm = (event.target as HTMLInputElement).value;
    const options = this.querySelectorAll('selection-list-item');
    options.forEach((option: Element) => {
      const optionElement = option as SelectionListItemElement;
      const label = optionElement.getAttribute('label') ?? '';
      if (label.toLowerCase().includes(searchTerm.toLowerCase())) {
        optionElement.style.display = 'block';
      } else {
        optionElement.style.display = 'none';
      }
    });
  };

  private clickHandler = (event: MouseEvent) => {
    if (!(event.target instanceof SelectionListItemElement)) return;
    const movieId = parseInt(event.target.getAttribute('value') ?? '0');
    if (this.selectedItemIds.includes(movieId)) {
      this.selectedItemIds = this.selectedItemIds.filter(
        (id) => id !== movieId,
      );
    } else {
      this.selectedItemIds.push(movieId);
    }
    if (this.onSelectionChanged) {
      this.onSelectionChanged(this.selectedItemIds);
    }
  };
}

customElements.define('selection-list', SelectionListElement);
