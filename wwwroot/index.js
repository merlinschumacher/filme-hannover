let eventSourcesData = [];
let selectedEventSources = [];
let calendar = null;
let modal = null;
let cinemas = [];
let cinemaList = null;
const widthDayNumBreakpoints = {
    600: 1,
    720: 2,
    1040: 3,
    1200: 4,
    1366: 5,
};

async function fetchJsonData(url) {
    const response = await fetch(url);
    return await response.json();
}

function buildIcsCopyModal(cinemaName, icsLink) {
    const template = document.querySelector('#icsCopyModalTemplate');
    const clone = template.content.cloneNode(true);
    var nameElements = clone.querySelectorAll('.cinemaName');
    for (var i = 0; i < nameElements.length; i++) {
        nameElements[i].textContent = cinemaName;
    }
    clone.querySelector('.copyLinkInput').value = icsLink;
    var btn = modal.addFooterBtn('Link kopieren', 'fc-button fc-button-primary', function (e) {
        navigator.clipboard.writeText(e.target.href);
        e.target.classList.remove('copyLinkButtonAnimate');
        e.target.innerText = 'Link kopiert!';
        e.target.classList.add('copyLinkButtonAnimate');
    });
    btn.href = icsLink;
    return clone;
}

function showIcsCopyModal(evt) {
    evt.preventDefault();
    const content = buildIcsCopyModal(evt.target.cinemaName, decodeURI(evt.target.href));
    modal.setContent(content);
    modal.open();
}
function showCinemaListModal(evt) {
    const template = document.querySelector('#cinemaListTemplate');
    const clone = template.content.cloneNode(true);
    clone.querySelector('#cinemaList').replaceWith(cinemaList);
    evt.preventDefault();
    modal.setContent(clone);
    modal.open();
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
    modal.close();
}

async function CreateCinemaList() {
    cinemas = await fetchJsonData('/cinemas.json');
    cinemaList = document.createElement('ul');
    cinemaList.id = 'cinemaList';
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

        listItemIcalLink.addEventListener('click', showIcsCopyModal);
        if (cinema.displayName === 'Alle Kinos') {
            listItemIcalLink.cinemaName = 'aller Kinos';
        } else {
            listItemIcalLink.cinemaName = 'des ' + cinema.displayName;
        }
        listItem.appendChild(listItemIcalLink);

        cinemaList.appendChild(listItem);
    }
}

function getHeaderToolbar() {
    let headerToolbar = {
        left: 'prev,next',
        center: 'title',
        right: 'filter'
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
        validRange: { start: new Date() },
        customButtons: {
            filter: {
                text: 'Filter',
                click: showCinemaListModal
            }
        },
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
        eventDisplay: 'list-item',
    }
}
async function initCalendar() {
    let calendarEl = document.getElementById('calendar');
    var calendarOptions = getCalendarOptions();
    calendar = new FullCalendar.Calendar(calendarEl, calendarOptions);
    calendar.render();
    toggleTodayBgColor(getDayNumBreakpoint());
}

async function initModal() {
    modal = new tingle.modal({
        footer: true,
        closeMethods: ['overlay', 'button', 'escape'],
        closeLabel: "Schließen",
        onClose: function () {
            modal.setContent('');
            modal.setFooterContent('');
        }
    });
}
document.addEventListener('DOMContentLoaded', async function () {
    CreateCinemaList();
    await getEventSources();
    initCalendar();
    initModal();
});
