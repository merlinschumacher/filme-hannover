import DayListElement from "../components/day-list/day-list.component";
import EmptyDayElement from "../components/empty-day/empty-day.component";
import Swiper from "../components/swiper/swiper.component";
import { EventData } from "../models/EventData";

export default class SwiperService {
  public onReachEnd?: () => void;

  private swiper: Swiper = Swiper.BuildElement();


  private onReachendEnabled = false;

  public GetSwiperElement(): Swiper {
    return this.swiper;
  }

  constructor() {
    this.swiper.addEventListener("scroll-threshold-reached", () => {
      if (this.onReachEnd && this.onReachendEnabled) {
        this.onReachEnd();
      }
    });
  }

  public ClearSlider(): void {
    this.onReachendEnabled = false;
    this.swiper.removeAllSlides();
  }

  public NoResults(): void {
    this.swiper.displayNoResults();
  }

  public async AddEvents(eventDays: Map<Date, EventData[]>): Promise<void> {
    let lastDate = eventDays.keys().next().value;
    eventDays.forEach((dayEvents, dateString) => {
      const date = new Date(dateString);
      if (!this.isConsecutiveDate(date, lastDate)) {
        this.swiper.appendSlide(new EmptyDayElement());
      }
      const dayList = DayListElement.BuildElement(date, dayEvents);
      this.swiper.appendSlide(dayList);
      lastDate = date;
    });
    this.onReachendEnabled = true;
    return Promise.resolve();
  }

  public async ReplaceEvents(eventDays: Map<Date, EventData[]>): Promise<void> {

    if (eventDays.size === 0) {
      this.swiper.displayNoResults();
      return Promise.resolve();
    }

    let lastDate = eventDays.keys().next().value;
    const dateElements: HTMLElement[]  = [];
    eventDays.forEach((dayEvents, dateString) => {
      const date = new Date(dateString);
      if (!this.isConsecutiveDate(date, lastDate)) {
        dateElements.push(new EmptyDayElement());
      }
      const dayList = DayListElement.BuildElement(date, dayEvents);
      dateElements.push(dayList);
      lastDate = date;
    });
    this.swiper.replaceSlides(dateElements);
    this.onReachendEnabled = true;
    return Promise.resolve();
  }

  private isConsecutiveDate(first: Date, second: Date): boolean {
    const firstDate = new Date(first.getFullYear(), first.getMonth(), first.getDate());
    const secondDate = new Date(second.getFullYear(), second.getMonth(), second.getDate());
    const diff = (firstDate.getTime() - secondDate.getTime()) / (1000 * 60 * 60 * 24);
    if (diff === 0) {
      return true;
    }
    return Math.abs(diff) === 1;
  }
}
