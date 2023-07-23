FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app
EXPOSE 5000

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS final-env
ENV ASPNETCORE_URLS http://+:5000
ENV ASPNETCORE_HOST_ENVIRONMENT Docker
<<<<<<< Updated upstream
=======
ENV AWS_ACCESS_KEY_ID AKIAYJZFYZYSVR24GFEL
ENV AWS_SECRET_ACCESS_KEY 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
ENV DB_SERVER recipi-db-mssql.cmnzbcgsvlqu.us-east-2.rds.amazonaws.com,1433
ENV DB_PRIMARY_DB recipi-db
ENV DB_USER_ID JDLRecipi
ENV DB_USER_PW Capstone_2023

>>>>>>> Stashed changes
WORKDIR /app
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "Recipi-API.dll"]
