
import Cinema from '../models/Cinema';
import { EventData } from '../models/EventData';
import Movie from '../models/Movie';
import CinemaDb from './CinemaDb';

export default class FilterService {
  private db: CinemaDb = null!
  private availableMovies: Movie[] = [];
  private availableCinemas: Cinema[] = [];
  private selectedMovies: Movie[] = [];
  private selectedCinemas: Cinema[] = [];
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
    this.availableMovies = await this.db.getAllMovies();
    this.availableCinemas = await this.db.getAllCinemas();
    this.selectedCinemas = this.availableCinemas;
    this.selectedMovies = this.availableMovies;
  }

  public async getAllMovies(): Promise<Movie[]> {
    if (this.selectedMovies.length === 0) {
      return this.availableMovies;
    }
    return this.availableMovies;
  }

  public async getAllCinemas(): Promise<Cinema[]> {
    return this.availableCinemas;
  }

  public async setSelectedMovies(movies: Movie[]): Promise<void> {
    if (movies.length === 0 && this.selectedCinemas.length === 0) {
      this.selectedMovies = await this.db.getAllMovies();
    } else if (movies.length === 0) {
      this.selectedMovies = this.availableMovies;
    } else {
      this.selectedMovies = movies;
    }

    if (this.resultListChanged) {
      this.resultListChanged();
    }
  }

  public async setSelectedCinemas(cinemas: Cinema[]): Promise<void> {
    if (cinemas.length === 0 && this.selectedMovies.length === 0) {
      this.selectedCinemas = await this.db.getAllCinemas();
    } else if (cinemas.length === 0) {
      this.selectedCinemas = this.availableCinemas;
    } else {
      this.selectedCinemas = cinemas;
    }
    if (this.resultListChanged) {
      this.resultListChanged();
    }
  }

  public async getEvents(startDate: Date, visibleDays: number): Promise<Map<Date, EventData[]>> {

    const selectedCinemaIds = this.selectedCinemas.map(c => c.id);
    const selectedMovieIds = this.selectedMovies.map(m => m.id);
    const firstShowTimeDate = await this.db.getFirstShowTimeDate(selectedCinemaIds, selectedMovieIds);

    if (startDate < firstShowTimeDate) {
      startDate = firstShowTimeDate;
    }

    return this.db.getEvents(startDate, visibleDays, selectedCinemaIds, selectedMovieIds);
  }

  public async getDataVersion(): Promise<string> {
    return this.db.dataVersionDate.toLocaleString();
  }
}
