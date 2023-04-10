#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

ENV PP_ENV=my_env_pp
ENV QQ=$IMAGE_TAG
ENV ZZ="asdasd"
ENV PORT_WEBAPI=$INTERNAL_PORT
ENV CLIENT_ORIGIN_URL=$CLIENT_ORIGIN_URL
ENV AUTH0_AUDIENCE=$AUTH0_AUDIENCE
ENV AUTH0_DOMAIN=$AUTH0_DOMAIN

EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/AIaaS.WebAPI/AIaaS.WebAPI.csproj", "src/AIaaS.WebAPI/"]
RUN dotnet restore "src/AIaaS.WebAPI/AIaaS.WebAPI.csproj"
COPY . .
WORKDIR "/src/src/AIaaS.WebAPI"
RUN dotnet build "AIaaS.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AIaaS.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AIaaS.WebAPI.dll"]