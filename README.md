# filme-hannover.de

[![Generate page](https://github.com/merlinschumacher/kinohannover/actions/workflows/run.yml/badge.svg)](https://github.com/merlinschumacher/kinohannover/actions/workflows/run.yml)
[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)

**[filme-hannover.de](https://filme-hannover.de/)** is a non-commercial, free, and open-source application that consolidates cinema programs in Hanover, Germany. This project makes it easier for users to find showtimes, especially for smaller cinemas that aren't listed on commercial websites.

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [How to Contribute](#how-to-contribute)
- [Data Sources](#data-sources)
- [Technologies Used](#technologies-used)
- [Legal Disclaimer](#legal-disclaimer)
- [License](#license)
- [Acknowledgements](#acknowledgements)

## Features

- **Comprehensive Listings:** Aggregates cinema programs from all theaters in Hanover.
- **Daily Updates:** GitHub Actions automatically update the page daily.
- **Static Deployment:** The site is static and hosted on GitHub Pages.
- **Open Source and Privacy-Focused:** No ads, no cookies, no tracking.

## Architecture

The project is divided into two main components: the **Backend** and the **Frontend**.

### Backend

- **.NET (C#)** for scraping and data processing.
- **SQLite** for temporary data storage and normalization.
- **Scraping:** HTML, ICS files, public APIs.

### Frontend

- **TypeScript** + [Vite](https://vitejs.dev/).
- **WebComponents** for a lightweight and modular UI.
- **IndexedDB** via [dexie.js](https://dexie.org/) for local data management.

## How to Contribute

We welcome contributions! Here’s how you can get involved:

1. **Find an Issue:** Check the [open issues](https://github.com/merlinschumacher/kinohannover/issues) and look for labels like "Good First Issue" to find beginner-friendly tasks.
2. **Fork the Repo:** Clone the repository and create a new branch for your work.
3. **Submit a Pull Request:** When you’re ready, submit a pull request for review.

### Areas to Contribute

- **Bug Fixes:** Help resolve [open bugs](https://github.com/merlinschumacher/kinohannover/issues?q=is%3Aissue+is%3Aopen+label%3Abug).
- **New Features:** Suggest or implement new functionalities.
- **Documentation:** Improve or expand the documentation, tutorials, or translations.
- **Testing:** Write and improve automated tests to ensure the code remains stable.

### Why Contribute?

- **Make an Impact:** Help users access cinema programs in an accessible and open format.
- **Learn & Grow:** Develop skills in C#, TypeScript, WebComponents, and web scraping.
- **Join a Community:** Be part of a project with a focus on privacy, accessibility, and open-source ethics.

## Data Sources

Data is sourced from the cinemas' official websites and [TMDB](https://www.themoviedb.org/).

_All movie data, titles, and images are the property of their respective owners._

## Technologies Used

- [.NET (C#)](https://dotnet.microsoft.com/)
- [Vite](https://vitejs.dev/)
- [dexie.js](https://dexie.org/)
- [Html Agility Pack](https://html-agility-pack.net/)

## Legal Disclaimer

This project is non-commercial, free, and open-source. Its sole purpose is to aggregate and present publicly available cinema program data from various cinemas in Hanover, Germany, for the convenience of users. The project does not intend to infringe on any copyrights or intellectual property rights, particularly under German and EU law.

- **Movie Data and Images:** All movie titles, descriptions, images, and related content remain the property of the respective film studios, distributors, or other rights holders. This project merely sources such content from publicly accessible platforms, including cinema websites and [The Movie Database (TMDB)](https://www.themoviedb.org/). The TMDB API is used for fetching supplementary data; however, this project is neither endorsed nor certified by TMDB.

- **Use of Scraping Tools:** The scraping of cinema program data is done in compliance with publicly available information provided by cinemas. No protected content is accessed or distributed beyond what is openly provided on the cinemas' websites or public APIs. We do not circumvent any protection mechanisms, nor do we scrape personal data or information protected under copyright.

- **No Editorial Changes:** The content displayed is provided as-is, directly from the data sources (cinemas and TMDB), without any editorial modification or interference. The website operates purely as an aggregator, presenting cinema programs in their original form.

- **No Commercial Intent:** The project is entirely non-commercial. There is no intent to generate revenue or monetize any aspect of the content. The website does not display ads, collect user data, or generate profits in any form. The goal is to make cinema listings more accessible, particularly for venues that may not have a strong online presence.

- **Protection of Copyright:** In the event that any content owner believes their rights have been infringed, we encourage them to contact us directly. Upon request, any infringing material will be promptly reviewed and removed if necessary. We strive to comply with all applicable intellectual property laws, including the German Copyright Act (Urheberrechtsgesetz, UrhG) and relevant EU directives.

For inquiries or concerns related to copyright or content usage, please open an issue in the repository or contact the project maintainers.

## License

This project is licensed under the **[GNU Affero General Public License v3.0 (AGPL-3.0)](https://www.gnu.org/licenses/agpl-3.0.en.html)**. It is free to use, modify, and distribute under the terms of the AGPL-3.0.

## Acknowledgements

- [Inter Font by Rasmus Andersson](https://rsms.me/inter/) (SIL Open Font License)
- [Material Design Icons by Google](https://github.com/google/material-design-icons/blob/master/LICENSE) (Apache 2 License)
- [Material Symbols by Marella](https://github.com/marella/material-symbols) (Apache 2 License)
- [Dexie.js by David Fahlander/Awarica AB](https://github.com/dexie/Dexie.js/blob/master/LICENSE) (Apache 2 License)
- [Scroll Snap Slider by Barthélémy Bonhomme/barthy-koeln](https://github.com/barthy-koeln/scroll-snap-slider/blob/main/LICENSE) (MIT License)
- [Gallopping horse gif by r8r](https://www.flickr.com/photos/r8r/3444024147/in/photostream/) (CC BY-NC 2.0)
- [Noto Emoji Font by Google](https://github.com/adobe-fonts/noto-emoji-svg#license) (SIL Open Font License, version 1.1)
- [CsvHelper by Josh Close](https://github.com/JoshClose/CsvHelper/blob/master/LICENSE.txt) (Apache 2 / MS-PL License)
- [Fastenshtein by DanHarltey](https://github.com/DanHarltey/Fastenshtein/blob/master/LICENSE) (MIT License)
- [Html Agility Pack by ZZZ Projects](https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE) (MIT License)
- [iCal.NET by Rian Stockbower/rianjs](https://github.com/rianjs/ical.net/blob/master/license.md) (MIT License)

- <img src="https://raw.githubusercontent.com/merlinschumacher/filme-hannover/master/frontend/src/assets/tmdb-logo.svg" style="height: 12px;" />This product uses the TMDB API but is not endorsed or certified by TMDB.
- **All Contributors:** Thanks to everyone who has contributed to this project! You can find the full list of contributors [here](https://github.com/merlinschumacher/kinohannover/graphs/contributors).
