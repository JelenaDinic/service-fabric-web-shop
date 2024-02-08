using Microsoft.ServiceFabric.Services.Remoting;

namespace Communication
{
    public interface IProductInterface : IService
    {
        Task<List<Product>> GetAllProducts();

        Task<Product> GetProductById(string productId);
    }
}
