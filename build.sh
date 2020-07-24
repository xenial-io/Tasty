#!/usr/bin/env bash
set -euo pipefail
dotnet run --project "./build/Tasty.Build/Tasty.Build.csproj" -- "$@"