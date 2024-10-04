import Cinema from '../../models/Cinema';
import EventItemElement from '../event-item/event-item.component';
import html from './cinema-legend.component.tpl';
import css from './cinema-legend.component.css?inline';
const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class CinemaLegendElement extends HTMLElement {
  private shadow: ShadowRoot;
  private cinemas: Cinema[] = [];
  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [styleSheet];
  }

  setCinemaData(data: Cinema[]) {
    this.cinemas = data.sort((a, b) =>
      a.displayName.localeCompare(b.displayName),
    );
  }

  connectedCallback() {
    const elements: EventItemElement[] = [];
    this.cinemas.forEach((cinema) => {
      const cinemaLegendItem = new EventItemElement();
      cinemaLegendItem.setAttribute('color', cinema.color);
      cinemaLegendItem.setAttribute('icon', cinema.iconClass);
      cinemaLegendItem.setAttribute('title', cinema.displayName);
      cinemaLegendItem.setAttribute('href', cinema.url);
      cinemaLegendItem.slot = 'items';
      elements.push(cinemaLegendItem);
    });
    this.replaceChildren(...elements);
  }
}

customElements.define('cinema-legend', CinemaLegendElement);
