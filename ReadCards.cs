using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Text;

namespace Cardalog.Api
{
  // Run with `func host start --cors *` to be able to access this from the web app.
  // I've also updated local.settings.json to allow CORS.
  // Write seeds to Mongo with `mongoimport -d cardalog -c cards .\mtg-cards.json`.
  // Make sure the JSON is minified (on one line).
  public static class Cards
    {
        [FunctionName("ReadCards")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cards")] HttpRequest req,
            ILogger log)
        {
            try
            {
                var client = new MongoClient("mongodb://127.0.0.1:27017");
                var db = client.GetDatabase("cardalog");
                var coll = db.GetCollection<BsonDocument>("cards");
                var projection = Builders<BsonDocument>.Projection.Exclude("_id");
                var cardsBson = await coll.Find(new BsonDocument()).Project(projection).ToListAsync();
                var cards = new StringBuilder("[");
                cardsBson.ForEach(it => cards.Append(it.ToJson()).Append(","));
                cards.Remove(cards.Length - 1, 1).Append("]");

                return (ActionResult)new OkObjectResult(cards.ToString());
            }
            catch (Exception e)
            {
                log.LogError(e, "Something went wrong");
                return new BadRequestObjectResult("Something went wrong.");
            }
        }
    }
}
