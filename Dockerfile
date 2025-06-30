# Use the .NET Core 8 SDK image with Alpine Linux
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-env
WORKDIR /apps

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /apps
COPY --from=build-env /apps/publish .

# Default labels
LABEL copyright="PT IDX Consulting"
LABEL website="idxpartners.com"
LABEL version="v1.0.0"

#Local Time
RUN apk add tzdata
RUN cp /usr/share/zoneinfo/Asia/Jakarta  /etc/localtime
RUN echo "Asia/Jakarta" >  /etc/timezone

# Upgrade musl to remove potential vulnerabilities
RUN apk update && \
    apk upgrade && \
    apk add --no-cache libcrypto3 libssl3 apk-tools ssl_client libcom_err musl openssl zlib libretls xz xz-libs zlib-dev busybox krb5-libs busybox-extras curl && \
    apk update && \
    apk upgrade && \
    rm -rf /var/cache/apk/*

# Create a new user and change directory ownership
RUN adduser --disabled-password \
  --home /apps \
  --gecos '' idcapps && chown -R idcapps /apps

# Switch to the new user
USER idcapps
WORKDIR /apps

#default setting
ENV ASPNETCORE_URLS=http://*:32064
ENV ASPNETCORE_ENVIRONMENT="production"
EXPOSE 32064

# Remove existing appsettings.json, create a symbolic link to masterappsettings_idc.json, and create a symbolic link for File
RUN rm -f /apps/appsettings.json && \
    ln -s /apps/config/masterappsettings_idc.json /apps/appsettings.json && \
    rm -rf /apps/idc-shr-dependency  && \
    ln -s /apps/config/idecision/idc-shr-dependency /apps/

ENTRYPOINT ["dotnet", "ScriptDeployerWeb.dll"]
