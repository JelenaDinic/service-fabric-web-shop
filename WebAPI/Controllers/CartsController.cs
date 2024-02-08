using Communication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartsController : ControllerBase
    {
        [HttpPost]
        [Route("api/add-to-cart")]
        public async Task AddToCart([FromBody] Cart cart)
        {
            var partitionId = cart.Id % 3;
            var statefulProxy = ServiceProxy.Create<ICartInterface>(
                new Uri("fabric:/NewServiceFabricApplication/CartsService"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

            await statefulProxy.AddToCart(cart);

        }
        [HttpDelete]
        [Route("api/remove-from-cart/{id}")]
        public async Task<string> RemoveFromCart(int id)
        {
            try
            {
                var partitionId = id % 3;
                var statefulProxy = ServiceProxy.Create<ICartInterface>(
                    new Uri("fabric:/NewServiceFabricApplication/CartsService"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

                await statefulProxy.RemoveFromCart(id);

                return $"Cart with Id {id} removed successfully.";
            }
            catch (Exception ex)
            {
                return $"Internal server error: {ex.Message}";
            }
        }

        // New method for getting all carts by customerId
        [HttpGet]
        [Route("api/get-carts-by-customer/{customerId}")]
        public async Task<List<Cart>> GetCartsByCustomer(int customerId)
        {
            try
            {
                var partitionId = customerId % 3;
                var statefulProxy = ServiceProxy.Create<ICartInterface>(
                    new Uri("fabric:/NewServiceFabricApplication/CartsService"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

                var carts = await statefulProxy.GetCartsByCustomer(customerId);

                return carts;
            }
            catch (Exception ex)
            {
                return [];
            }
        }

        [HttpGet]
        [Route("api/get-carts-history/{customerId}")]
        public async Task<List<Cart>> GetCartsHistory(int customerId)
        {
            try
            {
                var partitionId = customerId % 3;
                var statefulProxy = ServiceProxy.Create<ICartInterface>(
                    new Uri("fabric:/NewServiceFabricApplication/CartsService"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

                var carts = await statefulProxy.GetCartsHistory(customerId);

                return carts;
            }
            catch (Exception ex)
            {
                return [];
            }
        }

        [HttpPut]
        [Route("api/update-cart")]
        public async Task<string> UpdateCart([FromBody] Cart cart)
        {
            try
            {
                var partitionId = cart.CustomerId % 3;
                var statefulProxy = ServiceProxy.Create<ICartInterface>(
                    new Uri("fabric:/NewServiceFabricApplication/CartsService"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

                await statefulProxy.UpdateCart(cart);

                return "Cart updated successfully.";
            }
            catch (Exception ex)
            {
                return $"Internal server error: {ex.Message}";
            }
        }
    }
}
