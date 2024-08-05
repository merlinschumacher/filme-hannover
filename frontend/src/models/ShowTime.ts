import { Entity } from "dexie";
import CinemaDb from "../services/CinemaDb";
import { ShowTimeLanguage } from "./ShowTimeLanguage";
import { ShowTimeType } from "./ShowTimeType";


export default class ShowTime extends Entity<CinemaDb> {
  id!: number;
  date!: Date;
  startTime!: Date;
  endTime!: Date;
  movie!: number;
  cinema!: number;
  url!: URL;
  language!: ShowTimeLanguage;
  type!: ShowTimeType;
}
