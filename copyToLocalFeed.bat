dotnet pack .\src\Fancy.ResourceLinker.Models\ -c debug
xcopy .\src\Fancy.ResourceLinker.Models\bin\Debug\*.nupkg ..\..\Packages

dotnet pack .\src\Fancy.ResourceLinker.Gateway\ -c debug
xcopy .\src\Fancy.ResourceLinker.Gateway\bin\Debug\*.nupkg ..\..\Packages

dotnet pack .\src\Fancy.ResourceLinker.Gateway.EntityFrameworkCore\ -c debug
xcopy .\src\Fancy.ResourceLinker.Gateway.EntityFrameworkCore\bin\Debug\*.nupkg ..\..\Packages