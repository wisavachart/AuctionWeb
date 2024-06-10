using System.Net;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Models;
using SearchService.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy())
;


var app = builder.Build();

app.UseAuthorization();

app.MapControllers();
// await DB.InitAsync("SearchDb", MongoClientSettings
// .FromConnectionString(builder.Configuration.GetConnectionString("MongoDbConnection")));
app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    };
});

// await DB.Index<Item>()
// .Key(x => x.Make, KeyType.Text)
// .Key(x => x.Model, KeyType.Text)
// .Key(x => x.Color, KeyType.Text).CreateAsync();




app.Run();


static IAsyncPolicy<HttpResponseMessage> GetPolicy()
=> HttpPolicyExtensions.HandleTransientHttpError().OrResult(message => message.StatusCode == HttpStatusCode.NotFound)
.WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));