import { ShowTime } from "./ShowTime";
import { Movie } from "./Movie";
import { Cinema } from "./Cinema";


export interface JsonData {
  cinemas: readonly Cinema[];
  movies: readonly Movie[];
  showTimes: readonly ShowTime[];
}
