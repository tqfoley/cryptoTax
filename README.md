# cryptoTax
C# project to figure out realized gains when buying and selling crypto

## Some Commands used to create projects and references 
- dotnet new sln
- dotnet new console -o src/gaintax
- dotnet new classlib -o src/gaintaxlib
- dotnet sln add src/gaintaxlib/gaintaxlib.csproj 
- dotnet new xunit -o test/gaintaxtest
- dotnet sln add test/gaintaxtest/gaintaxtest.csproj
- dotnet add test/gaintaxtest/gaintaxtest.csproj reference src/gaintaxlib/gaintaxlib.csproj

## To run unit tests:
- dotnet test

## To run:
- cryptoTax % dotnet run --project src/gaintax/gaintax.csproj
- bin/net8.0 % dotnet run --project ../../../gaintax.csproj