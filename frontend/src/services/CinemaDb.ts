import Dexie, {
  EntityTable,
  IndexableTypePart,
} from "dexie";
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
        "id, date, startTime, endTime, movie, cinema, language, type",
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
    await this.checkData();
    console.log("Data version: " + this.dataVersionDate.toISOString());
    console.log("Cinemas loaded: " + ((await this.cinemas.count()).toString()));
    console.log("Movies loaded: " + ((await this.movies.count()).toString()));
    console.log("Showtimes loaded: " + ((await this.showTimes.count()).toString()));
    return this;
  }

  public static async Create() {
    const db = new CinemaDb();
    await db.init();
    return db;
  }

  private async checkData() {
    const dataVersion = await this.configurations.get(this.dataVersionKey);
    this.dataVersionDate = new Date(dataVersion?.value as string | number || 0);
    const dataReloadRequired = await this.DataReloadRequired(
      this.dataVersionDate
    );
    if (dataReloadRequired) {
      console.log("Data version changed, loading data.");
      await this.loadData();
    }
    await this.removeObsoleteData();
  }

  private async removeObsoleteData() {
    // Subtract one hour from the current date
    const currentDate = new Date();
    currentDate.setHours(currentDate.getHours() - 1);
    const showTimes = await this.showTimes
      .where("startTime")
      .below(currentDate)
      .primaryKeys();
    await this.showTimes.bulkDelete(showTimes);

    const movieIds = await this.showTimes.orderBy("movie").uniqueKeys();
    const movies = await this.movies
      .where("id")
      .noneOf(movieIds)
      .primaryKeys();
    await this.movies.bulkDelete(movies);

    const cinemaIds = await this.showTimes.orderBy("cinema").uniqueKeys();
    const cinemas = await this.cinemas
      .where("id")
      .noneOf(cinemaIds)
      .primaryKeys();

    await this.cinemas.bulkDelete(cinemas);
  }

  private async DataReloadRequired(currentDataVersion: Date): Promise<boolean> {
    const remoteVersionText =
      (await (await HttpClient.getData(this.remoteVersionDateUrl))?.text()) ??
      "0";
    const remoteVersionDate = new Date(remoteVersionText);
    if (
      currentDataVersion.getTime() !== remoteVersionDate.getTime()
    ) {
      try {
        await this.configurations.put({
          id: this.dataVersionKey,
          value: remoteVersionDate,
        });
        this.dataVersionDate = remoteVersionDate;
      } catch (error) {
        console.error(error);
      }
      return true;
    }
    return false;
  }

  private parseWithDate(jsonString: string): unknown {
    const reDateDetect = /(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})/; // startswith: 2015-04-29T22:06:55
    const resultObject = JSON.parse(jsonString, (_: unknown, value: unknown) => {
      if (typeof value == "string" && reDateDetect.exec(value)) {
        return new Date(value);
      }
      return value;
    }) as unknown;
    return resultObject;
  }

  private async loadData() {
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
    }
  }

  public async GetAllCinemas(): Promise<Cinema[]> {
    // Get all cinemas that have showtimes
    const cinemaIds = await this.cinemas.toCollection().primaryKeys();
    const cinemasWithShowTimes = await this.showTimes.where("cinema").anyOf(cinemaIds).uniqueKeys();
    return this.cinemas.orderBy("displayName").and(cinema => cinemasWithShowTimes.includes(cinema.id)).toArray();
  }

  public async GetAllMovies(): Promise<Movie[]> {
    const movieIds = await this.showTimes.orderBy("movie").uniqueKeys();
    return this.movies.orderBy("displayName").and(movie => movieIds.includes(movie.id)).toArray();
  }
  public async GetEarliestShowTimeDate(
    selectedCinemaIds: number[],
    selectedMovieIds: number[],
    selectedShowTimeTypes: number[]
  ): Promise<Date | null> {
    // get the first showtime date for the selected cinemas and movies
    const earliestShowTime = await this.showTimes
      .orderBy("startTime")
      .and(
        (showTime) =>
          showTime.startTime.getTime() >= new Date().getTime() &&
          selectedCinemaIds.includes(showTime.cinema) &&
          selectedMovieIds.includes(showTime.movie) &&
          selectedShowTimeTypes.includes(showTime.type)
      )
      .first();

    if (!earliestShowTime) {
      return null;
    }

    return earliestShowTime.startTime;
  }

  public async GetEventData(
    startDate: Date,
    endDate: Date,
    selectedCinemaIds: number[],
    selectedMovieIds: number[],
    selectedShowTimeTypes: number[]
  ): Promise<EventData[]> {
    const showTimeResults = await this.showTimes
      .where("startTime")
      .between(startDate, endDate)
      .and(showtime =>
        selectedCinemaIds.includes(showtime.cinema) &&
        selectedMovieIds.includes(showtime.movie) &&
        selectedShowTimeTypes.includes(showtime.type)
      )
      .sortBy("startTime");

    const eventDataPromises = showTimeResults.map(async (st) => {
      const cinema = await this.cinemas.get(st.cinema);
      const movie = await this.movies.get(st.movie);

      if (!cinema || !movie) {
        return null;
      }

      return new EventData(st, movie, cinema);
    });

    const eventData = (await Promise.all(eventDataPromises)).filter(
      (data) => data !== null
    );

    return eventData;
  }

  public async GetEndDate(
    startDate: Date,
    selectedCinemaIds: number[],
    selectedMovieIds: number[],
    selectedShowTimeTypes: number[],
    visibleDays: number
  ): Promise<Date> {

    const showTimes = await this.showTimes
      .orderBy("date")
      .filter(
        (showtime) =>
          showtime.date.getTime() >= startDate.getTime() &&
          selectedCinemaIds.includes(showtime.cinema) &&
          selectedMovieIds.includes(showtime.movie) &&
          selectedShowTimeTypes.includes(showtime.type)
      )
      .keys();

    const uniqueDates = Array.from(
      new Set(showTimes.map((d: IndexableTypePart) => (d as Date).getTime()))
    );

    let endDate = new Date(startDate);
    if (uniqueDates.length === 0) {
      endDate.setDate(startDate.getDate() + visibleDays);
    } else if (uniqueDates.length < visibleDays) {
      endDate = new Date(uniqueDates[uniqueDates.length - 1]);
    } else {
      endDate = new Date(uniqueDates[visibleDays - 1]);
    }

    endDate.setUTCHours(23, 59, 59, 999);
    return endDate;
  }
}
