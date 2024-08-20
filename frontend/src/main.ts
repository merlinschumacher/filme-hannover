import "inter-ui/inter-variable-latin.css";
import "./style.css";
import "swiper/element/css/grid";
import FilterModal from "./components/filter-modal/filter-modal.component";
import FilterService from "./services/FilterService";
import ViewPortService from "./services/ViewPortService";
import SwiperService from "./services/SwiperService";
import Cinema from "./models/Cinema";

export class Application {
  private filterService: FilterService = null!;
  private viewPortService: ViewPortService = new ViewPortService();
  private swiper: SwiperService = new SwiperService();
  private lastVisibleDate: Date = new Date();
  private visibleDays: number = this.viewPortService.getVisibleDays() * 2;

  private constructor() {}

  private async init() {
    console.log("Initializing application...");
    this.filterService = await FilterService.Create();
    const cinemas = await this.filterService.GetAllCinemas();
    document.adoptedStyleSheets = [
      this.buildSlideStyleSheet(),
      this.buildCinemaStyleSheet(cinemas),
    ];
    const filterModalEl = document.querySelector("#filterModal")!;
    const filterModal = await this.initFilter();
    filterModalEl.replaceWith(filterModal);
    document.querySelector("#lastUpdate")!.textContent =
      await this.filterService.getDataVersion();
    this.swiper.onReachEnd = this.updateSwiper;
    await this.updateSwiper();
  }

  public static async Init(): Promise<Application> {
    const app = new Application();
    await app.init();
    return app;
  }

  private async initFilter() {
    const movies = await this.filterService.GetAllMovies();
    const cinemas = await this.filterService.GetAllCinemas();
    const filterModal = FilterModal.BuildElement(cinemas, movies);
    filterModal.onFilterChanged = async (cinemas, movies) => {
      this.lastVisibleDate = new Date();
      await this.filterService.SetSelection(cinemas, movies);
    };
    this.filterService.resultListChanged = async () => {
      this.swiper.ClearSlider();
      return this.updateSwiper();
    };
    return filterModal;
  }

  private updateSwiper = async () => {
    const eventDataResult = await this.filterService.GetEvents(
      this.lastVisibleDate,
      this.visibleDays
    );
    // Set the last visible date to the last date in the event list
    const lastDate = new Date([...eventDataResult.EventData.keys()].pop()!);
    this.lastVisibleDate = new Date(lastDate.setDate(lastDate.getDate() + 1));
    await this.swiper.SetEvents(eventDataResult.EventData);
  };

  private buildSlideStyleSheet(): CSSStyleSheet {
    const slideStyle = new CSSStyleSheet();
    const swiperSlideDefaultWidth = 100 / this.viewPortService.getVisibleDays();
    slideStyle.insertRule(
      `swiper-slide.default-slide { width: ${swiperSlideDefaultWidth}%; }`
    );
    slideStyle.insertRule(`swiper-slide.placeholder-slide { width: 3em; }`);
    return slideStyle;
  }

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

Application.Init().then(() => console.log("Application running."));
