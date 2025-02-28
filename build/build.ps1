$projectPath = "$PSScriptRoot/../Withat/Withat.csproj" # Путь к .csproj файлу
$outputDir = "$PSScriptRoot/artifacts" # Директория для выходных файлов

# Создаем директорию для выходных файлов, если она не существует
if (-Not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

dotnet restore $projectPath
dotnet build $projectPath --no-restore --configuration Release
$version = Read-Host "Enter withat package version"
dotnet pack $projectPath --no-restore --no-build --configuration Release --output $outputDir /p:Version=$version 