<dialog id="filter-dialog">
  <div id="filter-header">
    <h3>Filter bearbeiten</h3>
    <button id="close-filter" class="action-button">
      <span id="filter-close-icon" class="icon"></span>
    </button>
  </div>
  <div class="movie-selection">
    <slot name="movie-selection"></slot>
  </div>
  <h4>Kinoauswahl</h4>
  <div class="cinema-selection">
    <slot name="cinema-selection"></slot>
  </div>
  <h4>Vorstellungsarten</h4>
  <div class="type-selection">
    <slot name="type-selection"></slot>
  </div>
  <h4>Altersfreigaben</h4>
  <div class="rating-selection">
    <slot name="rating-selection"></slot>
  </div>
  <button id="apply-filter" class="action-button">
    <span id="filter-apply-icon" class="icon"></span>Filter anwenden
  </button>
</dialog>
