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
        "id, date, startTime, endTime, movie, cinema, language, type, [movie+cinema]",
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
      await this.loadData();
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
      return Promise.reject();
    }
    return Promise.resolve();
  }

  public async GetAllCinemas(): Promise<Cinema[]> {
    return this.cinemas.orderBy("displayName").toArray();
  }

  public async GetAllMovies(): Promise<Movie[]> {
    return this.movies.orderBy("displayName").toArray();
  }
  public async GetEarliestShowTimeDate(
    selectedCinemaIds: number[],
    selectedMovieIds: number[]
  ): Promise<Date> {
    // get the first showtime date for the selected cinemas and movies
    let earliestShowTime = await this.showTimes
      .orderBy("startTime")
      .and(
        (showTime) =>
          showTime.startTime.getTime() >= new Date().getTime() &&
          selectedCinemaIds.some((e) => e == showTime.cinema) &&
          selectedMovieIds.some((e) => e == showTime.movie)
      )
      .first();

    if (!earliestShowTime) {
      return new Date();
    }

    return earliestShowTime.startTime;
  }

  public async GetEventData(
    startDate: Date,
    endDate: Date,
    selectedCinemaIds: number[],
    selectedMovieIds: number[]
  ): Promise<EventData[]> {
    const showTimeResults = await this.showTimes
      .where("startTime")
      .between(startDate, endDate)
      .and(showtime =>
        selectedCinemaIds.includes(showtime.cinema) &&
        selectedMovieIds.includes(showtime.movie)
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
    ) as EventData[];

    return eventData;
  }

  public async GetEndDate(
    startDate: Date,
    selectedCinemaIds: number[],
    selectedMovieIds: number[],
    visibleDays: number
  ): Promise<Date> {

    const showTimes = await this.showTimes
      .orderBy("date")
      .filter(
        (showtime) =>
          showtime.date.getTime() >= startDate.getTime() &&
          selectedCinemaIds.includes(showtime.cinema) &&
          selectedMovieIds.includes(showtime.movie)
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
