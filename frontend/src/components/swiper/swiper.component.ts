import html from './swiper.component.tpl';
import css from './swiper.component.css?inline';
import noContentHtml from '../no-content/no-content.component.tpl';
import { ScrollSnapDraggable, ScrollSnapSlider } from 'scroll-snap-slider';
import '../../extensions/ShadowRootExtensions';
import ChevronBackward from '@material-symbols/svg-400/outlined/chevron_backward.svg?raw';
import ChevronForward from '@material-symbols/svg-400/outlined/chevron_forward.svg?raw';
import loaderAnimation from '../../assets/loading.gif';
html.content.querySelector('#animation')?.setAttribute('src', loaderAnimation);

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class Swiper extends HTMLElement {
  private scrollSnapSlider: ScrollSnapSlider;
  private scrollSnapSliderEl: HTMLElement;
  private triggeredScrollThreshold = false;
  private shadow: ShadowRoot;
  private slideCount = 1;
  private clickPosition = { x: 0, y: 0 };

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: 'open' });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [styleSheet];
    this.scrollSnapSliderEl = this.shadow.safeQuerySelector(
      '.scroll-snap-slider',
    );
    this.scrollSnapSlider = new ScrollSnapSlider({
      element: this.scrollSnapSliderEl,
      sizingMethod(slider) {
        if (slider.element.firstElementChild) {
          return slider.element.firstElementChild.clientWidth;
        }
        return 0;
      },
    }).with([new ScrollSnapDraggable()]);
  }

  private handleMouseDown = (event: MouseEvent) => {
    this.clickPosition = { x: event.clientX, y: event.clientY };
  };
  private handleClick = (event: MouseEvent) => {
    const clickPositionDiffX = Math.abs(this.clickPosition.x - event.clientX);
    const clickPositionDiffY = Math.abs(this.clickPosition.y - event.clientY);

    if (clickPositionDiffX > 10 || clickPositionDiffY > 10) {
      event.preventDefault();
      return;
    }
  };

  private handleSwipeRight = () => {
    const targetSlide = this.scrollSnapSlider.slide + 1;
    if (targetSlide >= this.slideCount) {
      return;
    }
    this.scrollSnapSlider.slideTo(targetSlide);
  };

  private handleSwipeLeft = () => {
    const targetSlide = this.scrollSnapSlider.slide - 1;
    if (targetSlide < 0) {
      return;
    }
    this.scrollSnapSlider.slideTo(targetSlide);
  };

  private handleScrollThresholdReached = () => {
    if (this.triggeredScrollThreshold) {
      return;
    }
    if (
      this.scrollSnapSliderEl.scrollLeft +
        this.scrollSnapSliderEl.clientWidth >=
      this.scrollSnapSliderEl.scrollWidth / 2
    ) {
      this.triggeredScrollThreshold = true;
      this.dispatchEvent(
        new CustomEvent('scroll-threshold-reached', { bubbles: true }),
      );
    }
  };

  connectedCallback() {
    this.removeAllSlides();
    this.shadow.safeQuerySelector('#swipe-left').innerHTML = ChevronBackward;
    this.shadow.safeQuerySelector('#swipe-right').innerHTML = ChevronForward;

    this.scrollSnapSlider.addEventListener('mousedown', this.handleMouseDown);
    this.scrollSnapSlider.addEventListener('click', this.handleClick);
    // Detect if the user has swiped to the last page and load more data
    this.scrollSnapSliderEl.addEventListener('scroll', () => {
      this.handleScrollThresholdReached();
    });
    this.shadowRoot
      ?.safeQuerySelector('#swipe-left')
      .addEventListener('click', this.handleSwipeLeft);
    this.shadowRoot
      ?.safeQuerySelector('#swipe-right')
      .addEventListener('click', this.handleSwipeRight);
  }

  disconnectedCallback() {
    const link = this.shadow.safeQuerySelector('.link');
    link.removeEventListener('mousedown', this.handleMouseDown);
    link.removeEventListener('click', this.handleClick);
  }

  appendSlide(slide: HTMLElement): void {
    slide.slot = 'slides';
    slide.classList.add('scroll-snap-slide');
    this.scrollSnapSliderEl.appendChild(slide);
    this.slideCount++;
    this.triggeredScrollThreshold = false;
  }

  removeAllSlides(): void {
    this.scrollSnapSliderEl.replaceChildren();
    this.slideCount = 0;
    this.toggleLoading();
    this.triggeredScrollThreshold = false;
  }

  displayNoResults(): void {
    this.scrollSnapSliderEl.replaceChildren(
      noContentHtml.content.cloneNode(true),
    );
    this.slideCount = 0;
    this.toggleLoading();
    this.triggeredScrollThreshold = false;
  }

  private toggleLoading(): void {
    this.shadow.safeQuerySelector('.loading').classList.toggle('hidden');
    this.scrollSnapSliderEl.classList.toggle('disabled');
  }

  showLoading(): void {
    this.toggleLoading();
  }

  replaceSlides(slides: HTMLElement[]): void {
    slides.forEach((slide) => {
      slide.slot = 'slides';
      slide.classList.add('scroll-snap-slide');
    });
    this.scrollSnapSliderEl.replaceChildren(...slides);
    this.scrollSnapSlider.slideTo(0);
    this.toggleLoading();
    this.slideCount = slides.length;
    this.triggeredScrollThreshold = false;
  }

  static BuildElement(): Swiper {
    const item = new Swiper();
    return item;
  }
}

customElements.define('swiper-element', Swiper);
