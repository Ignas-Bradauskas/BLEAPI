using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLEBackend.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BLEBackend.Controllers
{
    [Route("api/devices")]
    [ApiController]
    public class DevicesController : ControllerBase
    {

        private bool IsMissing(DateTimeOffset datetime) => DateTime.Now.ToUniversalTime() - datetime > TimeSpan.FromSeconds(500);

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            DevicePersistenceService.MongoCollection = DevicePersistenceService.MongoDatabase.GetCollection<BsonDocument>("hub1");
            var rawDevices1 = DevicePersistenceService.MongoCollection.Find(_ => true).ToList();
            var devicesModified1 = rawDevices1.Select(x =>
                new {SignalStrength = (short)x["Value"]["SignalStrength"].AsInt32, LastSeen = DateTimeOffset.Parse(x["Value"]["LastSeen"].AsString), Key = x["Key"].AsInt64.ToString(), IsMissing = IsMissing(DateTimeOffset.Parse(x["Value"]["LastSeen"].AsString)), HubId = 1}).ToList();

            DevicePersistenceService.MongoCollection = DevicePersistenceService.MongoDatabase.GetCollection<BsonDocument>("hub2");
            var rawDevices2 = DevicePersistenceService.MongoCollection.Find(_ => true).ToList();
            var devicesModified2 = rawDevices2.Select(x =>
                new { SignalStrength = (short)x["Value"]["SignalStrength"].AsInt32, LastSeen = DateTimeOffset.Parse(x["Value"]["LastSeen"].AsString), Key = x["Key"].AsInt64.ToString(), IsMissing = IsMissing(DateTimeOffset.Parse(x["Value"]["LastSeen"].AsString)), HubId = 2 }).ToList();

            DevicePersistenceService.MongoCollection = DevicePersistenceService.MongoDatabase.GetCollection<BsonDocument>("hub3");
            var rawDevices3 = DevicePersistenceService.MongoCollection.Find(_ => true).ToList();
            var devicesModified3 = rawDevices3.Select(x =>
                new { SignalStrength = (short)x["Value"]["SignalStrength"].AsInt32, LastSeen = DateTimeOffset.Parse(x["Value"]["LastSeen"].AsString), Key = x["Key"].AsInt64.ToString(), IsMissing = IsMissing(DateTimeOffset.Parse(x["Value"]["LastSeen"].AsString)), HubId = 3 }).ToList();

            devicesModified1.AddRange(devicesModified2);
            devicesModified1.AddRange(devicesModified3);

            devicesModified1.RemoveAll(x => x.IsMissing);

            var dic = new Dictionary<string, object>();
            foreach (var dev in devicesModified1)
            {
                var samekey = devicesModified1.Where(x => x.Key == dev.Key).ToList();
                samekey.Sort((emp1, emp2) => emp1.SignalStrength.CompareTo(emp2.SignalStrength));
                var bestStr = samekey.Last();
                dic[bestStr.Key] = bestStr;
            }

            return Ok(dic);
        }
    }
}
