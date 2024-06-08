import Dexie, { type EntityTable } from 'dexie';
import { JsonData, Configuration, Cinema, Movie, ShowTime, EventData } from './interfaces';


class HttpClient {

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


class CinemaDb extends Dexie {
    configurations!: EntityTable<Configuration, 'key'>;
    cinemas!: EntityTable<Cinema, 'id'>;
    movies!: EntityTable<Movie, 'id'>;
    showTimes!: EntityTable<ShowTime, 'id'>;
    private readonly dataVersionKey = 'dataVersion';
    private readonly remoteDataUrl: string = '/data/data.json';
    private readonly remoteVersionDateUrl: string = '/data/data.json.update';

    constructor() {
        super('CinemaDb');
        this.version(1).stores({
            cinemas: '++id, displayName, url, shopUrl, color',
            movies: '++id, displayName, releaseDate, runtime',
            showTimes: '++id, startTime, endTime, movie, cinema, language, type',
            configurations: 'key, value'
        });
    }

    public async Init() {
        const dataVersionChanged = await this.dataLoadingRequired();
        if (dataVersionChanged) {
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

    async getAllCinemas(): Promise<Cinema[]> {
        return this.cinemas.toArray();
    }

    async getAllEvents(): Promise<EventData[]> {
        const showTimes = await this.showTimes.toArray();
        const movies = await this.movies.where('id').anyOf(showTimes.map(st => st.movie)).toArray();
        const cinemas = await this.cinemas.where('id').anyOf(showTimes.map(st => st.cinema)).toArray();

        // Create a map of movies by their id
        const movieMap = new Map(movies.map(movie => [movie.id, movie]));

        // Create a map of cinemas by their id
        const cinemaMap = new Map(cinemas.map(cinema => [cinema.id, cinema]));

        let eventDataElements = showTimes.map(st => {
            // Use the map to look up the movie for each showtime
            const movie = movieMap.get(st.movie);
            const cinema = cinemaMap.get(st.cinema);
            if (!movie || !cinema) {
                throw new Error('Movie or cinema not found');
            }
            return {
                startTime: st.startTime,
                endTime: st.endTime,
                displayName: movie!.displayName,
                runtime: movie!.runtime,
                cinema: cinema.displayName,
                language: st.language,
                type: st.type
            }
        });
        return eventDataElements;
    }

    async getEventsForDate(date: Date): Promise<EventData[]> {
        var endDate = new Date(new Date(date).setUTCHours(24,0,0,0));
        return this.getEventsForDateRange(date, endDate);
    }

    async getEventsForDateRange(startDate: Date, endDate: Date): Promise<EventData[]> {
        const startDateString = startDate.toISOString();
        const endDateString = endDate.toISOString();
        const showTimes = await this.showTimes.where('startTime').between(startDateString, endDateString).toArray();
        const movies = await this.movies.where('id').anyOf(showTimes.map(st => st.movie)).toArray();
        const cinemas = await this.cinemas.where('id').anyOf(showTimes.map(st => st.cinema)).toArray();

        // Create a map of movies by their id
        const movieMap = new Map(movies.map(movie => [movie.id, movie]));

        // Create a map of cinemas by their id
        const cinemaMap = new Map(cinemas.map(cinema => [cinema.id, cinema]));

        let eventDataElements = showTimes.map(st => {
            // Use the map to look up the movie for each showtime
            const movie = movieMap.get(st.movie);
            const cinema = cinemaMap.get(st.cinema);
            if (!movie || !cinema) {
                throw new Error('Movie or cinema not found');
            }
            return {
                startTime: st.startTime,
                endTime: st.endTime,
                displayName: movie!.displayName,
                runtime: movie!.runtime,
                cinema: cinema.displayName,
                language: st.language,
                type: st.type
            }
        });
        return eventDataElements;
    }

    async getEventDataForCinema(cinemaId: number): Promise<EventData[]> {
        const cinema = await this.cinemas.get(cinemaId);
        if (!cinema) {
            throw new Error('Cinema not found');
        }
        const showTimes = await this.showTimes.where('cinema').equals(cinemaId).toArray();
        const movies = await this.movies.where('id').anyOf(showTimes.map(st => st.movie)).toArray();

        // Create a map of movies by their id
        const movieMap = new Map(movies.map(movie => [movie.id, movie]));

        let eventDataElements = showTimes.map(st => {
            // Use the map to look up the movie for each showtime
            const movie = movieMap.get(st.movie);
            return {
                startTime: st.startTime,
                endTime: st.endTime,
                displayName: movie!.displayName,
                runtime: movie!.runtime,
                cinema: cinema.displayName,
                language: st.language,
                type: st.type
            }
        });
        return eventDataElements;
    }
}

export const cinemaDb = new CinemaDb();

// const dataUrlLastChange = new URL('/data.json.update');



