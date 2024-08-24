import html from './filter-modal.component.html?raw';
import css from './filter-modal.component.css?inline';
import CheckableButtonElement from '../checkable-button/checkable-button.component';
import SelectionListElement from '../selection-list/selection-list.component';
import Cinema from '../../models/Cinema';
import Movie from '../../models/Movie';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class FilterModal extends HTMLElement {

  public Cinemas: Cinema[] = [];
  public Movies: Movie[] = [];

  private SelectedCinemas: Cinema[] = [];
  private SelectedMovies: Movie[] = [];

  public onFilterChanged?: (cinemas: Cinema[], movies: Movie[]) => void;

  constructor() {
    super();
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));
    shadow.adoptedStyleSheets = [style];
  }

  handleCinemaSelectionChanged(e: Event) {
    const target = e.target as CheckableButtonElement;
    if (!target.checked) {
      this.SelectedCinemas.push(this.Cinemas.find(c => c.id === parseInt(target.value))!);
    } else {
      this.SelectedCinemas = this.SelectedCinemas.filter(c => c.id !== parseInt(target.value));
    }
    if (!this.onFilterChanged)
      return;
    this.onFilterChanged(this.SelectedCinemas, this.SelectedMovies);
    };

  connectedCallback() {
      this.SelectedCinemas = this.Cinemas;
      this.Cinemas.forEach(cinema => {
        const cinemaButton = new CheckableButtonElement();
        cinemaButton.slot = 'cinema-selection';
        cinemaButton.setAttribute('label', cinema.displayName);
        cinemaButton.setAttribute('value', cinema.id.toString());
        cinemaButton.setAttribute('color', cinema.color);
        cinemaButton.setAttribute('checked', '');
        cinemaButton.addEventListener('click', this.handleCinemaSelectionChanged.bind(this));
        this.append(cinemaButton);

      });

      const movieList = SelectionListElement.BuildElement(this.Movies);
      movieList.onSelectionChanged = (movies: Movie[]) => {
        this.SelectedMovies = movies;
        if (!this.onFilterChanged)
          return;
        this.onFilterChanged(this.SelectedCinemas, this.SelectedMovies);
      }
      movieList.slot = 'movie-selection';
      this.append(movieList);
    }


  public static BuildElement(Cinemas: Cinema[], Movies: Movie[]): FilterModal {
    var item = new FilterModal();
    item.Cinemas = Cinemas;
    item.Movies = Movies;
    return item;
  }
}

customElements.define('filter-modal', FilterModal);

