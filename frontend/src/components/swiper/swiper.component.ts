import html from './swiper.component.tpl';
import css from './swiper.component.css?inline';
import noContentHtml from '../no-content/no-content.component.tpl';
import { ScrollSnapDraggable, ScrollSnapSlider } from 'scroll-snap-slider';
import '../../extensions/ShadowRootExtensions';
import ChevronBackward from '@material-symbols/svg-400/outlined/chevron_backward.svg?raw';
import ChevronForward from '@material-symbols/svg-400/outlined/chevron_forward.svg?raw';
import LoaderElement from '../loader/loader.component';
import EmptyDayElement from '../empty-day/empty-day.component';
import DayListElement from '../day-list/day-list.component';
import { EventData } from '../../models/EventData';

const styleSheet = new CSSStyleSheet();
styleSheet.replaceSync(css);

export default class SwiperElement extends HTMLElement {
  private scrollSnapSlider: ScrollSnapSlider;
  private scrollSnapSliderEl: HTMLElement;
  private triggeredScrollThreshold = false;
  private shadow: ShadowRoot;
  private slideCount = 1;
  private clickPosition = { x: 0, y: 0 };
  private loader: LoaderElement = new LoaderElement();
  private onReachendEnabled = false;

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
    this.loader.setAttribute('visible', 'true');
    this.shadow.prepend(this.loader);
    this.shadow.safeQuerySelector('#swipe-left').innerHTML = ChevronBackward;
    this.shadow.safeQuerySelector('#swipe-right').innerHTML = ChevronForward;
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
      if (this.onReachendEnabled) {
        this.dispatchEvent(
          new CustomEvent('scrollThresholdReached', { bubbles: true }),
        );
      }
    }
  };

  connectedCallback() {
    this.clearSlides();
    this.scrollSnapSlider.addEventListener('mousedown', this.handleMouseDown);
    this.scrollSnapSlider.addEventListener('click', this.handleClick);
    // Detect if the user has swiped to the last page and load more data
    this.scrollSnapSliderEl.addEventListener('scroll', () => {
      this.handleScrollThresholdReached();
    });
    this.shadow
      .safeQuerySelector('#swipe-left')
      .addEventListener('click', this.handleSwipeLeft);
    this.shadow
      .safeQuerySelector('#swipe-right')
      .addEventListener('click', this.handleSwipeRight);
  }

  disconnectedCallback() {
    this.scrollSnapSlider.removeEventListener(
      'mousedown',
      this.handleMouseDown,
    );
    this.scrollSnapSlider.removeEventListener('click', this.handleClick);
    this.shadow
      .safeQuerySelector('#swipe-left')
      .removeEventListener('click', this.handleSwipeLeft);
    this.shadow
      .safeQuerySelector('#swipe-right')
      .removeEventListener('click', this.handleSwipeRight);
  }

  addEvents(eventDays: Map<Date, EventData[]>) {
    const firstKey = eventDays.keys().next();
    if (firstKey.done) {
      if (this.scrollSnapSliderEl.children.length === 0) {
        this.showNoResults();
      }
      return;
    }
    let lastDate: Date = firstKey.value;
    eventDays.forEach((dayEvents, dateString) => {
      const date = new Date(dateString);
      if (!this.isConsecutiveDate(date, lastDate)) {
        this.appendSlide(new EmptyDayElement());
      }
      const dayList = DayListElement.BuildElement(date, dayEvents);
      this.appendSlide(dayList);
      lastDate = date;
    });
    this.onReachendEnabled = true;
  }

  private appendSlide(slide: HTMLElement): void {
    slide.slot = 'slides';
    slide.classList.add('scroll-snap-slide');
    this.scrollSnapSliderEl.appendChild(slide);
    this.slideCount++;
    this.triggeredScrollThreshold = false;
    this.hideLoader();
  }

  clearSlides(): void {
    this.showLoader();
    this.onReachendEnabled = false;
    this.scrollSnapSliderEl.replaceChildren();
    this.slideCount = 0;
    this.triggeredScrollThreshold = false;
  }

  showNoResults(): void {
    this.scrollSnapSliderEl.replaceChildren(
      noContentHtml.content.cloneNode(true),
    );
    this.slideCount = 0;
    this.triggeredScrollThreshold = false;
    this.hideLoader();
  }

  showLoader(): void {
    this.loader.setAttribute('visible', 'true');
    this.scrollSnapSliderEl.classList.add('disabled');
  }

  private hideLoader(): void {
    this.loader.removeAttribute('visible');
    this.scrollSnapSliderEl.classList.remove('disabled');
  }

  private isConsecutiveDate(first: Date, second: Date): boolean {
    const firstDate = new Date(
      first.getFullYear(),
      first.getMonth(),
      first.getDate(),
    );
    const secondDate = new Date(
      second.getFullYear(),
      second.getMonth(),
      second.getDate(),
    );

    const dstDiff = Math.abs(
      first.getTimezoneOffset() - second.getTimezoneOffset(),
    );
    const DAY_IN_MS = 1000 * 60 * 60 * 24 + dstDiff * 60 * 1000;

    const diff = firstDate.getTime() - secondDate.getTime();
    if (Math.abs(diff) <= DAY_IN_MS) {
      return true;
    }
    return false;
  }
}

customElements.define('swiper-element', SwiperElement);
