import { Entity } from "dexie";
import CinemaDb from "../services/CinemaDb";

export default class Movie extends Entity<CinemaDb> {
  id!: number;
  displayName!: string;
  releaseDate!: Date | null;
  runtime!: number;
  rating!: number;
}
