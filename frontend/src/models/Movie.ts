import { Entity } from 'dexie';
import CinemaDb from '../services/CinemaDb';
import { MovieRating } from './MovieRating';

export default class Movie extends Entity<CinemaDb> {
  id!: number;
  displayName!: string;
  releaseDate!: Date | null;
  runtime!: number;
  rating!: MovieRating;
}
