import html from './selection-list.component.html?inline';
import css from './selection-list.component.css?inline';
import { Movie } from '../../models/Movie';
import CheckableButtonElement from '../checkable-button/checkable-button.component';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class SelectionListElement extends HTMLElement {

  public Movies: Movie[] = [];
  public SelectedMovies: Movie[] = [];

  constructor() {
    super();
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));
    shadow.adoptedStyleSheets = [style];
  }
  private buildMovieButtons(movies: Movie[]): CheckableButtonElement[] {
    var options: CheckableButtonElement[] = [];
    movies.forEach(movie => {
      const movieButton = new CheckableButtonElement();
      movieButton.slot = 'selection-list';
      movieButton.setAttribute('label', movie.displayName);
      movieButton.setAttribute('value', movie.id.toString());
      options.push(movieButton);
    });
    return options;
  }

  private buildAllMoviesButton(): CheckableButtonElement {
    const allMoviesButton = new CheckableButtonElement();
    allMoviesButton.slot = 'selection-list';
    allMoviesButton.classList.add('all-movies');
    allMoviesButton.setAttribute('label', 'Alle Filme');
    allMoviesButton.setAttribute('value', '0');
    allMoviesButton.setAttribute('checked', '');
    return allMoviesButton;
  }

  connectedCallback() {
    var options: CheckableButtonElement[] = [];
    var allAllMoviesButton = this.buildAllMoviesButton();
    options.push(allAllMoviesButton);
    var movieButtons = this.buildMovieButtons(this.Movies);
    options.push(...movieButtons);
    this.append(...options);
    options.forEach(option => {
      option.addEventListener('click', (e) => {
        if (e.target instanceof CheckableButtonElement && !e.target.checked) {
          if (e.target.value === '0') {
            this.querySelectorAll('checkable-button').forEach((element) => {
              element.removeAttribute('checked');
            })
          } else {
            this.querySelectorAll('.all-movies').forEach((element) => {
              element.removeAttribute('checked');
            })
          }
        }
      });
    });
  }

  public static BuildElement(movies: Movie[]): SelectionListElement {
    var item = new SelectionListElement();
    item.Movies = movies;
    return item;
  }
}

customElements.define('selection-list', SelectionListElement);


