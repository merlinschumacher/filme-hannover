import Dexie, { IndexableTypePart, Table, type EntityTable } from 'dexie';
import { ShowTime } from "../models/ShowTime";
import { Movie } from "../models/Movie";
import { Cinema } from "../models/Cinema";
import { Configuration } from "../models/Configuration";
import { JsonData } from "../models/JsonData";
import { EventData } from "../models/EventData";
import HttpClient from './HttpClient';

export default class CinemaDb extends Dexie {
  configurations!: EntityTable<Configuration, 'key'>;
  cinemas!: Table<Cinema, number>;
  movies!: Table<Movie, number>;
  showTimes!: Table<ShowTime, number>;
  private readonly dataVersionKey: string = 'dataVersion';
  private readonly remoteDataUrl: string = '/data/data.json';
  private readonly remoteVersionDateUrl: string = '/data/data.json.update';

  public dataVersionDate: Date = new Date();

  private constructor() {
    super('CinemaDb');
    this.version(1).stores({
      cinemas: 'id, displayName, url, shopUrl, color',
      movies: 'id, displayName, releaseDate, runtime',
      showTimes: 'id, date, startTime, endTime, movie, cinema, language, type, [startTime+cinema+movie], url',
      configurations: 'key, value'
    });
  }

  private async init() {
    await this.open();
    await this.checkDataVersion();
    const dataVersion = await this.configurations.get(this.dataVersionKey);
    this.dataVersionDate = new Date(dataVersion?.value || 0);
    console.log('Data version: ' + this.dataVersionDate);
    console.log('Cinemas loaded: ' + await this.cinemas.count());
    console.log('Movies loaded: ' + await this.movies.count());
    console.log('Showtimes loaded: ' + await this.showTimes.count());
    return this;
  }

  public static async Create() {
    const db = new CinemaDb();
    return await db.init();
  }


  private async checkDataVersion() {
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

  public async getFirstShowTimeDate(selectedCinemaIds: number[], selectedMovieIds: number[]): Promise<Date> {

    // If all cinemas and movies are selected, return the current date
    if (selectedCinemaIds.length == await this.cinemas.count() && selectedMovieIds.length == await this.movies.count()) {
      return new Date();
    }
    // get the first showtime date for the selected cinemas and movies
    let earliestShowTime = await this.showTimes.orderBy('startTime')
      .and(item => selectedCinemaIds.some(e => e == item.cinema) && selectedMovieIds.some(e => e == item.movie))
      .and(item => new Date(item.startTime) >= new Date())
      .first();

    if (!earliestShowTime) {
      return new Date();
    }

    return new Date(earliestShowTime.startTime);

  }



  async getEvents(startDate: Date, visibleDays: number, selectedCinemaIds: number[], selectedMovieIds: number[]): Promise<Map<Date, EventData[]>> {

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

    // Get the first showtime date, if the start date is before the first showtime date, set the start date to the first showtime date
    // Calculate the end date


    // To fill up the requested visible days, we need to get the nth day for that range.
    let endDate: Date = new Date();

    await this.showTimes
      .orderBy('date')
      .and(showtime => selectedCinemaIds.includes(showtime.cinema) && selectedMovieIds.includes(showtime.movie))
      .uniqueKeys(e => {

        let element: IndexableTypePart | undefined;
        if (e.length < visibleDays) {
          element = e.at(e.length - 1);
        } else {
          element = e.at(visibleDays - 1);
        }
        endDate = new Date(element as string);

      });



    let showTimesQuery = this.showTimes
      .orderBy('[startTime+cinema+movie]')
      .and(showtime => showtime.startTime >= startDate && showtime.endTime <= endDate)
      .and(showtime => selectedCinemaIds.includes(showtime.cinema) && selectedMovieIds.includes(showtime.movie));
    const showTimes = await showTimesQuery.toArray();

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

    return this.splitEventsByDay(events);

  }

  // Splits events into days
  async splitEventsByDay(events: EventData[]): Promise<Map<Date, EventData[]>> {
    const eventsByDay = new Map<Date, EventData[]>();
    events.forEach(event => {
      const day = new Date(new Date(event.startTime).setUTCHours(0, 0, 0, 0));
      if (!eventsByDay.has(day)) {
        eventsByDay.set(day, []);
      }
      eventsByDay.get(day)!.push(event);
    });
    return eventsByDay;
  }

}

