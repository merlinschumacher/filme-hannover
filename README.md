# filme-hannover.de

[![Generate page](https://github.com/merlinschumacher/kinohannover/actions/workflows/run.yml/badge.svg)](https://github.com/merlinschumacher/kinohannover/actions/workflows/run.yml)

This is the application code that generates the content for [filme-hannover.de](https://filme-hannover.de/)
The project is non-commercial, free and open-source. There is no monetary intent behind it. The only goal is to produce an easily accessible version of all cinema programs in Hanover for everybody. Especially as the smaller cinemas programs aren't listed on places like Google or other commercial pages.

The code mainly scrapes the websites for the cinemas in Hanover, Germany. It uses ways to get the data, like parsing HTML or accessing the public unprotected APIs of the cinemas. 

The page is updated once a day via a GitHub Actions workflow. The entire page is static and there is no live backend. Everything is hosted on GitHub Pages. 

The application is built in .NET and uses some VanillaJS + libraries for the frontend. 
