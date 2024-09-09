import { ShowTimeDubType } from "./ShowTimeDubType";
import { ShowTimeLanguage } from "./ShowTimeLanguage";
import ShowTime from "./ShowTime";
import Movie from "./Movie";
import Cinema from "./Cinema";

export class EventData {
  constructor(showTime: ShowTime, movie: Movie, cinema: Cinema) {
    this.date = showTime.date;
    this.startTime = showTime.startTime;
    this.endTime = showTime.endTime;
    this.title = movie.displayName;
    this.runtime = movie.runtime;
    this.cinema = cinema.displayName;
    this.color = cinema.color;
    this.iconClass = cinema.iconClass;
    this.url = showTime.url;
    this.language = showTime.language;
    this.dubType = showTime.dubType;
  }

  startTime: Date;
  date: Date;
  endTime: Date;
  title: string;
  runtime: number;
  cinema: string;
  color: string;
  iconClass: string;
  url: URL;
  language: ShowTimeLanguage;
  dubType: ShowTimeDubType;
}
