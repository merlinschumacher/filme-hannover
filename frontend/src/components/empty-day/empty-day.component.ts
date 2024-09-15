import html from "./empty-day.component.html?raw";
import css from "./empty-day.component.css" with { type: "css" };

const template = document.createElement("template");
template.innerHTML = html;

export default class EmptyDayElement extends HTMLElement {
  connectedCallback() {
    const shadow = this.attachShadow({ mode: "open" });
    shadow.appendChild(template.content.cloneNode(true));
    shadow.adoptedStyleSheets = [css];
  }
}

customElements.define("empty-day", EmptyDayElement);
