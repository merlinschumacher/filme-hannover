<dialog id="filter-dialog">
  <div id="filter-header">
    <h3>Filter bearbeiten</h3>
    <button id="close-filter">
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
  <button id="apply-filter">
    <span id="filter-apply-icon" class="icon"></span>Filter anwenden
  </button>
</dialog>
<div id="filter-controls">
  <button id="open-filter">
    <span id="filter-edit-icon" class="icon"></span>Filter bearbeiten
  </button>
  <div id="filter-info"></div>
</div>
<div id="cinema-legend">
  <slot name="cinema-legend"></slot>
</div>
