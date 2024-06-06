import './style.css'
import { db} from './event-data-loader' 

console.log(await db.getEventDataForCinema(1));
