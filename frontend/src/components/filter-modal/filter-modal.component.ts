import html from './filter-modal.component.tpl';
import css from './filter-modal.component.css?inline';
import CheckableButtonElement from '../checkable-button/checkable-button.component';
import SelectionListElement from '../selection-list/selection-list.component';
import Cinema from '../../models/Cinema';
import Movie from '../../models/Movie';
import {
  allMovieRatings,
  getMovieRatingLabelString,
  MovieRating,
  movieRatingColors,
} from '../../models/MovieRating';
import {
  allShowTimeDubTypes,
  getShowTimeDubTypeLabelString,
  ShowTimeDubType,
} from '../../models/ShowTimeDubType';
import FilterIcon from '@material-symbols/svg-400/rounded/filter_alt.svg?raw';
import Check from '@material-symbols/svg-400/outlined/check.svg?raw';
import Close from '@material-symbols/svg-400/outlined/close.svg?raw';
import EventItem from '../event-item/event-item.component';

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class FilterModal extends HTMLElement {
  public Cinemas: Cinema[] = [];
  public Movies: Movie[] = [];

  private SelectedCinemas: Cinema[] = [];
  private SelectedMovies: Movie[] = [];
  private SelectedShowDubTimeTypes: ShowTimeDubType[] = allShowTimeDubTypes;
  private SelectedMovieRatings: MovieRating[] = allMovieRatings;
  private shadow: ShadowRoot;
  private dialogEl: HTMLDialogElement;

  public onFilterChanged?: (
    cinemas: Cinema[],
    movies: Movie[],
    showTimeDubTypes: ShowTimeDubType[],
    movieRatings: MovieRating[],
  ) => void;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [styleSheet];
    this.shadow.safeQuerySelector('#filter-edit-icon').innerHTML = FilterIcon;
    this.shadow.safeQuerySelector('#filter-apply-icon').innerHTML = Check;
    this.shadow.safeQuerySelector('#filter-close-icon').innerHTML = Close;
    this.dialogEl = this.shadow.safeQuerySelector(
      '#filter-dialog',
    ) as HTMLDialogElement;
  }

  handleCinemaSelectionChanged(e: Event) {
    if (e.target instanceof CheckableButtonElement) {
      const value = e.target.getAttribute('value') ?? '';
      const cinemaId = parseInt(value);
      if (!this.SelectedCinemas.find((c) => c.id === cinemaId)) {
        const cinema = this.Cinemas.find((c) => c.id === cinemaId);
        if (cinema) {
          this.SelectedCinemas.push(cinema);
        }
      } else {
        this.SelectedCinemas = this.SelectedCinemas.filter(
          (c) => c.id !== cinemaId,
        );
      }
    }
  }

  handleShowTimeDubTypeSelected(e: Event) {
    if (e.target instanceof CheckableButtonElement) {
      const value = Number(e.target.getAttribute('value') ?? 0);
      const showTimeDubType: ShowTimeDubType = value as ShowTimeDubType;
      if (!this.SelectedShowDubTimeTypes.includes(showTimeDubType)) {
        this.SelectedShowDubTimeTypes.push(showTimeDubType);
      } else {
        this.SelectedShowDubTimeTypes = this.SelectedShowDubTimeTypes.filter(
          (t) => t !== showTimeDubType,
        );
      }
    }
  }

  handleMovieRatingSelected(e: Event) {
    if (e.target instanceof CheckableButtonElement) {
      const value = Number(e.target.getAttribute('value') ?? 0);
      const movieRating: MovieRating = value as MovieRating;
      if (!this.SelectedMovieRatings.includes(movieRating)) {
        this.SelectedMovieRatings.push(movieRating);
      } else {
        this.SelectedMovieRatings = this.SelectedMovieRatings.filter(
          (t) => t !== movieRating,
        );
      }
      console.log(this.SelectedMovieRatings);
      if (this.SelectedMovieRatings.length === 0) {
        this.SelectedMovieRatings = allMovieRatings;
        const ratingSlot = this.shadow.querySelector(
          'slot[name="rating-selection"]',
        );
        if (ratingSlot) {
          (ratingSlot as HTMLSlotElement).assignedElements().forEach((e) => {
            if (e instanceof CheckableButtonElement) {
              e.setAttribute('checked', 'true');
            }
          });
        }
        e.preventDefault();
      }
    }
  }

  connectedCallback() {
    this.buildButtonEvents();
    this.SelectedCinemas = this.Cinemas;
    const cinemaButtons: CheckableButtonElement[] =
      this.generateCinemaButtons();

    this.SelectedShowDubTimeTypes = allShowTimeDubTypes;
    const showTimeDubTypeButtons: CheckableButtonElement[] =
      this.generateShowTimeDubTypeButtons();
    const movieRatingButtons: CheckableButtonElement[] =
      this.generateMovieRatingButtons();

    const cinemaLegend: EventItem[] = this.generateCinemaLegend();
    this.append(...cinemaLegend);

    const movieList = SelectionListElement.BuildElement(this.Movies);
    movieList.onSelectionChanged = (movies: Movie[]) => {
      this.SelectedMovies = movies;
    };
    movieList.slot = 'movie-selection';

    this.append(...showTimeDubTypeButtons);
    this.append(...movieRatingButtons);
    this.append(...cinemaButtons);
    this.append(movieList);

    this.updateFilterInfo();
  }

  private updateFilterInfo() {
    const cinemaCount =
      this.SelectedCinemas.length === 0 ||
      this.SelectedCinemas.length === this.Cinemas.length
        ? 'Alle'
        : this.SelectedCinemas.length;
    const movieCount =
      this.SelectedMovies.length === 0 ||
      this.SelectedMovies.length === this.Movies.length
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

  private buildButtonEvents() {
    const openFilterDialogButtonEl =
      this.shadow.safeQuerySelector('#open-filter');
    const applyFilterDialogButtonEl =
      this.shadow.safeQuerySelector('#apply-filter');
    const closeFilterDialogButtonEl =
      this.shadow.safeQuerySelector('#close-filter');
    openFilterDialogButtonEl.addEventListener('click', () => {
      this.dialogEl.showModal();
    });
    closeFilterDialogButtonEl.addEventListener('click', () => {
      this.dialogEl.close();
    });

    applyFilterDialogButtonEl.addEventListener('click', () => {
      if (this.onFilterChanged) {
        this.onFilterChanged(
          this.SelectedCinemas,
          this.SelectedMovies,
          this.SelectedShowDubTimeTypes,
          this.SelectedMovieRatings,
        );
        this.updateFilterInfo();
      }
      this.dialogEl.close();
    });

    this.dialogEl.addEventListener('click', (event: Event) => {
      const mouseEvent = event as MouseEvent;
      const rect = this.dialogEl.getBoundingClientRect();
      const isInDialog =
        rect.top <= mouseEvent.clientY &&
        mouseEvent.clientY <= rect.top + rect.height &&
        rect.left <= mouseEvent.clientX &&
        mouseEvent.clientX <= rect.left + rect.width;
      if (!isInDialog) {
        this.dialogEl.close();
      }
    });
  }

  private generateCinemaButtons() {
    const cinemaButtons: CheckableButtonElement[] = [];
    this.Cinemas.forEach((cinema) => {
      const cinemaButton = CheckableButtonElement.BuildElement(
        cinema.displayName,
        cinema.id.toString(),
        cinema.color,
      );
      cinemaButton.slot = 'cinema-selection';
      cinemaButton.addEventListener(
        'click',
        this.handleCinemaSelectionChanged.bind(this),
      );
      cinemaButtons.push(cinemaButton);
    });
    return cinemaButtons;
  }

  private generateShowTimeDubTypeButtons() {
    const showTimeDubTypeButtons: CheckableButtonElement[] = [];
    const showTimeDubTypes: ShowTimeDubType[] = allShowTimeDubTypes;

    showTimeDubTypes.forEach((showTimeDubType) => {
      const showTimeDubTypeButton = CheckableButtonElement.BuildElement(
        getShowTimeDubTypeLabelString(showTimeDubType),
        showTimeDubType.valueOf().toString(),
      );
      showTimeDubTypeButton.slot = 'type-selection';
      showTimeDubTypeButton.addEventListener(
        'click',
        this.handleShowTimeDubTypeSelected.bind(this),
      );
      showTimeDubTypeButtons.push(showTimeDubTypeButton);
    });
    return showTimeDubTypeButtons;
  }

  private generateMovieRatingButtons() {
    const movieRatingButtons: CheckableButtonElement[] = [];
    const movieRatings: MovieRating[] = allMovieRatings;

    movieRatings.forEach((movieRating) => {
      const movieRatingButton = CheckableButtonElement.BuildElement(
        getMovieRatingLabelString(movieRating),
        movieRating.valueOf().toString(),
        movieRatingColors.get(movieRating),
      );
      movieRatingButton.slot = 'rating-selection';
      movieRatingButton.addEventListener(
        'click',
        this.handleMovieRatingSelected.bind(this),
      );
      movieRatingButtons.push(movieRatingButton);
    });
    return movieRatingButtons;
  }

  private generateCinemaLegend() {
    const elements: EventItem[] = [];
    this.SelectedCinemas.forEach((cinema) => {
      const cinemaLegendItem = new EventItem();
      cinemaLegendItem.setAttribute('color', cinema.color);
      cinemaLegendItem.setAttribute('icon', cinema.iconClass);
      cinemaLegendItem.setAttribute('title', cinema.displayName);
      cinemaLegendItem.setAttribute('href', cinema.url);
      cinemaLegendItem.slot = 'cinema-legend';
      elements.push(cinemaLegendItem);
    });
    return elements;
  }

  public static BuildElement(Cinemas: Cinema[], Movies: Movie[]): FilterModal {
    const item = new FilterModal();
    item.Cinemas = Cinemas;
    item.Movies = Movies;
    return item;
  }
}

customElements.define('filter-modal', FilterModal);
