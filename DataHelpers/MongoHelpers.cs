﻿using MongoDB.Bson;
using MongoDB.Driver;

namespace FenixAlliance.Data.Access.Helpers
{
    public static class MongoHelpers
    {

        public static IMongoCollection<BsonDocument> GetMongodbClient()
        {
            // TODO: Ensure mongodb is running.
            var client = new MongoClient();
            var db = client.GetDatabase("AllianceBusinessSuite");
            var AllianceBusinessSuites = db.GetCollection<BsonDocument>("AllianceBusinessSuites");
            return AllianceBusinessSuites;
        }
    }
}
