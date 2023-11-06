using Cursus.DTO.Cart;

namespace Cursus.Repositories.Interfaces
{
    public interface ICartItemRepository
    {
        CartItem AddToCart(string UserID, CartItem cartItem);
        bool RemoveItem(string UserID, string CourseID);
    }
}
