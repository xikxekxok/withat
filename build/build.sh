#!/bin/bash

# Определяем пути
script_dir=$(dirname "$0")
projectPath="$script_dir/../Withat/Withat.csproj"  # Путь к .csproj файлу
outputDir="$script_dir/artifacts"                 # Директория для выходных файлов

# Создаем директорию для выходных файлов
mkdir -p "$outputDir"

# Восстанавливаем зависимости и собираем проект
dotnet restore "$projectPath"
dotnet build "$projectPath" --no-restore --configuration Release

# Запрашиваем версию пакета
read -p "Enter withat package version: " version

# Упаковываем проект
dotnet pack "$projectPath" \
  --no-restore \
  --no-build \
  --configuration Release \
  --output "$outputDir" \
  -p:Version="$version"