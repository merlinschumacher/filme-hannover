import html from "./selection-list.component.html" with { type: "htmlTemplate" };
import css from "./selection-list.component.css" with { type: "css" };
import Movie from "../../models/Movie";
import SelectionListItemElement from "../selection-list-item/selection-list-item.component";

export default class SelectionListElement extends HTMLElement {
  public Movies: Movie[] = [];
  private SelectedMovies: Movie[] = [];
  public onSelectionChanged?: (movies: Movie[]) => void;

  private shadow: ShadowRoot;

  constructor() {
    super();
    this.shadow = this.attachShadow({ mode: "open" });
    this.shadow.appendChild(html.content.cloneNode(true));
    this.shadow.adoptedStyleSheets = [css];
  }
  private buildMovieButtons(movies: Movie[]): SelectionListItemElement[] {
    const options: SelectionListItemElement[] = [];
    movies.forEach((movie) => {
      const movieButton = new SelectionListItemElement();
      movieButton.slot = "selection-list";
      movieButton.setAttribute("label", movie.displayName);
      movieButton.setAttribute("value", movie.id.toString());
      movieButton.addEventListener("click", (ev: MouseEvent) => {
        const eventTarget = ev.target as SelectionListItemElement;
        const movieId = parseInt(eventTarget.getAttribute("value") ?? "0");

        if (this.SelectedMovies.some((m) => m.id === movieId)) {
          this.SelectedMovies = this.SelectedMovies.filter(
            (m) => m.id !== movieId,
          );
        } else {
          this.SelectedMovies.push(movie);
        }
        if (!this.onSelectionChanged) return;
        this.onSelectionChanged(this.SelectedMovies);
      });
      options.push(movieButton);
    });
    return options;
  }

  connectedCallback() {
    const options: SelectionListItemElement[] = [];
    const movieButtons = this.buildMovieButtons(this.Movies);

    options.push(...movieButtons);
    this.append(...options);

    const searchInput = this.shadow.safeQuerySelector(
      "input",
    ) as HTMLInputElement;
    searchInput.addEventListener("input", () => {
      this.searchMovies(searchInput.value);
    });
  }

  private searchMovies(searchTerm: string) {
    const options = this.querySelectorAll("selection-list-item");
    options.forEach((option: Element) => {
      const optionElement = option as SelectionListItemElement;
      const label = optionElement.getAttribute("label") ?? "";
      if (label.toLowerCase().includes(searchTerm.toLowerCase())) {
        optionElement.style.display = "block";
      } else {
        optionElement.style.display = "none";
      }
    });
  }

  public static BuildElement(movies: Movie[]): SelectionListElement {
    const item = new SelectionListElement();
    item.Movies = movies;
    return item;
  }
}

customElements.define("selection-list", SelectionListElement);
