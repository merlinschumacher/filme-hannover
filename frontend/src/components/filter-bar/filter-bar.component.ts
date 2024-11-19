import html from './filter-bar.component.tpl';
import css from './filter-bar.component.css?inline';
import {
  allMovieRatings,
  getMovieRatingLabelString,
  MovieRating,
} from '../../models/MovieRating';
import {
  allShowTimeDubTypes,
  getShowTimeDubTypeLabelString,
  ShowTimeDubType,
} from '../../models/ShowTimeDubType';
import FilterIcon from '@material-symbols/svg-400/rounded/filter_alt.svg?raw';
import FilterSelection from '../../models/FilterSelection';

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class FilterBarElement extends HTMLElement {
  static get observedAttributes(): string[] {
    return ['movies', 'cinemas'];
  }
  private TotalCinemaCount = 0;
  private TotalMovieCount = 0;

  attributeChangedCallback(
    name: string,
    oldValue: string | null,
    newValue: string | null,
  ) {
    console.log('attributeChangedCallback', name, oldValue, newValue);
    if (oldValue === newValue) return;
    if (newValue === null) return;
    switch (name) {
      case 'movies':
        this.TotalMovieCount = parseInt(newValue);
        break;
      case 'cinemas':
        this.TotalCinemaCount = parseInt(newValue);
        break;
    }
  }

  private selectedCinemaIds: number[] = [];
  private selectedMovieIds: number[] = [];
  private selectedDubTypes: ShowTimeDubType[] = allShowTimeDubTypes;
  private selectedRatings: MovieRating[] = allMovieRatings;
  private shadow: ShadowRoot;

  public setData(selection: FilterSelection) {
    this.selectedCinemaIds = selection.selectedCinemaIds;
    this.selectedMovieIds = selection.selectedMovieIds;
    this.selectedDubTypes = selection.selectedDubTypes;
    this.selectedRatings = selection.selectedRatings;
    this.updateFilterInfo();
  }

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [styleSheet];
    this.shadow.safeQuerySelector('#filter-edit-icon').innerHTML = FilterIcon;
  }

  private handleFilterEditClick = () => {
    const event = new CustomEvent('filterEditClick');
    this.dispatchEvent(event);
  };

  connectedCallback() {
    this.updateFilterInfo();
    this.shadow
      .safeQuerySelector('#filter-edit-button')
      .addEventListener('click', this.handleFilterEditClick);
  }

  disconnectedCallback() {
    this.shadow
      .safeQuerySelector('#filter-edit-button')
      .removeEventListener('click', this.handleFilterEditClick);
  }

  private updateFilterInfo() {
    const cinemaCount =
      this.selectedCinemaIds.length === 0 ||
        this.selectedCinemaIds.length === this.TotalCinemaCount
        ? 'Alle'
        : this.selectedCinemaIds.length;
    const movieCount =
      this.selectedMovieIds.length === 0 ||
        this.selectedMovieIds.length === this.TotalMovieCount
        ? 'alle'
        : this.selectedMovieIds.length;
    const filterInfo = this.shadow.safeQuerySelector('#filter-info');
    let showTimeDubTypeStringList = this.selectedDubTypes
      .map((t) => getShowTimeDubTypeLabelString(t))
      .join(', ');
    showTimeDubTypeStringList =
      this.selectedDubTypes.length === 0 ||
        this.selectedDubTypes.length == allShowTimeDubTypes.length
        ? 'alle Vorführungen'
        : showTimeDubTypeStringList;

    let movieRatingStringList = this.selectedRatings
      .map((m) => getMovieRatingLabelString(m))
      .sort((a, b) => a.localeCompare(b, undefined, { numeric: true }))
      .join(', ');
    movieRatingStringList =
      this.selectedRatings.length === allMovieRatings.length
        ? 'alle Altersfreigaben'
        : movieRatingStringList;

    const moviePluralSuffix = this.selectedMovieIds.length === 1 ? '' : 'e';
    const cinemaPluralSuffix = this.selectedCinemaIds.length === 1 ? '' : 's';
    filterInfo.textContent = `Aktueller Filter: ${cinemaCount.toString()} Kino${cinemaPluralSuffix}, ${movieCount.toString()} Film${moviePluralSuffix}, ${showTimeDubTypeStringList}, ${movieRatingStringList}`;
  }
}

customElements.define('filter-bar', FilterBarElement);
