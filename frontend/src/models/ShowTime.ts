import { ShowTimeLanguage } from "./ShowTimeLanguage";
import { ShowTimeType } from "./ShowTimeType";


export interface ShowTime {
  id: number;
  startTime: Date;
  endTime: Date;
  movie: number;
  cinema: number;
  url: URL;
  language: ShowTimeLanguage;
  type: ShowTimeType;
}
