// import 'sprucecss/css/spruce.min.css' 

import './style.css'
import { cinemaDb } from './cinemaDb' 
import DayListElement from './components/dayListElement';
import EventListElement from './components/eventListElement';
import FilterButtonElement from './components/filterButtonElement';

await cinemaDb.Init();
var today = new Date();
var tomorrow = new Date();
tomorrow = new Date(tomorrow.setDate(tomorrow.getDate() + 1));

cinemaDb.Init();
const app = document.querySelector<HTMLDivElement>('#app')!;

function getNextDay(today = new Date()) {
    var tomorrow = new Date(today);
    tomorrow = new Date(tomorrow.setDate(tomorrow.getDate() + 1));
    return tomorrow;
}
const button = document.createElement('button');
button.textContent = 'Refresh';
button.onclick = async () => {
   

    today = getNextDay(today);
    // const eventList = await cinemaDb.getEventsForDate(today);
    const eventList = await cinemaDb.getAllEvents();
const events= document.querySelector<HTMLDivElement>('#events')!;
    events.innerHTML = '';
   const dayList = document.createElement('day-list') as DayListElement;
   eventList.forEach(element => {
    
    const eventListElement = document.createElement('event-list-element') as EventListElement; 
    eventListElement.data = element;
    dayList.appendChild(eventListElement);
    events.appendChild(eventListElement);

   });
    events.appendChild(dayList);

}

app.appendChild(button);
app.appendChild( new FilterButtonElement('Action'));


