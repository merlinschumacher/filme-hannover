import Dexie, { EntityTable } from 'dexie';
import { JsonData, Configuration, Cinema, Movie, ShowTime, EventData } from './interfaces';

const dataUrl: string = '/data/data.json';
const dataVersionUrl: string = '/data/data.json.update';


export class CinemaDb extends Dexie {
    configurations!: EntityTable<Configuration, 'key'>;
    cinemas!: EntityTable<Cinema, 'id'>;
    movies!: EntityTable<Movie, 'id'>;
    showTimes!: EntityTable<ShowTime, 'id'>;

    constructor() {
        super('CinemaDb');
        this.version(1).stores({
            cinemas: '++id, displayName, url, shopUrl, color, movies, showTimes',
            movies: '++id, displayName, url, color, runtime, releaseDate, cinemas, showTimes',
            showTimes: '++id, time, movie, cinema, startTime, type, language',
            configurations: 'key, value'
        });

        this.checkDataVersion().then(async (updateRequired) => {
            if (updateRequired) {
                await this.loadData();
            }
        });
    }

    async checkDataVersion() {
        const response = await fetch(dataVersionUrl);
        const dataVersion = response.text;
        const currentVersion = await this.configurations.get('dataVersion');
        if (currentVersion && currentVersion.value === dataVersion) {
            return false;
        }
        await this.configurations.put({ key: 'dataVersion', value: dataVersion });
        return true;
    }

    async loadData() {
        const data: JsonData = await fetchJsonData(new URL(dataUrl, window.location.href));

        this.cinemas.bulkPut(data.cinemas);
        this.movies.bulkPut(data.movies);
        this.showTimes.bulkPut(data.showTimes);
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

export const db = new CinemaDb();

// const dataUrlLastChange = new URL('/data.json.update');


async function fetchJsonData(url: URL) {
    const response = await fetch(url);
    return await response.json() as JsonData;
}


// async function checkDataVersion() {
//     return await fetchJsonData(dataUrlLastChange);
// }

// export async function loadData() {

//     const data: JsonData = await fetchJsonData(new URL(dataUrl, window.location.href));

//     db.version(1).stores({
//         cinemas: 'id, displayName, url, shopUrl, color, movies, showTimes',
//         movies: 'id, displayName, url, color, runtime, releaseDate, cinemas, showTimes',
//         showTimes: 'id, time, movie, cinema, startTime, type, language',
//     });

//     db.cinemas.bulkPut(data.cinemas);
//     db.movies.bulkPut(data.movies);
//     db.showTimes.bulkPut(data.showTimes);
// }



