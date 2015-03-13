.\.nuget\nuget.exe restore log4net.Raygun.sln
msbuild log4net.Raygun.sln /t:Clean

cd log4net.Raygun.WebApi

msbuild log4net.Raygun.WebApicsproj /t:Build /p:Configuration="Release 4.5"

cd ..

cd log4net.Raygun

msbuild log4net.Raygun.csproj /t:Build /p:Configuration="Release 4.0"
msbuild log4net.Raygun.csproj /t:Build;Package /p:Configuration="Release 4.5"

cd ..