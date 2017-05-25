@echo off

dotnet restore

dotnet build

dotnet pack ".\src\ChatterToolkitForNetStandard" -c Release -o "..\..\bin\NuGetPackages"
dotnet pack ".\src\CommonLibrariesForNetStandard" -c Release -o "..\..\bin\NuGetPackages"
dotnet pack ".\src\ForceToolkitForNetStandard" -c Release -o "..\..\bin\NuGetPackages"

pause