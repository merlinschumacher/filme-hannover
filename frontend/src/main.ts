import "inter-ui/inter-variable-latin.css";
import './style.css';
import DayListElement from './components/day-list/day-list.component';
import { SwiperContainer, register } from "swiper/element";
import { Grid, Keyboard } from "swiper/modules";
import { SwiperOptions } from "swiper/types";
import "swiper/element/css/grid";
import FilterModal from "./components/filter-modal/filter-modal.component";
import FilterService from "./services/FilterService";
import { Cinema } from "./models/Cinema";
register();

const daySizeMap = new Map<number, number>([
  [400, 1],
  [600, 2],
  [800, 3],
  [1000, 4],
  [1200, 5],
]);

const filterService = new FilterService();

function getVisibleDays() {
  var width = window.innerWidth;
  daySizeMap.forEach((value, key) => {
    if (width < key) {
      return value;
    }
  })
  return 4;
}

async function setUpdateEl() {
  const updateEl = document.querySelector('#lastUpdate');
  if (updateEl) {
    updateEl.textContent =  await filterService.getDataVersion();
  }
}

async function init() {
  const swiperEl = document.querySelector('swiper-container')!;
  initSwiper(swiperEl);
  await setUpdateEl();
  const cinemas = await filterService.getCinemas();
  const style = buildCinemaStyleSheet(cinemas);
  document.adoptedStyleSheets.push(style);
  initFilter();
}

function initSwiper(swiperEl: SwiperContainer) {
  const days = getVisibleDays();
  const swiperParams: SwiperOptions = {
    modules: [Grid, Keyboard],
    slidesPerView: days,
    slidesPerGroup: days,
    grid: {
      rows: 1,
      fill: 'column',
    },
    keyboard: {
      enabled: true,
    },
  };
  Object.assign(swiperEl, swiperParams);

  swiperEl.initialize();
}

function buildCinemaStyleSheet(cinemas: Cinema[]) {
  const style = new CSSStyleSheet();
  cinemas.forEach(cinema => {
    style.insertRule(`.cinema-${cinema.id} { background-color: ${cinema.color}; }`);
  });
  return style;
};

function getDateRange(): { startDate: Date, endDate: Date }{
    const startDate = new Date();
    const days = getVisibleDays();
    let endDate = new Date();
    endDate = new Date(endDate.setDate(endDate.getDate() + (days * 2)));
    return {startDate, endDate};
}

async function initFilter() : Promise<void> {
  var cinemas = await filterService.getCinemas();
  var movies = await filterService.getMovies();
  const filterModal = FilterModal.BuildElement(cinemas, movies);
  filterModal.onFilterChanged = async (cinemas, movies) => {
    filterService.setSelectedCinemas(cinemas);
    filterService.setSelectedMovies(movies);
    updateEvents();
  }
  updateEvents();
  const app = document.querySelector('#app-root')!;
  app.prepend(filterModal);
}

async function updateEvents() : Promise<void> {
    const { startDate, endDate } = getDateRange();
    const eventDays = await filterService.getEvents(startDate, endDate);
    const swiperEl = document.querySelector('swiper-container')!;
    const slides: HTMLElement[] = [];
    eventDays.forEach((dayEvents, date) => {
      const dayList = DayListElement.BuildElement(new Date(date), dayEvents);
      const swiperSlide = document.createElement('swiper-slide');
      swiperSlide.appendChild(dayList);
      slides.push(swiperSlide);
    });
    swiperEl.querySelectorAll('swiper-slide').forEach(el => el.remove());
    swiperEl.append(...slides);
}

init().then((): void => {
  console.log('App initialized.');
});
