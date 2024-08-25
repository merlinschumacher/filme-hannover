import html from "./swiper.component.html?raw";
import css from "./swiper.component.css?inline";
import { ScrollSnapDraggable, ScrollSnapSlider } from "scroll-snap-slider";

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement("template");
template.innerHTML = html;

export default class Swiper extends HTMLElement {
  constructor() {
    super();
  }

  private shadow: ShadowRoot = null!;
  private scrollSnapSlider: ScrollSnapSlider = null!;
  private scrollSnapSliderEl: HTMLElement = null!;
  private triggeredScrollThreshold: boolean = false;

  connectedCallback() {
    this.shadow = this.attachShadow({ mode: "open" });
    this.shadow.appendChild(template.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [style];
    this.scrollSnapSliderEl = this.shadow.querySelector(".scroll-snap-slider") as HTMLElement;
    this.scrollSnapSlider = new ScrollSnapSlider({
      element: this.scrollSnapSliderEl,
      sizingMethod (slider) {
        if (slider.element.firstElementChild) {
          return slider.element.firstElementChild.clientWidth;
        }
        return 0;
      }
    }).with([
      new ScrollSnapDraggable
    ]);
    this.removeAllSlides();

    // Detect if the user has swiped to the last page and load more data
    this.scrollSnapSliderEl.addEventListener("scroll", () => {
      if (this.triggeredScrollThreshold) {
        return;
      }
      if (this.scrollSnapSliderEl.scrollLeft + this.scrollSnapSliderEl.clientWidth >= (this.scrollSnapSliderEl.scrollWidth / 2 )) {
        this.triggeredScrollThreshold = true;
        this.dispatchEvent(new CustomEvent("scroll-threshold-reached", { bubbles: true }));
      }
    });
  }

  disconnectedCallback() {}

  public appendSlide(slide: HTMLElement): void {
    slide.slot = "slides";
    slide.classList.add("scroll-snap-slide");
    this.scrollSnapSliderEl.appendChild(slide);
    this.triggeredScrollThreshold = false;
  }

  public removeAllSlides(): void {
    this.scrollSnapSliderEl.replaceChildren();
  }

  public static BuildElement(): Swiper {
    const item = new Swiper();
    return item;
  }
}

customElements.define("swiper-element", Swiper);
