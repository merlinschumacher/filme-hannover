
import Cinema from '../models/Cinema';
import { EventData } from '../models/EventData';
import EventDataResult from '../models/EventDataResult';
import Movie from '../models/Movie';
import { getAllShowTimeTypes, ShowTimeType } from '../models/ShowTimeType';
import CinemaDb from './CinemaDb';

export default class FilterService {
  private db: CinemaDb = null!
  private availableMovies: Movie[] = [];
  private availableCinemas: Cinema[] = [];
  private selectedMovies: Movie[] = [];
  private selectedCinemas: Cinema[] = [];
  private selectedShowTimeTypes: ShowTimeType[] = getAllShowTimeTypes();
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
    this.availableMovies = await this.db.GetAllMovies();
    this.availableCinemas = await this.db.GetAllCinemas();
    this.selectedCinemas = this.availableCinemas;
    this.selectedMovies = this.availableMovies;
  }

  public async GetAllMovies(): Promise<Movie[]> {
    if (this.selectedMovies.length === 0) {
      return this.availableMovies;
    }
    return this.availableMovies;
  }

  public async GetAllCinemas(): Promise<Cinema[]> {
    return this.availableCinemas;
  }

  public async SetSelection(cinemas: Cinema[], movies: Movie[], showTimeType: ShowTimeType[]): Promise<void> {
    await this.setSelectedCinemas(cinemas);
    await this.setSelectedMovies(movies);
    await this.setSelectedShowTimeTypes(showTimeType);
    if (this.resultListChanged) {
      this.resultListChanged();
    }
  }

  private async setSelectedMovies(movies: Movie[]): Promise<void> {
    if (movies.length === 0 && this.selectedCinemas.length === 0) {
      this.selectedMovies = await this.db.GetAllMovies();
    } else if (movies.length === 0) {
      this.selectedMovies = this.availableMovies;
    } else {
      this.selectedMovies = movies;
    }

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

  private async setSelectedShowTimeTypes(showTimeTypes: ShowTimeType[]): Promise<void> {
    if (showTimeTypes.length === 0) {
      this.selectedShowTimeTypes = getAllShowTimeTypes();
    }
    this.selectedShowTimeTypes = showTimeTypes;
  }

  public async GetEvents(startDate: Date, visibleDays: number): Promise<EventDataResult> {
    const selectedCinemaIds = this.selectedCinemas.map(c => c.id);
    const selectedMovieIds = this.selectedMovies.map(m => m.id);
    // Get the first showtime date, if the start date is before the first showtime date, set the start date to the first showtime date
    const firstShowTimeDate = await this.db.GetEarliestShowTimeDate(selectedCinemaIds, selectedMovieIds, this.selectedShowTimeTypes);
    if (!firstShowTimeDate) {
      return new EventDataResult(new Map<Date, EventData[]>(), null);
    }
    if (startDate.getTime() < firstShowTimeDate.getTime()) {
      startDate = firstShowTimeDate;
    }
    // To fill up the requested visible days, we need to get the nth day for that range.
    const endDate: Date = await this.db.GetEndDate(
      startDate,
      selectedCinemaIds,
      selectedMovieIds,
      this.selectedShowTimeTypes,
      visibleDays
    );

    const events = await this.db.GetEventData(startDate, endDate, selectedCinemaIds, selectedMovieIds, this.selectedShowTimeTypes);
    if (events.length === 0) {
      return new EventDataResult(new Map<Date, EventData[]>(), null);
    }
    const lastEventTime = events[events.length - 1].startTime;
    const splitEvents = await this.splitEventsIntoDays(events);

    return new EventDataResult(splitEvents, lastEventTime);
  }

  public async getDataVersion(): Promise<string> {
    return this.db.dataVersionDate.toLocaleString();
  }

  private async splitEventsIntoDays(
    eventData: EventData[],
  ): Promise<Map<Date, EventData[]>> {
    let eventsByDay = new Map<Date, EventData[]>();

    // The unique dates of the events
    const eventDates = Array.from(
      new Set(eventData.map((e) => e.date.toISOString()))
    ).map((d) => new Date(d));

    eventDates.forEach((date) => {
      eventsByDay.set(
        date,
        eventData.filter(
          (e) => e.date.toISOString() === date.toISOString()
        )
      );
    });

    return eventsByDay;
  }

}
