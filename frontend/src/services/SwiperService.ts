import { register, SwiperContainer } from "swiper/element";
import { Manipulation } from "swiper/modules";
import { Swiper, SwiperOptions } from "swiper/types";
import DayListElement from "../components/day-list/day-list.component";
import EmptyDayElement from "../components/emptyDay/empty-day.component";
import { EventData } from "../models/EventData";

register();

export default class SwiperService {

  public onReachEnd?: () => void;

  private swiper: Swiper;

  constructor() {
    const swiperEl = document.querySelector('swiper-container') as SwiperContainer;
    const swiperParams: SwiperOptions = {
      modules: [Manipulation],
      slidesPerView: "auto"
    };
    Object.assign(swiperEl, swiperParams);

    swiperEl.initialize();
    this.swiper = swiperEl.swiper;
  }

  private addSlide(slideContent: HTMLElement): void {
    const swiperSlide = document.createElement('swiper-slide');
    swiperSlide.appendChild(slideContent);
    this.swiper.appendSlide(swiperSlide);
  }

  public async SetEvents(eventDays: Map<Date, EventData[]>): Promise<void> {
    this.swiper.removeAllSlides();
    let lastDate = new Date();
    eventDays.forEach((dayEvents, date) => {

      if (!this.isConsecutiveDate(date, lastDate)) {
        this.addSlide(new EmptyDayElement());
      }
      const dayList = DayListElement.BuildElement(date, dayEvents);
      this.addSlide(dayList);
    });
    this.swiper.update();
    return Promise.resolve();
  }

  private isConsecutiveDate(first: Date, second: Date): boolean {
    const diff = first.getTime() - second.getTime();
    return Math.abs(diff) < 86400000;
  }

}
