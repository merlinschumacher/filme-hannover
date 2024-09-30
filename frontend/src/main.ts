import FilterService from './services/FilterService';
import ViewPortService from './services/ViewPortService';
import SwiperService from './services/SwiperService';
import Cinema from './models/Cinema';
import FilterBar from './components/filter-bar/filter-bar.component';
import FilterModal from './components/filter-modal/filter-modal.component';
import { ShowTimeDubType } from './models/ShowTimeDubType';
import { MovieRating } from './models/MovieRating';
import EventDataResult from './models/EventDataResult';

export class Application {
  private filterService: FilterService;
  private viewPortService: ViewPortService = new ViewPortService();
  private swiper: SwiperService;
  private nextVisibleDate: Date = new Date();
  private visibleDays: number = this.viewPortService.getVisibleDays() * 2;
  private appRootEl: HTMLElement = this.getAppRootEl();
  private filterBar: FilterBar;
  private filterModal: FilterModal;

  private getAppRootEl(): HTMLElement {
    const appRootEl = document.querySelector('#app-root');
    if (appRootEl) {
      return appRootEl as HTMLElement;
    }
    throw new Error('Failed to find app root element.');
  }

  public constructor() {
    this.swiper = new SwiperService();
    this.filterService = new FilterService();
    this.filterBar = new FilterBar();
    this.filterModal = new FilterModal();
    this.appRootEl.appendChild(this.swiper.GetSwiperElement());
    // this.swiper.onReachEnd = this.updateSwiper;
    console.log('Initializing application...');
    const cinemas = this.filterService.GetCinemas();
    document.adoptedStyleSheets = [this.buildCinemaStyleSheet(cinemas)];
    this.appRootEl.appendChild(this.filterBar);
    this.appRootEl.appendChild(this.filterModal);

    this.filterService.emitter.on('cinemaDataReady', (cinemas: Cinema[]) => {
      document.adoptedStyleSheets = [this.buildCinemaStyleSheet(cinemas)];
      const lastUpdateEl = document.querySelector('#lastUpdate');
      if (lastUpdateEl) {
        lastUpdateEl.textContent = this.filterService.getDataVersion();
      }
    });
    this.filterService.on('eventDataRady', (data) => {
      this.updateSwiper(data, true);
    });
    this.initFilter();
  }

  private initFilter(): void {
    this.filterModal.addEventListener('filterChanged', (event: Event) => {
      const customEvent = event as CustomEvent<{
        selectedCinemaIds: number[];
        selectedMovieIds: number[];
        selectedDubTypes: ShowTimeDubType[];
        selectedRatings: MovieRating[];
      }>;
      this.filterBar.setData(
        customEvent.detail.selectedCinemaIds,
        customEvent.detail.selectedMovieIds,
        customEvent.detail.selectedDubTypes,
        customEvent.detail.selectedRatings,
      );
      this.filterService
        .SetSelection(
          customEvent.detail.selectedCinemaIds,
          customEvent.detail.selectedMovieIds,
          customEvent.detail.selectedDubTypes,
          customEvent.detail.selectedRatings,
        )
        .then(() => {
          console.log('Filter changed.');
          console.debug('Cinemas:', customEvent.detail.selectedCinemaIds);
          console.debug('Movies:', customEvent.detail.selectedMovieIds);
          console.debug(
            'ShowTimeDubTypes:',
            customEvent.detail.selectedDubTypes,
          );
          console.debug('Ratings:', customEvent.detail.selectedRatings);
        })
        .catch((error: unknown) => {
          console.error('Failed to set filter.', error);
        });
    });
  }

  private updateSwiper = (
    eventDataResult: EventDataResult,
    replaceSlides = false,
  ): void => {
    if (replaceSlides) {
      this.nextVisibleDate = new Date();
      this.swiper.showLoading();
    }
    if (eventDataResult.EventData.size === 0) {
      if (replaceSlides) {
        this.swiper.NoResults();
      }
      return;
    }
    if (replaceSlides) {
      this.swiper.ReplaceEvents(eventDataResult.EventData);
    } else {
      this.swiper.AddEvents(eventDataResult.EventData);
    }
    // Set the last visible date to the last date in the event list
    const lastKey = [...eventDataResult.EventData.keys()].pop();
    if (lastKey) {
      lastKey.setDate(lastKey.getDate() + 1);
      this.nextVisibleDate = lastKey;
    }
  };

  private buildCinemaStyleSheet(cinemas: Cinema[]) {
    const style = new CSSStyleSheet();
    cinemas.forEach((cinema) => {
      style.insertRule(
        `.cinema-${cinema.id.toString()} { background-color: ${cinema.color}; }`,
      );
    });
    return style;
  }
}

document.addEventListener('DOMContentLoaded', () => {
  new Application();
});
