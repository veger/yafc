[ -d "Build" ] && rm -r Build 

dotnet publish YAFC/YAFC.csproj -r win-x64 -c Release -o Build/Windows -p:PublishTrimmed=true
dotnet publish YAFC/YAFC.csproj -r osx-x64 --self-contained false -c Release -o Build/OSX
dotnet publish YAFC/YAFC.csproj -r linux-x64 --self-contained false -c Release -o Build/Linux
