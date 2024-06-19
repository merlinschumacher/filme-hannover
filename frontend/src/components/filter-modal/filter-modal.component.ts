import html from './filter-modal.component.html?inline';
import css from './filter-modal.component.css?inline';
import { Cinema } from '../../models/Cinema';
import CheckableButtonElement from '../checkable-button/checkable-button.component';
import { Movie } from '../../models/Movie';
import SelectionListElement from '../selection-list/selection-list.component';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class FilterModal extends HTMLElement {

  public Cinemas: Cinema[] = [];
  public Movies: Movie[] = [];

  constructor() {
    super();
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));
    shadow.adoptedStyleSheets = [style];
  }

  connectedCallback() {
      this.Cinemas.forEach(cinema => {
        const cinemaButton = new CheckableButtonElement();
        cinemaButton.slot = 'cinema-selection';
        cinemaButton.setAttribute('label', cinema.displayName);
        cinemaButton.setAttribute('value', cinema.id.toString());
        cinemaButton.setAttribute('color', cinema.color);
        this.append(cinemaButton);
      });

      const movieList = SelectionListElement.BuildElement(this.Movies);
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

