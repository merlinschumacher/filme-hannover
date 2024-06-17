import "inter-ui/inter-variable-latin.css";
import './style.css';
import { db } from './models/CinemaDb';
import DayListElement from './components/day-list/day-list.component';
import { SwiperContainer, register } from "swiper/element";
import { Grid, Keyboard } from "swiper/modules";
import { SwiperOptions } from "swiper/types";
import "swiper/element/css/grid";
import FilterModal from "./components/filter-modal/filter-modal.component";
register();

const daySizeMap = new Map<number, number>([
  [400, 1],
  [600, 2],
  [800, 3],
  [1000, 4],
  [1200, 5],
]);

async function getVisibleDays() {
  var width = window.innerWidth;
  daySizeMap.forEach((value, key) => {
    if (width < key) {
      return value;
    }
  })
  return 4;
}

async function init() {
  const swiperEl = document.querySelector('swiper-container')!;

  db.Init();
  const app = document.querySelector('#app-root')!;
  const filterEl = new FilterModal();
  app.prepend(filterEl);


  const startDate = new Date();
  let endDate = new Date();
  const days = await getVisibleDays();
  endDate = new Date(endDate.setDate(endDate.getDate() + (days * 2)));
  const eventDays = await db.getEventsForDateRangeSplitByDay(startDate, endDate);
  await initSwiper(swiperEl);

  eventDays.forEach((dayEvents, date) => {
    const dayList = DayListElement.BuildElement(new Date(date), dayEvents);
    const swiperSlide = document.createElement('swiper-slide');
    swiperSlide.appendChild(dayList);
    swiperEl.appendChild(swiperSlide);
  });
}

async function initSwiper(swiperEl: SwiperContainer) {
  const days = await getVisibleDays();
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

async function initFilter(filterEl: FilterModal) {

}

init().then(() => { });
