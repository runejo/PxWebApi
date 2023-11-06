# PxWeb Api[![Mentioned in Awesome Official Statistics ](https://awesome.re/mentioned-badge.svg)](http://www.awesomeofficialstatistics.org)
This is the official source code repository for PxWebApi. PxWeb is a nice web application for dissemination of statistical tables please read more abou it at the official web page on Statistics Sweden web site at [www.scb.se/px-web](https://www.scb.se/px-web).

## Current activities
We are currently developing PxWebApi 2.0

## Developer notes

```sh
curl -i -H "API_ADMIN_KEY: test" -X 'PUT'  https://localhost:5001/api/v2/admin/database
```
```sh
curl -i -H "API_ADMIN_KEY: test" -X 'POST' https://localhost:5001/api/v2/admin/searchindex
```
```sh
docker build -t pxwebapi .
```
```sh
docker run -p 8080:8080 pxwebapi
```
