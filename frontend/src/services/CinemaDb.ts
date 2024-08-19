import Dexie, { EntityTable, IndexableTypePart } from "dexie";
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
    console.log("Opening database...");
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

  public async GetFirstShowTimeDate(
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


  public async GetEvents(
    startDate: Date,
    endDate: Date,
    selectedCinemaIds: number[],
    selectedMovieIds: number[]
  ) {
    let eventData: EventData[] = [];
    await this.transaction(
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
    return eventData;
  }

  public async getEndDate(
    startDate: Date,
    selectedCinemaIds: number[],
    selectedMovieIds: number[],
    visibleDays: number
  ): Promise<Date> {
    let endDate = new Date();
    endDate.setDate(startDate.getDate() + visibleDays);
    await this.showTimes
      .orderBy("date")
          .and(
            (showtime) =>
              showtime.startTime >= startDate &&
              selectedCinemaIds.includes(showtime.cinema) &&
              selectedMovieIds.includes(showtime.movie)
          )
      .uniqueKeys((e) => {
        let element: IndexableTypePart | undefined;
        if (e.length < visibleDays) {
          element = e.at(e.length - 1);
        } else {
          element = e.at(visibleDays - 1);
        }
        if (element) {
          endDate = element as Date;
        }
      });

    return endDate;
  }
}
