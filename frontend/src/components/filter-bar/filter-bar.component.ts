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
import EventItem from '../event-item/event-item.component';
import FilterModal from '../filter-modal/filter-modal.component';
import Cinema from '../../models/Cinema';

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class FilterBar extends HTMLElement {
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

  private SelectedCinemas: number[] = [];
  private SelectedMovies: number[] = [];
  private SelectedShowDubTimeTypes: ShowTimeDubType[] = allShowTimeDubTypes;
  private SelectedMovieRatings: MovieRating[] = allMovieRatings;
  private shadow: ShadowRoot;
  private filterModal: FilterModal | null = null;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [styleSheet];
    this.shadow.safeQuerySelector('#filter-edit-icon').innerHTML = FilterIcon;
    const openFilterDialogButtonEl =
      this.shadow.safeQuerySelector('#open-filter');
    openFilterDialogButtonEl.addEventListener('click', () => {
      if (this.filterModal) {
        this.filterModal.setData(
          this.SelectedCinemas,
          this.SelectedMovies,
          this.SelectedShowDubTimeTypes,
          this.SelectedMovieRatings,
        );
        this.filterModal.showModal();
      }
    });
    this.filterModal = new FilterModal();
    this.filterModal.onFilterChanged = (
      selectedCinemaIds,
      selectedMovieIds,
      selectedDubTypes,
      selectedRatings,
    ) => {
      this.SelectedCinemas = selectedCinemaIds;
      this.SelectedMovies = selectedMovieIds;
      this.SelectedShowDubTimeTypes = selectedDubTypes;
      this.SelectedMovieRatings = selectedRatings;
    };
    this.shadow.appendChild(this.filterModal);
  }

  connectedCallback() {
    const cinemaLegend: EventItem[] = this.generateCinemaLegend();
    this.append(...cinemaLegend);
    this.updateFilterInfo();
  }

  private updateFilterInfo() {
    const cinemaCount =
      this.SelectedCinemas.length === 0 ||
      this.SelectedCinemas.length === this.TotalCinemaCount
        ? 'Alle'
        : this.SelectedCinemas.length;
    const movieCount =
      this.SelectedMovies.length === 0 ||
      this.SelectedMovies.length === this.TotalMovieCount
        ? 'alle'
        : this.SelectedMovies.length;
    const filterInfo = this.shadow.safeQuerySelector('#filter-info');
    let showTimeDubTypeStringList = this.SelectedShowDubTimeTypes.map((t) =>
      getShowTimeDubTypeLabelString(t),
    ).join(', ');
    showTimeDubTypeStringList =
      this.SelectedShowDubTimeTypes.length === 0 ||
      this.SelectedShowDubTimeTypes.length == allShowTimeDubTypes.length
        ? 'alle Vorführungen'
        : showTimeDubTypeStringList;

    let movieRatingStringList = this.SelectedMovieRatings.map((m) =>
      getMovieRatingLabelString(m),
    )
      .sort((a, b) => a.localeCompare(b))
      .join(', ');
    movieRatingStringList =
      this.SelectedMovieRatings.length === allMovieRatings.length
        ? 'alle Altersfreigaben'
        : movieRatingStringList;

    const moviePluralSuffix = this.SelectedMovies.length === 1 ? '' : 'e';
    const cinemaPluralSuffix = this.SelectedCinemas.length === 1 ? '' : 's';
    filterInfo.textContent = `Aktueller Filter: ${cinemaCount.toString()} Kino${cinemaPluralSuffix}, ${movieCount.toString()} Film${moviePluralSuffix}, ${showTimeDubTypeStringList}, ${movieRatingStringList}`;
  }

  private generateCinemaLegend() {
    const elements: EventItem[] = [];
    // this.Cinema.forEach((cinema) => {
    //   const cinemaLegendItem = new EventItem();
    //   cinemaLegendItem.setAttribute('color', cinema.color);
    //   cinemaLegendItem.setAttribute('icon', cinema.iconClass);
    //   cinemaLegendItem.setAttribute('title', cinema.displayName);
    //   cinemaLegendItem.setAttribute('href', cinema.url);
    //   cinemaLegendItem.slot = 'cinema-legend';
    //   elements.push(cinemaLegendItem);
    // });
    return elements;
  }
}

customElements.define('filter-bar', FilterBar);
