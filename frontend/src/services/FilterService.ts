import Cinema from '../models/Cinema';
import { EventData } from '../models/EventData';
import EventDataResult from '../models/EventDataResult';
import { FilterServiceEvents } from '../models/Events';
import Movie from '../models/Movie';
import { allMovieRatings, MovieRating } from '../models/MovieRating';
import { createNanoEvents, Emitter } from 'nanoevents';
import {
  allShowTimeDubTypes,
  ShowTimeDubType,
} from '../models/ShowTimeDubType';
import CinemaDb from './CinemaDb';

export default class FilterService {
  emitter: Emitter;
  private db: CinemaDb;
  private availableCinemas: Cinema[] = [];
  private selectedMovieIds: number[] = [];
  private selectedCinemaIds: number[] = [];
  private selectedShowTimeDubTypes: ShowTimeDubType[] = allShowTimeDubTypes;
  private selectedRatings: MovieRating[] = allMovieRatings;

  public constructor() {
    this.emitter = createNanoEvents<FilterServiceEvents>();
    this.db = new CinemaDb();
    void this.db.cinemas.toArray().then((cinemas) => {
      this.availableCinemas = cinemas;
      this.emitter.emit('cinemaDataReady', cinemas);
    });
  }

  on<E extends keyof FilterServiceEvents>(
    event: E,
    callback: FilterServiceEvents[E],
  ) {
    return this.emitter.on(event, callback);
  }

  public async GetMovies(): Promise<Movie[]> {
    return await this.db.GetMovies(this.selectedRatings);
  }

  public async GetMovieCount(): Promise<number> {
    return await this.db.GetTotalMovieCount();
  }

  public GetCinemas(): Cinema[] {
    return this.availableCinemas;
  }

  public async GetCinemaCount(): Promise<number> {
    return await this.db.GetTotalCinemaCount();
  }

  public async SetSelection(
    cinemaIds: number[],
    movieIds: number[],
    showTimeDubType: ShowTimeDubType[],
    ratings: MovieRating[],
  ): Promise<void> {
    this.setSelectedMovieRatings(ratings);
    await this.setSelectedCinemas(cinemaIds);
    await this.setSelectedMovies(movieIds);
    this.setSelectedShowTimeDubTypes(showTimeDubType);
  }

  private async setSelectedMovies(movies: number[]): Promise<void> {
    if (movies.length === 0) {
      this.selectedMovieIds = await this.db.GetMovieIds(this.selectedRatings);
    } else {
      this.selectedMovieIds = movies;
    }
  }

  private async setSelectedCinemas(cinemas: number[]): Promise<void> {
    if (cinemas.length === 0) {
      this.selectedCinemaIds = await this.db.GetCinemaIds();
    } else {
      this.selectedCinemaIds = cinemas;
    }
  }

  private setSelectedShowTimeDubTypes(
    showTimeDubTypes: ShowTimeDubType[],
  ): void {
    if (showTimeDubTypes.length === 0) {
      this.selectedShowTimeDubTypes = allShowTimeDubTypes;
    }
    this.selectedShowTimeDubTypes = showTimeDubTypes;
  }

  private setSelectedMovieRatings(ratings: MovieRating[]): void {
    if (ratings.length === 0) {
      this.selectedRatings = allMovieRatings;
    }
    this.selectedRatings = ratings;
  }

  public async GetEvents(
    startDate: Date,
    visibleDays: number,
  ): Promise<EventDataResult> {
    const events = await this.db.transaction(
      'r',
      this.db.showTimes,
      this.db.cinemas,
      this.db.movies,
      async () => {
        // Get the first showtime date, if the start date is before the first showtime date, set the start date to the first showtime date
        const firstShowTimeDate = await this.db.GetEarliestShowTimeDate(
          this.selectedCinemaIds,
          this.selectedMovieIds,
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
          this.selectedCinemaIds,
          this.selectedMovieIds,
          this.selectedShowTimeDubTypes,
          visibleDays,
        );

        return await this.db.GetEventData(
          startDate,
          endDate,
          this.selectedCinemaIds,
          this.selectedMovieIds,
          this.selectedShowTimeDubTypes,
        );
      },
    );
    if (events.length === 0) {
      this.emitter.emit('eventDataRady', new EventDataResult(new Map(), null));
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
