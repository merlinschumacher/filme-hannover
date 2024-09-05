import { Entity } from "dexie";
import CinemaDb from "../services/CinemaDb";
import { ShowTimeLanguage } from "./ShowTimeLanguage";
import { ShowTimeType } from "./ShowTimeType";

export default class ShowTime extends Entity<CinemaDb> {
  constructor(
    id: number,
    date: Date,
    startTime: Date,
    endTime: Date,
    movie: number,
    cinema: number,
    url: URL,
    language: ShowTimeLanguage,
    type: ShowTimeType,
  ) {
    super();
    this.id = id;
    this.date = date;
    this.startTime = startTime;
    this.endTime = endTime;
    this.movie = movie;
    this.cinema = cinema;
    this.url = url;
    this.language = language;
    this.type = type;
  }

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
