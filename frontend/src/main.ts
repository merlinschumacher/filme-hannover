import FilterModal from "./components/filter-modal/filter-modal.component";
import FilterService from "./services/FilterService";
import ViewPortService from "./services/ViewPortService";
import SwiperService from "./services/SwiperService";
import Cinema from "./models/Cinema";

export class Application {
  private filterService!: FilterService;
  private viewPortService: ViewPortService = new ViewPortService();
  private swiper: SwiperService;
  private nextVisibleDate: Date = new Date();
  private visibleDays: number = this.viewPortService.getVisibleDays() * 2;
  private appRootEl: HTMLElement = this.getAppRootEl();

  private getAppRootEl(): HTMLElement {
    const appRootEl = document.querySelector("#app-root");
    if (appRootEl) {
      return appRootEl as HTMLElement;
    }
    throw new Error("Failed to find app root element.");
  }

  private constructor() {
    this.swiper = new SwiperService();
    FilterService.Create()
      .then((filterService) => {
        this.filterService = filterService;
        this.init();
        this.appRootEl.appendChild(this.swiper.GetSwiperElement());
        this.swiper.onReachEnd = this.updateSwiper;
        this.updateSwiper(true);
      })
      .catch((error: unknown) => {
        console.error("Failed to create filter service.", error);
      });
  }

  private init() {
    console.log("Initializing application...");
    const cinemas = this.filterService.GetAllCinemas();
    document.adoptedStyleSheets = [this.buildCinemaStyleSheet(cinemas)];
    const filterModal = this.initFilter();
    this.appRootEl.appendChild(filterModal);

    const lastUpdateEl = document.querySelector("#lastUpdate");
    if (lastUpdateEl) {
      lastUpdateEl.textContent = this.filterService.getDataVersion();
    }
  }

  public static Init(): Application {
    const app = new Application();
    return app;
  }

  private initFilter(): FilterModal {
    const movies = this.filterService.GetAllMovies();
    const cinemas = this.filterService.GetAllCinemas();
    const filterModal = FilterModal.BuildElement(cinemas, movies);
    filterModal.onFilterChanged = (cinemas, movies, showTimeDubTypes) => {
      this.nextVisibleDate = new Date();
      this.filterService
        .SetSelection(cinemas, movies, showTimeDubTypes)
        .then(() => {
          console.log("Filter changed.");
          console.debug("Cinemas:", cinemas);
          console.debug("Movies:", movies);
          console.debug("ShowTimeDubTypes:", showTimeDubTypes);
        })
        .catch((error: unknown) => {
          console.error("Failed to set selection.", error);
        });
    };
    this.filterService.resultListChanged = () => {
      this.updateSwiper(true);
    };
    return filterModal;
  }

  private updateSwiper = (replaceSlides = false): void => {
    if (replaceSlides) {
      this.nextVisibleDate = new Date();
      this.swiper.showLoading();
    }
    this.filterService
      .GetEvents(this.nextVisibleDate, this.visibleDays)
      .then((eventDataResult) => {
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
      })
      .catch((error: unknown) => {
        console.error("Failed to get events.", error);
      });
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

document.addEventListener("DOMContentLoaded", () => {
  Application.Init();
});
