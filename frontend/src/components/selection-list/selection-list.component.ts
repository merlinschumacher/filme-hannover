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

  constructor() {
    super();
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.appendChild(template.content.cloneNode(true));
    shadow.adoptedStyleSheets = [style];
  }

  connectedCallback() {
  }

  public static BuildElement(movies: Movie[]): SelectionListElement {
    var item = new SelectionListElement();
    var options: CheckableButtonElement[] = [];
      movies.forEach(movie => {
        const movieButton = new CheckableButtonElement();
        movieButton.slot = 'selection-list';
        movieButton.setAttribute('label', movie.displayName);
        movieButton.setAttribute('value', movie.id.toString());
        options.push(movieButton);
      });
      item.append(...options);
    return item;
  }
}

customElements.define('selection-list', SelectionListElement);


