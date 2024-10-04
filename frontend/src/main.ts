import FilterService from './services/FilterService';
import ViewPortService from './services/ViewPortService';
import Cinema from './models/Cinema';
import FilterBarElement from './components/filter-bar/filter-bar.component';
import FilterModalElement from './components/filter-modal/filter-modal.component';
import { ShowTimeDubType } from './models/ShowTimeDubType';
import { MovieRating } from './models/MovieRating';
import EventDataResult from './models/EventDataResult';
import SwiperElement from './components/swiper/swiper.component';
import CinemaLegendElement from './components/cinema-legend/cinema-legend.component';

export class Application {
  private filterService: FilterService;
  private viewPortService: ViewPortService = new ViewPortService();
  private swiper: SwiperElement;
  private nextVisibleDate: Date;
  private visibleDays: number = this.viewPortService.getVisibleDays() * 2;
  private appRootEl: HTMLElement = this.getAppRootEl();
  private filterBar: FilterBarElement;
  private filterModal: FilterModalElement;
  private cinemaLegend: CinemaLegendElement;

  private getAppRootEl(): HTMLElement {
    const appRootEl = document.querySelector('#app-root');
    if (appRootEl) {
      return appRootEl as HTMLElement;
    }
    throw new Error('Failed to find app root element.');
  }

  public constructor() {
    this.nextVisibleDate = new Date();
    this.filterService = new FilterService();
    this.filterBar = new FilterBarElement();
    this.filterModal = new FilterModalElement();
    this.cinemaLegend = new CinemaLegendElement();
    this.swiper = new SwiperElement();
    console.log('Initializing application...');
    const cinemas = this.filterService.GetCinemas();
    document.adoptedStyleSheets = [this.buildCinemaStyleSheet(cinemas)];
    this.appRootEl.appendChild(this.filterBar);
    this.appRootEl.appendChild(this.filterModal);
    this.appRootEl.appendChild(this.swiper);

    this.filterService.on('databaseReady', (dataVersion: Date) => {
      const lastUpdateEl = document.querySelector('#lastUpdate');
      if (lastUpdateEl) {
        lastUpdateEl.textContent = dataVersion.toLocaleString();
      }
    });
    this.filterService.on('eventDataReady', (data) => {
      this.updateSwiper(data, true);
    });
    this.filterService.on('cinemaDataReady', (data) => {
      this.cinemaLegend.setCinemaData(data);
      this.cinemaLegend.slot = 'cinema-legend';
      this.filterBar.appendChild(this.cinemaLegend);
    });
    this.swiper.addEventListener('scrollThresholdReached', this.loadNextEvents);
    this.filterService.loadData();
    this.initFilter();
  }

  private loadNextEvents = () => {
    this.filterService
      .getEventData(this.nextVisibleDate, this.visibleDays)
      .then((data) => {
        this.updateSwiper(data);
      })
      .catch((error: unknown) => {
        console.error('Failed to get event data.', error);
      });
  };

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
      this.swiper.toggleLoading();
    }
    if (eventDataResult.EventData.size === 0) {
      if (replaceSlides) {
        this.swiper.showNoResults();
      }
      return;
    }
    if (replaceSlides) {
      this.swiper.replaceEvents(eventDataResult.EventData);
    } else {
      this.swiper.addEvents(eventDataResult.EventData);
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
