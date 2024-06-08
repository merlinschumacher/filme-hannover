import './style.css'
import { cinemaDb } from './cinemaDb' 
import EventListElement from './components/eventlistElement'

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
eventList.forEach(element => {
  
    const eventElement = new EventListElement(element.startTime, element.displayName, element.type.toString(), element.language.toString(), 'red', new URL('https://www.google.com'));
    eventElement.setAttribute('date', element.startTime.toLocaleString());
    eventElement.setAttribute('title', element.displayName);
    eventElement.setAttribute('type', element.type.toString());
    eventElement.setAttribute('language', element.language.toString());
    events.appendChild(eventElement);
});

}

app.appendChild(button);



