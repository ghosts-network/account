using MongoDB.Driver;

namespace GhostNetwork.AspNetCore.Identity.Mongo
{
    public class MongoOptions
    {
        public MongoOptions(MongoClientSettings clientSettings, string dbName)
        {
            ClientSettings = clientSettings;
            DbName = dbName;
        }

        public MongoClientSettings ClientSettings { get; }

        public string DbName { get; }
    }
}