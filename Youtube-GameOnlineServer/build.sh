#!/bin/bash
git pull
dotnet build -c Release -o /app/build
dotnet publish -c Release -o /app/publish
