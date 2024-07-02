# PxWeb Api[![Mentioned in Awesome Official Statistics ](https://awesome.re/mentioned-badge.svg)](http://www.awesomeofficialstatistics.org)

This is the official source code repository for PxWebApi. PxWeb is a nice web application for dissemination of statistical tables please read more abou it at the official web page on Statistics Sweden web site at [www.scb.se/px-web](https://www.scb.se/px-web).

## Current activities

We are currently developing PxWebApi 2.0

## Demo installastions

- https://pxapi2-master-px.scb.se/swagger
- https://pxapi2-master-cnmm.scb.se/swagger

## Developer notes

```sh
curl -i -H "API_ADMIN_KEY: test" -X 'PUT'  https://localhost:8443/api/v2/admin/database
curl -i -H "API_ADMIN_KEY: test" -X 'POST' https://localhost:8443/api/v2/admin/searchindex
curl -i -H "API_ADMIN_KEY: test" -X 'PATCH' -H 'Content-Type: application/json' -d '["TAB001", "TAB004"]' https://localhost:8443/api/v2/admin/searchindex
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

### Code formatting

We use [dotnet format](https://github.com/dotnet/format) to clean the source code. The build pipeline aslo checks for formatting error.

If you don't want to manually run `dotnet format` or Code Cleanup in Visual Studio you can use git [pre-commit](https://pre-commit.com/). After installing pre-commit for your operating system, run `pre-commit install`from the root of this repo and you're done.

The rules for formatting are set in the [.editorconfig](.editorconfig) file. Visual Studio supports this automatically, and for VS Code we have the [EditorConfig extension](https://marketplace.visualstudio.com/items?itemName=EditorConfig.EditorConfig). More information on [EditorConfig](https://editorconfig.org/)
