import { ShowTimeType } from "./ShowTimeType";
import { ShowTimeLanguage } from "./ShowTimeLanguage";
import ShowTime from "./ShowTime";
import Movie from "./Movie";
import Cinema from "./Cinema";

export class EventData {
  constructor(showTime: ShowTime, movie: Movie, cinema: Cinema) {
    this.date = showTime.date;
    this.startTime = showTime.startTime;
    this.endTime = showTime.endTime;
    this.displayName = movie.displayName;
    this.runtime = movie.runtime;
    this.cinema = cinema.displayName;
    this.color = cinema.color;
    this.url = showTime.url;
    this.language = showTime.language;
    this.type = showTime.type;
  }

  startTime: Date;
  date: Date;
  endTime: Date;
  displayName: string;
  runtime: number;
  cinema: string;
  color: string;
  url: URL;
  language: ShowTimeLanguage;
  type: ShowTimeType;
}
