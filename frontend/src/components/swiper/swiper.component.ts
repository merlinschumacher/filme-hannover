import html from './swiper.component.html?inline';
import css from './swiper.component.css?inline';

const style = new CSSStyleSheet();
style.replaceSync(css);
const template = document.createElement('template');
template.innerHTML = html;

export default class Swiper extends HTMLElement {
  constructor() {
    super();
  }

  private shadow: ShadowRoot = null!;

  connectedCallback() {
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(template.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [style];
  }

  disconnectedCallback() {
  }

  public appendSlide(slide: HTMLElement): void {
    slide.slot = 'slides';
    this.appendChild(slide);
  }

  public removeAllSlides(): void {
    this.shadow.querySelector('slot')?.assignedElements().forEach((el) => {
      el.remove();
    });
  }

  public static BuildElement( ): Swiper {
    const item = new Swiper();
    return item;
  }
}

customElements.define('swiper-element', Swiper);
