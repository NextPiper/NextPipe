# Define builder stage
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore respective projects
COPY *.sln .
COPY NextPipe/*.csproj ./NextPipe/
COPY NextPipe.Core/*.csproj ./NextPipe.Core/

# Restore each projects
RUN dotnet restore

# After restore copy all the code and build the App
COPY NextPipe/. ./NextPipe/
COPY NextPipe.Core/. ./NextPipe.Core/

# Change workdir to NextPipe and build from the .csproj file
WORKDIR /app/NextPipe
RUN dotnet publish -c Release -o out

# Define runtime stage. Create /app workdir and copy the build
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/NextPipe/out ./
ENTRYPOINT [ "dotnet", "NextPipe.dll" ]