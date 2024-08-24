import html from "./swiper.component.html?inline";
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

  connectedCallback() {
    this.shadow = this.attachShadow({ mode: "open" });
    this.shadow.appendChild(template.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [style];
    this.scrollSnapSliderEl = this.shadow.querySelector(".scroll-snap-slider") as HTMLElement;
    this.scrollSnapSlider = new ScrollSnapSlider({
      element: this.scrollSnapSliderEl,
    }).with([
      new ScrollSnapDraggable
    ]);
    this.scrollSnapSlider.addEventListener("slide-start", function (event: Event) {
    });
    this.removeAllSlides();
  }

  disconnectedCallback() {}

  public appendSlide(slide: HTMLElement): void {
    slide.slot = "slides";
    slide.classList.add("scroll-snap-slide");
    this.scrollSnapSliderEl.appendChild(slide);


  }

  public removeAllSlides(): void {
    this.scrollSnapSliderEl.childNodes.forEach((child) => {
      this.scrollSnapSliderEl.removeChild(child);
    });
  }

  public static BuildElement(): Swiper {
    const item = new Swiper();
    return item;
  }
}

customElements.define("swiper-element", Swiper);
