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
    // See ./Seeds/mtg-cards.json for an example of useful JSON
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
        { "Title", (string)json.title ?? string.Empty },
        { "Type", (string)json.type ?? string.Empty },
        { "Subtype", (string)json.subtype ?? string.Empty },
        { "Rarity", (string)json.rarity ?? string.Empty },
        { "Cost", new BsonDocument
          {
            { "Blue", (int?)json.cost?.blue },
            { "Black", (int?)json.cost?.black },
            { "Colorless", (int?)json.cost?.colorless },
            { "Converted", (int?)CalculateConvertedManaCost(json.cost) },
            { "Green", (int?)json.cost?.green },
            { "Red", (int?)json.cost?.red },
            { "White", (int?)json.cost?.white }
          }},
        { "Text", (string)json.text ?? string.Empty },
        { "FlavorText", (string)json.flavorText ?? string.Empty },
        { "Power", (int?)json.power },
        { "Toughness", (int?)json.toughness },
        { "Expansion", new BsonDocument
          {
            { "Name", (string)json.expansion?.name ?? string.Empty },
            { "TotalCards", (int?)json.expansion?.totalCards },
            { "Abbreviation", (string)json.expansion?.abbreviation ?? string.Empty },
            { "Copyright", (string)json.expansion?.copyright ?? string.Empty }
          }
        },
        { "CardNumber", (int?)json.cardNumber },
        { "Artist", (string)json.artist ?? string.Empty }
      };
    }

    private static int? CalculateConvertedManaCost(dynamic cost)
    {
      return cost == null ? 0 : (int?)((int?)cost.blue + (int?)cost.black + (int?)cost.colorless + (int?)cost.green + (int?)cost.red + (int?)cost.white);
    }
  }
}
