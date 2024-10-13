import { allMovieRatings, MovieRating } from './MovieRating';
import { allShowTimeDubTypes, ShowTimeDubType } from './ShowTimeDubType';

export default class FilterSelection {
  constructor(
    selectedCinemaIds: number[],
    selectedMovieIds: number[],
    selectedDubTypes: ShowTimeDubType[],
    selectedRatings: MovieRating[],
  ) {
    this.selectedCinemaIds = selectedCinemaIds;
    this.selectedMovieIds = selectedMovieIds;
    this.selectedDubTypes = selectedDubTypes;
    this.selectedRatings = selectedRatings;
  }
  public selectedCinemaIds: number[] = [];
  public selectedMovieIds: number[] = [];
  public selectedDubTypes: ShowTimeDubType[] = allShowTimeDubTypes;
  public selectedRatings: MovieRating[] = allMovieRatings;
}
