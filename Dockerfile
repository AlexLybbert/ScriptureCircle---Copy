FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ScriptureCircle.Domain/ScriptureCircle.Domain.csproj src/ScriptureCircle.Domain/
COPY src/ScriptureCircle.Shared/ScriptureCircle.Shared.csproj src/ScriptureCircle.Shared/
COPY src/ScriptureCircle.Application/ScriptureCircle.Application.csproj src/ScriptureCircle.Application/
COPY src/ScriptureCircle.Infrastructure/ScriptureCircle.Infrastructure.csproj src/ScriptureCircle.Infrastructure/
COPY src/ScriptureCircle.Api/ScriptureCircle.Api.csproj src/ScriptureCircle.Api/
RUN dotnet restore src/ScriptureCircle.Api/ScriptureCircle.Api.csproj

COPY src/ src/
RUN dotnet publish src/ScriptureCircle.Api/ScriptureCircle.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ScriptureCircle.Api.dll"]
