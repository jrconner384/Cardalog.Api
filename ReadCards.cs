using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;

namespace Cardalog.Api
{
    // Run with `func host start --cors *` to be able to access this from the web app
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
                var cardsBson = coll.Find(new BsonDocument()).Project(projection);
                List<string> cards = new List<string>();
                await cardsBson.ForEachAsync(it => cards.Add(it.ToJson(new JsonWriterSettings{
                    Indent = false,
                    OutputMode = JsonOutputMode.Strict
                })));
                return new OkObjectResult(cards);
            }
            catch(Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }
    }
}
