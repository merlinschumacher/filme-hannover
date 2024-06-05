import Dexie from 'dexie';
import { RefResolver } from 'json-schema-ref-resolver';

const dataUrl: string = '/data/data.json';

// const dataUrlLastChange = new URL('/data.json.update');


async function fetchJsonData(url: URL) {
    const response = await fetch(url);
    return await response.json();
}


// async function checkDataVersion() {
//     return await fetchJsonData(dataUrlLastChange);
// }

export async function loadData() {
    
    const data = await fetchJsonData(new URL(dataUrl, window.location.href));
    console.log(data);

    var ref = new RefResolver();
    ref.addSchema(data);
    const de = ref.getDerefSchema(data);
    console.log(de);

    const db = new Dexie('CinemaDB');
    db.version(1).stores({
        cinemas: '++id, displayName, url, shopUrl, color, movies, showTimes',
        movies: '++id, displayName, url, color, runtime, releaseDate, cinemas, showTimes',
        showTimes: '++id, time, movie, cinema, startTime, type, language'
    });

    // db.cinemas.bulkPut(data.cinemas);
    // db.movies.bulkPut(data.movies);
    // db.showTimes.bulkPut(data.showTimes);
}

interface Cinema {
    id: number;
    displayName: string;
    url: URL;
    shopUrl: URL;
    color: string;
    movies: Movie[];
    showTimes: ShowTime[];
}
interface Movie {
    id: number;
    displayName: string;
    url: URL;
    color: string;
    runtime: number;
    releaseDate: Date;
    cinemas: Cinema[];
    showTimes: ShowTime[];
}

interface ShowTime {
    id: number;
    time: Date;
    movie: Movie;
    cinema: Cinema;
    startTime: Date;
    type: ShowTimeType;
    language: ShowTimeLanguage;
}

enum ShowTimeType {
    Regular,
    OriginalVersion,
    Subtitled,
}

enum ShowTimeLanguage {
    Danish,
    German,
    English,
    French,
    Spanish,
    Italian,
    Georgian,
    Russian,
    Turkish,
    Mayalam,
    Japanese,
    Miscellaneous,
    Other,
}