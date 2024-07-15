import { ShowTimeType } from "./ShowTimeType";
import { ShowTimeLanguage } from "./ShowTimeLanguage";


export class EventData {
  startTime: Date;

  constructor(startTime: Date, endTime: Date, displayName: string, runtime: number, cinema: string, colorClass: string, url: URL, language: ShowTimeLanguage, type: ShowTimeType) {
    this.startTime = startTime;
    this.endTime = endTime;
    this.displayName = displayName;
    this.runtime = runtime;
    this.cinema = cinema;
    this.color = colorClass;
    this.url = url;
    this.language = language;
    this.type = type;
  }
  endTime: Date;
  displayName: string;
  runtime: number;
  cinema: string;
  color: string;
  url: URL;
  language: ShowTimeLanguage;
  type: ShowTimeType;
}
