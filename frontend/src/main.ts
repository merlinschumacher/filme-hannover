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
  private appRootEl: HTMLElement;
  private filterBar: FilterBarElement;
  private filterModal: FilterModalElement | null = null;
  private cinemaLegend: CinemaLegendElement;
  private firstPageLoadCompleted: boolean = false;

  public constructor(appRootEl: HTMLElement) {
    this.appRootEl = appRootEl;
    this.nextVisibleDate = new Date();
    this.filterService = new FilterServiceWorkerAdapter();
    this.filterBar = new FilterBarElement();
    this.cinemaLegend = new CinemaLegendElement();
    this.swiper = new SwiperElement();
    this.swiper.addEventListener('scrollThresholdReached', this.loadNextEvents);
    this.filterBar.slot = 'filter-bar';
    this.cinemaLegend.slot = 'cinema-legend';
    this.appRootEl.appendChild(this.filterBar);
    this.appRootEl.appendChild(this.swiper);

    this.attachFilterServiceEventListeners();
    this.attachUIEventListeners();
  }

  private removeFilterModal = () => {
    if (this.filterModal) {
      this.filterModal.remove();
      this.filterModal = null;
    }
  };

  private async createAndShowFilterModal() {
    const modal = new FilterModalElement();
    modal.cinemas = await this.filterService.getCinemas();
    modal.slot = 'filter-modal';
    modal.addEventListener('filterChanged', this.filterChanged);
    modal.addEventListener('close', this.removeFilterModal);
    try {
      const movies = await this.filterService.getMovies();
      modal.movies = movies;
      modal.setSelection(await this.filterService.getSelection());
      this.appRootEl.appendChild(modal);
      this.filterModal = modal;
    } catch (error) {
      console.error('Failed to get movies.', error);
    }
  }

  private showFilterModal = () => {
    // Only allow one modal at a time
    if (!this.filterModal) {
      void this.createAndShowFilterModal();
    }
  };

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

  private attachUIEventListeners() {
    this.filterBar.addEventListener('filterEditClick', this.showFilterModal);
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

function bootstrap() {
  document.addEventListener('DOMContentLoaded', () => {
    const appRootEl = document.querySelector('#app-root');
    if (!appRootEl) {
      throw new Error('Failed to find app root element.');
    }
    new Application(appRootEl as HTMLElement);
  });
}

bootstrap();
