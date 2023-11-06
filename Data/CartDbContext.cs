using Cursus.Data.Interface;
using Cursus.Entities;
using MongoDB.Driver;

namespace Cursus.Data
{
    public class CartDbContext : ICartDbContext
    {
        public IMongoCollection<Cart> Cart { get; }

        //public CartDbContext(IMongoClient mongoClient, ICartDatabaseSettings settings)
        //{
        //    var _database = mongoClient.GetDatabase(settings.DatabaseName);
        //    Cart = _database.GetCollection<Cart>(settings.CartCollectionName);
        //}
        public CartDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabadeName"));

            Cart = database.GetCollection<Cart>(configuration.GetValue<string>("DatabaseSettings:CartCollectionName"));
        }
    }
}
