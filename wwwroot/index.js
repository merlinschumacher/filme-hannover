function debounce(func, time) {
    var time = time || 100; // 100 by default if no param
    var timer;
    return function (event) {
        if (timer) clearTimeout(timer);
        timer = setTimeout(func, time, event);
    };
}
var getFormatedDate = function (date = new Date()) {
    var year = date.getFullYear();
    var month = date.getMonth() + 1;
    var day = date.getDate();
    return year + '-' + (month < 10 ? '0' : '') + month + '-' + (day < 10 ? '0' : '') + day;
};
var cinemas = [
    {
        title: 'Alle Kinos',
        url: '/all.ics',
        format: 'ics',
        color: 'grey',
        skipSource: true,
    },
    {
        title: 'Astor',
        url: '/Astor.ics',
        format: 'ics',
        color: '#ceb07a',
        textColor: '#000000',
    },
    {
        title: 'Cinemaxx',
        url: '/Cinemaxx.ics',
        format: 'ics',
        color: 'purple',
    },
    {
        title: 'Apollo Kino',
        url: '/Apollo Kino.ics',
        format: 'ics',
        color: 'blue',
        textColor: 'white',
    },
    {
        title: 'Kino im Sprengel',
        url: '/Kino im Sprengel.ics',
        format: 'ics',
        color: 'lightblue',
        textColor: '#000000',
    },
    {
        title: 'Kino im Künstlerhaus',
        url: '/Kino im Künstlerhaus.ics',
        format: 'ics',
        color: '#2c2e35',
    },
    {
        title: 'Kino am Raschplatz',
        url: '/Kino am Raschplatz.ics',
        format: 'ics',
        color: '#ac001f',
    },
    {
        title: 'Hochhaus Lichtspiele',
        url: '/Hochhaus Lichtspiele.ics',
        format: 'ics',
        color: '#ffd45c',
        textColor: '#000000',
    }
];

var CreateCinemaList = function () {
    var list = document.getElementById('cinemaList');
    for (var i = 0; i < cinemas.length; i++) {
        var cinema = cinemas[i];
        var listItem = document.createElement('li');
        var listItemDot = document.createElement('span');
        listItemDot.style.backgroundColor = cinema.color;
        listItem.appendChild(listItemDot);
        listItem.classList.add('list-group-item');
        listItem.appendChild(document.createTextNode(cinema.title));

        var listItemIcalLink = document.createElement('a');
        listItemIcalLink.href = cinema.url;
        listItemIcalLink.textContent = '📅';
        listItem.appendChild(listItemIcalLink);
        list.appendChild(listItem);
    }
}

var widthDayNumBreakpoints = {
    769: 2,
    1025: 3,
    1367: 5,
}

function getDayNumBreakpoint() {
    var width = window.innerWidth;
    for (var breakpoint in widthDayNumBreakpoints) {
        if (width < breakpoint) {
            return widthDayNumBreakpoints[breakpoint];
        }
    }
    return 7;
}

function getUsableViews() {
    var width = window.innerWidth;
    if (width < 769) {
        return '';
    }
    return 'listWeek,dayGridWeek';
}

async function fetchJsonData() {
    const response = await fetch('/events.json');
    return await response.json();
}

async function initCalendar() {
    var calendarEl = document.getElementById('calendar');
    jsonData = await fetchJsonData();
    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: window.innerWidth < 769 ? 'listWeek' : 'dayGridWeek',
        contentHeight: 'auto',
        height: 'auto',
        visibleRange: {
            start: getFormatedDate()
        },
        validRange: {
            start: getFormatedDate()
        },
        headerToolbar: {
            left: 'prev,today,next',
            center: '',
            right: getUsableViews()
        },
        locale: 'de',
        views: {
            listWeek: {
                buttonText: 'Liste',
            },
            dayGridWeek: {
                buttonText: 'Woche',
                duration: {
                    days: getDayNumBreakpoint()
                }
            },
        },
        nextDayThreshold: '05:00:00',
        eventTimeFormat: {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false,
        },
        eventSources: jsonData
    });
    //for (var i = 0; i < cinemas.length; i++) {
    //    if (cinemas[i].skipSource) {
    //        continue;
    //    }
    //    calendar.addEventSource(cinemas[i]);
    //}
    calendar.render();
}

document.addEventListener('DOMContentLoaded', async function () {
    //CreateCinemaList();

    initCalendar();
});
