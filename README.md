# filme-hannover.de

[![Generate page](https://github.com/merlinschumacher/kinohannover/actions/workflows/run.yml/badge.svg)](https://github.com/merlinschumacher/kinohannover/actions/workflows/run.yml)

This is the application code that generates the content for [filme-hannover.de](https://filme-hannover.de/). The project is non-commercial, free, and open-source. There is no monetary intent behind it. The only goal is to produce an easily accessible version of all cinema programs in Hanover for everybody, especially as the smaller cinemas' programs aren't listed on places like Google or other commercial pages.

The code mainly scrapes the websites for the cinemas in Hanover, Germany. It uses methods like parsing HTML, ICS files or accessing the public unprotected APIs of the cinemas to get the data.

The page is updated once a day via a GitHub Actions workflow. The entire page is static, and there is no live backend. Everything is hosted on GitHub Pages.

The scraper backend is built in .NET and uses TypeScript + libraries for the frontend.

This website is free, non-commercial, and does not use cookies. All content is provided automatically and without editorial processing. All information is provided without guarantee or claim to completeness.

The data comes from the respective cinemas and from [The Movie Database](https://www.themoviedb.org/). Movie titles, descriptions, and images are the property of the respective film studios, distributors, and/or other rights holders.

Used APIs, software, graphics, and fonts:

- [Inter font by Rasmus Andersson (SIL Open Font License)](https://rsms.me/inter/)
- [Material Design Icons by Google (Apache 2 License)](https://github.com/google/material-design-icons/blob/master/LICENSE)
- [Material Symbols by Marella (Apache 2 License)](https://github.com/marella/material-symbols)
- [Dexie.js by David Fahlander/Awarica AB (Apache 2 License)](https://github.com/dexie/Dexie.js/blob/master/LICENSE)
- [Scroll Snap Slider by Barthélémy Bonhomme/barthy-koeln (MIT License)](https://github.com/barthy-koeln/scroll-snap-slider/blob/main/LICENSE)
- [Noto Emoji Font by Google (SIL Open Font License, version 1.1)](https://github.com/adobe-fonts/noto-emoji-svg#license)
- [CsvHelper by Josh Close (Apache 2 / MS-PL License)](https://github.com/JoshClose/CsvHelper/blob/master/LICENSE.txt)
- [Fastenshtein by DanHarltey (MIT License)](https://github.com/DanHarltey/Fastenshtein/blob/master/LICENSE)
- [Html Agility Pack by ZZZ Projects (MIT License)](https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE)
- [iCal.NET by Rian Stockbower/rianjs (MIT License)](https://github.com/rianjs/ical.net/blob/master/license.md)

The data is generally extracted from the websites of the cinemas themselves.

![TMDB](https://raw.githubusercontent.com/merlinschumacher/filme-hannover/master/frontend/src/assets/tmdb-logo.svg|width=15) This product uses the TMDB API but is not endorsed or certified by TMDB.
