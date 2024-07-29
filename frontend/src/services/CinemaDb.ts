import Dexie, { Table, type EntityTable } from 'dexie';
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
  cinemas!: Table<Cinema, number>;
  movies!: Table<Movie, number>;
  showTimes!: Table<ShowTime, number>;
  private readonly dataVersionKey: string = 'dataVersion';
  private readonly remoteDataUrl: string = '/data/data.json';
  private readonly remoteVersionDateUrl: string = '/data/data.json.update';

  public dataVersionDate: Date = new Date();

  constructor() {
    super('CinemaDb');
    this.version(1).stores({
      cinemas: 'id, displayName, url, shopUrl, color',
      movies: 'id, displayName, releaseDate, runtime',
      showTimes: 'id, startTime, endTime, movie, cinema, language, type, [startTime+cinema+movie]',
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
    this.delete({ disableAutoOpen: false });
    this.cinemas.bulkAdd(data.cinemas);
    this.movies.bulkAdd(data.movies);
    this.showTimes.bulkAdd(data.showTimes);
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

  private async getFirstShowTimeDate(selectedCinemas: Cinema[], selectedMovies: Movie[]): Promise<Date> {
    let showTimesQuery = this.showTimes.where('startTime').above(new Date().toISOString());
    if (selectedCinemas.length > 0)
      showTimesQuery = showTimesQuery.and(item => selectedCinemas.some(e => e.id == item.cinema));
    if (selectedMovies.length > 0)
      showTimesQuery = showTimesQuery.and(item => selectedMovies.some(e => e.id == item.movie));

    const showTimes = await showTimesQuery.toArray();
    const firstShowTime = showTimes.reduce((prev, current) => {
      return (prev.startTime < current.startTime) ? prev : current;
    });
    return new Date(firstShowTime.startTime);

  }



  async getEvents(startDate: Date, visibleDays: number, selectedCinemaIds: number[], selectedMovieIds: number[]): Promise<Map<string, EventData[]>> {

    // // Get the first showtime date, if the start date is before the first showtime date, set the start date to the first showtime date
    // const firstShowTimeDate = await this.getFirstShowTimeDate(selectedCinemas, selectedMovies);
    // if (startDate < firstShowTimeDate) {
    //   startDate = firstShowTimeDate;
    // }

    // // Calculate the end date
    // let endDate = new Date(startDate);
    // endDate.setDate(endDate.getDate() + visibleDays);

    // Convert the dates to ISO strings for querying the database
    // const startDateString = startDate.toISOString();
    // const endDateString = endDate.toISOString();

    // Query the database for showtimes between the start and end date
    // let showTimesQuery = this.showTimes.where('startTime').between(startDateString, endDateString);
    // if (selectedCinemas.length > 0)
    //   showTimesQuery = showTimesQuery.and(item => selectedCinemas.some(e => e.id == item.cinema));
    // if (selectedMovies.length > 0)
    //   showTimesQuery = showTimesQuery.and(item => selectedMovies.some(e => e.id == item.movie));
    // let showTimesQuery = this.showTimes.where('startTime').above(startDateString);
    // selectedCinemas.forEach(cinema => {
    //   selectedMovies.forEach(movie=> {
    //     showTimesQuery = showTimesQuery.or(e => e.movie == movie.id).and(e => e.cinema == cinema.id);
    //   }
    // });
    // let selectedCinemaIds = selectedCinemas.map(c => c.id);
    // let selectedMovieIds = selectedMovies.map(m => m.id);
    // // let selectedCinemaMovieCombinations = selectedCinemas.map(c => selectedMovies.map(m => ({ cinema: c.id, movie: m.id }))).flat();
    // // console.log(selectedCinemaMovieCombinations);
    // let selectedCinemaMovieCombinations = selectedCinemas.map(c => selectedMovies.map(m => [c.id, m.id])).flat();



    // // let showTimesQuery = this.showTimes.where('[cinema],movie]').anyOf([selectedCinemaIds, selectedMovieIds]).and(item => item.startTime > startDate && item.startTime < endDate);
    // // let showTimesQuery = this.showTimes.where('cinema').anyOf(selectedCinemaIds).and(item => item.startTime > startDate && item.startTime < endDate);
    // let showTimesQuery = this.showTimes.where('[cinema+movie]').anyOf(selectedCinemaMovieCombinations).and(item => item.startTime > startDate && item.startTime < endDate);
    // // if (selectedCinemaIds.length > 0 && selectedMovieIds.length > 0) {
    // //   selectedCinemaIds.forEach(cId => {
    // //     // showTimesQuery = showTimesQuery.and(s => s.cinema == cId).and( s => selectedMovieIds.some(m => m == s.movie));
    // //     selectedMovieIds.forEach(mId => {
    // //       showTimesQuery = showTimesQuery.or(s => s.cinema == cId && s.movie == mId);
    // //     });
    // //   });
    // // } else if (selectedCinemaIds.length > 0) {
    // //   showTimesQuery = showTimesQuery.and(s => selectedCinemaIds.some(c => c == s.cinema));
    // // } else if (selectedMovieIds.length > 0) {
    // //   showTimesQuery = showTimesQuery.and(s => selectedMovieIds.some(m => m == s.movie));
    // // }
    // const showTimes = await showTimesQuery.toArray();
    // console.log(showTimes);

    // // Get the movie and cinema data for the showtimes
    // let events = await Promise.all(showTimes.map(async st => {
    //   const movie = await this.movies.get(st.movie);
    //   const cinema = await this.cinemas.get(st.cinema);
    //   return new EventData(
    //     st.startTime,
    //     st.endTime,
    //     movie!.displayName,
    //     movie!.runtime,
    //     cinema!.displayName,
    //     cinema!.color,
    //     st.url,
    //     st.language,
    //     st.type,
    //   )
    // }));

    // var eventDays = await this.splitEventsByDay(events);

    // // if (eventDays.values.length > 0 && eventDays.size < visibleDays) {
    // //   // Fill up the missing days
    // //   const lastEventDay = Array.from(eventDays.keys()).sort().pop();
    // //   let lastDate = new Date(lastEventDay!);
    // //   lastDate.setDate(lastDate.getDate() + 1);
    // //   while (eventDays.size < visibleDays) {
    // //     const newEvents = await this.getEvents(lastDate, visibleDays, selectedCinemas, selectedMovies);
    // //     newEvents.forEach((value, key) => {
    // //       eventDays.set(key, value);
    // //     });
    // //   }

    // // }

    // return eventDays;


    let showTimesQuery = this.showTimes
      .orderBy('[startTime+cinema+movie]')
      .and(showtime => new Date(showtime.startTime) >= startDate);
    if (selectedMovieIds.length > 0) {
      showTimesQuery = showTimesQuery.and(showtime => selectedMovieIds.includes(showtime.movie))
    }
    if (selectedCinemaIds.length > 0) {
      showTimesQuery = showTimesQuery.and(showtime => selectedCinemaIds.includes(showtime.cinema))
    }
const    showTimes= await showTimesQuery.toArray();

    const events: EventData[] = await Promise.all(showTimes.map(async st => {

      const movie = await this.movies.get({ id: st.movie });
      const cinema = await this.cinemas.get({ id: st.cinema });
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
      );
    }));

    console.log(events);

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

