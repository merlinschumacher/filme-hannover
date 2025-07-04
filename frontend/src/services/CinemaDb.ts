import Dexie, { EntityTable } from 'dexie';
import { JsonData } from '../models/JsonData';
import { EventData } from '../models/EventData';
import { getData } from './HttpClient';
import Configuration from '../models/Configuration';
import Cinema from '../models/Cinema';
import Movie from '../models/Movie';
import ShowTime from '../models/ShowTime';
import { allMovieRatings, MovieRating } from '../models/MovieRating';
Dexie.debug = true;

export default class CinemaDb extends Dexie {
  configurations!: EntityTable<Configuration, 'id'>;
  cinemas!: EntityTable<Cinema, 'id'>;
  movies!: EntityTable<Movie, 'id'>;
  showTimes!: EntityTable<ShowTime, 'id'>;
  private readonly dataVersionKey: string = 'dataVersion';
  private readonly remoteDataUrl: string = '/data/data.json';
  private readonly remoteVersionDateUrl: string = '/data/data.json.update';

  public dataVersionDate: Date = new Date();

  public constructor() {
    super('CinemaDb');
    this.version(5).stores({
      cinemas: 'id, displayName, iconClass',
      movies: 'id, displayName, runtime, rating, releaseDate',
      showTimes:
        'id, date, startTime, endTime, movie, cinema, language, dubtype',
      configurations: 'id',
    });

    this.cinemas.mapToClass(Cinema);
    this.movies.mapToClass(Movie);
    this.showTimes.mapToClass(ShowTime);
    this.configurations.mapToClass(Configuration);
  }

  async init() {
    console.log('Opening database...');
    try {
      await this.open();
    } catch (error) {
      console.error(error);
      this.close();
      await this.delete();
      await this.open();
    }
    await this.checkData();
    console.log('Data version: ' + this.dataVersionDate.toISOString());
    console.log('Cinemas loaded: ' + (await this.cinemas.count()).toString());
    console.log('Movies loaded: ' + (await this.movies.count()).toString());
    console.log(
      'Showtimes loaded: ' + (await this.showTimes.count()).toString(),
    );
    return this;
  }

  private async checkData() {
    const dataVersion = await this.configurations.get(this.dataVersionKey);
    this.dataVersionDate = new Date((dataVersion?.value as string | number | Date) || 0);
    if (await this.DataReloadRequired(this.dataVersionDate)) {
      console.log('Data version changed, loading data.');
      await this.loadData();
    }
    await this.removeObsoleteData();
  }

  private async removeObsoleteData() {
    // Remove showtimes that started more than 1 hour ago
    const cutoff = new Date(Date.now() - 60 * 60 * 1000);
    await this.showTimes.where('startTime').below(cutoff).delete();

    // Remove movies without showtimes
    const movieIds = await this.showTimes.orderBy('movie').uniqueKeys();
    await this.movies.where('id').noneOf(movieIds).delete();

    // Remove cinemas without showtimes
    const cinemaIds = await this.showTimes.orderBy('cinema').uniqueKeys();
    await this.cinemas.where('id').noneOf(cinemaIds).delete();
  }

  private async DataReloadRequired(currentDataVersion: Date): Promise<boolean> {
    const remoteVersionText = (await (await getData(this.remoteVersionDateUrl))?.text()) ?? '0';
    const remoteVersionDate = new Date(remoteVersionText);
    if (currentDataVersion.getTime() !== remoteVersionDate.getTime()) {
      await this.configurations.put({ id: this.dataVersionKey, value: remoteVersionDate });
      this.dataVersionDate = remoteVersionDate;
      return true;
    }
    return false;
  }

