FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic AS builder
LABEL maintainer "amaya <mail@sapphire.in.net>"

WORKDIR /opt/Depreq
COPY . /opt/Depreq
RUN dotnet publish Depreq \
      -c Release --self-contained true -r linux-musl-x64 \
      -p:PublishSingleFile=true -p:PublishTrimmed=true


# --- #


FROM alpine:3.11
LABEL maintainer "amaya <mail@sapphire.in.net>"

COPY --from=builder /opt/Depreq/Depreq/bin/Release/netcoreapp3.1/linux-musl-x64/publish/Depreq \
      /usr/local/bin/depreq
RUN apk add --no-cache \
      libstdc++ libintl
