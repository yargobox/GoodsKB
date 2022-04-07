# GoodsKB

### PowerShell commads

#### Create solution
`dotnet new sln`
#### Create WebApi project
`dotnet new webapi -n GoodsKB.API`
#### Add WebApi project to solution
`dotnet sln add GoodsKB.API/GoodsKB.API.csproj`
#### Create dynamic library for Data Access Layer (DAL)
`dotnet new classlib -n GoodsKB.DAL`
#### Add DAL project to solution
`dotnet sln add GoodsKB.DAL/GoodsKB.DAL.csproj`
#### Create dynamic library for Business Logic Layer (BLL)
`dotnet new classlib -n GoodsKB.BLL`
#### Add BLL project to solution
`dotnet sln add GoodsKB.BLL/GoodsKB.BLL.csproj`
#### Add ref to API on BLL
`dotnet add GoodsKB.API/GoodsKB.API.csproj reference GoodsKB.BLL/GoodsKB.BLL.csproj`
#### Add ref to BLL on DAL
`dotnet add GoodsKB.BLL/GoodsKB.BLL.csproj reference GoodsKB.DAL/GoodsKB.DAL.csproj`
