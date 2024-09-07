FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS be-build-env
WORKDIR /app
# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -r linux-musl-x64 -o out -p:PublishReadyToRun=true -p:InvariantGlobalization=false

FROM node:lts-alpine AS fe-build-env
# Change the working directory
WORKDIR /app
# Copy the package.json and package-lock.json to store the dependencies as a distinct layer
COPY ./frontend/package.json ./frontend/package-lock.json* /app/
RUN npm ci && npm cache clean --force
# Copy the rest of the application
COPY ./frontend /app/
# Build the application
RUN npm run build

# Create an empty image to merge the output
FROM scratch AS artifacts 
# Copy the appsettings.json with the default values
COPY entrypoint.sh .
COPY ./backend/appsettings.json .
# Copy the output from the build environments
COPY --from=be-build-env /app/out .
COPY --from=fe-build-env /app/dist wwwroot

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS runtime
ENV TZ="Europe/Berlin"
LABEL org.opencontainers.image.description="This is the base image for the generation of filme-hannover.de"
LABEL org.opencontainers.image.source="https://github.com/merlinschumacher/filme-hannover"
RUN apk add --no-cache icu-libs tzdata
WORKDIR /app
COPY --from=artifacts / .
VOLUME /output /wwwroot/data
# Start the application
ENTRYPOINT /app/entrypoint.sh
