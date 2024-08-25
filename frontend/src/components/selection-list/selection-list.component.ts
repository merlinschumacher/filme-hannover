import html from './selection-list.component.html?raw';
import css from './selection-list.component.css?inline';
import Movie from '../../models/Movie';
import SelectionListItemElement from '../selection-list-item/selection-list-item.component';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class SelectionListElement extends HTMLElement {

  public Movies: Movie[] = [];
  private SelectedMovies: Movie[] = [];
  private allMoviesButton: SelectionListItemElement;
  public onSelectionChanged?: (movies: Movie[]) => void;

  private shadow: ShadowRoot;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(template.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [style];
    this.allMoviesButton = this.buildAllMoviesButton();
  }
  private buildMovieButtons(movies: Movie[]): SelectionListItemElement[] {
    var options: SelectionListItemElement[] = [];
    movies.forEach(movie => {
      const movieButton = new SelectionListItemElement();
      movieButton.slot = 'selection-list';
      movieButton.setAttribute('label', movie.displayName);
      movieButton.setAttribute('value', movie.id.toString());
      movieButton.addEventListener('click', (e: MouseEvent) => {

        if (this.SelectedMovies.some(m => m.id === movie.id)) {
          this.SelectedMovies = this.SelectedMovies.filter(m => m.id !== movie.id);
        } else {
          this.SelectedMovies.push(movie);
        }

        this.uncheckAllMoviesButton(e, this.allMoviesButton);
        if (!this.onSelectionChanged)
          return;
        this.onSelectionChanged(this.SelectedMovies);
      });
      options.push(movieButton);
    });
    return options;
  }

  private uncheckAllMoviesButton(e: MouseEvent, btn: SelectionListItemElement) {
    if (e.target instanceof SelectionListItemElement && !e.target.checked) {
      btn.removeAttribute('checked');
    }
  }


  private buildAllMoviesButton(): SelectionListItemElement {
    const allMoviesButton = new SelectionListItemElement();
    allMoviesButton.classList.add('all-movies');
    allMoviesButton.setAttribute('label', 'Alle Filme');
    allMoviesButton.setAttribute('value', '0');
    allMoviesButton.setAttribute('checked', '');
    return allMoviesButton;
  }

  connectedCallback() {
    var options: SelectionListItemElement[] = [];
    var movieButtons = this.buildMovieButtons(this.Movies);
    this.allMoviesButton.addEventListener('click', (e: MouseEvent) => this.uncheckMovieButtons(e, movieButtons), false);
    this.append(this.allMoviesButton);

    options.push(...movieButtons);
    this.append(...options);

    const searchInput = this.shadow.querySelector('input') as HTMLInputElement;
    searchInput.addEventListener('input', () => this.searchMovies(searchInput.value));

  };

  private uncheckMovieButtons(e: MouseEvent, options: SelectionListItemElement[]) {
    if (e.target instanceof SelectionListItemElement && !e.target.checked) {
      options.forEach((option: Element) => {
        option.removeAttribute('checked');
      });
    }
  }

  private searchMovies(searchTerm: string) {
    var options = this.querySelectorAll('selection-list-item') as NodeListOf<SelectionListItemElement>;
    options.forEach((option: SelectionListItemElement) => {
      if (option.label.toLowerCase().includes(searchTerm.toLowerCase())) {
        option.style.display = 'block';
      } else {
        option.style.display = 'none';
      }
    });
  }

  public static BuildElement(movies: Movie[]): SelectionListElement {
    var item = new SelectionListElement();
    item.Movies = movies;
    return item;
  }
}

customElements.define('selection-list', SelectionListElement);


