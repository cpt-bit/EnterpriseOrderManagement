# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY ["EnterpriseOrderManagement.slnx", "./"]
COPY ["src/OrderProcessing.API/OrderProcessing.API.csproj", "src/OrderProcessing.API/"]

# Restore dependencies
RUN dotnet restore "src/OrderProcessing.API/OrderProcessing.API.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/src/OrderProcessing.API"
RUN dotnet build "OrderProcessing.API.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish "OrderProcessing.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderProcessing.API.dll"]