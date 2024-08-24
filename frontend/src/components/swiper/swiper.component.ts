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
    this.scrollSnapSlider.addEventListener("slide-stop", function (event: Event) {
      console.log("slide-end", event);
      console.log(event.target);
      event.preventDefault();
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
    this.scrollSnapSliderEl.replaceChildren();
  }

  public static BuildElement(): Swiper {
    const item = new Swiper();
    return item;
  }
}

customElements.define("swiper-element", Swiper);
