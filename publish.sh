#!/bin/bash

targets=('linux-x64' 'linux-musl-x64' 'osx-x64' 'win-x64')
version="$(grep -oP '(?<=Version>)\d+\.\d+\.\d+' Depreq/Depreq.csproj)"

for target in "${targets[@]}"
do
  echo "Building for ${target}..."
  if [ "${target}" = 'linux-x64' ]; then
    dotnet publish Depreq -c Release -r "${target}" --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true
  else
    dotnet publish Depreq -c Release -r "${target}" --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
  fi
  mkdir -p bin
  cp -f "Depreq/bin/Release/netcoreapp3.1/${target}/publish/Depreq" "bin/depreq-${version}-${target}" &> /dev/null
  cp -f "Depreq/bin/Release/netcoreapp3.1/${target}/publish/Depreq.exe" "bin/depreq-${version}-${target}.exe" &> /dev/null
done
