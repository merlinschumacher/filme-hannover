
import { Cinema } from '../models/Cinema';
import { EventData } from '../models/EventData';
import { Movie } from '../models/Movie';
import CinemaDb from './CinemaDb';

export default class FilterService {
  private db = new CinemaDb();
  private availableMovies: Movie[] = [];
  private availableCinemas: Cinema[] = [];
  private selectedMovies: Movie[] = [];
  private selectedCinemas: Cinema[] = [];
  private initializationPromise: Promise<void>;
  public resultListChanged?: () => void;

  constructor() {
    this.initializationPromise = this.initialize();
  }

  private async initialize(): Promise<void> {
    await this.db.open();
    this.availableMovies = await this.db.getAllMovies();
    this.availableCinemas = await this.db.getAllCinemas();
    this.selectedCinemas = this.availableCinemas;
  }

  public async getMovies(): Promise<Movie[]> {
    await this.initializationPromise;
    if (this.selectedMovies.length === 0) {
      return this.availableMovies;
    }
    return this.availableMovies;
  }

  public async getCinemas(): Promise<Cinema[]> {
    await this.initializationPromise;
    return this.availableCinemas;
  }

  public async setSelectedMovies(movies: Movie[]): Promise<void> {
    await this.initializationPromise;
    if (movies.length === 0 && this.selectedCinemas.length === 0) {
      this.selectedMovies = await this.db.getAllMovies();
    } else if (movies.length === 0) {
      this.selectedMovies = this.availableMovies;
    } else {
      this.selectedMovies = movies;
    }
    this.availableCinemas = await this.db.getCinemasForMovies(this.selectedMovies);

    if (this.resultListChanged) {
      this.resultListChanged();
    }
  }

  public async setSelectedCinemas(cinemas: Cinema[]): Promise<void> {
    await this.initializationPromise;
    if (cinemas.length === 0 && this.selectedMovies.length === 0) {
      this.selectedCinemas = await this.db.getAllCinemas();
    } else if (cinemas.length === 0) {
      this.selectedCinemas = this.availableCinemas;
    } else {
      this.selectedCinemas = cinemas;
    }
    this.availableMovies = await this.db.getMoviesForCinemas(this.selectedCinemas);
    if (this.resultListChanged) {
      this.resultListChanged();
    }
  }

  public async getEvents(startDate: Date, visibleDays: number): Promise<Map<string, EventData[]>> {
    await this.initializationPromise;
    var selectedCinemaIds = this.selectedCinemas.map(c => c.id);
    var selectedMovieIds = this.selectedMovies.map(m => m.id);
    return this.db.getEvents(startDate, visibleDays, selectedCinemaIds, selectedMovieIds);
  }

  public async getDataVersion(): Promise<string> {
    await this.initializationPromise;
    return this.db.dataVersionDate.toLocaleString();
  }
}
