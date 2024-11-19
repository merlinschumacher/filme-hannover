import html from './filter-modal.component.tpl';
import css from './filter-modal.component.css?inline';
import CheckableButtonElement from '../checkable-button/checkable-button.component';
import MovieListElement from '../selection-list/selection-list.component';
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
import FilterSelection from '../../models/FilterSelection';
import SelectionListItemElement from '../selection-list-item/selection-list-item.component';

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class FilterModalElement extends HTMLElement {
  public cinemas: Cinema[] = [];
  public movies: Movie[] = [];

  private selectedCinemaIds: number[] = [];
  private selectedMovieIds: number[] = [];
  private selectedDubTypes: ShowTimeDubType[] = allShowTimeDubTypes;
  private selectedRatings: MovieRating[] = allMovieRatings;
  private shadow: ShadowRoot;
  private dialogEl: HTMLDialogElement;

  public setSelection(selection: FilterSelection) {
    if (selection.selectedMovieIds.length != this.movies.length) {
      this.selectedMovieIds = selection.selectedMovieIds;
    }
    this.selectedCinemaIds = selection.selectedCinemaIds;
    this.selectedDubTypes = selection.selectedDubTypes;
    this.selectedRatings = selection.selectedRatings;
  }

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [styleSheet];

    this.dialogEl = this.shadow.safeQuerySelector(
      '#filter-dialog',
    ) as HTMLDialogElement;

    this.shadow.safeQuerySelector('#filter-apply-icon').innerHTML = Check;
    this.shadow.safeQuerySelector('#filter-close-icon').innerHTML = Close;
  }

  private buildButtons() {
    const movieList = new MovieListElement();
    movieList.setSelections(this.selectedMovieIds);
    movieList.onSelectionChanged = (movieIds: number[]) => {
      this.selectedMovieIds = movieIds;
    };
    movieList.slot = 'movie-selection';
    const movieButtons = this.generateMovieButtons();
    movieList.append(...movieButtons);
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

  private handleSelectionChange<T extends { valueOf(): T }>(
    selectedItems: T[],
    selectedItem: T,
  ): T[] {
    console.log('Selected items', selectedItems);
    if (!selectedItems.includes(selectedItem)) {
      selectedItems.push(selectedItem);
    } else {
      selectedItems = selectedItems.filter((item) => item !== selectedItem);
    }
    console.log('Resulting selected items', selectedItems);
    return selectedItems;
  }

  handleCinemaSelectionChanged(e: Event) {
    if (e.target instanceof CheckableButtonElement) {
      this.selectedCinemaIds = this.handleSelectionChange(
        this.selectedCinemaIds,
        e.target.getValue(),
      );
    }
  }

  handleShowTimeDubTypeSelected(e: Event) {
    if (e.target instanceof CheckableButtonElement) {
      this.selectedDubTypes = this.handleSelectionChange(
        this.selectedDubTypes,
        e.target.getValue(),
      );
    }
  }

  handleMovieRatingSelected(e: Event) {
    if (e.target instanceof CheckableButtonElement) {
      this.selectedRatings = this.handleSelectionChange(
        this.selectedRatings,
        e.target.getValue(),
      );
    }
  }

  connectedCallback() {
    this.buildButtons();
    this.buildButtonEvents();
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
    const filterChangedEvent = new CustomEvent('filterChanged', {
      detail: new FilterSelection(
        this.selectedCinemaIds,
        this.selectedMovieIds,
        this.selectedDubTypes,
        this.selectedRatings,
      ),
    });
    this.dispatchEvent(filterChangedEvent);
    this.closeDialog();
  };

  private closeDialog = () => {
    this.dialogEl.close();

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

  private generateMovieButtons() {
    const allChecked = this.selectedMovieIds.length === this.movies.length;
    return this.movies.map((movie) => {
      const movieButton = new SelectionListItemElement();
      movieButton.slot = 'selection-list';
      movieButton.setAttribute('label', movie.displayName);
      movieButton.setAttribute('value', movie.id.toString());
      if (!allChecked) {
        movieButton.setChecked(this.selectedMovieIds.includes(movie.id));
      }
      return movieButton;
    });
  }

  private generateCinemaButtons() {
    return this.cinemas.map((cinema) => {
      const cinemaButton = new CheckableButtonElement();
      cinemaButton.slot = 'cinema-selection';
      cinemaButton.setAttribute('label', cinema.displayName);
      cinemaButton.setAttribute('value', cinema.id.toString());
      cinemaButton.setAttribute('color', cinema.color);
      cinemaButton.setChecked(this.selectedCinemaIds.includes(cinema.id));
      cinemaButton.addEventListener(
        'click',
        this.handleCinemaSelectionChanged.bind(this),
      );
      return cinemaButton;
    });
  }

  private generateShowTimeDubTypeButtons() {
    return allShowTimeDubTypes.map((showTimeDubType) => {
      const showTimeDubTypeButton = new CheckableButtonElement();
      showTimeDubTypeButton.setAttribute(
        'label',
        getShowTimeDubTypeLabelString(showTimeDubType),
      );
      showTimeDubTypeButton.setAttribute(
        'value',
        showTimeDubType.valueOf().toString(),
      );
      showTimeDubTypeButton.setAttribute('color', '#000000');
      showTimeDubTypeButton.setAttribute('checked', '');
      showTimeDubTypeButton.slot = 'type-selection';
      showTimeDubTypeButton.setChecked(
        this.selectedDubTypes.includes(showTimeDubType),
      );
      showTimeDubTypeButton.addEventListener(
        'click',
        this.handleShowTimeDubTypeSelected.bind(this),
      );
      return showTimeDubTypeButton;
    });
  }

  private generateMovieRatingButtons() {
    return allMovieRatings.map((rating) => {
      const movieRatingButton = new CheckableButtonElement();
      movieRatingButton.setAttribute(
        'label',
        getMovieRatingLabelString(rating),
      );
      const movieRatingColor = movieRatingColors.get(rating);
      movieRatingButton.setAttribute('color', movieRatingColor ?? '#000000');
      movieRatingButton.setAttribute('value', rating.valueOf().toString());
      movieRatingButton.slot = 'rating-selection';
      movieRatingButton.setChecked(this.selectedRatings.includes(rating));
      movieRatingButton.addEventListener(
        'click',
        this.handleMovieRatingSelected.bind(this),
      );
      return movieRatingButton;
    });
  }
}

customElements.define('filter-modal', FilterModalElement);
