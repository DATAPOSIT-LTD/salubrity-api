#!/bin/bash

set -e

echo "🔄 Resetting and pulling latest changes..."
git reset --hard
git clean -fd
git pull origin $(git rev-parse --abbrev-ref HEAD)

echo "🧹 Cleaning previous builds and caches..."
dotnet clean
rm -rf Salubrity.Api/bin Salubrity.Api/obj
rm -rf Salubrity.Infrastructure/bin Salubrity.Infrastructure/obj

echo "📦 Restoring dependencies..."
dotnet restore

echo "🔨 Building solution..."
dotnet build --configuration Production

echo "🚀 Publishing API..."
dotnet publish Salubrity.Api \
  --configuration Production \
  --output publish \
  --no-restore

echo "📂 Running EF Core migrations..."
ASPNETCORE_ENVIRONMENT=Production \
dotnet ef database update \
  --project Salubrity.Infrastructure \
  --startup-project Salubrity.Api \
  --configuration Production

echo "✅ Migration and deployment complete."
