using System;
using System.Collections.Generic;
using Cursus.Data.Interface;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using MongoDB.Driver;

namespace Cursus.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ICartDbContext _cartCollection;

        public CartRepository(ICartDbContext dbContext)
        {
            _cartCollection = dbContext;
        }

        public List<Cart> Get()
        {
            return _cartCollection.Cart.Find(cart => true).ToList();
        }

        public Cart GetByUserID(string id)
        {
            return _cartCollection.Cart.Find(cart => cart.UserID == id).FirstOrDefault();
        }

        public Cart Create(Cart cart)
        {
            if (cart == null)
            {
                throw new ArgumentNullException(nameof(cart));
            }

            _cartCollection.Cart.InsertOne(cart);
            return cart;
        }

        public void Update(string id, Cart cart)
        {
            if (cart == null)
            {
                throw new ArgumentNullException(nameof(cart));
            }

            _cartCollection.Cart.ReplaceOne(c => c.Id == id, cart);
        }

        public void Remove(string id)
        {
            _cartCollection.Cart.DeleteOne(cart => cart.Id == id);
        }


    }
}
