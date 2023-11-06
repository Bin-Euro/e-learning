using Cursus.DTO.Catalog;
using Cursus.DTO;
using Cursus.Entities;
using Cursus.DTO.Cart;

namespace Cursus.Services.Interfaces
{
    public interface ICartService
    {
        /*        Task<IEnumerable<CartResponse>> GetAll();*/
        Task<ResultDTO<CartResponse>> GetByUserID();
        Task<ResultDTO<CreateCart>> CreateCart(CreateCart catalogRequest);
        Task<ResultDTO<AddOrRemoveCartRequest>> AddToCart(AddOrRemoveCartRequest request );
        Task<ResultDTO<AddOrRemoveCartRequest>> RemoveItem(AddOrRemoveCartRequest request );
        Task<ResultDTO<CartResponse>> ConfirmCart(string userId, List<string> courseIds);


        /* Task<bool> DeleteCatalog(CatalogResDTO catalogRequest);

         Task<CatalogRepDTO> GetCatalogByName(CatalogResDTO catalogRequest);

         Task<ResultDTO<Catalog>> CatalogExists(string name);*/
    }
}
