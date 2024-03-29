using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using System;

namespace Cardalog.Api
{
  // Run with `func host start --cors *` to be able to access this from the web app.
  // I've also updated local.settings.json to allow CORS.
  // Write seeds to Mongo with `mongoimport -d cardalog -c cards .\mtg-cards.json --jsonArray`.
  // Make sure the JSON is minified (on one line).
  public static class ReadCards
  {
    [FunctionName("ReadCards")]
    public static async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cards")] HttpRequest req,
      ILogger log)
    {
      try
      {
        var conn = System.Environment.GetEnvironmentVariable("AtlasConnString", EnvironmentVariableTarget.Process);
        var client = new MongoClient(conn);
        var db = client.GetDatabase("cardalog");
        var coll = db.GetCollection<BsonDocument>("cards");
        var projection = Builders<BsonDocument>.Projection.Exclude("_id");
        var cardsBson = await coll.Find(new BsonDocument()).Project(projection).ToListAsync();
        var cards = new BsonArray(cardsBson);
        return (ActionResult)new OkObjectResult(cards.ToJson());
      }
      catch (Exception e)
      {
        log.LogError(e, "Something went wrong");
        return new BadRequestObjectResult("Something went wrong.");
      }
    }
  }
}
