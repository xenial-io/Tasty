@pushd %~dp0
@dotnet run --project ".\build\Tasty.Build\Tasty.Build.csproj" --no-launch-profile -- %*
@popd