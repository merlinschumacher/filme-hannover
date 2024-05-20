let eventSourcesData = [];
let calendar = null;
let widthDayNumBreakpoints = {
    600: 2,
    720: 3,
    1000: 5,
    1200: 7,
}

function getCurrentDate() {
    return { start: new Date() }
};

function showSnackbar(text) {
    // Get the snackbar DIV
    let snackBar = document.getElementById("snackbar");
    snackBar.textContent = text;

    // Add the "show" class to DIV
    snackBar.className = "show";

    // After 3 seconds, remove the show class from DIV
    setTimeout(function () { snackBar.className = snackBar.className.replace("show", ""); snackBar.textContent = "" }, 3000);
}

function copyURI(evt) {
    evt.preventDefault();
    navigator.clipboard.writeText(evt.target.getAttribute('href')).then(() => {
        showSnackbar('Kalenderlink kopiert');
    }, () => {
        showSnackbar('Fehler beim Kopieren des Kalenderlinks');
    });
}

function selectCinema(evt) {
    evt.preventDefault();
    let cinemaName = evt.target.textContent;
    let cinemaEventSources = [];
    calendar.removeAllEventSources();
    if (cinemaName === 'Alle Kinos') {
        cinemaEventSources = eventSourcesData;
    } else {
        cinemaEventSources.push(eventSourcesData.find(e => e.title === cinemaName));
    }
    for (let cinemaEventSource of cinemaEventSources) {
        calendar.addEventSource(cinemaEventSource);
    }
    calendar.refetchEvents();
}

async function CreateCinemaList() {
    let cinemas = await fetchJsonData('/cinemas.json');
    let list = document.getElementById('cinemaList');
    for (let i = 0; i < cinemas.length; i++) {
        let cinema = cinemas[i];
        let listItem = document.createElement('li');
        let listItemDot = document.createElement('span');

        listItemDot.style.backgroundColor = cinema.color;
        listItemDot.classList.add('dot');
        listItem.appendChild(listItemDot);
        listItem.classList.add('list-group-item');

        let cinemaName = document.createElement('a');
        cinemaName.href = "#";
        cinemaName.textContent = cinema.displayName;
        cinemaName.classList.add('cinemaName');
        cinemaName.addEventListener('click', selectCinema);
        listItem.appendChild(cinemaName);

        let listItemIcalLink = document.createElement('a');
        let href = new URL(cinema.calendarFile, document.baseURI).href;
        listItemIcalLink.href = href;
        listItemIcalLink.textContent = '📅 iCal';
        listItemIcalLink.addEventListener('click', copyURI);
        listItem.appendChild(listItemIcalLink);

        list.appendChild(listItem);
    }
}

function getHeaderToolbar() {
    let headerToolbar = {
        left: 'prev,next',
        center: 'title',
        right: 'listWeek,dayGridWeek'
    }

    return headerToolbar;
}

function getDefaultView() {
    let width = window.innerWidth;
    if (width < 600) {
        return 'listWeek';
    }
    return 'dayGridWeek';
}
function getDayNumBreakpoint() {
    let width = window.innerWidth;
    let duration = 7;
    if (getDefaultView() === 'dayGridWeek') {
        for (let breakpoint in widthDayNumBreakpoints) {
            if (width > breakpoint) {
                duration = widthDayNumBreakpoints[breakpoint];
            }
        }
    }
    return { days: duration };
}

async function fetchJsonData(url) {
    const response = await fetch(url);
    return await response.json();
}

async function updateCalendarView(arg) {
    let view = getDefaultView();
    let daysduration = getDayNumBreakpoint();

    calendar.changeView(view, {
        duration: daysduration
    });
}

async function getEventSources() {
    eventSourcesData = await fetchJsonData('/events.json');
    return eventSourcesData;
}

function eventClickHandler(info) {
    info.jsEvent.preventDefault();
    window.open(info.event.url);
}

async function initCalendar() {
    let calendarEl = document.getElementById('calendar');
    await getEventSources();
    calendar = new FullCalendar.Calendar(calendarEl, {
        height: 'auto',
        contentHeight: 'auto',
        initialView: getDefaultView(),
        visibleRange: getCurrentDate(),
        validRange: getCurrentDate(),
        headerToolbar: getHeaderToolbar(),
        locale: 'de',
        views: {
            listWeek: {
                type: 'list',
                buttonText: 'Liste',
            },
            dayGridWeek: {
                type: 'dayGrid',
                buttonText: 'Woche',
                duration: getDayNumBreakpoint()
            },
        },
        nextDayThreshold: '05:00:00',
        eventTimeFormat: {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false,
        },
        displayEventEnd: false,
        eventSources: eventSourcesData,
        windowResize: updateCalendarView,
        eventClick: eventClickHandler,
    });
    calendar.render();
}

document.addEventListener('DOMContentLoaded', async function () {
    CreateCinemaList();

    initCalendar();
});
