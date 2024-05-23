let eventSourcesData = [];
let selectedEventSources = [];
let calendar = null;
const widthDayNumBreakpoints = {
    600: 1,
    720: 2,
    900: 3,
    1040: 4,
    1200: 5,
    1366: 7,
};

async function fetchJsonData(url) {
    const response = await fetch(url);
    return await response.json();
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
    selectedEventSources = cinemaEventSources;
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
        right: 'today'
    }
    return headerToolbar;
}

function getDayNumBreakpoint() {
    let width = window.innerWidth;
    let duration = 7;

    var lastBreakpointSize = 0;
    Object.keys(widthDayNumBreakpoints).reverse().forEach(key => {
        if (width > lastBreakpointSize && width < key) {
            duration = widthDayNumBreakpoints[key];
            return;
        }
        lastBreakpointSize = key;
    })

    return duration;
}

function toggleTodayBgColor(dayNum) {
    // Get the root element
    if (dayNum === 1) {
        var r = document.querySelector(':root');
        r.style.setProperty('--fc-today-bg-color', 'transparent');
    } else {
        var r = document.querySelector(':root');
        r.style.setProperty('--fc-today-bg-color', 'rgba(255, 220, 40, 0.15)');
    }
}

async function handleWindowResize(arg) {
    let duration = getDayNumBreakpoint();
    toggleTodayBgColor(duration);
    calendar.setOption('duration', { days: duration });
}

async function getEventSources() {
    var eventSources = await fetchJsonData('/events.json');
    for (let eventSource of eventSources) {
        eventSource.events = eventSource.events.filter(e => {
            let startDate = new Date(e.start);
            let now = new Date();
            return startDate >= now;
        })
        eventSourcesData.push(eventSource);
    }
    selectedEventSources = eventSourcesData;
    return eventSourcesData;
}

function showTooltip(info) {
    let tooltip = new Tooltip(info, el, {
        title: info.event.title,
        placement: 'top',
        trigger: 'click',
        container: 'body'
    });
}

function eventClickHandler(info) {
    info.jsEvent.preventDefault();
    window.open(info.event.url);
}

function getCalendarOptions() {
    return {
        height: 'auto',
        contentHeight: 'auto',
        initialView: 'dayGrid',
        duration: { days: getDayNumBreakpoint() },
        validRange: getCurrentDate(),
        headerToolbar: getHeaderToolbar(),
        locale: 'de',
        views: {
            dayGrid: {
                type: 'dayGrid',
            }
        },
        nextDayThreshold: '05:00:00',
        eventTimeFormat: {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false,
        },
        displayEventEnd: false,
        eventSources: selectedEventSources,
        windowResize: handleWindowResize,
        eventClick: eventClickHandler,
        customButtons: {
            filterButton: {
                text: 'Filter',
                click: function () { }
            }
        }
    }
}
async function initCalendar() {
    let calendarEl = document.getElementById('calendar');
    var calendarOptions = getCalendarOptions();
    calendar = new FullCalendar.Calendar(calendarEl, calendarOptions);
    calendar.render();
    toggleTodayBgColor(getDayNumBreakpoint());
}

document.addEventListener('DOMContentLoaded', async function () {
    CreateCinemaList();
    await getEventSources();
    initCalendar();
});
