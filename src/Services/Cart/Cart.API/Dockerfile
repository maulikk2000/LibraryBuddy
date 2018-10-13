#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-sac2016 AS base
WORKDIR /app
EXPOSE 2750
EXPOSE 44388

FROM microsoft/dotnet:2.1-sdk-nanoserver-sac2016 AS build
WORKDIR /src
COPY ["src/Services/Cart/Cart.API/Cart.API.csproj", "src/Services/Cart/Cart.API/"]
RUN dotnet restore "src/Services/Cart/Cart.API/Cart.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Cart/Cart.API"
RUN dotnet build "Cart.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Cart.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Cart.API.dll"]
