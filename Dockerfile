FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:35792ea4ad1db051981f62b313f1be3b46b1f45cadbaa3c288cd0d3056eefb83 AS be-build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -r linux-x64 -o out

FROM node:lts-alpine AS fe-build-env
# Change the working directory
WORKDIR /App
# Copy the package.json and package-lock.json to store the dependencies as a distinct layer
COPY ./frontend/package.json ./frontend/package-lock.json* /App/
RUN npm ci && npm cache clean --force
# Copy the rest of the application
COPY ./frontend /App/
# Build the application
RUN npm run build

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:6c4df091e4e531bb93bdbfe7e7f0998e7ced344f54426b7e874116a3dc3233ff 
ENV DATAOUTPUTPATH=wwwroot/data/
WORKDIR /App
# Copy the output from the build environments
COPY --from=be-build-env /App/out .
COPY --from=fe-build-env /App/dist wwwroot
# Copy the appsettings.json with the default values
COPY ./backend/appsettings.json .

# Start the application
ENTRYPOINT ["dotnet", "backend.dll"]
