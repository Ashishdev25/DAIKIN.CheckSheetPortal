# Use the official ASP.NET Core runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

# Set the working directory inside the container
WORKDIR /app

# Install required dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    libc6-dev \
    libx11-dev \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Expose ports
EXPOSE 80
EXPOSE 443

# Set environment variable to enable Unix support
ENV COMPlus_EnableDiagnostics=0

# Define the working directory for the container
WORKDIR /app


# Set the base image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Copy the source code
COPY . /src

# Set the working directory inside the container
WORKDIR /src

# Restore dependencies
RUN dotnet restore "DAIKIN.CheckSheetPortal.API/DAIKIN.CheckSheetPortal.API.csproj"

# Build the application
RUN dotnet build "DAIKIN.CheckSheetPortal.API/DAIKIN.CheckSheetPortal.API.csproj" -c Release -o /app/build

# Set the base image for the publish stage
FROM build AS publish

# Publish the application
RUN dotnet publish "DAIKIN.CheckSheetPortal.API/DAIKIN.CheckSheetPortal.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Set the base image for the final stage
FROM base AS final

# Copy the published application to the final image
COPY --from=publish /app/publish .

# Define the entry point for the container
ENTRYPOINT ["dotnet", "DAIKIN.CheckSheetPortal.API.dll"]
