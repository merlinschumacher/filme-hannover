import { Entity } from "dexie";
import CinemaDb from "../services/CinemaDb";
import { ShowTimeLanguage } from "./ShowTimeLanguage";
import { ShowTimeDubType } from "./ShowTimeDubType";

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
    dubType: ShowTimeDubType,
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
    this.dubType = dubType;
  }

  id!: number;
  date!: Date;
  startTime!: Date;
  endTime!: Date;
  movie!: number;
  cinema!: number;
  url!: URL;
  language!: ShowTimeLanguage;
  dubType!: ShowTimeDubType;
}
