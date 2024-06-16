import "@fontsource-variable/inter";
import './style.css'
import { db } from './models/CinemaDb'
import DayListElement from './components/day-list/day-list.component';
import DayListContainerElement from "./components/day-list-container/day-list-container.component";

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
  db.Init();
  const startDate = new Date();
  let endDate = new Date();
  const days = await getVisibleDays();
  endDate = new Date(endDate.setDate(endDate.getDate() + days));

  const app = document.querySelector<HTMLDivElement>('#app')!;

  const eventDays = await db.getEventsForDateRangeSplitByDay(startDate, endDate);
  const dayListContainer = new DayListContainerElement();

  eventDays.forEach((dayEvents, date) => {
    const dayList = new DayListElement();
    dayList.setAttribute('date', date);
    dayList.slot = 'body';
    dayList.EventData = dayEvents;
    dayListContainer.appendChild(dayList);
  });
  app.appendChild(dayListContainer);
}

init().then(() => { });
