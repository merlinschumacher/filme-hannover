import Cinema from '../models/Cinema';
import { EventData } from '../models/EventData';
import EventDataResult from '../models/EventDataResult';
import FilterServiceEvents from '../models/FilterServiceEvents';
import Movie from '../models/Movie';
import { allMovieRatings, MovieRating } from '../models/MovieRating';
import { createNanoEvents, Emitter } from 'nanoevents';
import {
  allShowTimeDubTypes,
  ShowTimeDubType,
} from '../models/ShowTimeDubType';
import CinemaDb from './CinemaDb';
import FilterSelection from '../models/FilterSelection';

export default class FilterService {
  emitter: Emitter;
  private db: CinemaDb;
  private availableCinemas: Cinema[] = [];
  private selectedMovieIds: number[] = [];
  private selectedCinemaIds: number[] = [];
  private selectedShowTimeDubTypes: ShowTimeDubType[] = allShowTimeDubTypes;
  private selectedRatings: MovieRating[] = allMovieRatings;
  private visibleDays = 0;
  private startDate: Date;

  public constructor() {
    this.emitter = createNanoEvents<FilterServiceEvents>();
    this.db = new CinemaDb();
    this.startDate = new Date();
  }

  loadData(): void {
    this.db
      .init()
      .then(() => {
        console.log('Database initialized.');
        this.emitter.emit('databaseReady', this.db.dataVersionDate);
        this.loadCinemaData();
        this.setInitialSelection()
          .then(() => {
            this.emitter.emit('dataReady');
          })
          .catch((error: unknown) => {
            console.error('Failed to initialize database.', error);
          });
      })
      .catch((error: unknown) => {
        console.error('Failed to initialize database.', error);
      });
  }

  private async setInitialSelection() {
    const movies = await this.db.GetMovieIds(this.selectedRatings);
    const cinemas = await this.db.GetCinemaIds();
    const selection = new FilterSelection(
      cinemas,
      movies,
      allShowTimeDubTypes,
      allMovieRatings,
    );
    await this.setSelection(selection);
  }

  private loadCinemaData() {
    this.db.cinemas
      .toArray()
      .then((cinemas) => {
        this.availableCinemas = cinemas;
        this.selectedCinemaIds = cinemas.map((c) => c.id);
        this.emitter.emit('cinemaDataReady', cinemas);
      })
      .catch((error: unknown) => {
        console.error('Failed to load cinemas.', error);
      });
  }

  on<E extends keyof FilterServiceEvents>(
    event: E,
    callback: FilterServiceEvents[E],
  ) {
    return this.emitter.on(event, callback);
  }

  public async getMovies(): Promise<Movie[]> {
    return await this.db.getMovies(this.selectedRatings);
  }

  public async getMovieCount(): Promise<number> {
    return await this.db.getTotalMovieCount();
  }

  public getCinemas(): Cinema[] {
    return this.availableCinemas;
  }

  public async getCinemaCount(): Promise<number> {
    return await this.db.getTotalCinemaCount();
  }

  public async setSelection(selection: FilterSelection): Promise<void> {
    this.setSelectedMovieRatings(selection.selectedRatings);
    await this.setSelectedCinemas(selection.selectedCinemaIds);
    await this.setSelectedMovies(selection.selectedMovieIds);
    this.setSelectedShowTimeDubTypes(selection.selectedDubTypes);
    this.startDate = new Date();

    await this.getEventData();
  }

  public getSelection(): FilterSelection {
    return new FilterSelection(
      this.selectedCinemaIds,
      this.selectedMovieIds,
      this.selectedShowTimeDubTypes,
      this.selectedRatings,
    );
  }

  public setDateRange(startDate: Date, visibleDays: number): void {
    this.startDate = startDate;
    this.visibleDays = visibleDays;
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

  public async getNextPage(): Promise<void> {
    await this.getEventData();
  }

  private async getEventData() {
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
        if (this.startDate.getTime() < firstShowTimeDate.getTime()) {
          this.startDate = firstShowTimeDate;
        }
        // To fill up the requested visible days, we need to get the nth day for that range.
        const endDate: Date = await this.db.getEndDate(
          this.startDate,
          this.selectedCinemaIds,
          this.selectedMovieIds,
          this.selectedShowTimeDubTypes,
          this.visibleDays,
        );

        return await this.db.getEventData(
          this.startDate,
          endDate,
          this.selectedCinemaIds,
          this.selectedMovieIds,
          this.selectedShowTimeDubTypes,
        );
      },
    );
    if (events.length === 0) {
      this.emitter.emit('eventDataReady', new EventDataResult(new Map(), null));
      return;
    }
    const lastEventTime = events[events.length - 1].startTime;
    this.startDate = new Date(lastEventTime);
    this.startDate.setSeconds(this.startDate.getSeconds() + 1);
    const splitEvents = this.splitEventsIntoDays(events);

    this.emitter.emit(
      'eventDataReady',
      new EventDataResult(splitEvents, lastEventTime),
    );
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
