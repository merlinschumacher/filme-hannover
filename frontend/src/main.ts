import ViewPortService from './services/ViewPortService';
import Cinema from './models/Cinema';
import FilterBarElement from './components/filter-bar/filter-bar.component';
import FilterModalElement from './components/filter-modal/filter-modal.component';
import SwiperElement from './components/swiper/swiper.component';
import CinemaLegendElement from './components/cinema-legend/cinema-legend.component';
import FilterSelection from './models/FilterSelection';
import FilterServiceWorkerAdapter from './services/FilterServiceWorkerAdapter';

export class Application {
  private filterService: FilterServiceWorkerAdapter;
  private viewPortService: ViewPortService = new ViewPortService();
  private swiper: SwiperElement;
  private nextVisibleDate: Date;
  private visibleDays: number = this.viewPortService.getVisibleDays() * 2;
  private appRootEl: HTMLElement = this.getAppRootEl();
  private filterBar: FilterBarElement;
  private filterModal: FilterModalElement | null;
  private cinemaLegend: CinemaLegendElement;
  private firstPageLoadCompleted: boolean = false;

  private getAppRootEl(): HTMLElement {
    const appRootEl = document.querySelector('#app-root');
    if (appRootEl) {
      return appRootEl as HTMLElement;
    }
    throw new Error('Failed to find app root element.');
  }

  private removeFilterModal = () => {
    if (this.filterModal) {
      this.filterModal.remove();
      this.filterModal = null;
    }
  };

  private showFilterModal = async () => {
    this.filterModal = new FilterModalElement();
    this.filterModal.cinemas = await this.filterService.getCinemas();
    this.filterModal.slot = 'filter-modal';
    this.filterModal.addEventListener('filterChanged', this.filterChanged);
    this.filterModal.addEventListener('close', this.removeFilterModal);
    this.filterService
      .getMovies()
      .then(async (movies) => {
        if (this.filterModal) {
          console.debug('Setting movies in filter modal.');
          this.filterModal.movies = movies;
          this.filterModal.setSelection(await this.filterService.getSelection());

          this.appRootEl.appendChild(this.filterModal);
        }
      })
      .catch((error: unknown) => {
        console.error('Failed to get movies.', error);
      });
  };

  public constructor() {
    this.nextVisibleDate = new Date();
    this.filterService = new FilterServiceWorkerAdapter();
    this.filterBar = new FilterBarElement();
    this.filterModal = new FilterModalElement();
    this.filterBar.slot = 'filter-bar';
    this.filterModal.slot = 'filter-modal';
    this.cinemaLegend = new CinemaLegendElement();
    this.swiper = new SwiperElement();
    this.swiper.addEventListener('scrollThresholdReached', this.loadNextEvents);
    console.log('Initializing application...');
    this.appRootEl.appendChild(this.filterBar);
    this.appRootEl.appendChild(this.swiper);

    this.attachFilterServiceEventListeners();
  }

  private loadNextEvents = () => {
    void this.filterService.getNextPage();
  };

  private filterChanged = (event: Event) => {
    console.debug('Filter changed event.');
    const customEvent = event as CustomEvent<FilterSelection>;
    this.filterBar.setData(customEvent.detail);
    this.swiper.clearSlides();
    this.filterService
      .setSelection(customEvent.detail)
      .then(() => {
        console.debug('Filter changed.');
        console.debug('Cinemas:', customEvent.detail.selectedCinemaIds);
        console.debug('Movies:', customEvent.detail.selectedMovieIds);
        console.debug('ShowTimeDubTypes:', customEvent.detail.selectedDubTypes);
        console.debug('Ratings:', customEvent.detail.selectedRatings);
      })
      .catch((error: unknown) => {
        console.error('Failed to set filter.', error);
      });
  };

  private async setFilterBarValues() {
    const movieCount = await this.filterService.getMovieCount();
    this.filterBar.setAttribute('movies', movieCount.toString());
    const cinemaCount = await this.filterService.getCinemaCount();
    this.filterBar.setAttribute('cinemas', cinemaCount.toString());
  }

  private attachFilterServiceEventListeners() {
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
      void this.setFilterBarValues();
      this.filterBar.addEventListener('filterEditClick', this.showFilterModal);
    });

    this.filterService.on('eventDataReady', (data) => {
      console.log('Updating swiper.');
      this.swiper.addEvents(data.EventData);
      if (!this.firstPageLoadCompleted) {
        const footerEl = document.querySelector('footer');
        if (footerEl) {
          footerEl.style.display = 'unset';
        }
        this.firstPageLoadCompleted = true;
      }
    });

    this.filterService.setDateRange(this.nextVisibleDate, this.visibleDays);
    this.filterService.loadData();
  }

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
