import "inter-ui/inter-variable-latin.css";
import './style.css';
import "swiper/element/css/grid";
import FilterModal from "./components/filter-modal/filter-modal.component";
import FilterService from "./services/FilterService";
import { Cinema } from "./models/Cinema";
import ViewPortService from "./services/ViewPortService";
import { EventData } from "./models/EventData";
import SwiperService from "./services/SwiperService";

export class Application {
  private filterService: FilterService = null!;
  private viewPortService: ViewPortService = new ViewPortService();
  private swiper: SwiperService = new SwiperService();
  private lastVisibleDate: Date = new Date();
  private visibleDays: number = this.viewPortService.getVisibleDays();

  private constructor() {
    this.init().then(() => {
      this.updateSwiper();
    });
  }

  private async init() {
    this.filterService = await FilterService.Init();
    document.adoptedStyleSheets = [this.buildSlideStyleSheet(), this.buildCinemaStyleSheet(await this.filterService.getAllCinemas())];
    const filterModal = await this.initFilter();
    const filterModalEl = document.querySelector('#filterModal')!;
    filterModalEl.replaceWith(filterModal);
    // document.querySelector('#lastUpdate')!.textContent = await this.filterService.getDataVersion();
    this.swiper.onReachEnd = this.updateSwiper;
  }

  public static async Init(): Promise<Application> {
    return new Application();
  }

  private async initFilter() {
    const movies = await this.filterService.getAllMovies();
    const cinemas = await this.filterService.getAllCinemas();
    const filterModal = FilterModal.BuildElement(cinemas, movies);
    filterModal.onFilterChanged = async (cinemas, movies) => {
      await this.filterService.setSelectedCinemas(cinemas);
      await this.filterService.setSelectedMovies(movies);
    }
    this.filterService.resultListChanged = async () => this.updateSwiper();
    return filterModal;
  }

  private updateSwiper = async () => {
    const eventDays = await this.getEvents(this.lastVisibleDate, this.visibleDays);
    await this.swiper.SetEvents(eventDays);
  };

  private async getEvents(startDate: Date, visibleDays: number): Promise<Map<Date, EventData[]>> {
    const eventDays = await this.filterService.getEvents(startDate, visibleDays);
    // Set the last visible date to the last date in the event list
    this.lastVisibleDate = Array.from(eventDays.keys()).pop() ?? startDate;
    return eventDays;
  }

  private buildSlideStyleSheet(): CSSStyleSheet {
    const slideStyle = new CSSStyleSheet();
    const swiperSlideDefaultWidth = 100 / this.viewPortService.getVisibleDays();
    slideStyle.insertRule(`swiper-slide.default-slide { width: ${swiperSlideDefaultWidth}%; }`);
    slideStyle.insertRule(`swiper-slide.placeholder-slide { width: 3em; }`);
    return slideStyle;
  }

  private buildCinemaStyleSheet(cinemas: Cinema[]) {
    const style = new CSSStyleSheet();
    cinemas.forEach(cinema => {
      style.insertRule(`.cinema-${cinema.id} { background-color: ${cinema.color}; }`);
    });
    return style;
  };


}

Application.Init().then(app => console.log('Application initialized.', app));
