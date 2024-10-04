import html from './filter-bar.component.tpl';
import css from './filter-bar.component.css?inline';
import buttonCss from '../common/action-button.css?inline';
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

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);
const buttonStyleSheet = new CSSStyleSheet();
buttonStyleSheet.replaceSync(buttonCss);

export default class FilterBarElement extends HTMLElement {
  public static observedAttributes = ['totalMovieCount', 'totalCinemaCount'];
  private TotalCinemaCount = 0;
  private TotalMovieCount = 0;

  attributeChangedCallback(name: string, oldValue: string, newValue: string) {
    if (oldValue === newValue) return;
    switch (name) {
      case 'totalMovieCount':
        this.TotalMovieCount = parseInt(newValue);
        break;
      case 'totalCinemaCount':
        this.TotalCinemaCount = parseInt(newValue);
        break;
    }
  }

  private selectedCinemaIds: number[] = [];
  private selectedMovieIds: number[] = [];
  private selectedDubTypes: ShowTimeDubType[] = allShowTimeDubTypes;
  private selectedRatings: MovieRating[] = allMovieRatings;
  private shadow: ShadowRoot;

  public setData(
    selectedCinemaIds: number[],
    selectedMovieIds: number[],
    selectedDubTypes: ShowTimeDubType[],
    selecteRatings: MovieRating[],
  ) {
    this.selectedCinemaIds = selectedCinemaIds;
    this.selectedMovieIds = selectedMovieIds;
    this.selectedDubTypes = selectedDubTypes;
    this.selectedRatings = selecteRatings;
  }

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [styleSheet, buttonStyleSheet];
    this.shadow.safeQuerySelector('#filter-edit-icon').innerHTML = FilterIcon;
  }

  connectedCallback() {
    this.updateFilterInfo();
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
      .sort((a, b) => a.localeCompare(b))
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
