# Stage 1: Build the backend
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine@sha256:5f12aa62868b69dcb41de9cd7f8759822f7d1f56c3b31908048ad65df0981e67 AS be-build-env
WORKDIR /app
# Copy the backend source code and publish
COPY ./backend/ ./backend/
RUN dotnet publish ./backend -c Release -r linux-musl-x64 -o out -p:InvariantGlobalization=false --nologo -v q --property WarningLevel=0 /clp:ErrorsOnly
# Stage 2: Build the frontend application
FROM node:lts-alpine@sha256:f70403e87646dc51b45295f4b8b70cdad0b63d2297c4c9899119b03f7af7a6b3 AS fe-build-env
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
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine@sha256:bb3688be790cc31713761ccd7fba1c8ed0e88958affd230c32c40f841cac3f3f AS runtime
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