import "inter-ui/inter-variable-latin.css";
import "./style.css";
import FilterModal from "./components/filter-modal/filter-modal.component";
import FilterService from "./services/FilterService";
import ViewPortService from "./services/ViewPortService";
import SwiperService from "./services/SwiperService";
import Cinema from "./models/Cinema";

export class Application {
  private filterService: FilterService = null!;
  private viewPortService: ViewPortService = new ViewPortService();
  private swiper: SwiperService = null!;
  private lastVisibleDate: Date = new Date();
  private visibleDays: number = this.viewPortService.getVisibleDays() * 2;
  private appRootEl: HTMLElement = document.querySelector("#app-root")!;

  private constructor() {
    this.init().then(() => {
      this.swiper = new SwiperService();
      this.appRootEl.appendChild(this.swiper.GetSwiperElement());
    this.swiper.onReachEnd = this.updateSwiper;
    this.updateSwiper(true).then(() => {
    });
    });
  }

  private async init() {
    console.log("Initializing application...");
    this.filterService = await FilterService.Create();
    const cinemas = await this.filterService.GetAllCinemas();
    document.adoptedStyleSheets = [
      this.buildCinemaStyleSheet(cinemas),
    ];
    const filterModal = await this.initFilter();
    this.appRootEl.appendChild(filterModal);


    document.querySelector("#lastUpdate")!.textContent =
      await this.filterService.getDataVersion();
  }

  public static async Init(): Promise<Application> {
    const app = new Application();
    return app;
  }

  private async initFilter() {
    const movies = await this.filterService.GetAllMovies();
    const cinemas = await this.filterService.GetAllCinemas();
    const filterModal = FilterModal.BuildElement(cinemas, movies);
    filterModal.onFilterChanged = async (cinemas, movies, showTimeTypes) => {
      this.lastVisibleDate = new Date();
      await this.filterService.SetSelection(cinemas, movies, showTimeTypes);
    };
    this.filterService.resultListChanged = async () => {
      return this.updateSwiper(true);
    };
    return filterModal;
  }

  private updateSwiper = async (replaceSlides: boolean = false) => {
    const eventDataResult = await this.filterService.GetEvents(
      this.lastVisibleDate,
      this.visibleDays
    );
    if (eventDataResult.EventData.size === 0) {
      if (replaceSlides) {
        this.swiper.NoResults();
      };
      return;
    }
    // Set the last visible date to the last date in the event list
    const lastDate = new Date([...eventDataResult.EventData.keys()].pop()!);
    this.lastVisibleDate = new Date(lastDate.setDate(lastDate.getDate() + 1));
    if (replaceSlides) {
      this.swiper.ReplaceEvents(eventDataResult.EventData);
    } else {
      await this.swiper.AddEvents(eventDataResult.EventData);
    }
  };

  private buildCinemaStyleSheet(cinemas: Cinema[]) {
    const style = new CSSStyleSheet();
    cinemas.forEach((cinema) => {
      style.insertRule(
        `.cinema-${cinema.id} { background-color: ${cinema.color}; }`
      );
    });
    return style;
  }
}


document.addEventListener("DOMContentLoaded", () => {
Application.Init().then(() => console.log("Application running."));
});
