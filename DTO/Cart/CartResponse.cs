using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Cursus.DTO.Cart
{
    public class CartResponse
    {
        public string Id { get; set; }

        public string UserID { get; set; }

        public List<CartItemsDTO> Items { get; set; }

        public double TotalPrice
        {
            get
            {
                double totalPrice = Items.Sum(item => item.Price); 
                return totalPrice;
            }
            set { }
        }
    }
}
