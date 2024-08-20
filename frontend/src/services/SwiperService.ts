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
  private onReachendEnabled: boolean = false;

  constructor() {
    const swiperEl = document.querySelector(
      "swiper-container"
    ) as SwiperContainer;
    const swiperParams: SwiperOptions = {
      modules: [Manipulation],
      slidesPerView: "auto",
    };
    Object.assign(swiperEl, swiperParams);

    swiperEl.initialize();
    this.swiper = swiperEl.swiper;

    this.swiper.on("reachEnd", () => {
      if (this.onReachEnd && this.onReachendEnabled) {
        this.onReachEnd();
      }
    });
  }

  public ClearSlider(): void {
    this.onReachendEnabled = false;
    this.swiper.removeAllSlides();
  }

  private addSlide(slideContent: HTMLElement): void {
    const swiperSlide = document.createElement("swiper-slide");
    swiperSlide.appendChild(slideContent);
    this.swiper.appendSlide(swiperSlide);
  }

  public async SetEvents(eventDays: Map<Date, EventData[]>): Promise<void> {
    let lastDate = eventDays.keys().next().value;

    eventDays.forEach((dayEvents, dateString) => {
      const date = new Date(dateString);
      if (!this.isConsecutiveDate(date, lastDate)) {
        this.addSlide(new EmptyDayElement());
      }
      const dayList = DayListElement.BuildElement(date, dayEvents);
      this.addSlide(dayList);
      lastDate = date;
    });
    this.swiper.update();
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
