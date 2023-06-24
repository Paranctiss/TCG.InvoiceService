#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 7239
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["NuGet.Config", "./"]
COPY ["NuGet.Config", "TCG.InvoiceService.API/"]
COPY ["NuGet.Config", "TCG.InvoiceService.Application/"]
COPY ["NuGet.Config", "TCG.InvoiceService.Domain/"]
COPY ["NuGet.Config", "TCG.InvoiceService.Persistence/"]
COPY ["TCG.InvoiceService.API/TCG.InvoiceService.API.csproj", "TCG.InvoiceService.API/"]
COPY ["TCG.InvoiceService.Application/TCG.InvoiceService.Application.csproj", "TCG.InvoiceService.Application/"]
COPY ["TCG.InvoiceService.Domain/TCG.InvoiceService.Domain.csproj", "TCG.InvoiceService.Domain/"]
COPY ["TCG.InvoiceService.Persistence/TCG.InvoiceService.Persistence.csproj", "TCG.InvoiceService.Persistence/"]
RUN dotnet restore "TCG.InvoiceService.API/TCG.InvoiceService.API.csproj"
RUN dotnet restore "TCG.InvoiceService.Application/TCG.InvoiceService.Application.csproj"
RUN dotnet restore "TCG.InvoiceService.Domain/TCG.InvoiceService.Domain.csproj"
RUN dotnet restore "TCG.InvoiceService.Persistence/TCG.InvoiceService.Persistence.csproj"
COPY . .
WORKDIR "/src/TCG.InvoiceService.API"
RUN dotnet build "TCG.InvoiceService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TCG.InvoiceService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TCG.InvoiceService.API.dll"]