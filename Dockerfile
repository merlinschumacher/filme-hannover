# Stage 1: Build the backend
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine@sha256:3f0fb43bb36dbce018486be9ba00a88121b838ac8ea47f8e3ddc9579a7391dda AS be-build-env
WORKDIR /app
# Copy only the necessary files for restoring dependencies
COPY ./backend/*.csproj ./backend/
RUN dotnet restore ./backend/*.csproj
# Copy the rest of the backend source code and build
COPY ./backend/ ./backend/
RUN dotnet publish ./backend -c Release -r linux-musl-x64 -o out -p:PublishReadyToRun=true -p:InvariantGlobalization=false

# Stage 2: Build the frontend application
FROM node:lts-alpine@sha256:cb3143549582cc5f74f26f0992cdef4a422b22128cb517f94173a5f910fa4ee7 AS fe-build-env
WORKDIR /app
# Copy only the necessary files for restoring dependencies
COPY ./frontend/package.json ./frontend/package-lock.json* ./
RUN npm ci && npm cache clean --force
# Copy the rest of the frontend source code and build
COPY ./frontend/ ./
RUN npm run build

# Stage 3: Create an empty image to merge the output from both backend and frontend builds
FROM scratch AS artifacts
# Copy the appsettings.json with the default values
COPY entrypoint.sh .
COPY ./backend/appsettings.json .
# Copy the output from the build environments
COPY --from=be-build-env /app/out .
COPY --from=fe-build-env /app/dist wwwroot

# Stage 4: Build runtime image to run the application with necessary dependencies
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine@sha256:83ac89532aed59b91078025389d67deb00bd35db770e7d496e5c4cae8cf04ec7 AS runtime
ENV TZ="Europe/Berlin"
ENV DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION=1
ENV TERM=xterm

LABEL org.opencontainers.image.description="This is the base image for the generation of filme-hannover.de"
LABEL org.opencontainers.image.source="https://github.com/merlinschumacher/filme-hannover"
RUN apk add --no-cache icu-libs icu-data-full tzdata
WORKDIR /app
COPY --from=artifacts / .
RUN chmod +x /app/entrypoint.sh
VOLUME ["/output", "/wwwroot/data"]
# Define the entry point for the container to start the application
ENTRYPOINT ["/app/entrypoint.sh"]
