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
        dynamic data = JsonConvert.DeserializeObject(body);

        if (data == null)
        {
          return new BadRequestObjectResult("The request body could not be deserialized.");
        }

        var card = RequestJsonToBson(data);
        coll.InsertOne(card);
        return new OkObjectResult(card);
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
            { "Title", (string)json.title },
            { "Type", (string)json.type },
            { "Subtype", (string)json.subtype },
            { "Rarity", (string)json.rarity },
            { "Cost", (string)json.cost },
            { "ConvertedCost", (int?)json.convertedCost },
            { "Text", (string)json.text },
            { "FlavorText", (string)json.flavorText },
            { "Power", (int?)json.power },
            { "Toughness", (int?)json.toughness },
            { "Expansion", new BsonDocument
                {
                    { "Name", (string)json.expansion.name },
                    { "TotalCards", (int?)json.expansion.totalCards },
                    { "Abbreviation", (string)json.expansion.abbreviation },
                    { "Copyright", (string)json.expansion.copyright }
                }
            },
            { "CardNumber", (int?)json.cardNumber },
            { "Artist", (string)json.artist }
        };
    }
  }
}
