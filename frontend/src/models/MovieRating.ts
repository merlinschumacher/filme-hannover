export enum MovieRating {
  FSK0 = 0,
  FSK6 = 6,
  FSK12 = 12,
  FSK16 = 16,
  FSK18 = 18,
  Unrated = 99,
  Unknown = -1,
}

export const allMovieRatings: MovieRating[] = [
  MovieRating.FSK0,
  MovieRating.FSK6,
  MovieRating.FSK12,
  MovieRating.FSK16,
  MovieRating.FSK18,
  MovieRating.Unrated,
  MovieRating.Unknown,
];

export const movieRatingColors: Map<MovieRating, string> = new Map<
  MovieRating,
  string
>([
  [MovieRating.FSK0, 'white'],
  [MovieRating.FSK6, '#ffe842'],
  [MovieRating.FSK12, '#33b540'],
  [MovieRating.FSK16, '#38a7e4'],
  [MovieRating.FSK18, '#ed1c24'],
  [MovieRating.Unrated, 'black'],
  [MovieRating.Unknown, 'gray'],
]);

export function getMovieRatingLabelString(movieRating: MovieRating): string {
  switch (movieRating) {
    case MovieRating.FSK0:
      return 'FSK 0';
    case MovieRating.FSK6:
      return 'FSK 6';
    case MovieRating.FSK12:
      return 'FSK 12';
    case MovieRating.FSK16:
      return 'FSK 16';
    case MovieRating.FSK18:
      return 'FSK 18';
    case MovieRating.Unrated:
      return 'Ohne Freigabe';
    default:
      return 'Unbekannt';
  }
}
