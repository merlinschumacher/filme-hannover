export interface Configuration {
    key: string;
    value: any;
}
export interface Cinema {
  id: number;
  displayName: string;
  url: string;
  shopUrl: string;
  color: string;
  movies: number[];
  showTimes: number[];
}

export interface Movie {
  id: number;
  displayName: string;
  releaseDate: Date | null;
  cinemas: number[];
  runtime: number | null;
}

export interface ShowTime {
  id: number;
  startTime: Date;
  endTime: Date;
  movie: number;
  cinema: number;
  language: ShowTimeLanguage;
  type: ShowTimeType;
}

export enum ShowTimeType
    {
        Regular,
        OriginalVersion,
        Subtitled,
    }

export enum ShowTimeLanguage
    {
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

export interface JsonData {
    cinemas: readonly Cinema[];
    movies: readonly Movie[];
    showTimes: readonly ShowTime[];
}

export interface EventData {
  startTime: Date;
  endTime: Date;
  displayName: string;
  runtime: number | null;
  cinema: string;
  language: ShowTimeLanguage;
  type: ShowTimeType;
}