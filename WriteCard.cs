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
                { "Title", json.Title.ToString() },
                { "Type", json.Type.ToString() },
                { "Subtype", json.Subtype.ToString() },
                { "Rarity", json.Rarity.ToString() },
                { "Cost", json.Cost.ToString() },
                { "ConvertedCost", (int?)json.ConvertedCost },
                { "Text", json.Text.ToString() },
                { "FlavorText", json.FlavorText.ToString() },
                { "Power", (int?)json.Power },
                { "Toughness", (int?)json.Toughness },
                { "Expansion", new BsonDocument
                    {
                        { "Name", json.Expansion.Name.ToString() },
                        { "TotalCards", (int?)json.Expansion.TotalCards },
                        { "Abbreviation", json.Expansion.Abbreviation.ToString() },
                        { "Copyright", json.Expansion.Copyright.ToString() }
                    }
                },
                { "CardNumber", (int?)json.CardNumber },
                { "Artist", json.Artist.ToString() }
            };
        }
    }
}
