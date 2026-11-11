FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SchoolAdmission.slnx ./
COPY src/SchoolAdmission.Api/SchoolAdmission.Api.csproj src/SchoolAdmission.Api/
RUN dotnet restore src/SchoolAdmission.Api/SchoolAdmission.Api.csproj
COPY src/ src/
RUN dotnet publish src/SchoolAdmission.Api/SchoolAdmission.Api.csproj -c Release -o /app/publish --no-restore

FROM build AS test
COPY tests/SchoolAdmission.Api.Tests/SchoolAdmission.Api.Tests.csproj tests/SchoolAdmission.Api.Tests/
RUN dotnet restore tests/SchoolAdmission.Api.Tests/SchoolAdmission.Api.Tests.csproj
COPY tests/ tests/
RUN dotnet build tests/SchoolAdmission.Api.Tests/SchoolAdmission.Api.Tests.csproj -c Release --no-restore
ENTRYPOINT ["dotnet", "test", "tests/SchoolAdmission.Api.Tests/SchoolAdmission.Api.Tests.csproj", "-c", "Release", "--no-build"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "SchoolAdmission.Api.dll"]