  private parseWithDate(jsonString: string): unknown {
    if (!jsonString || jsonString === '') {
      return {};
    }
    const reDateDetect = /(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})/; // startswith: 2015-04-29T22:06:55
    const resultObject = JSON.parse(
      jsonString,
      (_: unknown, value: unknown) => {
        if (typeof value == 'string' && reDateDetect.exec(value)) {
          return new Date(value);
        }
        return value;
      },
    ) as unknown;
    return resultObject;
  }

  private async loadData() {
    const response = await getData(
      this.remoteDataUrl
    );
    if (!response) {
      throw new Error('Failed to load data');
    }
    const json = this.parseWithDate(await response.text());
    const data = Object.assign(new JsonData(), json);
    try {
      await this.transaction(
        'rw',
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
        },
      );
    } catch (error) {
      console.error(error);
    }
  }

  public async GetAllCinemas(): Promise<Cinema[]> {
    // Get all cinemas that have showtimes
    const cinemaIds = await this.showTimes.orderBy('cinema').uniqueKeys();
    return this.cinemas.where('id').anyOf(cinemaIds).sortBy('displayName');
  }

  public async GetCinemaIds(): Promise<number[]> {
    return await this.cinemas.toCollection().primaryKeys();
  }

  public async GetMovieIds(ratings: MovieRating[] = []): Promise<number[]> {
    if (ratings.length === 0) {
      ratings = allMovieRatings;
    }
    return this.movies.where('rating').anyOf(ratings).primaryKeys();
  }

  public async getMovies(ratings: MovieRating[] = []): Promise<Movie[]> {
    if (ratings.length === 0) {
      ratings = allMovieRatings;
    }
    const movieIds = await this.showTimes.orderBy('movie').uniqueKeys();
    const movies = await this.movies.where('id').anyOf(movieIds).and(m => ratings.includes(m.rating)).toArray();
    return movies.sort((a, b) => new Intl.Collator(undefined, { numeric: true, sensitivity: 'base' }).compare(a.displayName, b.displayName));
  }

  public async getTotalMovieCount(): Promise<number> {
    return this.movies.count();
  }
  public async getTotalCinemaCount(): Promise<number> {
    return this.cinemas.count();
  }

  public async GetEarliestShowTimeDate(
    cinemaIds: number[],
    movieIds: number[],
    dubTypes: number[],
  ): Promise<Date | null> {
    // get the first showtime date for the selected cinemas and movies
    const earliestShowTime = await this.showTimes
      .orderBy('startTime')
      .and(
        (showTime) =>
          showTime.startTime.getTime() >= new Date().getTime() &&
          cinemaIds.includes(showTime.cinema) &&
          movieIds.includes(showTime.movie) &&
          dubTypes.includes(showTime.dubType),
      )
      .first();

    if (!earliestShowTime) {
      return null;
    }

    return earliestShowTime.startTime;
  }

  public async getEventData(
    startDate: Date,
    endDate: Date,
    selectedCinemaIds: number[],
    selectedMovieIds: number[],
    selectedShowTimeDubTypes: number[],
  ): Promise<EventData[]> {
    const showTimeResults = await this.showTimes
      .where('startTime')
      .between(startDate, endDate)
      .and(
        (showtime) =>
          selectedCinemaIds.includes(showtime.cinema) &&
          selectedMovieIds.includes(showtime.movie) &&
          selectedShowTimeDubTypes.includes(showtime.dubType),
      )
      .sortBy('startTime');

    const eventDataPromises = showTimeResults.map(async (st) => {
      const cinema = await this.cinemas.get(st.cinema);
      const movie = await this.movies.get(st.movie);

      if (!cinema || !movie) {
        return null;
      }

      return new EventData(st, movie, cinema);
    });

    const eventData = (await Promise.all(eventDataPromises)).filter(
      (data) => data !== null,
    );

    return eventData;
  }

  public async getEndDate(
    startDate: Date,
    selectedCinemaIds: number[],
    selectedMovieIds: number[],
    selectedShowTimeDubTypes: number[],
    visibleDays: number,
  ): Promise<Date> {
    let lastKey = new Date(startDate);
    let keyCount = 0;
    await this.showTimes
      .where('date')
      .aboveOrEqual(startDate)
      .and((showtime) => selectedCinemaIds.includes(showtime.cinema))
      .and((showtime) => selectedMovieIds.includes(showtime.movie))
      .and((showtime) => selectedShowTimeDubTypes.includes(showtime.dubType))
      .until(() => keyCount === visibleDays)
      .eachKey((key) => {
        if (key instanceof Date) {
          if (new Date(key).getTime() > lastKey.getTime()) {
            lastKey = key;
            keyCount++;
          }
        }
      });

    lastKey.setHours(23);
    lastKey.setMinutes(59);
    lastKey.setSeconds(59);

    return lastKey;
  }
}
