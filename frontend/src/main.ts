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
    this.filterBar.slot = 'filter-bar';
    this.filterModal.slot = 'filter-modal';
    this.filterBar.addEventListener('filterEditClick', () => {
      this.appRootEl.appendChild(this.filterModal);
    });
    this.filterModal.addEventListener('filterChanged', this.filterChanged);
    this.filterModal.addEventListener('close', () => {
      this.filterModal.remove();
    });
    this.cinemaLegend = new CinemaLegendElement();
    this.swiper = new SwiperElement();
    console.log('Initializing application...');
    this.appRootEl.appendChild(this.filterBar);
    this.appRootEl.appendChild(this.swiper);

    this.filterService.on('databaseReady', (dataVersion: Date) => {
      console.log('Database ready.');
      const lastUpdateEl = document.querySelector('#lastUpdate');
      if (lastUpdateEl) {
        lastUpdateEl.textContent = dataVersion.toLocaleString();
      }
    });

    this.filterService.on('cinemaDataReady', (data) => {
      this.cinemaLegend.setCinemaData(data);
      this.cinemaLegend.slot = 'cinema-legend';
      this.filterBar.appendChild(this.cinemaLegend);
      document.adoptedStyleSheets = [this.buildCinemaStyleSheet(data)];
    });

    this.filterService.on('dataReady', () => {
      console.log('Data ready.');
      this.filterModal.cinemas = this.filterService.getCinemas();
      this.filterService
        .getMovies()
        .then((movies) => {
          this.filterModal.movies = movies;
        })
        .catch((error: unknown) => {
          console.error('Failed to get movies.', error);
        });
    });

    this.filterService.on('eventDataReady', (data) => {
      this.updateSwiper(data);
    });

    this.filterModal.addEventListener('filterChanged', this.filterChanged);
    this.swiper.addEventListener('scrollThresholdReached', this.loadNextEvents);
    this.filterService.setDateRange(this.nextVisibleDate, this.visibleDays);
    this.filterService.loadData();
  }

  private loadNextEvents = () => {
    void this.filterService.getNextPage();
  };

  private filterChanged = (event: Event) => {
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
    this.swiper.clearSlides();
    this.filterService
      .setSelection(
        customEvent.detail.selectedCinemaIds,
        customEvent.detail.selectedMovieIds,
        customEvent.detail.selectedDubTypes,
        customEvent.detail.selectedRatings,
      )
      .then(() => {
        console.log('Filter changed.');
        console.debug('Cinemas:', customEvent.detail.selectedCinemaIds);
        console.debug('Movies:', customEvent.detail.selectedMovieIds);
        console.debug('ShowTimeDubTypes:', customEvent.detail.selectedDubTypes);
        console.debug('Ratings:', customEvent.detail.selectedRatings);
      })
      .catch((error: unknown) => {
        console.error('Failed to set filter.', error);
      });
  };

  private updateSwiper = (eventDataResult: EventDataResult): void => {
    console.log('Updating swiper.');
    this.swiper.addEvents(eventDataResult.EventData);
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
