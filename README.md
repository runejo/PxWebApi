# PxWebApi [![Mentioned in Awesome Official Statistics ](https://awesome.re/mentioned-badge.svg)](http://www.awesomeofficialstatistics.org) [![MSBuild](https://github.com/PxTools/PxWebApi/actions/workflows/msbuild.yml/badge.svg)](https://github.com/PxTools/PxWebApi/actions/workflows/msbuild.yml)

This is the official source code repository for PxWebApi. PxWeb is a nice web application for dissemination of statistical tables please read more abou it at the official web page on Statistics Sweden web site at [www.scb.se/px-web](https://www.scb.se/px-web).

## Current activities
We are currently developing PxWebApi 2.0

## Demo installastions

* https://pxapi2-master-px.scb.se/swagger
* https://pxapi2-master-cnmm.scb.se/swagger 

## Developer notes

```sh
curl -i -H "API_ADMIN_KEY: test" -X 'PUT'  https://localhost:5001/api/v2/admin/database
curl -i -H "API_ADMIN_KEY: test" -X 'POST' https://localhost:5001/api/v2/admin/searchindex
curl -i -H "API_ADMIN_KEY: test" -X 'PATCH' -H 'Content-Type: application/json' -d '["TAB001", "TAB004"]' https://localhost:5001/api/v2/admin/searchindex
```
```sh
docker build -t pxwebapi .
docker run -p 8080:8080 pxwebapi

# multi platform build
docker buildx create --use --platform linux/amd64,linux/arm64   
docker buildx build --platform linux/amd64,linux/arm64 --push --tag runejo/pxwebapi:2.0-beta .

```
```sh
DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ASPNETCORE_URLS=http://*:8080
```
