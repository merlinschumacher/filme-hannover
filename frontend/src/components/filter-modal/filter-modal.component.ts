import html from './filter-modal.component.tpl';
import css from './filter-modal.component.css?inline';
import buttonCss from '../common/action-button.css?inline';
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
import Check from '@material-symbols/svg-400/outlined/check.svg?raw';
import Close from '@material-symbols/svg-400/outlined/close.svg?raw';

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);
const buttonStyleSheet = new CSSStyleSheet();
buttonStyleSheet.replaceSync(buttonCss);

export default class FilterModalElement extends HTMLElement {
  public cinemas: Cinema[] = [];
  public movies: Movie[] = [];

  private selectedCinemaIds: number[] = [];
  private selectedMovieIds: number[] = [];
  private selectedDubTypes: ShowTimeDubType[] = allShowTimeDubTypes;
  private selectedRatings: MovieRating[] = allMovieRatings;
  private shadow: ShadowRoot;
  private dialogEl: HTMLDialogElement;
  filterChangedEvent = new CustomEvent('filterChanged', {
    detail: {
      selectedCinemaIds: this.selectedCinemaIds,
      selectedMovieIds: this.selectedMovieIds,
      selectedDubTypes: this.selectedDubTypes,
      selectedRatings: this.selectedRatings,
    },
  });

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

  private updateDialog() {
    const cinemaSelectionSlot = this.shadow.safeQuerySelector(
      'slot[name="cinema-selection"]',
    ) as HTMLSlotElement;
    cinemaSelectionSlot.childNodes.forEach((node) => {
      if (node instanceof CheckableButtonElement) {
        node.setChecked(this.selectedCinemaIds.includes(node.getValue()));
      }
    });

    const DubTypeSlot = this.shadow.safeQuerySelector(
      'slot[name="type-selection"]',
    ) as HTMLSlotElement;
    DubTypeSlot.childNodes.forEach((node) => {
      if (node instanceof CheckableButtonElement) {
        node.setChecked(this.selectedDubTypes.includes(node.getValue()));
      }
    });

    const movieRatingSlot = this.shadow.safeQuerySelector(
      'slot[name="rating-selection"]',
    ) as HTMLSlotElement;
    movieRatingSlot.childNodes.forEach((node) => {
      if (node instanceof CheckableButtonElement) {
        node.setChecked(this.selectedRatings.includes(node.getValue()));
      }
    });

    const movieSelectionSlot = this.shadow.safeQuerySelector(
      'slot[name="movie-selection"]',
    ) as HTMLSlotElement;

    movieSelectionSlot.childNodes.forEach((node) => {
      if (node instanceof SelectionListElement) {
        node.setData(this.selectedMovieIds);
      }
    });
  }

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [styleSheet, buttonStyleSheet];

    this.dialogEl = this.shadow.safeQuerySelector(
      '#filter-dialog',
    ) as HTMLDialogElement;

    this.shadow.safeQuerySelector('#filter-apply-icon').innerHTML = Check;
    this.shadow.safeQuerySelector('#filter-close-icon').innerHTML = Close;
  }

  private buildButtons() {
    const movieList = SelectionListElement.BuildElement(this.movies);
    movieList.onSelectionChanged = (movieIds: number[]) => {
      this.selectedMovieIds = movieIds;
    };
    movieList.slot = 'movie-selection';
    this.append(movieList);

    const cinemaButtons: CheckableButtonElement[] =
      this.generateCinemaButtons();
    this.append(...cinemaButtons);

    const showTimeDubTypeButtons: CheckableButtonElement[] =
      this.generateShowTimeDubTypeButtons();
    this.append(...showTimeDubTypeButtons);

    const movieRatingButtons: CheckableButtonElement[] =
      this.generateMovieRatingButtons();
    this.append(...movieRatingButtons);
  }

  handleCinemaSelectionChanged(e: Event) {
    if (e.target instanceof CheckableButtonElement) {
      const value = e.target.getValue();
      this.selectedMovieIds.toggleElement(value);
    }
  }

  handleShowTimeDubTypeSelected(e: Event) {
    if (e.target instanceof CheckableButtonElement) {
      const value = e.target.getValue();
      this.selectedDubTypes.toggleElement(value);
    }
  }

  handleMovieRatingSelected(e: Event) {
    if (e.target instanceof CheckableButtonElement) {
      const value = e.target.getValue();
      this.selectedDubTypes.toggleElement(value);
    }
  }

  connectedCallback() {
    this.buildButtons();
    this.buildButtonEvents();
    this.updateDialog();
    this.dialogEl.showModal();
  }

  disconnectedCallback() {
    this.closeDialog();
  }

  private buildButtonEvents() {
    const applyFilterDialogButtonEl =
      this.shadow.safeQuerySelector('#apply-filter');
    const closeFilterDialogButtonEl =
      this.shadow.safeQuerySelector('#close-filter');

    closeFilterDialogButtonEl.addEventListener('click', this.closeDialog);
    applyFilterDialogButtonEl.addEventListener(
      'click',
      this.applyFilterSelection,
    );
    this.dialogEl.addEventListener('click', this.clickOutsideDialog);
  }
  private applyFilterSelection = () => {
    this.dispatchEvent(this.filterChangedEvent);
    this.closeDialog();
  };

  private closeDialog = () => {
    this.dialogEl.close();

    const movieSelectionSlot = this.shadow.safeQuerySelector(
      "slot[name='movie-selection']",
    ) as HTMLSlotElement;
    movieSelectionSlot.innerHTML = '';

    const applyFilterDialogButtonEl =
      this.shadow.safeQuerySelector('#apply-filter');
    const closeFilterDialogButtonEl =
      this.shadow.safeQuerySelector('#close-filter');

    closeFilterDialogButtonEl.removeEventListener('click', this.closeDialog);
    applyFilterDialogButtonEl.removeEventListener(
      'click',
      this.applyFilterSelection,
    );
    this.dialogEl.removeEventListener('click', this.clickOutsideDialog);
    this.dispatchEvent(new CustomEvent('close'));
  };

  private clickOutsideDialog = (event: Event) => {
    if (event.target instanceof HTMLDialogElement) {
      const mouseEvent = event as MouseEvent;
      const rect = event.target.getBoundingClientRect();
      const isInDialog =
        rect.top <= mouseEvent.clientY &&
        mouseEvent.clientY <= rect.top + rect.height &&
        rect.left <= mouseEvent.clientX &&
        mouseEvent.clientX <= rect.left + rect.width;
      if (!isInDialog) {
        this.closeDialog();
      }
    }
  };

  private generateCinemaButtons() {
    const cinemaButtons: CheckableButtonElement[] = [];
    this.cinemas.forEach((cinema) => {
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
}

customElements.define('filter-modal', FilterModalElement);
