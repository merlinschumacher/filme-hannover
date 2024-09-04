import html from "./swiper.component.html?raw";
import css from "./swiper.component.css?inline";
import noContentHtml from "../no-content/no-content.component.html?raw";
import { ScrollSnapDraggable, ScrollSnapSlider } from "scroll-snap-slider";
import '../../extensions/ShadowRootExtensions';
import ChevronBackward from "@material-symbols/svg-400/outlined/chevron_backward.svg?raw";
import ChevronForward from "@material-symbols/svg-400/outlined/chevron_forward.svg?raw";

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement("template");
template.innerHTML = html;
const noResultsElement = document.createElement("div");
noResultsElement.innerHTML = noContentHtml;

export default class Swiper extends HTMLElement {

  private scrollSnapSlider: ScrollSnapSlider;
  private scrollSnapSliderEl: HTMLElement;
  private triggeredScrollThreshold = false;
  private shadow: ShadowRoot;
  private slideCount = 1;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: "open" });
    this.shadow.appendChild(template.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [style];
    this.scrollSnapSliderEl = this.shadow.safeQuerySelector(".scroll-snap-slider");
    this.scrollSnapSlider = new ScrollSnapSlider({
      element: this.scrollSnapSliderEl,
      sizingMethod(slider) {
        if (slider.element.firstElementChild) {
          return slider.element.firstElementChild.clientWidth;
        }
        return 0;
      }
    }).with([
      new ScrollSnapDraggable
    ]);
  }

  connectedCallback() {
    this.removeAllSlides();
    this.shadow.safeQuerySelector("#swipe-left").innerHTML = ChevronBackward;
    this.shadow.safeQuerySelector("#swipe-right").innerHTML = ChevronForward;

    // Detect if the user has swiped to the last page and load more data
    this.scrollSnapSliderEl.addEventListener("scroll", () => {
      if (this.triggeredScrollThreshold) {
        return;
      }
      if (this.scrollSnapSliderEl.scrollLeft + this.scrollSnapSliderEl.clientWidth >= (this.scrollSnapSliderEl.scrollWidth / 2)) {
        this.triggeredScrollThreshold = true;
        this.dispatchEvent(new CustomEvent("scroll-threshold-reached", { bubbles: true }));
      }
    });

    this.shadowRoot?.safeQuerySelector("#swipe-left").addEventListener("click", () => {
      console.log("swipe left");
      const targetSlide = this.scrollSnapSlider.slide - 1
      if (targetSlide < 0) {
        return;
      }
      this.scrollSnapSlider.slideTo(targetSlide);
    });

    this.shadowRoot?.safeQuerySelector("#swipe-right").addEventListener("click", () => {
      console.log("swipe right");
      const targetSlide = this.scrollSnapSlider.slide + 1
      if (targetSlide >= this.slideCount) {
        return;
      }
      this.scrollSnapSlider.slideTo(targetSlide);
    });
  }

  public appendSlide(slide: HTMLElement): void {
    slide.slot = "slides";
    slide.classList.add("scroll-snap-slide");
    this.scrollSnapSliderEl.appendChild(slide);
    this.slideCount++;
    this.triggeredScrollThreshold = false;
  }

  public removeAllSlides(): void {
    this.scrollSnapSliderEl.replaceChildren();
    this.slideCount = 0;
    this.triggeredScrollThreshold = false;
  }

  public displayNoResults(): void {
    this.scrollSnapSliderEl.replaceChildren(noResultsElement);
    this.slideCount = 0;
    this.triggeredScrollThreshold = false;
  }

  public replaceSlides(slides: HTMLElement[]): void {
    slides.forEach((slide) => {
      slide.slot = "slides";
      slide.classList.add("scroll-snap-slide");
    });
    this.scrollSnapSliderEl.replaceChildren(...slides);
    this.scrollSnapSlider.slideTo(0);
    this.slideCount = slides.length;
    this.triggeredScrollThreshold = false;
  }

  public static BuildElement(): Swiper {
    const item = new Swiper();
    return item;
  }
}

customElements.define("swiper-element", Swiper);
