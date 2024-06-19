import html from './selection-list.component.html?inline';
import css from './selection-list.component.css?inline';
import { Movie } from '../../models/Movie';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class SelectionListElement extends HTMLElement {

  public Movies: Movie[] = [];

  constructor() {
    super();
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));
    shadow.adoptedStyleSheets = [style];
  }

  connectedCallback() {
      var options: HTMLOptionElement[] = [];
      this.Movies.forEach(movie => {
        const movieOption = document.createElement('option');
        movieOption.textContent = movie.displayName;
        movieOption.value = movie.id.toString();
        options.push(movieOption);
      });
      var select = this.shadowRoot?.querySelector('select') as HTMLSelectElement;
      select.append(...options);
  }

  public static BuildElement(movies: Movie[]): SelectionListElement {
    var item = new SelectionListElement();
    item.Movies = movies;
    return item;
  }
}

customElements.define('selection-list', SelectionListElement);


