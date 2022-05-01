using GoodsKB.API.Middlewares;
using GoodsKB.BLL.Services;
using GoodsKB.DAL;
using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using BsonType = MongoDB.Bson.BsonType;
using AutoMapper;


BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new EnumSerializer<UserRoles>(BsonType.String));



var builder = WebApplication.CreateBuilder(args);

var mongoDbConfig = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

builder.Services.AddSingleton<IMongoDbContext>(serviceProvider => new MongoDbContext(mongoDbConfig));
builder.Services.AddSingleton<IDALContext, DALContext>();
builder.Services.AddControllers(options => { options.SuppressAsyncSuffixInActionNames = false; });
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSwaggerGen();


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
//app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
