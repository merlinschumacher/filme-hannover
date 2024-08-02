import "inter-ui/inter-variable-latin.css";
import './style.css';
import DayListElement from './components/day-list/day-list.component';
import { SwiperContainer, register } from "swiper/element";
import { Manipulation } from "swiper/modules";
import { Swiper, SwiperOptions } from "swiper/types";
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
let swiper: Swiper = null!;
let lastVisibleDate = new Date();

function getVisibleDays(): number {
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
    updateEl.textContent = await filterService.getDataVersion();
  }
}

async function init() {
  await setUpdateEl();
  initSwiper();
  const cinemas = await filterService.getCinemas();
  const cinemaStyle = buildCinemaStyleSheet(cinemas);
  document.adoptedStyleSheets.push(cinemaStyle);
  const slideStyle = buildSlideStyleSheet();
  document.adoptedStyleSheets.push(slideStyle);
  initFilter();
}

function initSwiper() {

  const swiperEl = document.querySelector('swiper-container') as SwiperContainer;
  // const days = getVisibleDays();
  const swiperParams: SwiperOptions = {
    modules: [Manipulation],
    slidesPerView: "auto"
    // slidesPerGroup: days,
    // grid: {
    //   rows: 1,
    //   fill: 'column',
    // },
    // keyboard: {
    //   enabled: true,
    // },
  };
  Object.assign(swiperEl, swiperParams);

  swiperEl.initialize();
  swiper = swiperEl.swiper;
  swiper.on('reachEnd', async () => {
    await updateEvents();
  });
}

function buildSlideStyleSheet(): CSSStyleSheet {
  const slideStyle = new CSSStyleSheet();
  const swiperSlideDefaultWidth = 100 / getVisibleDays();
  slideStyle.insertRule(`swiper-slide.default-slide { width: ${swiperSlideDefaultWidth}%; }`);
  slideStyle.insertRule(`swiper-slide.placeholder-slide { width: 3em; }`);
  return slideStyle;
}

function buildCinemaStyleSheet(cinemas: Cinema[]) {
  const style = new CSSStyleSheet();
  cinemas.forEach(cinema => {
    style.insertRule(`.cinema-${cinema.id} { background-color: ${cinema.color}; }`);
  });
  return style;
};

function getNextDateRange(): { startDate: Date, visibleDays: number } {
  let startDate = lastVisibleDate;
  const visibleDays = getVisibleDays() * 2;
  return { startDate, visibleDays };
}

async function initFilter(): Promise<void> {
  var cinemas = await filterService.getCinemas();
  var movies = await filterService.getMovies();
  const filterModal = FilterModal.BuildElement(cinemas, movies);
  filterModal.onFilterChanged = async (cinemas, movies) => {
    await filterService.setSelectedCinemas(cinemas);
    await filterService.setSelectedMovies(movies);
    lastVisibleDate = new Date();
    swiper.removeAllSlides();
    await updateEvents();
  }
  updateEvents();
  const app = document.querySelector('#app-root')!;
  app.prepend(filterModal);
}

async function updateEvents(): Promise<void> {
  const { startDate, visibleDays } = getNextDateRange();
  await addEventSlides(startDate, visibleDays);
  swiper.update();
}
async function addEventSlides(startDate: Date, visibleDays: number): Promise<void> {
  const eventDays = await filterService.getEvents(startDate, visibleDays);
  // Get last date in the event list
  let lastDate = new Date([...eventDays.keys()].pop() ?? startDate).getTime();
  // Set the last visible date to the last date in the event list
  lastVisibleDate = new Date(lastDate);
  eventDays.forEach((dayEvents, date) => {
    const dateTime = new Date(date).getTime()
    const dateDiff = dateTime - lastDate;
    lastDate = dateTime;
    if (dateDiff > 86400000) {
      const placeholder = document.createElement('div');
      placeholder.classList.add('placeholder');
      const swiperSlide = document.createElement('swiper-slide');
      swiperSlide.classList.add('placeholder-slide');
      swiperSlide.appendChild(placeholder);
      swiper.appendSlide(swiperSlide);
    }

    const dayList = DayListElement.BuildElement(new Date(date), dayEvents);
    const swiperSlide = document.createElement('swiper-slide');
    swiperSlide.classList.add('default-slide');
    swiperSlide.appendChild(dayList);
    swiper.appendSlide(swiperSlide);
  });
}

init().then((): void => {
  console.log('App initialized.');
});
