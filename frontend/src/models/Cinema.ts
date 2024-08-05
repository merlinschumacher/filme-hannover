import { Entity } from "dexie";
import CinemaDb from "../services/CinemaDb";

export default class Cinema extends Entity<CinemaDb> {
  id!: number;
  displayName!: string;
  url!: string;
  shopUrl!: string;
  color!: string;

}
