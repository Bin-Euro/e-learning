using MongoDB.Driver;

namespace Cursus.Data.Interface
{
    public interface ICartDbContext
    {
        IMongoCollection<Cart> Cart { get; }
    }
}
