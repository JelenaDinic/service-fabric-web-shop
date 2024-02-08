using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
    public interface ICartInterface : IService
    {
        Task AddToCart(Cart cart);

        Task RemoveFromCart(int cartId);

        Task<List<Cart>> GetCartsByCustomer(int customerId);
        Task<List<Cart>> GetCartsHistory(int customerId);

        Task UpdateCart(Cart cart);
    }
}
