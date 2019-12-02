using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.IO;
using Newtonsoft.Json;

namespace Cardalog.Api
{
  public static class WriteCard
    {
        // See ./Seeds/mtg-cards.json for an example of the expected JSON
        [FunctionName("WriteCard")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cards")] HttpRequest req,
            ILogger log)
        {
            try
            {
                var client = new MongoClient("mongodb://127.0.0.1:27017");
                var db = client.GetDatabase("cardalog");
                var coll = db.GetCollection<BsonDocument>("cards");
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                // Seems quickest to start with a dynamic. Might want to look into a data types library to share amongst the UI and Functions like this.
                dynamic data = JsonConvert.DeserializeObject(body);

                if (data == null)
                {
                    return new BadRequestObjectResult("The request body could not be deserialized.");
                }

                var card = RequestJsonToBson(data);
                coll.InsertOne(card);
                return new OkObjectResult(card); // Is it appropriate to respond with the object I just inserted? Seems chatty.
            }
            catch (Exception e)
            {
                log.LogError(e, "Something went wrong.");
                return new BadRequestObjectResult("Something went wrong.");
            }
        }

        private static BsonDocument RequestJsonToBson(dynamic json)
        {
            return new BsonDocument
            {
                { "Title", json.title.ToString() },
                { "Type", json.type.ToString() },
                { "Subtype", json.subtype.ToString() },
                { "Rarity", json.rarity.ToString() },
                { "Cost", json.cost.ToString() },
                { "ConvertedCost", (int?)json.convertedCost },
                { "Text", json.text.ToString() },
                { "FlavorText", json.flavorText.ToString() },
                { "Power", (int?)json.power },
                { "Toughness", (int?)json.toughness },
                { "Expansion", new BsonDocument
                    {
                        { "Name", json.expansion.name.ToString() },
                        { "TotalCards", (int?)json.expansion.totalCards },
                        { "Abbreviation", json.expansion.abbreviation.ToString() },
                        { "Copyright", json.expansion.copyright.ToString() }
                    }
                },
                { "CardNumber", (int?)json.cardNumber },
                { "Artist", json.artist.ToString() }
            };
        }
    }
}
