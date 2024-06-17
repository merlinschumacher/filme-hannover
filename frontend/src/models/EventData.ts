import { ShowTimeType } from "./ShowTimeType";
import { ShowTimeLanguage } from "./ShowTimeLanguage";


export interface EventData {
  startTime: Date;
  endTime: Date;
  displayName: string;
  runtime: number;
  cinema: string;
  colorClass: string;
  url: URL;
  language: ShowTimeLanguage;
  type: ShowTimeType;
}
