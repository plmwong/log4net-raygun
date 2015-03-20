@echo off

echo Cleaning up output files...
cd output
del *.nupkg
cd ..

echo Cleaning all build files...
.\.nuget\nuget.exe restore log4net.Raygun.sln
msbuild log4net.Raygun.sln /t:Clean /p:Configuration="Debug"
msbuild log4net.Raygun.sln /t:Clean /p:Configuration="Release 4.0"
msbuild log4net.Raygun.sln /t:Clean /p:Configuration="Release 4.5"

cd log4net.Raygun.WebApi

echo Building and packaging log4net.Raygun.WebApi...
msbuild log4net.Raygun.WebApi.csproj /t:Build;Package /p:Configuration="Release 4.5"
copy .\NuGet\*.nupkg ..\output

cd ..

cd log4net.Raygun

echo Building and packaging log4net.Raygun and log4net.Raygun.Mvc...
msbuild log4net.Raygun.csproj /t:Build /p:Configuration="Release 4.0"
msbuild log4net.Raygun.csproj /t:Build;Package /p:Configuration="Release 4.5"

copy .\NuGet\*.nupkg ..\output

cd ..

echo Done.