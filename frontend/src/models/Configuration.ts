import { Entity } from "dexie";
import CinemaDb from "../services/CinemaDb";

export default class Configuration extends Entity<CinemaDb> {
  id!: string;
  value!: any;
}
