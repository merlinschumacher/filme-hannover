import html from "./day-list.component.html?raw";
import css from "./day-list.component.css" with { type: "css" };
import { EventData } from "../../models/EventData";
import EventItem from "../event-item/event-item.component";

const template = document.createElement("template");
template.innerHTML = html;

export default class DayListElement extends HTMLElement {
  static get observedAttributes() {
    return ["date", "duration", "eventcount"];
  }

  public EventData: EventData[] = [];
  private shadow: ShadowRoot;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: "open" });
    this.shadow.appendChild(template.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [css];
  }

  attributeChangedCallback(name: string, oldValue: string, newValue: string) {
    if (oldValue === newValue) return;

    switch (name) {
      case "date": {
        const date = new Date(newValue);
        const headerClass = this.getHeaderClass(date);
        if (headerClass) this.classList.add(headerClass);
        const dateString = date.toLocaleDateString([], {
          weekday: "long",
          year: "numeric",
          month: "long",
          day: "numeric",
        });
        this.shadow.safeQuerySelector(".header").textContent = dateString;
        break;
      }
      case "duration": {
        this.updateFooterText();
        break;
      }
      case "eventcount": {
        this.updateFooterText();
        break;
      }
    }
  }

  private updateFooterText() {
    const eventCount = this.getAttribute("eventcount") ?? 0;
    const duration = this.getAttribute("duration") ?? 0;
    const eventHours = +duration / 60;
    const footerText = `${eventCount.toString()} Vorführungen, ca. ${eventHours.toFixed(0)} h`;
    this.shadow.updateElement(".footer", (el) => (el.textContent = footerText));
  }

  private getHeaderClass(date: Date): string | undefined {
    const isToday = new Date().toDateString() === date.toDateString();
    const isSundayOrHoliday = date.getDay() === 0;
    if (isToday) {
      return "today";
    }
    if (isSundayOrHoliday) {
      return "sunday";
    }
    if (date.getDay() === 6) {
      return "saturday";
    }
  }

  static BuildElement(date: Date, events: EventData[]) {
    let eventCumulativeDuration = 0;
    const eventElements: EventItem[] = [];
    events.forEach((event) => {
      const eventItem = EventItem.BuildElement(event);
      eventItem.slot = "body";
      eventCumulativeDuration += +event.runtime;
      eventElements.push(eventItem);
    });

    const item = new DayListElement();
    item.setAttribute("date", date.toDateString());
    item.setAttribute("eventcount", events.length.toString());
    item.setAttribute("duration", eventCumulativeDuration.toString());
    item.replaceChildren(...eventElements);
    return item;
  }
}

customElements.define("day-list", DayListElement);
