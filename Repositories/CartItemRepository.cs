using Amazon.Runtime.Internal;
using Cursus.Data;
using Cursus.Data.Interface;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using MongoDB.Driver;

namespace Cursus.Repositories
{
    public class CartItemRepository : ICartItemRepository
    {
        private readonly ICartDbContext _dbContext;
        public CartItemRepository(ICartDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public CartItem AddToCart( string UserID,CartItem cartItem)
        {
            var filter = Builders<Cart>.Filter.Eq(c => c.UserID, UserID);
            var update = Builders<Cart>.Update.Push(c => c.Items, cartItem);

            var updateResult =  _dbContext.Cart.UpdateOneAsync(filter, update);

            if (updateResult.IsCompleted)
            {
                return cartItem;
            }
            else
            {
                return null;
            }
        }

        public bool RemoveItem(string userID, string courseID)
        {
            var filter = Builders<Cart>.Filter.Eq(c => c.UserID, userID);
            var update = Builders<Cart>.Update.PullFilter(c => c.Items, ci => ci.CourseID == courseID);
            var findOptions = new FindOneAndUpdateOptions<Cart>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updatedCart = _dbContext.Cart.FindOneAndUpdateAsync(filter, update, findOptions);

            if (updatedCart != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }   
}
