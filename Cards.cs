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
  public static class Cards
    {
        [FunctionName("Cards")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                var client = new MongoClient("mongodb://127.0.0.1:27017");
                var db = client.GetDatabase("cardalog");
                var coll = db.GetCollection<BsonDocument>("cards");
                var cards = coll.Find(new BsonDocument()).ToList();
                return (ActionResult)new OkObjectResult(cards);
            }
            catch(Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }
    }
}
