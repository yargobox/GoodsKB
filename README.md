# GoodsKB

### PowerShell commads

- Create solution
`dotnet new sln`
- Create WebApi project
`dotnet new webapi -n GoodsKB.API`
- Add WebApi project to solution
`dotnet sln add GoodsKB.API/GoodsKB.API.csproj`
- Create dynamic library for Data Access Layer (DAL)
`dotnet new classlib -n GoodsKB.DAL`
- Add DAL project to solution
`dotnet sln add GoodsKB.DAL/GoodsKB.DAL.csproj`
- Create dynamic library for Business Logic Layer (BLL)
`dotnet new classlib -n GoodsKB.BLL`
- Add BLL project to solution
`dotnet sln add GoodsKB.BLL/GoodsKB.BLL.csproj`
- Add ref to API on BLL
`dotnet add GoodsKB.API/GoodsKB.API.csproj reference GoodsKB.BLL/GoodsKB.BLL.csproj`
- Add ref to BLL on DAL
`dotnet add GoodsKB.BLL/GoodsKB.BLL.csproj reference GoodsKB.DAL/GoodsKB.DAL.csproj`


## DAL

### PowerShell commads

- Add package MongoDB.Driver
`dotnet add GoodsKB.DAL package MongoDB.Driver`

## BLL

### PowerShell commads

- Add package AutoMapper
`dotnet add GoodsKB.BLL package AutoMapper `

## API

### PowerShell commads

- Add package AutoMapper
`dotnet add GoodsKB.API package AutoMapper `
`dotnet add GoodsKB.API package AutoMapper.Extensions.Microsoft.DependencyInjection`
- Add package MongoDB.Driver
`dotnet add GoodsKB.API package MongoDB.Driver`
- Set user-secrets
`dotnet user-secrets -p GoodsKB.API init`
`dotnet user-secrets -p GoodsKB.API set MongoDbSettings:Password Pass#word1`


## Docker

### Mongo

`docker run -d --rm --name mongo -p 27017:27017 -v mongodbdata:/data/db -e MONGO_INITDB_ROOT_USERNAME=mongoadmin -e MONGO_INITDB_ROOT_PASSWORD=Pass#word1 mongo`

## To consider

- Ordering of text search results stackoverflow.com/questions/24688161/retrieve-relevance-ordered-result-from-text-query-on-mongodb-collection-using-th
- Filters for text search