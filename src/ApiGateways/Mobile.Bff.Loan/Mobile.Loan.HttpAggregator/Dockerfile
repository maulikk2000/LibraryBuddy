FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY src/ApiGateways/Mobile.Bff.Loan/Mobile.Loan.HttpAggregator/Mobile.Loan.HttpAggregator.csproj src/ApiGateways/Mobile.Bff.Loan/Mobile.Loan.HttpAggregator/
RUN dotnet restore src/ApiGateways/Mobile.Bff.Loan/Mobile.Loan.HttpAggregator/Mobile.Loan.HttpAggregator.csproj
COPY . .
WORKDIR /src/src/ApiGateways/Mobile.Bff.Loan/Mobile.Loan.HttpAggregator
RUN dotnet build Mobile.Loan.HttpAggregator.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Mobile.Loan.HttpAggregator.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Mobile.Loan.HttpAggregator.dll"]
