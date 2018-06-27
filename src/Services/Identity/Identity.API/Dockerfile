FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk as build
WORKDIR /src
COPY src/Services/Identity/Identity.API/Identity.API.csproj src/Services/Identity/Identity.API/
RUN dotnet restore src/Services/Identity/Identity.API/Identity.API.csproj
COPY . .
WORKDIR /src/src/Services/Identity/Identity.API
RUN dotnet build Identity.API.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Identity.API.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Identity.API.dll"]
