namespace Cursus.Repositories.Interfaces
{
    public interface ICartRepository
    {
        List<Cart> Get();
        Cart GetByUserID(string id);
        Cart Create(Cart Cart);
        void Update(string id, Cart Cart);
        void Remove(string id);
    }
}
