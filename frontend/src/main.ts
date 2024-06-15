// import 'sprucecss/css/spruce.min.css'

import './style.css'
import { cinemaDb } from './cinemaDb'
import FilterButtonElement from './components/filterButtonElement';
import { BuildSpanSlotElement } from './htmlTemplateHelpers';
import EventItem from './components/eventItem/eventItem.component';

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
    const events = await cinemaDb.getAllEvents();
const eventId= document.querySelector<HTMLDivElement>('#events')!;
    eventId.innerHTML = '';
   const dayList = document.createElement('day-list');
   const header = BuildSpanSlotElement(today.toLocaleDateString(), 'header');
    dayList.appendChild(header);
   const eventListDiv = document.createElement('div');
   eventListDiv.setAttribute('slot', 'body');

   events.forEach(element => {
    const eventListElement = new EventItem();
    if (element.url !== undefined) {
        eventListElement.setAttribute('href',element.url.toString());
    }

    const timeSpan = BuildSpanSlotElement(new Date(element.startTime).toLocaleTimeString(), 'time');
    const typeSpan = BuildSpanSlotElement(element.type.toString(), 'type');
    const languageSpan = BuildSpanSlotElement(element.language.toString(), 'language');

    const titleSpan = BuildSpanSlotElement(element.displayName, 'title');

    eventListElement.appendChild(timeSpan);
    eventListElement.appendChild(titleSpan);
    eventListElement.appendChild(typeSpan);
    eventListElement.appendChild(languageSpan);

    eventListElement.classList.add(element.colorClass);

    eventListDiv.appendChild(eventListElement);
   });
    dayList.appendChild(eventListDiv);
eventId.appendChild(dayList);


}

app.appendChild(button);
app.appendChild( new FilterButtonElement('Action'));


