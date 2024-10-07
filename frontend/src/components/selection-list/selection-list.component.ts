import html from './selection-list.component.tpl';
import css from './selection-list.component.css?inline';
import Movie from '../../models/Movie';
import SelectionListItemElement from '../selection-list-item/selection-list-item.component';

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class SelectionListElement extends HTMLElement {
  setData(selectedItemIds: number[]) {
    this.selectedItemIds = selectedItemIds;
  }
  public Movies: Movie[] = [];
  public selectedItemIds: number[] = [];
  public onSelectionChanged?: (movies: number[]) => void;

  private shadow: ShadowRoot;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [styleSheet];
    const searchInput = this.shadow.safeQuerySelector(
      'input',
    ) as HTMLInputElement;
    searchInput.addEventListener('input', () => {
      this.searchMovies(searchInput.value);
    });

    this.addEventListener('click', (ev: MouseEvent) => {
      if (!(ev.target instanceof SelectionListItemElement)) return;
      const movieId = parseInt(ev.target.getAttribute('value') ?? '0');
      this.selectedItemIds.toggleElement(movieId);

      if (!this.onSelectionChanged) return;
      this.onSelectionChanged(this.selectedItemIds);
    });
  }

  private buildMovieButtons(): SelectionListItemElement[] {
    const options: SelectionListItemElement[] = [];
    this.Movies.forEach((movie) => {
      const movieButton = new SelectionListItemElement();
      movieButton.slot = 'selection-list';
      movieButton.setAttribute('label', movie.displayName);
      movieButton.setAttribute('value', movie.id.toString());
      options.push(movieButton);
    });
    return options;
  }

  connectedCallback() {
    const movieButtons = this.buildMovieButtons();
    this.append(...movieButtons);
    this.updateSelections();
  }

  private updateSelections() {
    const options = this.querySelectorAll('selection-list-item');
    options.forEach((option: Element) => {
      const node = option as SelectionListItemElement;
      node.setChecked(this.selectedItemIds.includes(node.getValue()));
    });
  }

  private searchMovies(searchTerm: string) {
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
  }

  public static BuildElement(movies: Movie[]): SelectionListElement {
    const item = new SelectionListElement();
    item.Movies = movies;
    return item;
  }
}

customElements.define('selection-list', SelectionListElement);
