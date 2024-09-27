import Cinema from '../models/Cinema';
import { EventData } from '../models/EventData';
import EventDataResult from '../models/EventDataResult';
import Movie from '../models/Movie';
import { allMovieRatings, MovieRating } from '../models/MovieRating';
import {
  allShowTimeDubTypes,
  ShowTimeDubType,
} from '../models/ShowTimeDubType';
import CinemaDb from './CinemaDb';

export default class FilterService {
  private db: CinemaDb;
  private availableCinemas: Cinema[] = [];
  private selectedMovies: Movie[] = [];
  private selectedCinemas: Cinema[] = [];
  private selectedShowTimeDubTypes: ShowTimeDubType[] = allShowTimeDubTypes;
  private selectedRatings: MovieRating[] = allMovieRatings;
  public resultListChanged?: () => void;

  private constructor(CinemaDb: CinemaDb) {
    this.db = CinemaDb;
  }

  public static async Create(): Promise<FilterService> {
    const db = await CinemaDb.Create();
    const service = new FilterService(db);
    await service.initialize();
    return service;
  }

  private async initialize(): Promise<void> {
    this.availableCinemas = await this.db.GetAllCinemas();
    this.selectedCinemas = this.availableCinemas;
    this.selectedMovies = await this.db.GetAllMovies(this.selectedRatings);
  }

  public async GetAllMovies(): Promise<Movie[]> {
    return await this.db.GetAllMovies(this.selectedRatings);
  }

  public GetAllCinemas(): Cinema[] {
    return this.availableCinemas;
  }

  public async SetSelection(
    cinemas: Cinema[],
    movies: Movie[],
    showTimeDubType: ShowTimeDubType[],
    ratings: MovieRating[],
  ): Promise<void> {
    this.setSelectedMovieRatings(ratings);
    await this.setSelectedCinemas(cinemas);
    await this.setSelectedMovies(movies);
    this.setSelectedShowTimeDubTypes(showTimeDubType);
    if (this.resultListChanged) {
      this.resultListChanged();
    }
  }

  private async setSelectedMovies(movies: Movie[]): Promise<void> {
    if (movies.length === 0) {
      this.selectedMovies = await this.db.GetAllMovies(this.selectedRatings);
    } else {
      this.selectedMovies = movies;
    }
    console.log('Selected movies:', this.selectedMovies);
  }

  private async setSelectedCinemas(cinemas: Cinema[]): Promise<void> {
    if (cinemas.length === 0 && this.selectedMovies.length === 0) {
      this.selectedCinemas = await this.db.GetAllCinemas();
    } else if (cinemas.length === 0) {
      this.selectedCinemas = this.availableCinemas;
    } else {
      this.selectedCinemas = cinemas;
    }
  }

  private setSelectedShowTimeDubTypes(
    showTimeDubTypes: ShowTimeDubType[],
  ): void {
    if (showTimeDubTypes.length === 0) {
      this.selectedShowTimeDubTypes = allShowTimeDubTypes;
    }
    this.selectedShowTimeDubTypes = showTimeDubTypes;
    console.log('Selected show time dub types:', showTimeDubTypes);
  }

  private setSelectedMovieRatings(ratings: MovieRating[]): void {
    if (ratings.length === 0) {
      this.selectedRatings = allMovieRatings;
    }
    this.selectedRatings = ratings;
    console.log('Selected ratings:', ratings);
  }

  public async GetEvents(
    startDate: Date,
    visibleDays: number,
  ): Promise<EventDataResult> {
    const selectedCinemaIds = this.selectedCinemas.map((c) => c.id);
    const selectedMovieIds = this.selectedMovies.map((m) => m.id);

    const events = await this.db.transaction(
      'r',
      this.db.showTimes,
      this.db.cinemas,
      this.db.movies,
      async () => {
        // Get the first showtime date, if the start date is before the first showtime date, set the start date to the first showtime date
        const firstShowTimeDate = await this.db.GetEarliestShowTimeDate(
          selectedCinemaIds,
          selectedMovieIds,
          this.selectedShowTimeDubTypes,
        );
        if (!firstShowTimeDate) {
          return [];
        }
        if (startDate.getTime() < firstShowTimeDate.getTime()) {
          startDate = firstShowTimeDate;
        }
        // To fill up the requested visible days, we need to get the nth day for that range.
        const endDate: Date = await this.db.GetEndDate(
          startDate,
          selectedCinemaIds,
          selectedMovieIds,
          this.selectedShowTimeDubTypes,
          visibleDays,
        );

        return await this.db.GetEventData(
          startDate,
          endDate,
          selectedCinemaIds,
          selectedMovieIds,
          this.selectedShowTimeDubTypes,
        );
      },
    );
    if (events.length === 0) {
      return new EventDataResult(new Map<Date, EventData[]>(), null);
    }
    const lastEventTime = events[events.length - 1].startTime;
    const splitEvents = this.splitEventsIntoDays(events);

    return new EventDataResult(splitEvents, lastEventTime);
  }

  public getDataVersion(): string {
    return this.db.dataVersionDate.toLocaleString();
  }

  private splitEventsIntoDays(eventData: EventData[]): Map<Date, EventData[]> {
    const eventsByDay = new Map<Date, EventData[]>();

    // The unique dates of the events
    const eventDates = Array.from(
      new Set(eventData.map((e) => e.date.toISOString())),
    ).map((d) => new Date(d));

    eventDates.forEach((date) => {
      eventsByDay.set(
        date,
        eventData.filter((e) => e.date.toISOString() === date.toISOString()),
      );
    });

    return eventsByDay;
  }
}
