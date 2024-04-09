using Cursus.Data.Interface;
using Cursus.Entities;
using MongoDB.Driver;

namespace Cursus.Data
{
    public class MongoDbContext : IMongoDbContext
    {
        public IMongoCollection<Quiz> Quiz { get; }
        public IMongoCollection<QuizAnswer> QuizAnswer { get; }

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabadeName"));

            Quiz = database.GetCollection<Quiz>("quiz");
            QuizAnswer = database.GetCollection<QuizAnswer>("quizAnswer");
        }
    }
}