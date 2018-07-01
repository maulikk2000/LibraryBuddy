FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 15282
EXPOSE 44399

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY src/WebApps/WebStatus/WebStatus.csproj src/WebApps/WebStatus/
RUN dotnet restore src/WebApps/WebStatus/WebStatus.csproj
COPY . .
WORKDIR /src/src/WebApps/WebStatus
RUN dotnet build WebStatus.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish WebStatus.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebStatus.dll"]
