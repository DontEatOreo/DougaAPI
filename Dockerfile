FROM mcr.microsoft.com/dotnet/aspnet:7.0 as base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /src
COPY ["DougaAPI/DougaAPI.csproj", "./"]
RUN dotnet restore "DougaAPI.csproj"
COPY DougaAPI . 
RUN dotnet build "DougaAPI.csproj" -c Release -o /app/build

FROM build as publish
RUN dotnet publish "DougaAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "DougaAPI.dll" ]
