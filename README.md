# SignalR.Hmg

## Introduction

SignalR.Hmg - Hubs metadata generator

### Why you need this

Unfortunetly, during using SignalR in your dotnet backend application, you don't have a way how to define how to integrate with your SignalR service

In this repo we're trying to create "Swagger" for SignalR service.

List of fetures:

1. Generate Metadata for a SignalR service
2. Create Asp.net middleware for SignalR interactive page
3. Create MsBuild lib for injection metadata generation into your build pipeline

### How to use

Right now we have only console application. You can navigate to this app and provide path to your signalr csproj.

As the result of execution you will have a json file with metadata.

### What can be improved

There is a list of improvements:

1. e2e tests don't work. The reason is we should incude asp.net as reference
2. type system doesn't cover all dotnet types. For this moment we support only promitives, you custom classes and general types. We don't support generics, dictionaries and etc.
3. Repository Ci/Cd.  

## Repo structure

``` text
+
|- src
|   |- Signalr.Hmg.Core
|   |- clients
|       |- Signalr.Hmg.Clients.ConsoleApp
|- tests
    |- units
    |   |- *
    |- e2es
        |- Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice
        |- Signalr.Hmg.Tests.E2es.E2eTests

```

## How to test

Comming soon...

## Contribution

Comming soon...