import Dexie, { EntityTable } from "dexie";
import { JsonData } from "../models/JsonData";
import { EventData } from "../models/EventData";
import HttpClient from "./HttpClient";
import Configuration from "../models/Configuration";
import Cinema from "../models/Cinema";
import Movie from "../models/Movie";
import ShowTime from "../models/ShowTime";
Dexie.debug = true;

export default class CinemaDb extends Dexie {
  configurations!: EntityTable<Configuration, "id">;
  cinemas!: EntityTable<Cinema, "id">;
  movies!: EntityTable<Movie, "id">;
  showTimes!: EntityTable<ShowTime, "id">;
  private readonly dataVersionKey: string = "dataVersion";
  private readonly remoteDataUrl: string = "/data/data.json";
  private readonly remoteVersionDateUrl: string = "/data/data.json.update";

  public dataVersionDate: Date = new Date();

  private constructor() {
    super("CinemaDb");
    this.version(1).stores({
      cinemas: "id, displayName",
      movies: "id, displayName",
      showTimes:
        "id, date, startTime, endTime, movie, cinema, language, type, [startTime+movie+cinema], [startTime+endTime]",
      configurations: "id",
    });

    this.cinemas.mapToClass(Cinema);
    this.movies.mapToClass(Movie);
    this.showTimes.mapToClass(ShowTime);
    this.configurations.mapToClass(Configuration);
  }

  private async init() {
    console.log("Opening database");
    await this.open();
    await this.checkDataVersion();
    console.log("Data version: " + this.dataVersionDate);
    console.log("Cinemas loaded: " + (await this.cinemas.count()));
    console.log("Movies loaded: " + (await this.movies.count()));
    console.log("Showtimes loaded: " + (await this.showTimes.count()));
    return this;
  }

  public static async Create() {
    const db = new CinemaDb();
    await db.init();
    return db;
  }

  private async checkDataVersion() {
    const dataVersion = await this.configurations.get(this.dataVersionKey);
    this.dataVersionDate = new Date(dataVersion?.value || 0);
    const dataReloadRequired = await this.DataReloadRequired(
      this.dataVersionDate
    );
    if (dataReloadRequired) {
      console.log("Data version changed, loading data.");
      await this.LoadData();
    }
  }

  private async DataReloadRequired(currentDataVersion: Date): Promise<boolean> {
    const remoteVersionText =
      (await (await HttpClient.getData(this.remoteVersionDateUrl))?.text()) ??
      "0";
    const remoteVersionDate = new Date(remoteVersionText);
    if (
      currentDataVersion.toDateString() !== remoteVersionDate.toDateString()
    ) {
      try {
        await this.configurations.put({
          id: this.dataVersionKey,
          value: remoteVersionDate,
        });
      } catch (error) {
        console.error(error);
      }
      return true;
    }
    return false;
  }

  private parseWithDate(jsonString: string): any {
    var reDateDetect = /(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})/; // startswith: 2015-04-29T22:06:55
    var resultObject = JSON.parse(jsonString, (_: any, value: any) => {
      if (typeof value == "string" && reDateDetect.exec(value)) {
        return new Date(value);
      }
      return value;
    });
    return resultObject;
  }

  private async LoadData() {
    const response = await fetch(
      new URL(this.remoteDataUrl, window.location.href)
    );
    const json = this.parseWithDate(await response.text());
    const data = Object.assign(new JsonData(), json);
    try {
      await this.transaction(
        "rw",
        this.cinemas,
        this.movies,
        this.showTimes,
        async () => {
          await this.cinemas.clear();
          await this.cinemas.bulkAdd(data.cinemas);
          await this.movies.clear();
          await this.movies.bulkAdd(data.movies);
          await this.showTimes.clear();
          await this.showTimes.bulkAdd(data.showTimes);
        }
      );
    } catch (error) {
      console.error(error);
      return Promise.reject();
    }
    return Promise.resolve();
  }

  async getAllCinemas(): Promise<Cinema[]> {
    return this.cinemas.orderBy("displayName").toArray();
  }

  async getAllMovies(): Promise<Movie[]> {
    return this.movies.orderBy("displayName").toArray();
  }
  async getAllMoviesOrderedByShowTimeCount(): Promise<Movie[]> {
    const startDateString = new Date().toISOString();
    // Get all showtimes that are in the future
    const showTimes = await this.showTimes
      .where("startTime")
      .above(startDateString)
      .toArray();
    // Count the number of showtimes for each movie
    const movieCountMap = new Map<number, number>();
    showTimes.forEach((st) => {
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

  public async getFirstShowTimeDate(
    selectedCinemaIds: number[],
    selectedMovieIds: number[]
  ): Promise<Date> {
    // If all cinemas and movies are selected, return the current date
    if (
      selectedCinemaIds.length == (await this.cinemas.count()) &&
      selectedMovieIds.length == (await this.movies.count())
    ) {
      return new Date();
    }
    // get the first showtime date for the selected cinemas and movies
    let earliestShowTime = await this.showTimes
      .orderBy("startTime")
      .and(
        (item) =>
          selectedCinemaIds.some((e) => e == item.cinema) &&
          selectedMovieIds.some((e) => e == item.movie)
      )
      .and((item) => new Date(item.startTime) >= new Date())
      .first();

    if (!earliestShowTime) {
      return new Date();
    }

    return new Date(earliestShowTime.startTime);
  }

  async getEvents(
    startDate: Date,
    visibleDays: number,
    selectedCinemaIds: number[],
    selectedMovieIds: number[]
  ): Promise<Map<Date, EventData[]>> {
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
    endDate.setDate(startDate.getDate() + visibleDays);

    // await this.showTimes
    //   .orderBy('date')
    //   .and(showtime => selectedCinemaIds.includes(showtime.cinema) && selectedMovieIds.includes(showtime.movie))
    //   .uniqueKeys(e => {

    //     let element: IndexableTypePart | undefined;
    //     if (e.length < visibleDays) {
    //       element = e.at(e.length - 1);
    //     } else {
    //       element = e.at(visibleDays - 1);
    //     }
    //     endDate = new Date(element as string);

    //   });

    let eventData: EventData[] = [];
    this.transaction(
      "r",
      this.showTimes,
      this.cinemas,
      this.movies,
      async () => {
        const showTimeResults = await this.showTimes
          .where("startTime")
          .between(startDate, endDate)
          .and(
            (showtime) =>
              selectedCinemaIds.includes(showtime.cinema) &&
              selectedMovieIds.includes(showtime.movie)
          )
          .sortBy("startTime");

        showTimeResults.forEach(async (st) => {
          const cinema = await this.cinemas.get(st.cinema);
          const movie = await this.movies.get(st.movie);

          if (!cinema || !movie) {
            return;
          }

          eventData.push(new EventData(st, movie, cinema));
        });
      }
    );

    console.log(eventData);

    // Split the events into days
    let eventsByDay = new Map<string, EventData[]>();
    eventData.forEach((event) => {
      const date = event.startTime;
      if (!eventsByDay.has(date.toISOString())) {
        eventsByDay.set(date.toISOString(), []);
      }
      eventsByDay.get(date.toISOString())?.push(event);
    });

    console.log(eventsByDay);

    let eventsByDayDateKey = new Map<Date, EventData[]>();
    eventsByDay.forEach((value, key) => {
      eventsByDayDateKey.set(new Date(key), value);
    });

    console.log(eventsByDayDateKey);

    return eventsByDayDateKey;

    // Get the movie and cinema data for the showtimes

    // .and(showtime => selectedCinemaIds.includes(showtime.cinema) && selectedMovieIds.includes(showtime.movie));
  }
}
