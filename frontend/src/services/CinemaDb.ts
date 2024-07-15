import Dexie, { type EntityTable } from 'dexie';
import { ShowTime } from "../models/ShowTime";
import { Movie } from "../models/Movie";
import { Cinema } from "../models/Cinema";
import { Configuration } from "../models/Configuration";
import { JsonData } from "../models/JsonData";
import { EventData } from "../models/EventData";

class HttpClient {
  protected constructor() { }

  static async getData(url: string) {
    try {
      let response = await fetch(url);
      if (!response.ok) throw response.statusText;
      return response
    } catch (e) {
      console.error(e);
      return null
    }
  }

  static async getJsonData(url: string) {
    try {
      let response = await this.getData(url);
      if (!response) return null;
      return await response.json();
    } catch (e) {
      console.error(e);
      return null;
    }
  }

  static async getDate(url: string) {
    try {
      let response = await this.getData(url);
      if (!response) return null;
      return new Date(await response.text());
    } catch (e) {
      console.error(e);
      return null;
    }
  }

}


export default class CinemaDb extends Dexie {
  configurations!: EntityTable<Configuration, 'key'>;
  cinemas!: EntityTable<Cinema, 'id'>;
  movies!: EntityTable<Movie, 'id'>;
  showTimes!: EntityTable<ShowTime, 'id'>;
  private readonly dataVersionKey: string = 'dataVersion';
  private readonly remoteDataUrl: string = '/data/data.json';
  private readonly remoteVersionDateUrl: string = '/data/data.json.update';

  public dataVersionDate: Date = new Date();

  constructor() {
    super('CinemaDb');
    this.version(1).stores({
      cinemas: '++id, displayName, url, shopUrl, color',
      movies: '++id, displayName, releaseDate, runtime',
      showTimes: '++id, startTime, endTime, movie, cinema, language, type',
      configurations: 'key, value'
    });

    this.init().then(() => {
      this.configurations.get(this.dataVersionKey).then(v => {
        this.dataVersionDate = new Date(v?.value || 0);
        console.log('Data version: ' + this.dataVersionDate);
      });
      this.cinemas.count().then(count => {
        console.log('Cinemas loaded: ' + count);
      });
      this.movies.count().then(count => {
        console.log('Movies loaded: ' + count);
      });
      this.showTimes.count().then(count => {
        console.log('Showtimes loaded: ' + count);
      });
    });
  }

  private async init() {
    const dataVersionChanged = await this.dataLoadingRequired();
    if (dataVersionChanged) {
      console.log('Data version changed, loading data.');
      await this.loadData();
    }
  }

  async dataLoadingRequired() {
    const dateString = await this.configurations.get(this.dataVersionKey);
    const currentVersionDate = dateString ? new Date(dateString.value) : undefined;
    const remoteVersionDate = await HttpClient.getDate(this.remoteVersionDateUrl);
    if (currentVersionDate == undefined || currentVersionDate !== remoteVersionDate) {
      await this.configurations.put({ key: this.dataVersionKey, value: remoteVersionDate });
      return true;
    }
    return false;
  }

  async loadData() {
    const response = await fetch(new URL(this.remoteDataUrl, window.location.href));
    const data: JsonData = await response.json();
    this.cinemas.bulkPut(data.cinemas);
    this.movies.bulkPut(data.movies);
    this.showTimes.bulkPut(data.showTimes);
  }

  async getCinemasForMovies(movies: Movie[]): Promise<Cinema[]> {
    const movieIds = movies.map(m => m.id);
    const showTimes = await this.showTimes.where('movie').anyOf(movieIds).toArray();
    const cinemaIds = showTimes.map(st => st.cinema);
    return this.cinemas.where('id').anyOf(cinemaIds).toArray();
  }

  async getMoviesForCinemas(cinemas: Cinema[]): Promise<Movie[]> {
    const cinemaIds = cinemas.map(c => c.id);
    const showTimes = await this.showTimes.where('cinema').anyOf(cinemaIds).toArray();
    const movieIds = showTimes.map(st => st.movie);
    return this.movies.where('id').anyOf(movieIds).toArray();
  }

  async getAllCinemas(): Promise<Cinema[]> {
    return this.cinemas.orderBy('displayName').toArray();
  }

  async getAllMovies(): Promise<Movie[]> {
    return this.movies.toArray();
  }
  async getAllMoviesOrderedByShowTimeCount(): Promise<Movie[]> {
    const startDateString = new Date().toISOString();
    // Get all showtimes that are in the future
    const showTimes = await this.showTimes.where('startTime').above(startDateString).toArray();
    // Count the number of showtimes for each movie
    const movieCountMap = new Map<number, number>();
    showTimes.forEach(st => {
      if (!movieCountMap.has(st.movie)) {
        movieCountMap.set(st.movie, 0);
      }
      movieCountMap.set(st.movie, movieCountMap.get(st.movie)! + 1);
    });
    // Sort the movies by the number of showtimes
    const movies = await this.movies.toArray();
    movies.sort((a, b) => {
      return (movieCountMap.get(b.id) || 0) - (movieCountMap.get(a.id) || 0);
    });

    return movies;
  }


  async getEvents(startDate: Date, endDate: Date, selectedCinemas: Cinema[], selectedMovies: Movie[]): Promise<Map<string, EventData[]>> {
    const startDateString = startDate.toISOString();
    const endDateString = endDate.toISOString();
    let showTimesQuery = this.showTimes.where('startTime').between(startDateString, endDateString);
    if (selectedCinemas.length > 0)
      showTimesQuery = showTimesQuery.and(item => selectedCinemas.some(e => e.id == item.cinema));
    if (selectedMovies.length > 0)
      showTimesQuery = showTimesQuery.and(item => selectedMovies.some(e => e.id == item.movie));

    const showTimes = await showTimesQuery.toArray();
    let events = await Promise.all(showTimes.map(async st => {
      const movie = await this.movies.get(st.movie);
      const cinema = await this.cinemas.get(st.cinema);
      return new EventData(
        st.startTime,
        st.endTime,
        movie!.displayName,
        movie!.runtime,
        cinema!.displayName,
        cinema!.color,
        st.url,
        st.language,
        st.type,
      )
    }));

    return this.splitEventsByDay(events);
  }

  // Splits events into days
  async splitEventsByDay(events: EventData[]): Promise<Map<string, EventData[]>> {
    const eventsByDay = new Map<string, EventData[]>();
    events.forEach(event => {
      const day = new Date(new Date(event.startTime).setUTCHours(0, 0, 0, 0)).toISOString();
      if (!eventsByDay.has(day)) {
        eventsByDay.set(day, []);
      }
      eventsByDay.get(day)!.push(event);
    });
    return eventsByDay;
  }

}

